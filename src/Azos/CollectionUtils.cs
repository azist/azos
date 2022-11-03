/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos
{
  /// <summary>
  /// Extensions for standard collections
  /// </summary>
  public static class CollectionUtils
  {

    /// <summary>
    /// Adds the specified element at the end of the sequence. Sequence may be null
    /// </summary>
    public static IEnumerable<T> AddOneAtEnd<T>(this IEnumerable<T> existing, T one)
    {
      if (existing != null) foreach (var e in existing) yield return e;
      yield return one;
    }

    /// <summary>
    /// Adds the specified element at the start of the sequence. Sequence may be null
    /// </summary>
    public static IEnumerable<T> AddOneAtStart<T>(this IEnumerable<T> existing, T one)
    {
      yield return one;
      if (existing != null) foreach (var e in existing) yield return e;
    }

    /// <summary>
    /// Fluent notation for foreach
    /// </summary>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T> action)
    {
      foreach (var item in src.NonNull(nameof(src))) action(item);

      return src;
    }

    /// <summary>
    /// Fluent notation for foreach with index
    /// </summary>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T, int> action)
    {
      int i = 0;
      foreach (var item in src.NonNull(nameof(src))) action(item, i++);

      return src;
    }

    /// <summary>
    /// Partitions the source sequence into subsequences of up to the specified size.
    /// The operation enumerates the source only once via an internal accumulator, making
    /// this function safe for deferred execution
    /// </summary>
    public static IEnumerable<IEnumerable<T>> BatchBy<T>(this IEnumerable<T> src, int size)
    {
      List<T> batch = null;

      foreach (var e in src.NonNull(nameof(src)))
      {
        if (batch != null && batch.Count == size)
        {
          yield return batch;
          batch = null;
        }

        if (batch == null) batch = new List<T>();
        batch.Add(e);
      }

      if (batch != null)
        yield return batch;
    }

    /// <summary>
    /// Takes all elements except for the last element from the given source
    /// </summary>
    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> src)
    {
      var buffer = default(T);
      var buffered = false;

      foreach(var x in src.NonNull(nameof(src)))
      {
        if (buffered)
          yield return buffer;

        buffer = x;
        buffered = true;
      }
    }

    /// <summary>
    /// Takes all but the last N elements from the source
    /// </summary>
    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> src, int n)
    {
      var buffer = new Queue<T>(n + 1);

      foreach(var x in src.NonNull(nameof(src)))
      {
        buffer.Enqueue(x);

        if (buffer.Count == n + 1)
          yield return buffer.Dequeue();
      }
    }


    public static TResult FirstMin<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector) where TComparand: IComparable
    {
      TComparand dummy;
      return source.FirstMin(selector, out dummy);
    }


    public static TResult FirstMin<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector,
                                                        out TComparand minComparand) where TComparand: IComparable
    {
      return firstMinMax(true, source, selector, out minComparand);
    }


    public static TResult FirstMax<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector) where TComparand: IComparable
    {
      TComparand dummy;
      return source.FirstMax(selector, out dummy);
    }


    public static TResult FirstMax<TResult, TComparand>(this IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector,
                                                        out TComparand maxComparand) where TComparand: IComparable
    {
      return firstMinMax(false, source, selector, out maxComparand);
    }

    private static TResult firstMinMax<TResult, TComparand>(bool min,
                                                        IEnumerable<TResult> source,
                                                        Func<TResult, TComparand> selector,
                                                        out TComparand latchedComparand) where TComparand: IComparable
    {
      var latchedResult = default(TResult);
      latchedComparand = default(TComparand);

      if (source==null || selector==null) return latchedResult;

      bool was = false;
      foreach(var elm in source)
      {
        var c = selector(elm);
        if (!was || (min ? c.CompareTo(latchedComparand)<0 : c.CompareTo(latchedComparand)>0))
        {
          latchedResult = elm;
          latchedComparand = c;
          was = true;
        }
      }

      return latchedResult;
    }


    /// <summary>
    /// Tries to find the first element that matches the predicate and returns it,
    /// otherwise returns the first element found or default (i.e. null)
    /// </summary>
    public static TResult FirstOrAnyOrDefault<TResult>(this IEnumerable<TResult> source, Func<TResult, bool> predicate)
    {
      if (source==null) return default(TResult);

      if (predicate!=null)
        foreach(var elm in source) if (predicate(elm)) return elm;

      return source.FirstOrDefault();
    }

    /// <summary>
    /// Returns a new array that contains source elements with additional elements appended at the end
    /// </summary>
    public static T[] AppendToNew<T>(this T[] source, params T[] elements)
    {
      if (source == null && elements == null) return new T[0];
      if (source == null) return (T[])elements.Clone();
      if (elements == null) return (T[])source.Clone();

      var result = new T[source.Length + elements.Length];

      source.CopyTo(result, 0);
      elements.CopyTo(result, source.Length);

      return result;
    }

    /// <summary>
    /// Returns an array concatenated from the first element and the rest, similar to JS rest spread operator: let x = [first, ...rest];
    /// </summary>
    public static T[] ConcatArray<T>(this T first, params T[] theRest)
    {
      if (theRest==null) return new[] { first };
      var result = new T[theRest.Length+1];
      result[0] = first;
      theRest.CopyTo(result, 1);
      return result;
    }

    /// <summary>
    /// Returns a sequence of items distinct by the selected key.
    /// If the key selector functor is null then returns input as-is
    /// </summary>
    public static IEnumerable<TResult> DistinctBy<TResult, TKey>(this IEnumerable<TResult> source,
                                                                 Func<TResult, TKey> selector,
                                                                 IEqualityComparer<TKey> distinctEqualityComparer = null)
    {
      if (source == null) yield break;
      if (selector == null)
      {
        foreach (var item in source) yield return item;
        yield break;
      }

      var set = distinctEqualityComparer!=null ? new HashSet<TKey>(distinctEqualityComparer) : new HashSet<TKey>();
      foreach(var item in source)
      {
        if (set.Add(selector(item)))
        {
          yield return item;
        }
      }
    }

    /// <summary>
    /// Makes an enumerable of T starting from the first element and concatenating others
    /// </summary>
    public static IEnumerable<T> ToEnumerable<T>(this T first, params T[] others)
    {
      yield return first;

      if (others != null)
        foreach(var other in others) yield return other;
    }

  }
}
