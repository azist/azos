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
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Security
{
  /// <summary>
  /// Provides security manager implementation that authenticates and authorizes users from configuration
  /// </summary>
  public class ConfigSecurityManager : DaemonWithInstrumentation<IApplicationComponent>, ISecurityManagerImplementation
  {
    #region CONSTS
    public const string CONFIG_USERS_SECTION = "users";
    public const string CONFIG_USER_SECTION = "user";

    public const string CONFIG_RIGHTS_SECTION = Rights.CONFIG_ROOT_SECTION;
    public const string CONFIG_PERMISSION_SECTION = "permission";
    public const string CONFIG_PASSWORD_MANAGER_SECTION = "password-manager";
    public const string CONFIG_CRYPTOGRAPHY_SECTION = "cryptography";

    public const string CONFIG_DESCRIPTION_ATTR = "description";
    public const string CONFIG_STATUS_ATTR = "status";
    public const string CONFIG_ID_ATTR = "id";
    public const string CONFIG_PASSWORD_ATTR = "password";
    #endregion

    #region .ctor
    /// <summary>
    /// Constructs security manager that authenticates users listed in application configuration
    /// </summary>
    public ConfigSecurityManager(IApplication app) : base(app) { }

    /// <summary>
    /// Constructs security manager that authenticates users listed in the supplied configuration section
    /// </summary>
    public ConfigSecurityManager(IApplicationComponent director) : base(director) { }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_PasswordManager);
      DisposeAndNull(ref m_Cryptography);
    }
    #endregion

    #region Fields
    private IConfigSectionNode m_Config;
    private IPasswordManagerImplementation m_PasswordManager;
    private ICryptoManagerImplementation m_Cryptography;
    private bool m_InstrumentationEnabled;
    #endregion

    #region Properties
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    public bool SupportsTrueAsynchrony => false;

    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_PAY)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set { m_InstrumentationEnabled = value; }
    }

    public override string ComponentCommonName { get { return "secman"; } }

    /// <summary>
    /// Returns config node that this instance is configured from.
    /// If null is returned then manager performs authentication from application configuration
    /// </summary>
    public IConfigSectionNode Config          => m_Config;

    public ICryptoManager     Cryptography    => m_Cryptography;
    public IPasswordManager   PasswordManager => m_PasswordManager;


    [Config(Default = SecurityLogMask.Custom)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public SecurityLogMask SecurityLogMask { get; set;}

    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public Log.MessageType SecurityLogLevel { get; set;}
    #endregion

    #region Public

    public string GetUserLogArchiveDimensions(IIdentityDescriptor identity)
    {
      if (identity==null) return null;
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

    protected virtual User MakeBadUser(Credentials credentials)
    {
      return new User(credentials,
                           new SysAuthToken(),
                           UserStatus.Invalid,
                           StringConsts.SECURITY_NON_AUTHENTICATED,
                           StringConsts.SECURITY_NON_AUTHENTICATED,
                           Rights.None, App.TimeSource.UTCNow);
    }

    protected virtual User MakeUser(Credentials credentials, SysAuthToken sysToken, UserStatus status, string name, string descr, Rights rights)
    {
      return new User(credentials,
                          sysToken,
                          status,
                          name,
                          descr,
                          rights, App.TimeSource.UTCNow);
    }

    public Task<User> AuthenticateAsync(Credentials credentials, AuthenticationRequestContext ctx = null) => Task.FromResult(Authenticate(credentials, ctx));

    public User Authenticate(Credentials credentials, AuthenticationRequestContext ctx = null)
    {
      if (credentials is BearerCredentials bearer)
      {
        var oauth = App.ModuleRoot.TryGet<Services.IOAuthModule>();
        if (oauth == null) return MakeBadUser(credentials);

        var accessToken = oauth.TokenRing.GetAsync<Tokens.AccessToken>(bearer.Token).GetAwaiter().GetResult();//since this manager is sync-only
        if (accessToken!=null)//if token is valid
        {
          if (SysAuthToken.TryParse(accessToken.SubjectSysAuthToken, out var sysToken))
            return Authenticate(sysToken, ctx);
        }
      }

      var sect = m_Config ?? App.ConfigRoot[CommonApplicationLogic.CONFIG_SECURITY_SECTION];

      if (sect.Exists)
      {
        IConfigSectionNode usern = sect.Configuration.EmptySection;

        if (credentials is IDPasswordCredentials idpass) usern = findUserNode(sect, idpass);
        if (credentials is EntityUriCredentials enturi) usern = findUserNode(sect, enturi);

        if (usern.Exists)
        {
          var name = usern.AttrByName(CONFIG_NAME_ATTR).ValueAsString(string.Empty);
          var descr = usern.AttrByName(CONFIG_DESCRIPTION_ATTR).ValueAsString(string.Empty);
          var status = usern.AttrByName(CONFIG_STATUS_ATTR).ValueAsEnum(UserStatus.Invalid);

          var rights = Rights.None;

          var rightsn = usern[CONFIG_RIGHTS_SECTION];

          if (rightsn.Exists)
          {
            var data = new MemoryConfiguration();
            data.CreateFromNode(rightsn);
            rights = new Rights(data);
          }

          return MakeUser(credentials,
                          credToAuthToken(credentials),
                          status,
                          name,
                          descr,
                          rights);
        }
      }

      return MakeBadUser(credentials);
    }

    public Task<User> AuthenticateAsync(SysAuthToken token, AuthenticationRequestContext ctx = null) => Task.FromResult(Authenticate(token, ctx));

    public User Authenticate(SysAuthToken token, AuthenticationRequestContext ctx = null)
    {
      var credentials = authTokenToCred(token);
      return Authenticate(credentials, ctx);
    }

    public Task AuthenticateAsync(User user, AuthenticationRequestContext ctx = null) { Authenticate(user, ctx); return Task.CompletedTask;}

    public void Authenticate(User user, AuthenticationRequestContext ctx = null)
    {
      if (user == null) return;
      var token = user.AuthToken;
      var reuser = Authenticate(token, ctx);

      user.___update_status(reuser.Status, reuser.Name, reuser.Description, reuser.Rights, App.TimeSource.UTCNow);
    }

    public Task<AccessLevel> AuthorizeAsync(User user, Permission permission) => Task.FromResult(Authorize(user, permission));

    public AccessLevel Authorize(User user, Permission permission)
    {
      if (user == null || permission == null)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".Authorize(user==null|permission==null)");

      var node = user.Rights.Root.NavigateSection(permission.FullPath);
      return new AccessLevel(user, permission, node);
    }

    public Task<IEntityInfo> LookupEntityAsync(string uri)
    {
      return Task.FromResult<IEntityInfo>(null);//for now
    }

    #endregion

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      m_Config = node;

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
    }

    protected override void DoStart()
    {
      if (m_PasswordManager == null) throw new SecurityException("{0}.PasswordManager == null/not configured");
      if (m_Cryptography == null) throw new SecurityException("{0}.Cryptography == null/not configured");

      m_PasswordManager.Start();
      m_Cryptography.Start();
    }

    protected override void DoSignalStop()
    {
      m_PasswordManager.SignalStop();
      m_Cryptography.SignalStop();
    }

    protected override void DoWaitForCompleteStop()
    {
      m_PasswordManager.WaitForCompleteStop();
      m_Cryptography.WaitForCompleteStop();
    }
    #endregion

    #region Private

    private IConfigSectionNode findUserNode(IConfigSectionNode securityRootNode, EntityUriCredentials cred)
    {
      var users = securityRootNode[CONFIG_USERS_SECTION];

      return users.Children
                  .FirstOrDefault(cn => cn.IsSameName(CONFIG_USER_SECTION) &&
                                        cn.ValOf(CONFIG_ID_ATTR).EqualsOrdSenseCase(cred.Uri)) ?? users.Configuration.EmptySection;
    }

    private IConfigSectionNode findUserNode(IConfigSectionNode securityRootNode, IDPasswordCredentials cred)
    {
      var users = securityRootNode[CONFIG_USERS_SECTION];

      using (var password = cred.SecurePassword)
      {
        bool needRehash = false;
        return users.Children.FirstOrDefault(cn => cn.IsSameName(CONFIG_USER_SECTION)
                                                && cn.ValOf(CONFIG_ID_ATTR).EqualsOrdSenseCase(cred.ID)
                                                && m_PasswordManager.Verify(password, HashedPassword.FromString(cn.ValOf(CONFIG_PASSWORD_ATTR)), out needRehash)
                                            ) ?? users.Configuration.EmptySection;

      }
    }

    private SysAuthToken credToAuthToken(Credentials credentials)
    {
      if (credentials is IDPasswordCredentials idpass)
        return new SysAuthToken(this.GetType().FullName, "idp\n{0}\n{1}".Args(idpass.ID, idpass.Password));

      if (credentials is EntityUriCredentials enturi)
        return new SysAuthToken(this.GetType().FullName, "uri\n{0}".Args(enturi.Uri));

      return new SysAuthToken();//invalid token
    }

    private Credentials authTokenToCred(SysAuthToken token)
    {
      if (token.Data.IsNullOrWhiteSpace())
        return BlankCredentials.Instance;

      var seg = token.Data.Split('\n');

      if (seg.Length < 2)
        return BlankCredentials.Instance;


      if (seg[0].EqualsOrdSenseCase("idp"))
      {
        if (seg.Length < 3)  return BlankCredentials.Instance;
        return new IDPasswordCredentials(seg[1], seg[2]);
      }

      if (seg[0].EqualsOrdSenseCase("uri")) return new EntityUriCredentials(seg[1]);

      return BlankCredentials.Instance;
    }


    private void logSecurityMessage(Log.Message msg)
    {
      msg.Channel = CoreConsts.LOG_CHANNEL_SECURITY;
      msg.From = "{0}.{1}".Args(GetType().Name, msg.From);
      App.Log.Write(msg);
    }
    #endregion
  }
}
