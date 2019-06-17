/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Serialization.BSON;

namespace Azos.Data.Access.MongoDb
{
  internal static class MongoConsts
  {
    public const string ADMIN_DB = "admin";
    public const string MONGO_TOPIC = "mongo";

    /// <summary>
    /// Default parameterless SHUTDOWN command (should be sent into ADMIN database)
    /// See: https://docs.mongodb.com/manual/reference/command/shutdown/#dbcmd.shutdown
    /// </summary>
    public static readonly BSONDocument CMD_SHUTDOWN = new BSONDocument().Set(new BSONInt32Element("shutdown", 1));
  }
}

