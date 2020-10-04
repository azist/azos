/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Instrumentation;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Data.Access.MongoDb.Client;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Provides IMinIdpStore implementation based on MongoDb
  /// </summary>
  public sealed class MinIdpMongoDbStore : DaemonWithInstrumentation<IApplicationComponent>, IMinIdpStoreImplementation
  {
    public MinIdpMongoDbStore(IApplicationComponent dir) : base(dir)
    {
    }

    protected override void Destructor()
    {
      base.Destructor();
    }


    private MongoDbService m_Mongo;
    private string m_RemoteAddress;

    void example()
    {
      m_Mongo.CallSync("REMOTE ADDREESS", "*", 0, (tx, c) => tx.Db["ronj"].Find(Query.ID_EQ_Int32(1234)));
    }


    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }

    [Config("$remoteaddress;$remote;$address;$remote-address")]
    public string RemoteAddress
    {
      get => m_RemoteAddress;
      set
      {
        CheckDaemonInactive();
        m_RemoteAddress = value;
      }
    }


    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
     => null;


    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
     => null;


    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
     => null;



    protected override void DoStart()
    {
      m_RemoteAddress.NonBlank("RemoteAddress string");
    }

    protected override void DoSignalStop() { }

    protected override void DoWaitForCompleteStop() { }



  }
}


