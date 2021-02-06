using SimpleEventSourcing.Bus;
using SimpleEventSourcing.Domain;
using SimpleEventSourcing.Messaging;
using SimpleEventSourcing.WriteModel;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleEventSourcing.UI.ConsoleUI
{
    public static class InjectRepoEx
    {

        public static IDisposable SubscribeToAndUpdate<TMessage, TEntity>(this ObservableMessageBus source, Action<TEntity, TMessage> onNext, IEventRepository repo)
            where TMessage : class, IMessage<IEventSourcedEntityCommand>
            where TEntity : class, IEventSourcedEntity
        {
            Func<TMessage, Task> a = async obj =>
            {
                var ent = await repo.GetAsync<TEntity>(obj.Body.Id);

                onNext(ent, obj);

                await repo.SaveAsync(ent);
            };

            return BusExtensions.SubscribeTo<TMessage>(source)
                .Select(x => Observable.FromAsync(() => a(x)))
                .Concat()
                .Subscribe();
        }
        
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T, IEventRepository> onNext, IEventRepository repo)
        {
            Action<T> a = null;
            a = obj => onNext(obj, repo);
            return source.Subscribe(a);
        }

        public static Action<A> WithRepo<A>(this Action<A, IEventRepository> f, IEventRepository repo)
        {
            return b => f(b, repo);
        }
        public static Func<B, R> Partial<A, B, R>(this Func<A, B, R> f, A a)
        {
            return b => f(a, b);
        }

        public static Func<A, Func<B, R>> Curry<A, B, R>(this Func<A, B, R> f)
        {
            return a => b => f(a, b);
        }
    }
}