/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Client;

namespace Azos.Data.Access.MongoDb.Client
{
  public class MongoDbTransport : DisposableObject, ITransportImplementation, IMongoDbTransport
  {
    protected internal MongoDbTransport(EndpointAssignment assignment)
    {
      m_Assignment = assignment;
    }

    protected readonly EndpointAssignment m_Assignment;

    public EndpointAssignment Assignment => m_Assignment;
    public MongoDbEndpoint Endpoint => Assignment.Endpoint as MongoDbEndpoint;
    public Connector.Database Db => Endpoint.Db;
  }
}
