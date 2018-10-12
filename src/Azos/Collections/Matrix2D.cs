
namespace Azos.Collections
{
  /// <summary>
  /// Represents a two deminsional matrix of T.
  /// This class uses jagged arrays for internal implementation ensuring proper array sizing per matrix structure
  /// </summary>
  /// <typeparam name="T">Any desired type</typeparam>
  public class Matrix2D<T>: Matrix2DBase<T>
  {
    #region .ctor

      public Matrix2D(int width, int height): base(width, height)
      {
        Array = new T[height][];
        for (int y = 0; y < height; y++)
          Array[y] = new T[width];
      }

    #endregion

    #region Fields / Properties

      public readonly T[][] Array;

      public override T this[int x, int y]
      {
        get { return Array[y][x]; }
        set { Array[y][x] = value; }
      }

    #endregion

    #region Public

      public void Fill(T value)
      {
        for (int y = 0; y < Array.Length; y++)
          for (int x = 0; x < Array.Length; x++)
            Array[y][x] = value;
      }

    #endregion

  }//class

}
