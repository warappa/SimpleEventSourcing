using System.Collections;
using System.Linq;
using System.Reflection;

namespace SimpleEventSourcing.Utils
{
    public static class EqualityExtensions
    {
        public static bool CollectionEqualtity(this IEnumerable objA, IEnumerable objB)
        {
            if (ReferenceEquals(objA, objB))
            {
                return true;
            }

            if (objA == null ||
                objB == null)
            {
                return false;
            }

            if (objA.Cast<object>().Count() != objB.Cast<object>().Count())
            {
                return false;
            }

            var bEnumerator = objB.GetEnumerator();
            bEnumerator.MoveNext();

            foreach (var valA in objA)
            {
                var valB = bEnumerator.Current;
                if (valA != valB)
                {
                    return false;
                }

                bEnumerator.MoveNext();
            }

            return true;
        }

        public static bool PropertyEqualtity(this object objA, object objB)
        {
            if (ReferenceEquals(objA, objB))
            {
                return true;
            }

            if (objB == null)
            {
                return false;
            }

            var typeA = objA.GetType();
            var typeB = objB.GetType();

            if (typeA != typeB)
            {
                return false;
            }

            var propsA = typeA.GetTypeInfo().DeclaredProperties
                .ToDictionary(x => x.Name, x => x);

            var propsB = typeB.GetTypeInfo().DeclaredProperties
                .ToDictionary(x => x.Name, x => x);

            if (propsA.Count != propsB.Count)
            {
                return false;
            }

            foreach (var prop in propsA)
            {
                if (!propsB.ContainsKey(prop.Key))
                {
                    return false;
                }

                var valueA = prop.Value.GetValue(objA);
                var valueB = propsB[prop.Key].GetValue(objB);

                if (ReferenceEquals(valueA, valueB))
                {
                    continue;
                }

                if (valueA == null ||
                    valueB == null)
                {
                    return false;
                }

                if (valueA is IEnumerable ||
                    valueB is IEnumerable)
                {
                    if (!CollectionEqualtity(valueA as IEnumerable, valueB as IEnumerable))
                    {
                        return false;
                    }
                }
                else if (!valueA.Equals(valueB))
                {
                    return false;
                }
            }

            return true;
        }

        public static int PropertyHashCode(this object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            var type = obj.GetType();

            var props = type.GetTypeInfo().DeclaredProperties;

            var hash = 0;

            foreach (var prop in props)
            {
                hash += (hash * 17) + prop.GetValue(obj)?.GetHashCode() ?? 0;
            }

            return hash;
        }
    }
}
