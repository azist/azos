/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Server
{
  public sealed class DefaultIdpHandlerLogic : ModuleBase, IIdpHandlerLogic
  {
    public DefaultIdpHandlerLogic(IApplication application) : base(application) { }
    public DefaultIdpHandlerLogic(IModule parent) : base(parent) { }

    Registry<LoginProvider> m_Providers;

    [Config] Atom m_DefaultLoginProvider;

    public bool IsServerImplementation => true;
    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    public IRegistry<LoginProvider> Providers => m_Providers;

    public Atom DefaultLoginProvider => m_DefaultLoginProvider;

    public string SysTokenCryptoAlgorithmName => throw new NotImplementedException();

    public double SysTokenLifespanHours => throw new NotImplementedException();


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
      m_Providers.ForEach( p => this.DontLeak( () => p.Dispose()) );
      m_Providers = null;
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Providers.NonNull("configured providers")[DefaultLoginProvider.Value]
                 .NonNull("default login provider `{0}`".Args(DefaultLoginProvider));
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      cleanup();
      return base.DoApplicationBeforeCleanup();
    }
    #endregion

    public string MakeSystemTokenData(Atom realm, GDID gUser, JsonDataMap auxData = null)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Parses the supplied login string expressed in EntityId format.
    /// The string has to be formatted as EntityId or plain string which then assumes defaults.
    /// The EntityId.System is Provider.Name, and EntityId.Type is login type.
    /// Throws `DataValidationException/400` on wrong ID
    /// </summary>
    public EntityId ParseId(string id)
    {
      var isplain = id.NonBlank(nameof(id))
                      .IndexOf(EntityId.SYS_PREFIX) == -1;

      if (isplain)
      {
        var p = Providers[DefaultLoginProvider.Value].NonNull(nameof(DefaultLoginProvider));
        return new EntityId(DefaultLoginProvider, p.DefaultLoginType, Atom.ZERO, id);
      }

      if (!EntityId.TryParse(id, out var result))
      {
        throw new ValidationException("Bad id format") { HttpStatusDescription = "The ID is not parsable as EntityId"};
      }

      var provider = Providers[result.System.Value];
      if (provider == null)
      {
        throw new ValidationException("Unknown provider") { HttpStatusDescription = "Login provider `{0}` is not known".Args(result.System) };
      }

      if (result.Type.IsZero)
      {
        result = new EntityId(result.System, provider.DefaultLoginType, Atom.ZERO, result.Address);
      }
      else if (!provider.SupportedLoginTypes.Any(t => t == result.Type))
      {
        throw new ValidationException("Bad login type") { HttpStatusDescription = "Login provider `{0}` does not support login type `{1}`".Args(result.System, result.Type) };
      }

      return result;
    }

    public EntityId ParseUri(string uri)
    {
      throw new NotImplementedException();
    }
  }
}
