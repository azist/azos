/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Client;
using Azos.Conf;
using Azos.Data.Access.MongoDb.Connector;

namespace Azos.Data.Access.MongoDb.Client
{
  /// <summary>
  /// Defines endpoints of MongoDb
  /// </summary>
  public class MongoDbEndpoint : EndpointBase<MongoDbService>, IMongoDbEndpoint
  {
    public const int DEFAULT_TIMEOUT_MS = 10_000;

    public MongoDbEndpoint(MongoDbService service, IConfigSectionNode conf) : base(service, conf)
    {
    }

    protected override void Destructor()
    {
      base.Destructor();
    }

    private object m_Lock = new object();
    private Database m_Db;

    /// <summary>
    /// Physical connect string of the MongoDB instance, you can use `appliance://` redirect
    /// </summary>
    [Config]
    public string ConnectString { get; private set; }

    /// <summary>
    /// Returns MongoDb.Connector database object which represents this endpoint
    /// </summary>
    public Database Db
    {
      get
      {
        EnsureObjectNotDisposed();
        lock (m_Lock)
        {
          if (m_Db == null)
          {
            m_Db = MakeDatabase();
          }
          return m_Db;
        }
      }
    }

    /// <summary>
    /// Override factory to make and configure/bind MongoDb.Connector.Database instance.
    /// </summary>
    protected virtual Database MakeDatabase()
      => App.GetMongoDatabaseFromConnectString(ConnectString.NonBlank("`{0}` is not configured".Args(nameof(ConnectString))));

    public override CallErrorClass NotifyCallError(ITransport transport, Exception cause)
    {
    //delegate this into the Extension, so we can classify things like 500 -> logic error via pattern match on exception etc...

      if (cause==null) return CallErrorClass.MakingCall;

      var isCallProblem = cause is System.Net.Sockets.SocketException ||
                          cause is TaskCanceledException; //timeout

      if (isCallProblem)
      {
        //mutate circuit breaker state machine
       // this.m_CircuitBreakerTimeStampUtc = now;//trip
      }

      return isCallProblem ? CallErrorClass.MakingCall : CallErrorClass.ServiceLogic;
    }

    public override void NotifyCallSuccess(ITransport transport)
    {
      //throw new NotImplementedException();
      //reset circuit breaker etc...
    }

  }

}
