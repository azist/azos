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
using Azos.Serialization.BSON;

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

    private Task<BSONDocument> fetch(Atom realm, string collection, Query query)
     => Task.FromResult(m_Mongo.CallSync(m_RemoteAddress,  //the driver is synchronous as of today
                                 "MinIdp",
                                 null,
                                 (tx, c) => tx.Db[BsonDataModel.GetCollectionName(realm, collection)].FindOne(query)
                        ));


    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
    {
      if (id.IsNullOrWhiteSpace()) return null;
      var login = await fetch(realm, BsonDataModel.COLLECTION_LOGIN, Query.ID_EQ_String(id));
      if (login==null) return null;

      var result = new MinIdpUserData();
      BsonDataModel.ReadLogin(login, result);
      if (result.SysId==0) return null;

      var user = await fetch(realm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(result.SysId));
      if (user==null) return null;
      BsonDataModel.ReadUser(user, result);

      var role = await fetch(realm, BsonDataModel.COLLECTION_ROLE, Query.ID_EQ_String(result.Role));
      if (role == null) return null;
      BsonDataModel.ReadRole(role, result);

      return result;
    }


    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
    {
      var sysId = sysToken.AsULong();
      if (sysId==0) return null;

      var user = await fetch(realm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(sysId));
      if (user == null) return null;

      var result = new MinIdpUserData();
      BsonDataModel.ReadUser(user, result);

      var role = await fetch(realm, BsonDataModel.COLLECTION_ROLE, Query.ID_EQ_String(result.Role));
      if (role == null) return null;
      BsonDataModel.ReadRole(role, result);

      return result;
    }


    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
    {
      if (uri.IsNullOrWhiteSpace()) return null;

      var qry = new Query();
      qry.Set(new BSONStringElement(BsonDataModel.FLD_SCREENNAME, uri));

      var user = await fetch(realm, BsonDataModel.COLLECTION_USER, qry);
      if (user == null) return null;

      var result = new MinIdpUserData();
      BsonDataModel.ReadUser(user, result);

      var role = await fetch(realm, BsonDataModel.COLLECTION_ROLE, Query.ID_EQ_String(result.Role));
      if (role == null) return null;
      BsonDataModel.ReadRole(role, result);

      return result;
    }



    protected override void DoStart()
    {
      m_RemoteAddress.NonBlank("RemoteAddress string");
    }

    protected override void DoSignalStop() { }

    protected override void DoWaitForCompleteStop() { }



  }
}


