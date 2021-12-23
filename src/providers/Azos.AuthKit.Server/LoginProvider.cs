/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
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
    protected LoginProvider(IIdpHandlerLogic handler, IConfigSectionNode cfg) : base(handler)
    {
      cfg.NonEmpty(nameof(cfg));
      m_Name = cfg.Of(Configuration.CONFIG_NAME_ATTR).ValueAsAtom(Atom.ZERO).Value;
      m_Name.NonBlank("attribute ${0}".Args(Configuration.CONFIG_NAME_ATTR));
    }

    private string m_Name;//atom

    public string Name => m_Name;

    /// <summary>
    /// Returns a list of LOGN types this provider supports,
    /// e.g. `SystemA` may support "email" and "userid"
    /// </summary>
    public abstract IEnumerable<Atom> SupportedLoginTypes { get; }

    /// <summary>
    /// What login type to use if it is not specified by login
    /// </summary>
    public abstract Atom DefaultLoginType { get; }

  }
}
