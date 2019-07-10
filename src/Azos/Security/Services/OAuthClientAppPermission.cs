/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Apps;
using Azos.Text;

namespace Azos.Security.Services
{
  /// <summary>
  /// Designates bearer as an OAuth client application, having optional redirect Uri and backend IP patterns
  /// which are tried to match against the caller.
  /// User .ctor(uri,addr) to assert the access for the specific URI/backend caller IP.
  /// The ACL/Rights provider is expected to supply ACL in the specific form (see remarks)
  /// </summary>
  /// <remarks>
  /// The ACL/Rights provider is expected to supply ACL data node in the form:
  /// <code>
  /// acl{ uri{pat="https://*.domain1.com/*"} uri{pat="https://*.domain2.com/*"} addr{pat="27.11.7*"} ... }
  /// </code>
  /// </remarks>
  public sealed class OAuthClientAppPermission : TypedPermission
  {
    public const string CONFIG_ACL_URI_SECTION = "uri";
    public const string CONFIG_ACL_ADDR_SECTION = "addr";
    public const string CONFIG_ACL_PAT_ATTR = "pat";

    public OAuthClientAppPermission(int level) : base(level){ }

    public OAuthClientAppPermission(string uri, string backchannelCallerAddress = null, int level = AccessLevel.VIEW) : base(level)
    {
      RedirectUri = uri;
      BackchannelCallerAddress = backchannelCallerAddress;
    }

    /// <summary>
    /// The asserted Redirect URI - supplied by the caller.
    /// If set, permission checks it against the list of authorized URI patterns
    /// </summary>
    public readonly string RedirectUri;

    /// <summary>
    /// The asserted BackchannelCallerAddress - supplied by the back channel caller (relying party calling OAUTH server).
    /// If set, permission checks it against the list of authorized (ip) address patterns
    /// </summary>
    public readonly string BackchannelCallerAddress;


    protected override bool DoCheckAccessLevel(IApplication app, ISession session, AccessLevel access)
    {
      if (!base.DoCheckAccessLevel(app, session, access)) return false;//deny on insufficient level

      if (RedirectUri.IsNotNullOrWhiteSpace())
        return checkPatterns(access, CONFIG_ACL_URI_SECTION, RedirectUri);

      if (BackchannelCallerAddress.IsNotNullOrWhiteSpace())
        return checkPatterns(access, CONFIG_ACL_ADDR_SECTION, BackchannelCallerAddress);

      return true;//granted: default
    }

    private bool checkPatterns(AccessLevel access, string section, string value)
    {
      if (access.Data == null || !access.Data.Exists) return false;

      //loop through values in ACL
      foreach (var uripat in access.Data.Children.Where(c => c.IsSameName(section)))
      {
        var pattern = uripat.ValOf(CONFIG_ACL_PAT_ATTR);
        if (Utils.MatchPattern(value, pattern)) return true;//grant: match found
      }

      return false;//deny: the value was not matched by any of the allowed patterns
    }

  }

}
