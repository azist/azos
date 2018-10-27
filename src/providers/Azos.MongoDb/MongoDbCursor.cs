
using System.Collections.Generic;


namespace Azos.Data.Access.MongoDb
{
  public sealed class MongoDbCursor : Cursor
  {
    internal MongoDbCursor(Connector.Cursor cursor, IEnumerable<Doc> source) : base(source)
    {
      m_Cursor = cursor;
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposableObject.DisposeAndNull(ref m_Cursor);
    }

    private Connector.Cursor m_Cursor;
  }
}
