/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Azos.Collections
{
  /// <summary>
  /// Checks for reference equality
  /// </summary>
  public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
  {
    public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

    private ReferenceEqualityComparer() {}

    public bool Equals(T x, T y) => object.ReferenceEquals(x, y);

    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);

    bool IEqualityComparer.Equals(object x, object y) => object.ReferenceEquals(x, y);

    int IEqualityComparer.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
  }
}

