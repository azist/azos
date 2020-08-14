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

namespace Azos.Security
{
  /// <summary>
  /// Outlines the contract for stores which service MinIdp (Minimum identity provider)
  /// </summary>
  public interface IMinIdpStore : IApplicationComponent
  {
    /// <summary>
    /// Cache age limit in seconds, set to 0 to disable caching
    /// </summary>
    int MaxCacheAgeSec{ get;}

    Task<MinIdpUserData> GetByIdAsync(string id);
    Task<MinIdpUserData> GetByUriAsync(string uri);
    Task<MinIdpUserData> GetBySysAsync(SysAuthToken sysId);
  }


  /// <summary>
  /// Sets contract for DTO - data stored in MinIdp system
  /// </summary>
  public sealed class MinIdpUserData
  {
    public long SysId { get; set; }
    public Atom Realm { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreateUtc { get; set;}
    public DateTime ModifyUtc { get; set; }
    public DateTime EndUtc    { get; set; }
    public string Id          { get; set; }
    public string Password    { get; set; }
    public string Name        { get; set; }
    public string Description { get; set; }
    public string Role        { get; set; }
    public Rights Rights      { get; set; }
    public string Note { get; set; }
  }


  /// <summary>
  /// MinIdp Provides security manager implementation that authenticates and authorizes users via IMinIdpStore
  /// </summary>
  public class MinIdpSecurityManager : DaemonWithInstrumentation<IApplicationComponent>, ISecurityManagerImplementation
  {
    #region CONSTS
    public const string CONFIG_RIGHTS_SECTION = Rights.CONFIG_ROOT_SECTION;
    public const string CONFIG_PASSWORD_MANAGER_SECTION = "password-manager";
    public const string CONFIG_CRYPTOGRAPHY_SECTION = "cryptography";
    #endregion

    #region .ctor
    public MinIdpSecurityManager(IApplication app) : base(app) { }
    public MinIdpSecurityManager(IApplicationComponent director) : base(director) { }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_PasswordManager);
      DisposeAndNull(ref m_Cryptography);
    }
    #endregion

    #region Fields
    private IMinIdpStore m_Store;
    private IPasswordManagerImplementation m_PasswordManager;
    private ICryptoManagerImplementation m_Cryptography;
    private bool m_InstrumentationEnabled;
    #endregion

    #region Properties
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;
    public override string ComponentCommonName => "secman";

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

    public User Authenticate(Credentials credentials) => AuthenticateAsync(credentials).GetAwaiter().GetResult();
    public virtual async Task<User> AuthenticateAsync(Credentials credentials)
    {
      if (credentials is BearerCredentials bearer)
      {
        var oauth = App.ModuleRoot.Get<Services.IOAuthModule>();
        var accessToken = oauth.TokenRing.GetAsync<Tokens.AccessToken>(bearer.Token).GetAwaiter().GetResult();//since this manager is sync-only
        if (accessToken!=null)//if token is valid
        {
          if (SysAuthToken.TryParse(accessToken.SubjectSysAuthToken, out var sysToken))
            return await AuthenticateAsync(sysToken);
        }
      } else if (credentials is IDPasswordCredentials idpass)
      {
        var data = await m_Store.GetByIdAsync(idpass.ID);
        if (data!=null)
        {
          var user = TryAuthenticateUser(data, idpass);
          if (user!=null) return user;
        }
      } else if (credentials is EntityUriCredentials enturi)
      {
        var data = await m_Store.GetByUriAsync(enturi.Uri);
        if (data!=null)
        {
          var user = TryAuthenticateUser(data, enturi);
          if (user != null) return user;
        }
      }

      return MakeBadUser(credentials);
    }



    public User Authenticate(SysAuthToken token) => AuthenticateAsync(token).GetAwaiter().GetResult();

    public virtual async Task<User> AuthenticateAsync(SysAuthToken token)
    {
      var data = await m_Store.GetBySysAsync(token);
      if (data!=null)
      {
        var user = TryAuthenticateUser(data);
        if (user != null) return user;
      }

      return MakeBadUser(null);
    }


    public void Authenticate(User user) => AuthenticateAsync(user).GetAwaiter().GetResult();

    public async Task AuthenticateAsync(User user)
    {
      if (user == null) return;
      var token = user.AuthToken;
      var reuser = await AuthenticateAsync(token);

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

    /// <summary>
    /// Return null if cant
    /// </summary>
    protected virtual User TryAuthenticateUser(MinIdpUserData data, IDPasswordCredentials idpass)
    {
       return null;
    }

    /// <summary>
    /// Return null if cant
    /// </summary>
    protected virtual User TryAuthenticateUser(MinIdpUserData data, EntityUriCredentials enturi)
    {
       return null;
    }

    /// <summary>
    /// Return null if cant
    /// </summary>
    protected virtual User TryAuthenticateUser(MinIdpUserData data)
    {
      return null;
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

    //////private IConfigSectionNode findUserNode(IConfigSectionNode securityRootNode, EntityUriCredentials cred)
    //////{
    //////  var users = securityRootNode[CONFIG_USERS_SECTION];

    //////  return users.Children
    //////              .FirstOrDefault(cn => cn.IsSameName(CONFIG_USER_SECTION) &&
    //////                                    cn.ValOf(CONFIG_ID_ATTR).EqualsOrdSenseCase(cred.Uri)) ?? users.Configuration.EmptySection;
    //////}

    //////private IConfigSectionNode findUserNode(IConfigSectionNode securityRootNode, IDPasswordCredentials cred)
    //////{
    //////  var users = securityRootNode[CONFIG_USERS_SECTION];

    //////  using (var password = cred.SecurePassword)
    //////  {
    //////    bool needRehash = false;
    //////    return users.Children.FirstOrDefault(cn => cn.IsSameName(CONFIG_USER_SECTION)
    //////                                            && cn.ValOf(CONFIG_ID_ATTR).EqualsOrdSenseCase(cred.ID)
    //////                                            && m_PasswordManager.Verify(password, HashedPassword.FromString(cn.ValOf(CONFIG_PASSWORD_ATTR)), out needRehash)
    //////                                        ) ?? users.Configuration.EmptySection;

    //////  }
    //////}

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
