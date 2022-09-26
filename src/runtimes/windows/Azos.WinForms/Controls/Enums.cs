/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.WinForms.Controls
{
    /// <summary>
    /// Defines horizontal alignment
    /// </summary>
    public enum HAlignment
    {
      Left = 0,
      Center,
      Right,
      Near,
      Far
    }


    /// <summary>
    /// Defines types of background brushes
    /// </summary>
    public enum BGKind
    {
      Solid = 0,
      Hatched,
      VerticalGradient,
      HorizontalGradient,
      ForwardDiagonalGradient,
      BackwardDiagonalGradient
    }


    /// <summary>
    /// Defines directions for sorting in grid columns
    /// </summary>
    public enum SortDirection
    {
      FIRST = 0,


      None = 0,
      Up,
      Down,


      LAST = Down
    }

}
