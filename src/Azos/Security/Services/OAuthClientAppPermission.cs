using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Text;

namespace Azos.Security.Services
{
  /// <summary>
  /// Designates bearer as an OAuth client application, having specifically authorized redirect Uri patterns.
  /// User .ctor(uri) to assert the access for the specific URI.
  /// The ACL/Rights provider is expected to supply ACL in the specific form (see remarks)
  /// </summary>
  /// <remarks>
  /// The ACL/Rights provider is expected to supply ACL data node in the form:
  /// <code>
  /// acl{ uri{pat="https://*.domain1.com/*"} uri{pat="https://*.domain2.com/*"} ... }
  /// </code>
  /// </remarks>
  public sealed class OAuthClientAppPermission : TypedPermission
  {
    public const string CONFIG_ACL_URI_SECTION = "uri";
    public const string CONFIG_ACL_PAT_ATTR = "pat";

    public OAuthClientAppPermission(int level) : base(level){ }

    public OAuthClientAppPermission(string uri, int level = AccessLevel.VIEW) : base(level)
    {
      RedirectUri = uri;
    }

    public readonly string RedirectUri;

    protected override bool DoCheckAccessLevel(IApplication app, ISession session, AccessLevel access)
    {
      if (!base.DoCheckAccessLevel(app, session, access)) return false;

      if (RedirectUri.IsNotNullOrWhiteSpace())
      {
        //check URI pattern match
        if (access.Data==null || !access.Data.Exists) return false;
        //loop through uris in ACL
        foreach (var uripat in access.Data.Children.Where(c=>c.IsSameName(CONFIG_ACL_URI_SECTION)))
        {
          var pattern = uripat.ValOf(CONFIG_ACL_PAT_ATTR);
          if (Utils.MatchPattern(RedirectUri, pattern)) return true;//match found
        }
      }
      return true;//granted
    }
  }

}
