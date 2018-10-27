

namespace Azos.Data.Access.MongoDb.Connector
{
  /// <summary>
  /// Defines data safety modes http://docs.mongodb.org/manual/core/write-concern/
  /// </summary>
  public enum WriteConcern
  {
    Unacknowledged = -1,
    Acknowledged   = 0,
    Journaled = +1
  }
}
