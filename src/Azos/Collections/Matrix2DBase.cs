
using System;
using System.Collections.Generic;

using System.Text;

namespace Azos.Collections
{
  public abstract class Matrix2DBase<T>: MatrixBase<T>
  {
    #region .ctor

      public Matrix2DBase(int width, int height)
      {
        if (width <= 0 || height <= 0)
          throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".ctor(width>0&height>0)");

        Width = width;
        Height = height;
      }

    #endregion

    #region Properties

      public readonly int Width, Height;

    #endregion

    #region Abstract

      public abstract T this[int x, int y] { get; set; }

    #endregion

    #region MatrixBase<T> implementation

      public override int Rank
      {
        get { return 2; }
      }

      public override int GetLowerBound(int dimension)
      {
        return 0;
      }

      public override int GetUpperBound(int dimension)
      {
        switch (dimension)
        {
          case 0:
            return Width;
          case 1:
            return Height;
          default:
            throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetUpperBound(dimension>=0&<2)");
        }
      }

      public override IEnumerator<T> GetMatrixEnumerator()
      {
        for (int y = 0; y < Height; y++)
          for (int x = 0; x < Width; x++)
            yield return this[x, y];
      }

    #endregion

    #region Object overrides

      public override string ToString()
      {
        StringBuilder b = new StringBuilder();

        for (int y = 0; y < Height; y++)
        {
          if (y != 0)
            b.AppendLine();

          for (int x = 0; x < Width; x++)
          {
            if (x != 0)
              b.Append(' ');

            b.Append(this[x, y]);
          }
        }

        return b.ToString();
      }

    #endregion
  }//class

}
