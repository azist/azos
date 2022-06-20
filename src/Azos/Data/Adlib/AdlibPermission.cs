/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Apps;
using Azos.Data;
using Azos.Security;
using Azos.Text;

namespace Azos.Security.Adlib
{
  /// <summary>
  /// Defines access levels for ADLIB server access
  /// </summary>
  public enum AdlibAccessLevel
  {
    Denied  = AccessLevel.DENIED,
    Read    = AccessLevel.VIEW,
    Change  = AccessLevel.VIEW_CHANGE,
    Delete  = AccessLevel.VIEW_CHANGE_DELETE,
    Drop    = AccessLevel.VIEW_CHANGE_DELETE + 1,
    Admin   = AccessLevel.VIEW_CHANGE_DELETE + 1000
  }


  /// <summary>
  /// Grants the assignee to have desired access level to Adlib data
  /// </summary>
  public sealed class AdlibPermission : TypedPermission
  {
    public const string ALLOW_SECT = "allow";
    public const string DENY_SECT = "deny";
    public const string PATH_ATTR = "path";


    public AdlibPermission(AdlibAccessLevel level) : base((int)level){ }

    public AdlibPermission(AdlibAccessLevel level, EntityId target) : base((int)level)
    {
      Target = target.HasRequiredValue(nameof(target));
    }

    /// <summary>
    /// Optional target subject item id protected by permission
    /// </summary>
    public readonly EntityId Target;

    public override string Description => $"Grants the assignee to have desired access level to Adlib data";

    protected override bool DoCheckAccessLevel(ISecurityManager secman, ISession session, AccessLevel access)
    {
      if (!base.DoCheckAccessLevel(secman, session, access)) return false;

      if (!Target.IsAssigned) return true;

      var id = Target.AsString();

      //allow{ path='*' } - match all
      //deny { path='*@fin::*' } - but deny access to `fin` forest
      //deny { path='geo@class::*' } - any `geo` tree in `class` forest
      if (!access.Data.ChildrenNamed(ALLOW_SECT)
                      .Any(c => id.MatchPattern(c.ValOf(PATH_ATTR)))) return false;//NONE allowed

      if (access.Data.ChildrenNamed(DENY_SECT)
                     .Any(c => id.MatchPattern(c.ValOf(PATH_ATTR)))) return false;//Deny match

      return true;
    }
  }
}
