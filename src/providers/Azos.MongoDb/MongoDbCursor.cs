/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

namespace Azos.Data.Access.MongoDb
{
  /// <summary>
  /// Implements Cursor abstraction using Mongo db technology
  /// </summary>
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
