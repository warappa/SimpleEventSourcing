using FluentAssertions;
using NUnit.Framework;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.State;
using SimpleEventSourcing.Tests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SimpleEventSourcing.WriteModel.Tests
{
    [TestFixture]
    public abstract class EventRepositoryTestsBase
    {
        protected TestsBaseConfig config;
        private IPersistenceEngine persistenceEngine;
        private EventRepository target;

        protected EventRepositoryTestsBase(TestsBaseConfig config)
        {
            this.config = config;
        }

        [SetUp]
        public void Setup()
        {
            persistenceEngine = config.WriteModel.GetPersistenceEngine();

            target = new EventRepository(config.WriteModel.GetInstanceProvider(), persistenceEngine, config.WriteModel.GetRawStreamEntryFactory());
        }

        [TearDown]
        public void Teardown()
        {
            target?.Dispose();
        }

        [Test]
        public async Task Save_and_load_entity_with_child_entities()
        {
            var entity = new TestEntity(Guid.NewGuid().ToString(), "test");
            var child = entity.AddChild(Guid.NewGuid().ToString(), "child");
            child.Rename("child new name");
            await target.SaveAsync(entity);

            var loadedEntity = await target.GetAsync<TestEntity>(entity.Id);
            loadedEntity.Should().BeEquivalentTo(entity, x => x.ComparingByMembers<TestEntity>().ComparingByMembers<TestEntityState>().WithTracing());
            loadedEntity.State.Name.Should().Be("test");
        }

        [Test]
        public async Task Save_and_load_entity_with_child_entities_2()
        {
            var entity = new TestEntity(Guid.NewGuid().ToString(), "test");
            var child = entity.AddChild(Guid.NewGuid().ToString(), "child");
            child.Rename("child new name");
            child.Rename("child new name2");
            child.Rename("child new name3");
            child.Rename("child new name4");
            await target.SaveAsync(entity);

            var loadedEntity = await target.GetAsync<TestEntity>(entity.Id);
            //loadedEntity.Should().BeEquivalentTo(entity, x => x.ComparingByMembers<TestEntity>().ComparingByMembers<TestEntityState>().WithTracing());
            loadedEntity.State.Children.First().Name.Should().Be("child new name4");
        }

        class MagicConverter : JsonConverterFactory
        {

            public override bool CanConvert(Type typeToConvert) =>
                !typeToConvert.IsAbstract &&
                typeToConvert.GetConstructor(Type.EmptyTypes) != null &&
                typeToConvert
                    .GetProperties()
                    .Where(x => !x.CanWrite)
                    .Where(x => x.PropertyType.IsGenericType)
                    .Select(x => new
                    {
                        Property = x,
                        CollectionInterface = x.PropertyType.GetGenericInterfaces(typeof(IEnumerable<>)).FirstOrDefault()
                    })
                    .Where(x => x.CollectionInterface != null)
                    .Any();

            public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => (JsonConverter)Activator.CreateInstance(typeof(SuperMagicConverter<>).MakeGenericType(typeToConvert))!;

            class SuperMagicConverter<T> : JsonConverter<T> where T : new()
            {
                readonly Dictionary<string, (Type PropertyType, Action<T, object>? Setter, Action<T, object>? Adder)> PropertyHandlers;
                public SuperMagicConverter()
                {
                    PropertyHandlers = typeof(T)
                        .GetProperties()
                        .Select(x => new
                        {
                            Property = x,
                            CollectionInterface = !x.CanWrite && x.PropertyType.IsGenericType ? x.PropertyType.GetGenericInterfaces(typeof(ICollection<>)).FirstOrDefault() : null
                        })
                        .Select(x =>
                        {
                            var tParam = Expression.Parameter(typeof(T));
                            var objParam = Expression.Parameter(typeof(object));
                            Action<T, object>? setter = null;
                            Action<T, object>? adder = null;
                            Type? propertyType = null;
                            if (x.Property.CanWrite)
                            {
                                propertyType = x.Property.PropertyType;
                                setter = Expression.Lambda<Action<T, object>>(
                                    Expression.Assign(
                                        Expression.Property(tParam, x.Property),
                                        Expression.Convert(objParam, propertyType)),
                                    tParam,
                                    objParam)
                                    .Compile();
                            }
                            else
                            {
                                if (x.CollectionInterface != null)
                                {
                                    propertyType = x.CollectionInterface.GetGenericArguments()[0];
                                    adder = Expression.Lambda<Action<T, object>>(
                                        Expression.Call(
                                            Expression.Property(tParam, x.Property),
                                            x.CollectionInterface.GetMethod("Add"),
                                            Expression.Convert(objParam, propertyType)),
                                        tParam,
                                        objParam)
                                        .Compile();
                                }
                            }
                            return new
                            {
                                x.Property.Name,
                                setter,
                                adder,
                                propertyType
                            };
                        })
                        .Where(x => x.propertyType != null)
                        .ToDictionary(x => x.Name, x => (x.propertyType!, x.setter, x.adder));
                }
                public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotImplementedException();
                public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    var item = new T();
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            break;
                        }
                        if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            if (PropertyHandlers.TryGetValue(reader.GetString(), out var handler))
                            {
                                if (!reader.Read())
                                {
                                    throw new JsonException($"Bad JSON");
                                }
                                if (handler.Setter != null)
                                {
                                    handler.Setter(item, JsonSerializer.Deserialize(ref reader, handler.PropertyType, options));
                                }
                                else
                                {
                                    if (reader.TokenType == JsonTokenType.StartArray)
                                    {
                                        while (true)
                                        {
                                            if (!reader.Read())
                                            {
                                                throw new JsonException($"Bad JSON");
                                            }
                                            if (reader.TokenType == JsonTokenType.EndArray)
                                            {
                                                break;
                                            }
                                            handler.Adder!(item, JsonSerializer.Deserialize(ref reader, handler.PropertyType, options));
                                        }
                                    }
                                    else
                                    {
                                        reader.Skip();
                                    }
                                }
                            }
                            else
                            {
                                reader.Skip();
                            }
                        }
                    }
                    return item;
                }
            }
        }

        [Test]
        public async Task Serialize_snapshot_state_with_NewtonsoftJson()
        {
            var entity = new TestEntity(Guid.NewGuid().ToString(), "test");
            var child = entity.AddChild(Guid.NewGuid().ToString(), "child");

            for (var i = 0; i < 100; i++)
            {
                child.Rename($"child new name {i + 1}");
            }

            var binder = persistenceEngine.Serializer.Binder;
            var serializer = new JsonNetSerializer(binder);
            var json = serializer.Serialize(entity.State.GetType(), (object)entity.State);
            Console.WriteLine(json);
            var deserialized = (TestEntityState)serializer.Deserialize(entity.State.GetType(), json);

            deserialized.Children.First().Name.Should().Be("child new name 100");
        }

        [Test]
        public async Task Serialize_snapshot_state_with_SystemTextJson()
        {
            var entity = new TestEntity(Guid.NewGuid().ToString(), "test");
            var child = entity.AddChild(Guid.NewGuid().ToString(), "child");

            for (var i = 0; i < 100; i++)
            {
                child.Rename($"child new name {i + 1}");
            }

            var binder = persistenceEngine.Serializer.Binder;
            var serializer = new SystemTextJsonSerializer(binder);

            var json = serializer.Serialize(entity.State.GetType(), (object)entity.State);
            Console.WriteLine(json);
            var deserialized = (TestEntityState)serializer.Deserialize(entity.State.GetType(), json);

            deserialized.Children.First().Name.Should().Be("child new name 100");
        }
    }
}
