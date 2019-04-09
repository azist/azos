/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Azos.Collections
{
  /// <summary>
  /// Checks for reference equality. Use ReferenceEqualityComparer(T).Default
  /// </summary>
  public sealed class ReferenceEqualityComparer<T> : EqualityComparer<T>
  {
    public ReferenceEqualityComparer() {}

    public override bool Equals(T x, T y)
    {
        return object.ReferenceEquals(x, y);
    }

    public override int GetHashCode(T obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
  }
}

