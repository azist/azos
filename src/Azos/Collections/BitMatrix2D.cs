/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections;

namespace Azos.Collections
{
  /// <summary>
  /// Provides an efficient access to a square 2D bit matrix accessible as boolean values
  /// </summary>
  public class BitMatrix2D : Matrix2DBase<bool>
  {
    #region .ctor

    public BitMatrix2D(int dimension) : this(dimension, dimension) { }

    public BitMatrix2D(int width, int height) : base(width, height)
    {
      m_Bits = new BitArray(width * height);
    }

    #endregion

    #region Pvt/Prot/Int Fields

    private BitArray m_Bits;

    #endregion

    #region Properties

    /// <summary>
    /// Provides access to bit values by X,Y matric coordinates
    /// </summary>
    public override bool this[int x, int y]
    {
      get { return m_Bits[Width * y + x]; }
      set { m_Bits[Width * y + x] = value; }
    }

    #endregion

    #region Public

    /// <summary>
    /// Fills homogeneous sub-area of this square with the specified value
    /// </summary>
    public void FillSubArea(int left, int top, int width, int height, bool value)
    {
      if (left < 0 || top < 0 || width < 1 || height < 1)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + "left and top couldn't be less than 0 and width and height couldn't be less than 1");

      int right = left + width;
      int bottom = right + height;

      if (right >= Width || bottom >= Height)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + "required subarea is out of main square bounds");

      for (int x = left; x <= right; x++)
        for (int y = top; y <= bottom; y++)
          m_Bits[x + y * Width] = value;
    }

    public override bool Equals(object otherObj)
    {
      BitMatrix2D otherSquare = otherObj as BitMatrix2D;
      if (otherSquare == null || otherSquare.Width != Width || otherSquare.Height != Height)
        return false;

      var ieThis = m_Bits.GetEnumerator();
      var ieOther = otherSquare.m_Bits.GetEnumerator();

      while (ieThis.MoveNext() && ieOther.MoveNext())
        if (ieThis.Current != ieOther.Current)
          return false;

      return true;
    }

    public override int GetHashCode() => Width ^ Height ^ m_Bits.GetHashCode();

    public override string ToString()
      => "{0}[{1}x{2}]".Args(GetType().Name, Width, Height);

    #endregion

    #region MatrixBase<bool> implementation

    #endregion
  }//class

}
