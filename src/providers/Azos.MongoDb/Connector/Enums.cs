/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


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
