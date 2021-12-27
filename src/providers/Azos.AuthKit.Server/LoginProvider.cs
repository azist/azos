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

namespace Azos.AuthKit.Server
{
  /// <summary>
  /// Abstraction for login provider. AuthKit stores logins for providers.
  /// A provider may be internal (such as a specific business system), or
  /// an external one like Facebook federated login in which case the login data
  /// stores Facebook-provided key/id
  /// </summary>
  public abstract class LoginProvider : ApplicationComponent<IIdpHandlerLogic>, INamed
  {
    public const string CONFIG_PROVIDER_SECTION = "provider";
    public const string CONFIG_DEFAULT_LOGIN_TYPE_ATTR = "default-login-type";

    protected LoginProvider(IIdpHandlerLogic handler, IConfigSectionNode cfg) : base(handler)
    {
      cfg.NonEmpty(nameof(cfg));
      m_Name = cfg.Of(Configuration.CONFIG_NAME_ATTR).ValueAsAtom(Atom.ZERO).Value;
      m_Name.NonBlank("attribute ${0}".Args(Configuration.CONFIG_NAME_ATTR));
      m_DefaultLoginType = cfg.Of(CONFIG_DEFAULT_LOGIN_TYPE_ATTR).ValueAsAtom(Atom.ZERO);
      SupportedLoginTypes.Any(t => t == m_DefaultLoginType)
                         .IsTrue("Known login type");
    }

    private string m_Name;//atom
    private Atom m_DefaultLoginType;

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;
    public string Name => m_Name;

    /// <summary>
    /// Returns a list of LOGIN types this provider supports,
    /// e.g. `SystemA` may support "email" and "userid"
    /// </summary>
    public abstract IEnumerable<Atom> SupportedLoginTypes { get; }

    /// <summary>
    /// What login type to use if it is not specified by login
    /// </summary>
    public virtual Atom DefaultLoginType => m_DefaultLoginType;

  }


  /// <summary>
  /// Default system login provider implemented by the AuthKit itself
  /// </summary>
  public sealed class SystemLoginProvider : LoginProvider
  {
    public SystemLoginProvider(IIdpHandlerLogic handler, IConfigSectionNode cfg) : base(handler, cfg){  }

    public override IEnumerable<Atom> SupportedLoginTypes
    {
      get
      {
        yield return Constraints.LTP_SYS_ID;
        yield return Constraints.LTP_SYS_EMAIL;
        yield return Constraints.LTP_SYS_PHONE;
      }
    }
  }

}
