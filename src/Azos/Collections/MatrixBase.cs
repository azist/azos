/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;
using System.Runtime.CompilerServices;

namespace Azos.Collections
{
  /// <summary>
  /// Provides base for various matrices
  /// </summary>
  public abstract class MatrixBase<T> : IEnumerable<T>, IEquatable<MatrixBase<T>>
  {
    #region Abstract

    public abstract int Rank { get; }

    public abstract int GetLowerBound(int dimension);

    public abstract int GetUpperBound(int dimension);

    public abstract IEnumerator<T> GetMatrixEnumerator();

    #endregion

    #region Object overrides

    public override string ToString()
    {
      StringBuilder b = new StringBuilder();

      b.Append(GetType().DisplayNameWithExpandedGenericArgs());

      b.Append('[');

      for (int r = 0; r < Rank; r++)
      {
        if (r > 0)
          b.Append(" x ");

        int lowerBound = GetLowerBound(r);
        int upperBound = GetUpperBound(r);

        if (lowerBound == 0)
          b.Append(upperBound.ToString());
        else
          b.AppendFormat("{0}..{1}", lowerBound, upperBound);
      }
      b.Append("]");

      return b.ToString();
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;

      if (!obj.GetType().Equals(this.GetType()))
        return false;

      return this.Equals((MatrixBase<T>)obj);
    }

    #endregion

    #region IEnumerable<T>

    public IEnumerator<T> GetEnumerator() => GetMatrixEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region IEquatable<T>

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    public bool Equals(MatrixBase<T> other)
    {
      if (other == null)
        return false;

      if (object.ReferenceEquals(this, other))
        return true;

      if (this.Rank != other.Rank)
        return false;

      for (int r = 0; r < Rank; r++)
        if (this.GetLowerBound(r) != other.GetLowerBound(r) || this.GetUpperBound(r) != other.GetUpperBound(r))
          return false;

      IEnumerator<T> ie0 = GetEnumerator();
      IEnumerator<T> ie1 = other.GetEnumerator();

      while (ie0.MoveNext() && ie1.MoveNext())
      {
        T cur0 = ie0.Current;
        T cur1 = ie1.Current;

        if (!object.Equals(cur0, cur1))
          return false;
      }

      return true;
    }

    #endregion
  }

}
