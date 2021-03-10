/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Client;

namespace Azos.Data.Access.MongoDb.Client
{
  /// <summary>
  /// Marks services that provide MongoDb connectivity  - the ones based on MongoDb connector
  /// </summary>
  public interface IMongoDbService : IService
  {
  }

  /// <summary>
  /// Marks endpoints that provide MongoDb connectivity
  /// </summary>
  public interface IMongoDbEndpoint : IEndpoint
  {
    /// <summary>
    /// Mongo DB connect string, you can use `appliance://` as a shortcut to a IMongoDbAppliance module
    /// </summary>
    string ConnectString { get; }
  }

  /// <summary>
  /// Marks transports that provide MongoDb connector functionality
  /// </summary>
  public interface IMongoDbTransport : ITransport
  {
    /// <summary>
    /// Returns MongoDb.Connector database for making calls
    /// </summary>
    Connector.Database Db { get; }
  }
}
