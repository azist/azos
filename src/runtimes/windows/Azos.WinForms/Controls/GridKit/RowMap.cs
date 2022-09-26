/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

namespace Azos.WinForms.Controls.GridKit
{

  /// <summary>
  /// Holds mappings of rows to grid vertical plane
  /// </summary>
  public sealed class RowMap : List<RowMapEntry>
  {
     public bool HasRow(object row)
     {
       return this.FirstOrDefault( entry => entry.Row == row) != null;
     }
  }


  /// <summary>
  /// Contains mapping information of rows to vertical grid plane
  /// </summary>
  public class RowMapEntry
  {
    public object Row;
    public int Top;
    public int Height;
  }


}
