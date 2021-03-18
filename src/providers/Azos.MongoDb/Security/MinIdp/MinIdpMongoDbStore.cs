/*<FILE_LICENSE>
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
    public override string ComponentCommonName => "minidp";


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

    [Config("$sys-token-algo;$sys-token-algorithm")]
    public string SysTokenCryptoAlgorithmName { get; set; }

    [Config("$sys-token-life-hrs")]
    public double SysTokenLifespanHours { get; set; }

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


    [Config("$msg-algo;$msg-algorithm")]
    public string MessageProtectionAlgorithmName { get; set; }

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => MessageProtectionAlgorithmName.IsNullOrWhiteSpace() ? null :
                                                                      App.SecurityManager
                                                                         .Cryptography
                                                                         .MessageProtectionAlgorithms[MessageProtectionAlgorithmName]
                                                                         .NonNull("Algo `{0}`".Args(MessageProtectionAlgorithmName))
                                                                         .IsTrue(a => a.Audience == CryptoMessageAlgorithmAudience.Internal &&
                                                                                      a.Flags.HasFlag(CryptoMessageAlgorithmFlags.Cipher) &&
                                                                                      a.Flags.HasFlag(CryptoMessageAlgorithmFlags.CanUnprotect),
                                                                                      "Algo `{0}` !internal !cipher".Args(MessageProtectionAlgorithmName));



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

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id, AuthenticationRequestContext ctx = null)
    {
      //0. check login, put to lower invariant
      if (id.IsNullOrWhiteSpace()) return null;
      id = id.ToLowerInvariant();

      //1. Fetch by login ID
      var login = await fetch(realm, BsonDataModel.COLLECTION_LOGIN, Query.ID_EQ_String(id));
      if (login == null) return null;

      //2. Make subject principal data
      var result = new MinIdpUserData{ Realm = realm };
      BsonDataModel.ReadLogin(login, result);
      if (result.SysId == 0) return null;

      //3. Try to fetch user
      var user = await fetch(realm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(result.SysId));
      if (user == null) return null;
      BsonDataModel.ReadUser(user, result);

      //4. Check and fetch role rights
      if (result.Role.IsNotNullOrWhiteSpace())
      {
        var role = await fetch(realm, BsonDataModel.COLLECTION_ROLE, Query.ID_EQ_String(result.Role));
        if (role == null) return null;
        BsonDataModel.ReadRole(role, result);
      }
      if (result.Status < UserStatus.System && result.Role.IsNullOrWhiteSpace()) return null;


      //5. Check date spans
      result = checkDates(result);
      if (result == null) return null;

      //6. Issue new SysAuthToken
      var sysSpanHrs = SysTokenLifespanHours > 0 ? SysTokenLifespanHours : 0.35d;//21 minutes by default

      if (ctx is OAuthSubjectAuthenticationRequestContext oauth)
      {
        var ssec = oauth.SysAuthTokenValiditySpanSec ?? 0;
        if (ssec > 0) sysSpanHrs = ssec / 3_600;
      }

      var sysExpiration = App.TimeSource.UTCNow.AddHours(sysSpanHrs);

      var msg = new { id = result.SysId, exp = sysExpiration };
      result.SysTokenData = SysTokenCryptoAlgorithm.ProtectAsString(msg);

      return result;
    }




    private ICryptoMessageAlgorithm SysTokenCryptoAlgorithm => App.SecurityManager
                                                       .Cryptography
                                                       .MessageProtectionAlgorithms[SysTokenCryptoAlgorithmName]
                                                       .NonNull("Algo `{0}`".Args(SysTokenCryptoAlgorithmName))
                                                       .IsTrue(a => a.Audience == CryptoMessageAlgorithmAudience.Internal &&
                                                                    a.Flags.HasFlag(CryptoMessageAlgorithmFlags.Cipher) &&
                                                                    a.Flags.HasFlag(CryptoMessageAlgorithmFlags.CanUnprotect),
                                                                    "Algo `{0}` !internal !cipher".Args(SysTokenCryptoAlgorithmName));

    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx = null)
    {
      //0. Check access token integrity by using message protection API
      // only the server has the key to issue and check the message token.
      // the downstream security managers (e.g. caches, clients, etc.)
      // can not re-generate token as they don't have the key
      if (sysToken.IsNullOrWhiteSpace()) return null;
      var msg = SysTokenCryptoAlgorithm.UnprotectObject(sysToken) as JsonDataMap;
      if (msg == null) return null;//corrupted or forged token
      var sysId = msg["id"].AsULong(0);
      if (sysId == 0) return null;
      var expire = msg["exp"].AsDateTime(default(DateTime),
                                       ConvertErrorHandling.ReturnDefault,
                                       System.Globalization.DateTimeStyles.AssumeUniversal |
                                       System.Globalization.DateTimeStyles.AdjustToUniversal
                                      );
      if (expire <= App.TimeSource.UTCNow) return null;//expired

      //1. Fetch user record by sysId(ULONG)
      var user = await fetch(realm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(sysId));
      if (user == null) return null;

      //2. Make subject principal data
      //Notice: the sys access token is NOT regenerated here, as the act of token consumption does not
      //extend its life. Only a true login via credentials generates a NEW token
      //that is why we make subject data with the `sysToken` as supplied in the call param
      var result = new MinIdpUserData{ Realm = realm, SysTokenData = sysToken };
      BsonDataModel.ReadUser(user, result);

      //3. Check and fetch role rights
      if (result.Role.IsNotNullOrWhiteSpace())
      {
        var role = await fetch(realm, BsonDataModel.COLLECTION_ROLE, Query.ID_EQ_String(result.Role));
        if (role == null) return null;
        BsonDataModel.ReadRole(role, result);
      }
      if (result.Status < UserStatus.System && result.Role.IsNullOrWhiteSpace()) return null;

      //4. Check validity dates
      return checkDates(result);
    }


    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx = null)
    => await GetByIdAsync(realm, uri, ctx);//for MinIdp mongo the URI is the login name for simplicity


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

      var algo = this.SysTokenCryptoAlgorithm;//this throws if not configured properly
      algo = this.MessageProtectionAlgorithm;//this throws if not configured properly

      var rel = Guid.NewGuid();
      checkRootAccess(rel);
    }

    protected override void DoSignalStop() { }

    protected override void DoWaitForCompleteStop() { }

    private void checkRootAccess(Guid rel)
    {
      try
      {
        checkRootAccessUnsafe(rel);
      }
      catch(Exception error)
      {
        WriteLog(Log.MessageType.CriticalAlert,
                nameof(checkRootAccess),
                "Operation leaked: " + error.ToMessageWithType(),
                related: rel,
                error: error);
      }
    }

    private void checkRootAccessUnsafe(Guid rel)
    {
      if (!Running) return;

      var dbup = Access(tx => { try{ tx.Db.Ping(); return true; }catch{ return false; }} );

      if (!dbup && Running)
      {
        Task.Delay(App.Random.NextScaledRandomInteger(50, 250))
            .ContinueWith(a => checkRootAccess(rel));
        return;
      }

      if (RootRealm.IsZero ||
          RootLogin.IsNullOrWhiteSpace() ||
          RootPwdVector.IsNullOrWhiteSpace()) return;

      WriteLog(Log.MessageType.Info,
                 nameof(checkRootAccessUnsafe),
                 "Min Idp db is up. Starting to check the presence of root login SID = {0}`".Args(ROOT_USER_SYSID),
                 related: rel);

      var rootUser = fetch(RootRealm, BsonDataModel.COLLECTION_USER, Query.ID_EQ_UInt64(ROOT_USER_SYSID)).GetAwaiter().GetResult();//sync call;
      if (rootUser == null)
      {
        WriteLog(Log.MessageType.Notice,
                 nameof(checkRootAccessUnsafe),
                 "No root user found. Creating root user SID = {0} LOGIN = `{1}`".Args(ROOT_USER_SYSID, RootLogin),
                 related: rel);

        makeRootLogin(rel);

        WriteLog(Log.MessageType.Notice,
                 nameof(checkRootAccessUnsafe),
                 "Root user created".Args(ROOT_USER_SYSID, RootLogin),
                 related: rel);
      }
      else
      {
        WriteLog(Log.MessageType.Info,
                    nameof(checkRootAccessUnsafe),
                    "Root login record already exists",
                    related: rel);
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


