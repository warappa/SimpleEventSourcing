using SimpleEventSourcing.WriteModel;
using System;
using System.Reactive.Linq;

namespace Shop
{

    public static class InjectRepoEx
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T, IEventRepository> onNext, IEventRepository repo)
        {
            void a(T x) => onNext(x, repo);
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
