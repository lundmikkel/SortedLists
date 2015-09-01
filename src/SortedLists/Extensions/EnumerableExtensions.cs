namespace SortedLists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    public static class EnumerableExtensions
    {
        /// <summary>
        /// Convert an IEnumerator to an IEnumerable.
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext()) yield return enumerator.Current;
        }

        [Pure]
        public static bool ForAllConsecutiveElements<T>(this IEnumerable<T> collection, Func<T, T, bool> predicate)
        {
            Contract.Requires(collection != null);
            Contract.Requires(predicate != null);

            using (var enumerator = collection.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    var previous = enumerator.Current;

                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;

                        if (!predicate(previous, current))
                            return false;

                        previous = current;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Check if an IEnumerable is sorted in non-descending order using a comparer.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="comparer">The comparer for the collection.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>True if sorted.</returns>
        [Pure]
        public static bool IsSorted<T>(this IEnumerable<T> collection, IComparer<T> comparer)
        {
            Contract.Requires(collection != null);
            Contract.Requires(comparer != null);

            return collection.ForAllConsecutiveElements((x, y) => comparer.Compare(x, y) <= 0);
        }

        /// <summary>
        /// Check if an IEnumerable is sorted in non-descending order using a comparer.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="comparer">The comparer for the collection.</param>
        /// <typeparam name="T">The element type.</typeparam>
        /// <returns>True if sorted.</returns>
        [Pure]
        public static bool IsSorted<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            Contract.Requires(collection != null);

            return collection.ForAllConsecutiveElements((x, y) => x.CompareTo(y) <= 0);
        }

        [Pure]
        public static bool IsStrictlySorted<T>(this IEnumerable<T> collection, IComparer<T> comparer)
        {
            Contract.Requires(collection != null);
            Contract.Requires(comparer != null);

            return collection.ForAllConsecutiveElements((x, y) => comparer.Compare(x, y) < 0);
        }

        [Pure]
        public static bool IsStrictlySorted<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            Contract.Requires(collection != null);

            return collection.ForAllConsecutiveElements((x, y) => x.CompareTo(y) < 0);
        }

        [Pure]
        public static bool IsSortedBackwards<T>(this IEnumerable<T> collection, IComparer<T> comparer)
        {
            Contract.Requires(collection != null);
            Contract.Requires(comparer != null);

            return collection.ForAllConsecutiveElements((x, y) => comparer.Compare(x, y) >= 0);
        }

        [Pure]
        public static bool IsSortedBackwards<T>(this IEnumerable<T> collection) where T : IComparable<T>
        {
            Contract.Requires(collection != null);

            return collection.ForAllConsecutiveElements((x, y) => x.CompareTo(y) >= 0);
        }
    }
}
