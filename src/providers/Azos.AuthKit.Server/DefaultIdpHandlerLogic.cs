/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Collections;
using Azos.Conf;
using Azos.Conf.Forest;
using Azos.Data;
using Azos.Security;
using Azos.Security.Config;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Server
{
  public sealed class DefaultIdpHandlerLogic : ModuleBase, IIdpHandlerLogic
  {
    public DefaultIdpHandlerLogic(IApplication application) : base(application) { }
    public DefaultIdpHandlerLogic(IModule parent) : base(parent) { }

    Registry<LoginProvider> m_Providers;
    [Inject] private IForestLogic m_Forest;

    private TreeNodeInfo m_TreeAuthKitSysNode;

    [Config] Atom m_DefaultLoginProvider;

    [Config] Atom m_IdpConfigForestId;

    public bool IsServerImplementation => true;
    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    public IRegistry<LoginProvider> Providers => m_Providers;

    public Atom DefaultLoginProvider => m_DefaultLoginProvider;

    public Atom IdpConfigForestId => m_IdpConfigForestId;

    public IConfigSectionNode SysConfigNode => m_TreeAuthKitSysNode.EffectiveConfig.Node;

    [Config("$sys-token-algo;$sys-token-algorithm")]
    public string SysTokenCryptoAlgorithmName { get; set; }

    private ICryptoMessageAlgorithm SysTokenCryptoAlgorithm => App.SecurityManager
                                                       .Cryptography
                                                       .MessageProtectionAlgorithms[SysTokenCryptoAlgorithmName]
                                                       .NonNull("Algo `{0}`".Args(SysTokenCryptoAlgorithmName))
                                                       .IsTrue(a => a.Audience == CryptoMessageAlgorithmAudience.Internal &&
                                                                    a.Flags.HasFlag(CryptoMessageAlgorithmFlags.Cipher) &&
                                                                    a.Flags.HasFlag(CryptoMessageAlgorithmFlags.CanUnprotect),
                                                                    "Algo `{0}` !internal !cipher".Args(SysTokenCryptoAlgorithmName));

    [Config("$sys-token-life-hrs")]
    public double SysTokenLifespanHours { get; set; }


    #region Protected/Lifecycle
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node==null || !node.Exists) return;

      cleanup();
      m_Providers = new Registry<LoginProvider>();

      foreach(var np in node.ChildrenNamed(LoginProvider.CONFIG_PROVIDER_SECTION))
      {
        var provider = FactoryUtils.MakeDirectedComponent<LoginProvider>(this, np, extraArgs: new object[]{ np });
        if (!m_Providers.Register(provider))
        {
          throw new AuthKitException("Duplicate provider `{0}` name in config".Args(provider.Name));
        }
      }
    }

    private void cleanup()
    {
      if(m_Providers == null) return;
      m_Providers.ForEach( p => this.DontLeak( () => p.Dispose()) );
      m_Providers = null;
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Providers.NonNull("configured providers")[DefaultLoginProvider.Value]
                 .NonNull("default login provider `{0}`".Args(DefaultLoginProvider));
      m_IdpConfigForestId.HasRequiredValue("configured `{0}`".Args(nameof(IdpConfigForestId)));

      var idSys = GetIdpConfigTreeNodePath(Constraints.TREE_AUTHKIT, Constraints.TREE_AUTHKIT_SYS_PATH);

      using (var scope = new SecurityFlowScope(TreePermission.SYSTEM_USE_FLAG))
      {
        m_TreeAuthKitSysNode = m_Forest.GetNodeInfoAsync(idSys)
                                      .GetAwaiter()
                                      .GetResult()
                                      .NonNull("configured `{0}`".Args(idSys));
      }

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      cleanup();
      return base.DoApplicationBeforeCleanup();
    }
    #endregion

    public EntityId GetIdpConfigTreeNodePath(Atom realm, string path)
      => new EntityId(m_IdpConfigForestId,
                      realm.HasRequiredValue(nameof(realm)),
                      Azos.Conf.Forest.Constraints.SCH_PATH,
                      path.NonBlank(nameof(path)));

    /// <summary>
    /// Parses the supplied login string expressed in EntityId format.
    /// The string has to be formatted as EntityId or plain string which then assumes defaults.
    /// The EntityId.System is Provider.Name, and EntityId.Type is login type.
    /// Throws `DataValidationException/400` on wrong ID
    /// </summary>
    public (LoginProvider provider, EntityId id) ParseId(string id)
    {
      var isplain = id.NonBlank(nameof(id))
                      .IndexOf(EntityId.SYS_PREFIX) == -1;

      if (isplain)
      {
        var p = Providers[DefaultLoginProvider.Value].NonNull(nameof(DefaultLoginProvider));
        return (p, new EntityId(DefaultLoginProvider, p.DefaultLoginType, Atom.ZERO, id));
      }

      if (!EntityId.TryParse(id, out var result))
      {
        throw new ValidationException("Bad id format") { HttpStatusDescription = "The id value is not a parsable `EntityId`"};
      }

      var provider = Providers[result.System.Value];
      if (provider == null)
      {
        throw new ValidationException("Unknown provider") { HttpStatusDescription = "Login provider `{0}` is not known".Args(result.System) };
      }

      if (result.Type.IsZero)
      {
        return (provider, new EntityId(result.System, provider.DefaultLoginType, Atom.ZERO, result.Address));
      }
      else if (!provider.SupportedLoginTypes.Any(t => t == result.Type))
      {
        throw new ValidationException("Bad login type") { HttpStatusDescription = "Login provider `{0}` does not support login type `{1}`".Args(result.System, result.Type) };
      }

      return (provider, result);
    }

    public (LoginProvider provider, EntityId id) ParseUri(string uri)
    {
      var isplain = uri.NonBlank(nameof(uri))
                       .IndexOf(EntityId.SYS_PREFIX) == -1;

      if (isplain)
      {
        var p = Providers[DefaultLoginProvider.Value].NonNull(nameof(DefaultLoginProvider));
        return (p, new EntityId(DefaultLoginProvider, Constraints.LTP_SYS_URI, Atom.ZERO, uri));
      }

      if (!EntityId.TryParse(uri, out var result))
      {
        throw new ValidationException("Bad URI format") { HttpStatusDescription = "The URI value is not a parsable `EntityId`" };
      }

      var provider = Providers[result.System.Value];
      if (provider == null)
      {
        throw new ValidationException("Unknown provider") { HttpStatusDescription = "Login URI provider `{0}` is not known".Args(result.System) };
      }

      if (result.Type.IsZero || result.Type == Constraints.LTP_SYS_URI)
      {
        return (provider, new EntityId(result.System, Constraints.LTP_SYS_URI, Atom.ZERO, result.Address));
      }

      throw new ValidationException("Bad URI type") { HttpStatusDescription = "The URI type must be omitted or set to `uri`" };
    }

    public AuthContext MakeNewUserAuthenticationContext(AuthenticationRequestContext ctx)
    {
      // Realm Atom is provided here as an implicit ambient context
      return new AuthContext(Ambient.CurrentCallSession.GetAtomDataContextName(), ctx);
    }

    //complementary pair method for TryDecodeSystemTokenData()
    public void MakeSystemTokenData(AuthContext context)
    {
      var sysSpanHrs = SysTokenLifespanHours > 0 ? SysTokenLifespanHours : 0.35d;//21 minutes by default
      var request = context.RequestContext;

      if (request != null && request.Intent.EqualsOrdSenseCase(AuthenticationRequestContext.INTENT_OAUTH))
      {
        var ssec = request.SysAuthTokenValiditySpanSec ?? 0;
        if (ssec > 0) sysSpanHrs = Math.Max(sysSpanHrs, ssec / 3_600d);
      }

      var sysExpiration = App.TimeSource.UTCNow.AddHours(sysSpanHrs);

      var msgToken = new
      {
        r = context.Realm,
        g = context.G_Login,
        i = context.LoginId, //Login Id
        e = sysExpiration
      };

      context.SysTokenData = SysTokenCryptoAlgorithm.ProtectAsString(msgToken);
    }

    //complementary pair method for MakeSystemTokenData()
    public LoginProvider TryDecodeSystemTokenData(string token, AuthContext context)
    {
      if (token.IsNullOrWhiteSpace()) return null;
      var msgToken = SysTokenCryptoAlgorithm.UnprotectObject(token) as JsonDataMap;
      if (msgToken == null) return null;//corrupted or forged token

      var expire = msgToken["e"].AsDateTime(default(DateTime),
                                       ConvertErrorHandling.ReturnDefault,
                                       System.Globalization.DateTimeStyles.AssumeUniversal |
                                       System.Globalization.DateTimeStyles.AdjustToUniversal
                                      );
      if (expire <= App.TimeSource.UTCNow) return null;//expired

      var tokenRealm = msgToken["r"].AsAtom(Atom.ZERO);
      if (tokenRealm != context.Realm) return null;//realm mismatch

      context.G_Login = msgToken["g"].AsGDID(GDID.ZERO);
      if (context.G_Login.IsZero) return null;//G_Login missing

      context.LoginId = msgToken["i"].AsEntityId(EntityId.EMPTY);
      if (!context.LoginId.IsAssigned) return null;//Login id missing

      //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      //ATTENTION!!! We MUST NOT re-generate the token but use existing one AS-IS!!!
      context.SysTokenData = token;//DO NOT re-issue token!!!

      var pvd = Providers[context.LoginId.System.Value];//may be null if not found
      return pvd;
    }

    public async Task ApplyEffectivePoliciesAsync(AuthContext context)
    {
      using (var scope = new SecurityFlowScope(TreePermission.SYSTEM_USE_FLAG))
      {
        await applyEffectivePoliciesAsync(context).ConfigureAwait(false);

        //Log
        var rel = Guid.NewGuid();
        if (!context.HasResult && ComponentEffectiveLogLevel <= Log.MessageType.TraceErrors)
        {
          var parJson = new
          {
            r = context.Result,
            p = context.Provider?.Name,
            rlm = context.Realm,
            g_u = context.G_User,
            g_l = context.G_Login,
            nm = context.Name
          }.ToJson(JsonWritingOptions.CompactASCII);

          WriteLog(Log.MessageType.TraceErrors, nameof(ApplyEffectivePoliciesAsync), "Bad auth", related: rel, pars: parJson);
        }

        if (ComponentEffectiveLogLevel <= Log.MessageType.Trace)
        {
          var parJson = new
          {
            r = context.Result,
            p = context.Provider?.Name,
            rlm = context.Realm,
            g_u = context.G_User,
            g_l = context.G_Login,
            nm = context.Name,
            context.ResultRole,
            context.ResultRoleConfig,
            context.ResultProps,
            context.ResultRights,
            context.Rights,
            context.LoginRights,
            context.Props,
            context.LoginProps,
          }.ToJson(JsonWritingOptions.CompactASCII);

          WriteLog(Log.MessageType.TraceC, nameof(ApplyEffectivePoliciesAsync), "Auth ctx", related: rel, pars: parJson);
        }
      }
    }

    private async Task applyEffectivePoliciesAsync(AuthContext context)
    {
      // Check for locked user/login account here
      checkLockStatus(context);
      if(!context.HasResult) return; // User or Login is locked out!

      // see below for next steps

      var eProps = new MemoryConfiguration { Application = App };
      var eRoleConfig = new MemoryConfiguration { Application = App };

      if (context.OrgUnit.HasValue &&
          context.OrgUnit.Value.System == Constraints.SYS_AUTHKIT &&
          context.OrgUnit.Value.Type == Constraints.ETP_ORGUNIT &&
          context.OrgUnit.Value.Schema == Azos.Conf.Forest.Constraints.SCH_PATH)
      {
        var idNode = GetIdpConfigTreeNodePath(context.Realm, context.OrgUnit.Value.Address);
        var tNode = await m_Forest.GetNodeInfoAsync(idNode).ConfigureAwait(false);
        if (tNode == null)
        {
          context.SetResult(AuthContext.Outcome.Negative(-100, "OrgUnit not found", map => map["ounit"] = idNode));
          return;
        }

        if (tNode.EffectiveConfig == null)
        {
          context.SetResult(AuthContext.Outcome.Negative(-101, "OrgUnit node data is missing", map => map["ounit"] = idNode));
          return;
        }

        IConfigSectionNode nProp = null;
        try
        {
          nProp = tNode.EffectiveConfig.Node[Constraints.CONFIG_PROP_ROOT_SECTION];
        }
        catch(Exception ex)
        {
          context.SetResult(AuthContext.Outcome.Negative(-102, "OrgUnit node data corrupted", map =>
          {
            map["ounit"] = idNode;
            map["err"] = new WrappedExceptionData(ex, captureStack: false);
          }));
          return;
        }

        if (nProp != null && nProp.Exists)
        {
          eProps.CreateFromNode(nProp);
        }
      }


      if (!eProps.Root.Exists)
      {
        eProps.Create(Constraints.CONFIG_PROP_ROOT_SECTION);
      }


      //1. Assign PROPS =======================================
      if (context.OrgUnit.HasValue)//#809
      {
        eProps.Root.AddAttributeNode(Constraints.CONFIG_ORG_UNIT_ATTR, context.OrgUnit.Value.AsString);
      }

      if (context.Props.Node != null) //user props are required, use safeguard
      {
        eProps.Root.OverrideBy(context.Props.Node);
      }
      if (context.LoginProps != null) //login props are optional
      {
        eProps.Root.OverrideBy(context.LoginProps.Node);
      }
      context.ResultProps = new ConfigVector(eProps.Root);


      //2. Assign minidp Primary role ================================================
      context.ResultRole = context.ResultProps.Node.ValOf(Constraints.CONFIG_ROLE_ATTR);


      //3. Assign RIGHTS =======================================
      if (context.ResultRole.IsNotNullOrWhiteSpace())
      {
        var idNode = GetIdpConfigTreeNodePath(context.Realm, context.ResultRole);
        var tNode = await m_Forest.GetNodeInfoAsync(idNode).ConfigureAwait(false);
        if (tNode == null)
        {
          context.SetResult(AuthContext.Outcome.Negative(-200, "Role not found", map => map["role"] = idNode));
          return;
        }

        if (tNode.EffectiveConfig == null)
        {
          context.SetResult(AuthContext.Outcome.Negative(-201, "Role node data is missing", map => map["role"] = idNode));
          return;
        }

        IConfigSectionNode nRole = null;
        try
        {
          nRole = tNode.EffectiveConfig.Node;
        }
        catch (Exception ex)
        {
          context.SetResult(AuthContext.Outcome.Negative(-202, "Role node data corrupted", map =>
          {
            map["role"] = idNode;
            map["err"] = new WrappedExceptionData(ex, captureStack: false);
          }));
          return;
        }

        eRoleConfig.CreateFromNode(nRole);
      }
      else
      {
        eRoleConfig.Create(Rights.CONFIG_ROOT_SECTION);
      }

      context.ResultRoleConfig = new ConfigVector(eRoleConfig.Root);//#803

      var eRightsNode = eRoleConfig.Root[Rights.CONFIG_ROOT_SECTION];
      if (!eRightsNode.Exists)
      {
        eRightsNode = Configuration.NewEmptyRoot(Rights.CONFIG_ROOT_SECTION);
      }

      if (context.Rights != null)
      {
        IConfigSectionNode nRights = null;
        try
        {
          nRights = context.Rights.Node;
        }
        catch (Exception ex)
        {
          context.SetResult(AuthContext.Outcome.Negative(-203, "User.Rights node data corrupted", map =>
          {
            map["usrlid"] = context.LoginId;
            map["usrn"] = context.Name;
            map["err"] = new WrappedExceptionData(ex, captureStack: false);
          }));
          return;
        }

        eRightsNode.OverrideBy(nRights);
      }

      if (context.LoginRights != null)
      {
        IConfigSectionNode nRights = null;
        try
        {
          nRights = context.LoginRights.Node;
        }
        catch (Exception ex)
        {
          context.SetResult(AuthContext.Outcome.Negative(-204, "UserLogin.Rights node data corrupted", map =>
          {
            map["usrlid"] = context.LoginId;
            map["usrn"] = context.Name;
            map["err"] = new WrappedExceptionData(ex, captureStack: false);
          }));
          return;
        }

        eRightsNode.OverrideBy(nRights);
      }

      context.ResultRights = new ConfigVector(eRightsNode);
    }

    private void checkLockStatus(AuthContext context)
    {
      var now = App.GetUtcNow();

      if (context.LockSpanUtc.HasValue && context.LockSpanUtc.Value.Contains(now))
      {
        context.SetResult(AuthContext.Outcome.Negative(-700, "User lock", map => map["lck-span"] = context.LockSpanUtc.Value));
        return;
      }

      if (context.LoginLockSpanUtc.HasValue && context.LoginLockSpanUtc.Value.Contains(now))
      {
        context.SetResult(AuthContext.Outcome.Negative(-701, "Login lock", map => map["lck-span"] = context.LoginLockSpanUtc.Value));
        return;
      }
    }

  }
}


/*

We would first Calculate props

Then take effective props take effective roles and traverse them for effective rights

The default handler-wide default props and rights must be somehow realm aware!!!!!!!!!!!!!
The realm MUST be a part of ORG UNIT and role paths, so everywhere else we store a partial tree path!!!!!!!

#1 Calculating effective props
  2. If Org Unit is specified navigate tree and fetch props
  3. Override effective props with user props
  4. Override effective props with login props
  5. Effective props now contains a role/s / multiple roles listed as config sections merged by id

#2 Calculating effective rights
  2. In order iterate through roles
  3. Override with user rights
  4. Override with login rights

EntityIds for AuthKit Tree are system = handler.IdpForestId, type = {realm}, schema = SCH_PATH, address = {roles|props}

*/