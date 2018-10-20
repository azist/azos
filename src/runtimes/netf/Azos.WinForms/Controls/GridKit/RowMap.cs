
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
