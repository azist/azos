/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// MinIdp Provides security manager implementation that authenticates and authorizes users via IMinIdpStore implementation
  /// </summary>
  public class MinIdpSecurityManager : DaemonWithInstrumentation<IApplicationComponent>, ISecurityManagerImplementation, IMinIdpStoreContainer
  {
    #region CONSTS
    public const string CONFIG_RIGHTS_SECTION = Rights.CONFIG_ROOT_SECTION;
    public const string CONFIG_PASSWORD_MANAGER_SECTION = "password-manager";
    public const string CONFIG_CRYPTOGRAPHY_SECTION = "cryptography";
    public const string CONFIG_STORE_SECTION = "store";
    #endregion

    #region .ctor
    public MinIdpSecurityManager(IApplication app) : base(app) { }
    public MinIdpSecurityManager(IApplicationComponent director) : base(director) { }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Store);
      DisposeAndNull(ref m_PasswordManager);
      DisposeAndNull(ref m_Cryptography);
    }
    #endregion

    #region Fields
    private Atom m_Realm;
    private IMinIdpStoreImplementation m_Store;
    private IPasswordManagerImplementation m_PasswordManager;
    private ICryptoManagerImplementation m_Cryptography;
    private bool m_InstrumentationEnabled;
    #endregion

    #region Properties
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;
    public override string ComponentCommonName => "secman";

    public IMinIdpStore       Store => m_Store;
    public ICryptoManager     Cryptography    => m_Cryptography;
    public IPasswordManager   PasswordManager => m_PasswordManager;

    public bool SupportsTrueAsynchrony => true;

    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_PAY)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set { m_InstrumentationEnabled = value; }
    }


    /// <summary>
    /// Required. Dictates in what realm this security implementation operates
    /// </summary>
    [Config]
    public Atom Realm
    {
      get => m_Realm;
      set
      {
        CheckDaemonInactive();
        m_Realm = value;
      }
    }

    [Config(Default = SecurityLogMask.Custom)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public SecurityLogMask SecurityLogMask { get; set;}

    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public Log.MessageType SecurityLogLevel { get; set;}
    #endregion

    #region Public

    public string GetUserLogArchiveDimensions(IIdentityDescriptor identity)
    {
      if (identity == null) return null;
      return ArchiveConventions.EncodeArchiveDimensions(new { un = identity.IdentityDescriptorName });
    }

    public void LogSecurityMessage(SecurityLogAction action, Log.Message msg, IIdentityDescriptor identity = null)
    {
      if ((SecurityLogMask & action.ToMask()) == 0) return;
      if (msg==null) return;
      if (SecurityLogLevel > msg.Type) return;
      if (msg.ArchiveDimensions.IsNullOrWhiteSpace())
      {
        if (identity==null)
          identity = ExecutionContext.Session.User;

        msg.ArchiveDimensions = GetUserLogArchiveDimensions(identity);
      }

      logSecurityMessage(msg);
    }

    public User Authenticate(Credentials credentials, AuthenticationRequestContext ctx = null)
      => AuthenticateAsync(credentials, ctx).GetAwaiter().GetResult();

    public virtual async Task<User> AuthenticateAsync(Credentials credentials, AuthenticationRequestContext ctx = null)
    {
      if (credentials is BearerCredentials bearer)
      {
        var oauth = App.ModuleRoot.TryGet<Services.IOAuthModule>();
        if (oauth == null) return MakeBadUser(credentials);

        var accessToken = await oauth.TokenRing.GetAsync<Tokens.AccessToken>(bearer.Token).ConfigureAwait(false);
        if (accessToken != null)//if token is valid
        {
          if (SysAuthToken.TryParse(accessToken.SubjectSysAuthToken, out var sysToken))
            return await AuthenticateAsync(sysToken, ctx).ConfigureAwait(false);
        }
      } else if (credentials is IDPasswordCredentials idpass)
      {
        var data = await m_Store.GetByIdAsync(Realm, idpass.ID, ctx).ConfigureAwait(false);
        if (data != null)
        {
          var user = TryAuthenticateUser(data, idpass);
          if (user != null) return user;
        }
      } else if (credentials is EntityUriCredentials enturi)
      {
        var data = await m_Store.GetByUriAsync(Realm, enturi.Uri, ctx).ConfigureAwait(false);
        if (data != null)
        {
          var user = TryAuthenticateUser(data, enturi);
          if (user != null) return user;
        }
      }

      return MakeBadUser(credentials);
    }



    public User Authenticate(SysAuthToken token, AuthenticationRequestContext ctx = null)
      => AuthenticateAsync(token, ctx).GetAwaiter().GetResult();

    public virtual async Task<User> AuthenticateAsync(SysAuthToken token, AuthenticationRequestContext ctx = null)
    {
      if (Realm.Value.EqualsOrdSenseCase(token.Realm))
      {
        var data = await m_Store.GetBySysAsync(Realm, token.Data, ctx).ConfigureAwait(false);
        if (data!=null)
        {
          var user = TryAuthenticateUser(data);
          if (user != null) return user;
        }
      }

      return MakeBadUser(null);
    }


    public void Authenticate(User user, AuthenticationRequestContext ctx = null)
      => AuthenticateAsync(user, ctx).GetAwaiter().GetResult();

    public virtual async Task AuthenticateAsync(User user, AuthenticationRequestContext ctx = null)
    {
      if (user == null) return;
      var token = user.AuthToken;
      var reuser = await AuthenticateAsync(token, ctx).ConfigureAwait(false);

      user.___update_status(reuser.Status, reuser.Name, reuser.Description, reuser.Rights, App.TimeSource.UTCNow);
    }

    public virtual Task<AccessLevel> AuthorizeAsync(User user, Permission permission) => Task.FromResult(Authorize(user, permission));

    public virtual AccessLevel Authorize(User user, Permission permission)
    {
      if (user == null || permission == null)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Authorize(user==null|permission==null)");

      var node = user.Rights.Root.NavigateSection(permission.FullPath);
      return new AccessLevel(user, permission, node);
    }

    public Task<IEntityInfo> LookupEntityAsync(string uri)
    {
      return null;//for now
    }

    #endregion

    #region Protected

    protected virtual User MakeBadUser(Credentials credentials)
    {
      return new User(credentials ?? BlankCredentials.Instance,
                      new SysAuthToken(),
                      UserStatus.Invalid,
                      StringConsts.SECURITY_NON_AUTHENTICATED,
                      StringConsts.SECURITY_NON_AUTHENTICATED,
                      Rights.None, App.TimeSource.UTCNow);
    }

    protected virtual User MakeOkUser(Credentials credentials, MinIdpUserData data)
    {
      var rights = Rights.None;
      if (credentials == null) credentials = BlankCredentials.Instance;

      if (data.Rights.IsNotNullOrWhiteSpace())
      {
        var cfg = data.Rights.AsLaconicConfig(handling: ConvertErrorHandling.ReturnDefault);
        if (cfg == null)
          WriteLog(MessageType.Warning, nameof(MakeOkUser), "Rights could not be read for `{0}`@`{1}`".Args(credentials, Realm));
        else
          rights = new Rights(cfg.Configuration);
      }

      return new User(credentials,
                      data.SysToken,
                      data.Status,
                      data.Name,
                      data.Description,
                      rights,
                      App.TimeSource.UTCNow);
    }


    protected virtual bool CheckDates(MinIdpUserData data)
    {
      data.NonNull(nameof(data));
      var now = App.TimeSource.UTCNow;
      if (data.StartUtc > now) return false;
      if (data.EndUtc <= now) return false;

      if (data.LoginStartUtc.HasValue && data.LoginStartUtc.Value > now) return false;
      if (data.LoginEndUtc.HasValue && data.LoginEndUtc.Value <= now) return false;

      return true;
    }

    /// <summary>
    /// Plain user id/password
    /// </summary>
    protected virtual User TryAuthenticateUser(MinIdpUserData data, IDPasswordCredentials cred)
    {
      if (data.Realm != Realm) return null;
      if (!CheckDates(data)) return null;

      using (var password = cred.SecurePassword)
      {
        cred.Forget();
        var pass = m_PasswordManager.Verify(password, HashedPassword.FromString(data.LoginPassword), out var needRehash);
        if (!pass) return null;
      }

      return MakeOkUser(cred, data);
    }

    /// <summary>
    /// URI reference
    /// </summary>
    protected virtual User TryAuthenticateUser(MinIdpUserData data, EntityUriCredentials cred)
    {
      if (data.Realm != Realm) return null;
      if (!CheckDates(data)) return null;

      cred.Forget();
      return MakeOkUser(cred, data);
    }

    /// <summary>
    /// For SysAuthToken
    /// </summary>
    protected virtual User TryAuthenticateUser(MinIdpUserData data)
    {
      if (data.Realm != Realm) return null;
      if (!CheckDates(data)) return null;

      return MakeOkUser(null, data);
    }



    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      DisposeAndNull(ref m_Cryptography);
      m_Cryptography = FactoryUtils.MakeAndConfigureDirectedComponent<ICryptoManagerImplementation>(
                                              this,
                                              node[CONFIG_CRYPTOGRAPHY_SECTION],
                                              typeof(DefaultCryptoManager));

      DisposeAndNull(ref m_PasswordManager);
      m_PasswordManager = FactoryUtils.MakeAndConfigureDirectedComponent<IPasswordManagerImplementation>(
                                              this,
                                              node[CONFIG_PASSWORD_MANAGER_SECTION],
                                              typeof(DefaultPasswordManager));

      DisposeAndNull(ref m_Store);
      m_Store = FactoryUtils.MakeAndConfigureDirectedComponent<IMinIdpStoreImplementation>(
                                              this,
                                              node[CONFIG_STORE_SECTION]);
    }

    protected override void DoStart()
    {
      if (m_Realm.IsZero || !m_Realm.IsValid)
        throw new CallGuardException(nameof(MinIdpSecurityManager), nameof(Realm), "Must be configured");

      m_PasswordManager.NonNull($"{nameof(PasswordManager)} config").Start();
      m_Cryptography.NonNull($"{nameof(Cryptography)} config").Start();
      m_Store.NonNull($"{nameof(Store)} config").Start();
    }

    protected override void DoSignalStop()
    {
      m_PasswordManager.SignalStop();
      m_Cryptography.SignalStop();
      m_Store.SignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_Store.WaitForCompleteStop();
      m_Cryptography.WaitForCompleteStop();
      m_PasswordManager.WaitForCompleteStop();
    }
    #endregion

    #region Private

    private void logSecurityMessage(Log.Message msg)
    {
      msg.Channel = CoreConsts.LOG_CHANNEL_SECURITY;
      msg.From = "{0}.{1}".Args(GetType().Name, msg.From);
      App.Log.Write(msg);
    }
    #endregion
  }
}
