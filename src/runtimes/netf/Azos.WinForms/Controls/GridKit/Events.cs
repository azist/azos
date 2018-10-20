namespace Azos.WinForms.Controls.GridKit
{
  public class NotifyDataSourceChangedEventArgs
  {
      /// <summary>
      /// Returns row that changed in data source or null
      /// </summary>
      public readonly object Row;

      public NotifyDataSourceChangedEventArgs(object row)
      {
        Row = row;
      }
  }

  public delegate void NotifyDataSourceChangedEventHandler(Grid sender, NotifyDataSourceChangedEventArgs args);

  public delegate void ColumnAttributesChangedEventHandler(Column sender);


  /// <summary>
  /// Event handler that gets called after usee select a cell
  /// </summary>
  public delegate void CellSelectionEventHandler(CellElement oldCell, CellElement newCell);

}
