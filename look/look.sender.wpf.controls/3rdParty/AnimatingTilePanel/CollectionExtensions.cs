﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace look.sender.wpf.controls._3rdParty.AnimatingTilePanel
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics;

    public static class CollectionExtensions
    {
        /// <summary>
        /// Calls the provided action on each item, providing the item and its index into the source.
        /// </summary>
        public static void CountForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int i = 0;
            source.ForEach(item => action(item, i++));
        }

        public static IEnumerable<TTarget> CountSelect<TSource, TTarget>(this IEnumerable<TSource> source, Func<TSource, int, TTarget> func)
        {
            int i = 0;
            foreach (var item in source)
            {
                yield return func(item, i++);
            }
        }

        /// <summary>
        ///     Returns true if all items in the list are unique using
        ///     <see cref="EqualityComparer{T}.Default">EqualityComparer&lt;T&gt;.Default</see>.
        /// </summary>
        /// <exception cref="ArgumentNullException">if <param name="source"/> is null.</exception>
        public static bool AllUnique<T>(this IList<T> source)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            return source.TrueForAllPairs((a, b) => !comparer.Equals(a, b));
        }

        /// <summary>
        ///     Returns true if <paramref name="compare"/> returns
        ///     true for every pair of items in <paramref name="source"/>.
        /// </summary>
        public static bool TrueForAllPairs<T>(this IList<T> source, Func<T, T, bool> compare)
        {
            for (int i = 0; i < source.Count; i++)
            {
                for (int j = i + 1; j < source.Count; j++)
                {
                    if (!compare(source[i], source[j]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns true if <paramref name="compare"/> returns true of every
        ///     adjacent pair of items in the <paramref name="source"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     If there are n items in the collection, n-1 comparisons are done.
        /// </para>
        /// <para>
        ///     Every valid [i] and [i+1] pair are passed into <paramref name="compare"/>.
        /// </para>
        /// <para>
        ///     If <paramref name="source"/> has 0 or 1 items, true is returned.
        /// </para>
        /// </remarks>
        public static bool TrueForAllAdjacentPairs<T>(this IEnumerable<T> source, Func<T, T, bool> compare)
        {
            return source.SelectAdjacentPairs().All(t => compare(t.Item1, t.Item2));
        }

        public static IEnumerable<Tuple<T, T>> SelectAdjacentPairs<T>(this IEnumerable<T> source)
        {
            bool hasPrevious = false;
            T previous = default(T);

            foreach (var item in source)
            {
                if (!hasPrevious)
                {
                    previous = item;
                    hasPrevious = true;
                }
                else
                {
                    yield return Tuple.Create(previous, item);
                    previous = item;
                }
            }
        }

        /// <summary>
        ///     Returns true if all of the items in <paramref name="source"/> are not
        ///     null or empty.
        /// </summary>
        /// <exception cref="ArgumentNullException">if <param name="source"/> is null.</exception>
        public static bool AllNotNullOrEmpty(this IEnumerable<string> source)
        {
            return source.All(item => !string.IsNullOrEmpty(item));
        }

        /// <summary>
        ///     Returns true if all items in <paramref name="source"/> exist
        ///     in <paramref name="set"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">if <param name="source"/> or <param name="set"/> are null.</exception>
        public static bool AllExistIn<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> set)
        {
            return source.All(item => set.Contains(item));
        }

        /// <summary>
        ///     Returns true if <paramref name="source"/> has no items in it; otherwise, false.
        /// </summary>
        /// <remarks>
        /// <para>
        ///     If an <see cref="ICollection{TSource}"/> is provided,
        ///     <see cref="ICollection{TSource}.Count"/> is used.
        /// </para>
        /// <para>
        ///     Yes, this does basically the same thing as the
        ///     <see cref="System.Linq.Enumerable.Any{TSource}(IEnumerable{TSource})"/>
        ///     extention. The differences: 'IsEmpty' is easier to remember and it leverages
        ///     <see cref="ICollection{TSource}.Count">ICollection.Count</see> if it exists.
        /// </para>
        /// </remarks>
        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source)
        {
            if (source is ICollection<TSource>)
            {
                return ((ICollection<TSource>)source).Count == 0;
            }
            else
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    return !enumerator.MoveNext();
                }
            }
        }

        /// <summary>
        ///     Returns the index of the first item in <paramref name="source"/>
        ///     for which <paramref name="predicate"/> returns true. If none, -1.
        /// </summary>
        /// <param name="source">The source enumerable.</param>
        /// <param name="predicate">The function to evaluate on each element.</param>
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            int index = 0;
            foreach (TSource item in source)
            {
                if (predicate(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        ///     Returns a new <see cref="ReadOnlyCollection{TSource}"/> using the
        ///     contents of <paramref name="source"/>.
        /// </summary>
        /// <remarks>
        ///     The contents of <paramref name="source"/> are copied to
        ///     an array to ensure the contents of the returned value
        ///     don't mutate.
        /// </remarks>
        public static ReadOnlyCollection<TSource> ToReadOnlyCollection<TSource>(this IEnumerable<TSource> source)
        {
            return new ReadOnlyCollection<TSource>(source.ToArray());
        }

        /// <summary>
        ///     Performs the specified <paramref name="action"/>
        ///     on each element of the specified <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The sequence to which is applied the specified <paramref name="action"/>.</param>
        /// <param name="action">The action applied to each element in <paramref name="source"/>.</param>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
            }
        }

        /// <summary>
        ///     Removes the last element from <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The list from which to remove the last element.</param>
        /// <returns>The last element.</returns>
        /// <remarks><paramref name="source"/> must have at least one element and allow changes.</remarks>
        public static TSource RemoveLast<TSource>(this IList<TSource> source)
        {
            TSource item = source[source.Count - 1];
            source.RemoveAt(source.Count - 1);
            return item;
        }

        /// <summary>
        ///     If <paramref name="source"/> is null, return an empty <see cref="IEnumerable{TSource}"/>;
        ///     otherwise, return <paramref name="source"/>.
        /// </summary>
        public static IEnumerable<TSource> EmptyIfNull<TSource>(this IEnumerable<TSource> source)
        {
            return source ?? Enumerable.Empty<TSource>();
        }

        /// <summary>
        ///     Recursively projects each nested element to an <see cref="IEnumerable{TSource}"/>
        ///     and flattens the resulting sequences into one sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="recursiveSelector">A transform to apply to each element.</param>
        /// <returns>
        ///     An <see cref="IEnumerable{TSource}"/> whose elements are the
        ///     result of recursively invoking the recursive transform function
        ///     on each element and nested element of the input sequence.
        /// </returns>
        /// <remarks>This is a depth-first traversal. Be careful if you're using this to find something
        /// shallow in a deep tree.</remarks>
        public static IEnumerable<TSource> SelectRecursive<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>> recursiveSelector)
        {
            var stack = new Stack<IEnumerator<TSource>>();
            stack.Push(source.GetEnumerator());

            try
            {
                while (stack.Any())
                {
                    if (stack.Peek().MoveNext())
                    {
                        var current = stack.Peek().Current;

                        yield return current;

                        stack.Push(recursiveSelector(current).GetEnumerator());
                    }
                    else
                    {
                        stack.Pop().Dispose();
                    }
                }
            }
            finally
            {
                while (stack.Any())
                {
                    stack.Pop().Dispose();
                }
            }
        } //*** SelectRecursive

        public static IList<TTo> ToCastList<TFrom, TTo>(this IList<TFrom> source) where TFrom : TTo
        {
            return new CastList<TFrom, TTo>(source);
        }

        public static T Random<T>(this IList<T> source)
        {
            return source[Util.Rnd.Next(source.Count)];
        }

        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, T, bool> comparer)
        {
            return source.Distinct(comparer.ToEqualityComparer());
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> source, params T[] items)
        {
            return source.Concat(items.AsEnumerable());
        }

        public static bool Contains<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            return dictionary.Contains(new KeyValuePair<TKey, TValue>(key, value));
        }

        public static bool CountAtLeast<T>(this IEnumerable<T> source, int count)
        {
            if (source is ICollection<T>)
            {
                return ((ICollection<T>)source).Count >= count;
            }
            else
            {
                using (var enumerator = source.GetEnumerator())
                {
                    while (count > 0)
                    {
                        if (enumerator.MoveNext())
                        {
                            count--;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public static IEnumerable<TSource> Except<TSource, TOther>(this IEnumerable<TSource> source, IEnumerable<TOther> other, Func<TSource, TOther, bool> comparer)
        {
            return from item in source
                   where !other.Any(x => comparer(item, x))
                   select item;
        }

        public static IEnumerable<TSource> Intersect<TSource, TOther>(this IEnumerable<TSource> source, IEnumerable<TOther> other, Func<TSource, TOther, bool> comparer)
        {
            return from item in source
                   where other.Any(x => comparer(item, x))
                   select item;
        }

        public static INotifyCollectionChanged AsINPC<T>(this ReadOnlyObservableCollection<T> source)
        {
            return (INotifyCollectionChanged)source;
        }

        /// <summary>
        /// Creates an <see cref="ObservableCollection"/> from the <see cref="IEnumerable"/>.
        /// </summary>
        /// <typeparam name="T">The type of the source elements.</typeparam>
        /// <param name="source">The <see cref="IEnumerable"/> to create the <see cref="ObservableCollection"/> from.</param>
        /// <returns>An <see cref="ObservableCollection"/> that contains elements from the input sequence.</returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
        }

        public static void Synchronize<TSource, TTarget>(this ObservableCollectionPlus<TTarget> targetCollection, IList<TSource> sourceCollection, Func<TSource, TTarget, bool> matcher, Func<TSource, TTarget> mapper) where TSource : IEquatable<TSource>
        {
            using (targetCollection.BeginMultiUpdate())
            {
                // move wrappers around to the right places
                // or create a new one
                for (int i = 0; i < sourceCollection.Count; i++)
                {
                    var sourceItem = sourceCollection[i];
                    var targetIndex = targetCollection.IndexOf(targetItem => matcher(sourceItem, targetItem));
                    if (targetIndex >= 0)
                    {
                        if (targetIndex != i)
                        {
                            Debug.Assert(targetIndex > i, "this would only happen if we have duplicates...which we should never have!");
                            targetCollection.Move(targetIndex, i);
                        }
                        else
                        {
                            // NOOP - already in the right spot! :-)
                        }
                    }
                    else
                    {
                        var newItem = mapper(sourceItem);
                        Debug.Assert(matcher(sourceItem, newItem));
                        targetCollection.Insert(i, newItem);
                    }
                }

                // Remove anything left
                while (targetCollection.Count > sourceCollection.Count)
                {
                    targetCollection.RemoveLast();
                }

                Debug.Assert(sourceCollection.Count == targetCollection.Count);
#if DEBUG
                for (int i = 0; i < sourceCollection.Count; i++)
                {
                    Debug.Assert(matcher(sourceCollection[i], targetCollection[i]));
                }
#endif
            }
        }

        public static IDictionary<TKey, TValue> Clone<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            return source.ToDictionary(p => p.Key, p => p.Value);
        }

        public static bool TryGetTypedValue<TOutput, TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TOutput value) where TOutput : TValue
        {
            TValue val;
            if (dictionary.TryGetValue(key, out val))
            {
                if (val is TOutput)
                {
                    value = (TOutput)val;
                    return true;
                }
            }
            value = default(TOutput);
            return false;
        }

        public static TValue EnsureItem<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = valueFactory();
                dictionary.Add(key, value);
            }
            return value;
        }
    }
}
