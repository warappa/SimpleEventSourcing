﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.EntityFrameworkCore.Internal
{
    // From: https://github.com/jsakamoto/EntityFrameworkCore.IndexAttribute/blob/master/EntityFrameworkCore.IndexAttribute/AttributedIndexBuilderExtension.cs
    public static class AttributedIndexBuilderExtension
    {
        private class IndexParam
        {
            public string IndexName { get; }

            public bool IsUnique { get; }

            public string[] PropertyNames { get; }

            public IndexParam(IndexAttribute indexAttr, params PropertyInfo[] properties)
            {
                this.IndexName = indexAttr.Name;
                this.IsUnique = indexAttr.IsUnique;
                this.PropertyNames = properties.Select(prop => prop.Name).ToArray();
            }
        }

        public static void BuildIndexesFromAnnotations(this ModelBuilder modelBuilder)
        {
            var entityTypes = modelBuilder.Model.GetEntityTypes().ToList();
            entityTypes.ForEach(entityType =>
            {
                var items = entityType.ClrType
                    .GetProperties()
                    .SelectMany(prop =>
                        Attribute.GetCustomAttributes(prop, typeof(IndexAttribute))
                            .Cast<IndexAttribute>()
                            .Select(index => new { prop, index })
                    )
                    .ToArray();

                var indexParams = items
                    .Where(item => item.index.Name == "")
                    .Select(item => new IndexParam(item.index, item.prop));

                var namedIndexParams = items
                    .Where(item => item.index.Name != "")
                    .GroupBy(item => item.index.Name)
                    .Select(g => new IndexParam(
                        g.First().index,
                        g.OrderBy(item => item.index.Order).Select(item => item.prop).ToArray())
                    );

                var entity = modelBuilder.Entity(entityType.ClrType);
                foreach (var indexParam in indexParams.Concat(namedIndexParams))
                {
                    var indexBuilder = entity
                        .HasIndex(indexParam.PropertyNames)
                        .IsUnique(indexParam.IsUnique);
                    if (indexParam.IndexName != "")
                    {
                        indexBuilder.HasName(indexParam.IndexName);
                    }
                }
            });
        }
    }
}
