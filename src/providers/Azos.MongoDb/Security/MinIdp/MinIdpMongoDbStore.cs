﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Instrumentation;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Data.Access.MongoDb.Client;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Provides IMinIdpStore implementation based on MongoDb.
  /// The store supports users with multiple logins and grants permission sets via named role assignments
  /// </summary>
  /// <remarks>
  /// In order to bootstrap the security system for the first use,
  /// the store implements a root login data seeding - upon start it tests if the database has a
  /// root system user id with `ROOT_USER_SYSID = 1`, and if there is no such user, creates it with the limited lifespan
  /// so the root login can then be used to create other users/logins/roles. The auto created root login can then be disabled
  /// explicitly or it will expire past its lifetime length (default: 24 hrs)
  /// </remarks>
  public sealed class MinIdpMongoDbStore : DaemonWithInstrumentation<IApplicationComponent>, IMinIdpStoreImplementation, IExternallyCallable
  {
    public const string CONFIG_MONGO_SECTION = "mongo";
    public const long   ROOT_USER_SYSID = 1;
    public const double DEFAUL_ROOT_LOGIN_LIFETIME_HRS = 24;

    public MinIdpMongoDbStore(IApplicationComponent dir) : base(dir)
    {
      m_ExternalCallHandler = new ExternalCallHandler<MinIdpMongoDbStore>(App, this, null,
            typeof(Instrumentation.ListRoles),
            typeof(Instrumentation.GetRole),
            typeof(Instrumentation.SetRole),
            typeof(Instrumentation.DropRole),

            typeof(Instrumentation.ListUsers),
            typeof(Instrumentation.GetUser),
            typeof(Instrumentation.SetUser),
            typeof(Instrumentation.DropUser),

            typeof(Instrumentation.ListLogins),
            typeof(Instrumentation.GetLogin),
            typeof(Instrumentation.SetLogin),
            typeof(Instrumentation.DropLogin)
          );
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Mongo);
      base.Destructor();
    }


    private ExternalCallHandler<MinIdpMongoDbStore> m_ExternalCallHandler;
    private MongoDbService m_Mongo;
    private string m_RemoteAddress;


    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }

    public IExternalCallHandler GetExternalCallHandler() => m_ExternalCallHandler;

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

    /// <summary>
    /// If set, checks whether the named root SYSTEM login exists in the specified realm,
    /// and creates it if it does not with 24 hr expiration.
    /// This is needed to bootstrap the system
    /// </summary>
    [Config] public Atom RootRealm{  get; set; }

    /// <summary>
    /// If set, checks whether the named root SYSTEM login exists in the specified realm,
    /// and creates it if it does not with 24 hr expiration.
    /// This is needed to bootstrap the system
    /// </summary>
    [Config] public string RootLogin { get; set; }

    /// <summary>
    /// If set, checks whether the named root SYSTEM login exists in the specified realm,
    /// and creates it if it does not with 24 hr expiration.
    /// This is needed to bootstrap the system
    /// </summary>
    [Config] public string RootPwdVector { get; set; }


    /// <summary>
    /// When set, limits the life of root login which is auto-created
    /// </summary>
    [Config] public double RootLoginLifetimeHours { get; set; }




    internal TResult Access<TResult>(Func<IMongoDbTransport, TResult> body)
     => m_Mongo.CallSync(m_RemoteAddress,  //the driver is by-design synchronous as of today
                                 nameof(IMinIdpStore),
                                 null,
                                 (tx, c) => body(tx)
                        );

    private Task<BSONDocument> fetch(Atom realm, string collection, Query query)
     => Task.FromResult(m_Mongo.CallSync(m_RemoteAddress,  //the driver is by-design synchronous as of today
                                 nameof(IMinIdpStore),
                                 null,
                                 (tx, c) => tx.Db[BsonDataModel.GetCollectionName(realm, collection)].FindOne(query)
                        ));


    private MinIdpUserData checkDates(MinIdpUserData data)
    {
      if (data==null) return null;
      var now = App.TimeSource.UTCNow;
      if (data.StartUtc > now) return null;
      if (data.EndUtc <= now) return null;

      if (data.LoginStartUtc.HasValue && data.LoginStartUtc.Value > now) return null;
      if (data.LoginEndUtc.HasValue && data.LoginEndUtc.Value <= now) return null;

      return data;
    }

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
    {
      if (id.IsNullOrWhiteSpace()) return null;
      id = id.ToLowerInvariant();
      var login = await fetch(realm, BsonDataModel.COLLECTION_LOGIN, Query.ID_EQ_String(id));
      if (login==null) return null;

      var result = new MinIdpUserData();
      BsonDataModel.ReadLogin(login, result);
      if (result.SysId==0) return null;

      var user = await fetch(realm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(result.SysId));
      if (user==null) return null;
      BsonDataModel.ReadUser(user, result);

      if (result.Role.IsNotNullOrWhiteSpace())
      {
        var role = await fetch(realm, BsonDataModel.COLLECTION_ROLE, Query.ID_EQ_String(result.Role));
        if (role==null) return null;
        BsonDataModel.ReadRole(role, result);
      }
      if (result.Status < UserStatus.System && result.Role.IsNullOrWhiteSpace()) return null;


      return checkDates(result);
    }


    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
    {
      var sysId = sysToken.AsULong();
      if (sysId==0) return null;

      var user = await fetch(realm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(sysId));
      if (user == null) return null;

      var result = new MinIdpUserData();
      BsonDataModel.ReadUser(user, result);

      if (result.Role.IsNotNullOrWhiteSpace())
      {
        var role = await fetch(realm, BsonDataModel.COLLECTION_ROLE, Query.ID_EQ_String(result.Role));
        if (role == null) return null;
        BsonDataModel.ReadRole(role, result);
      }
      if (result.Status < UserStatus.System && result.Role.IsNullOrWhiteSpace()) return null;


      return checkDates(result);
    }


    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
    => await GetByIdAsync(realm, uri);//for MinIdp mongo the URI is the login name for simplicity


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Mongo);
      if (node == null) return;

      var nServer = node[CONFIG_MONGO_SECTION];
      m_Mongo = FactoryUtils.MakeDirectedComponent<MongoDbService>(this,
                                                                 nServer,
                                                                 typeof(MongoDbService),
                                                                 new object[] { nServer });
    }


    protected override void DoStart()
    {
      m_Mongo.NonNull("Not configured Mongo of config section `{0}`".Args(CONFIG_MONGO_SECTION));
      m_RemoteAddress.NonBlank("RemoteAddress string");

      checkRootAccess();
    }

    protected override void DoSignalStop() { }

    protected override void DoWaitForCompleteStop() { }

    private void checkRootAccess()
    {
      if (!Running) return;

      var dpup = Access(tx => { try{ tx.Db.Ping(); return true; }catch{ return false; }} );

      if (!dpup && Running)
      {
        Task.Delay(App.Random.NextScaledRandomInteger(50, 250))
            .ContinueWith(a => checkRootAccess());
        return;
      }


      if (RootRealm.IsZero ||
          RootLogin.IsNullOrWhiteSpace() ||
          RootPwdVector.IsNullOrWhiteSpace()) return;

      var rootUser = fetch(RootRealm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(ROOT_USER_SYSID)).GetAwaiter().GetResult();//sync call;
      if (rootUser == null)
      {
        var rel = Guid.NewGuid();

        WriteLog(Log.MessageType.Notice,
                 nameof(checkRootAccess),
                 "No root user found. Creating root user SID = {0} LOGIN = `{1}`".Args(ROOT_USER_SYSID, RootLogin),
                 related: rel);

        makeRootLogin(rel);
     }
    }

    private void makeRootLogin(Guid rel)
    {
      var now = App.TimeSource.UTCNow;

      var lifeHrs = RootLoginLifetimeHours;
      if (lifeHrs <= 0) lifeHrs = DEFAUL_ROOT_LOGIN_LIFETIME_HRS;

      var setUser = new Instrumentation.SetUser(this){
        Id = ROOT_USER_SYSID,
        Realm = RootRealm,
        Status = UserStatus.System,//root login is SYSTEM by definition
        Name = RootLogin,
        Description = RootLogin,
        Note = "Root login auto created by `{0}`".Args(Platform.Computer.HostName),
        StartUtc = now,
        EndUtc = now.AddHours(lifeHrs)
        //role is optional so it is null
      };

      var setLogin = new Instrumentation.SetLogin(this){
         Realm = RootRealm,
         SysId  = ROOT_USER_SYSID,
         Id = RootLogin,
         Password = RootPwdVector,
         StartUtc = now,
         EndUtc = now.AddHours(lifeHrs)
      };

      var got = setUser.Execute();
      WriteLog(Log.MessageType.Notice,
               nameof(makeRootLogin),
               "{0} result".Args(nameof(setUser)),
               related: rel,
               pars: got.ToJson());

      got = setLogin.Execute();
      WriteLog(Log.MessageType.Notice,
               nameof(makeRootLogin),
               "{0} result".Args(nameof(setLogin)),
               related: rel,
               pars: got.ToJson());
    }

  }
}


