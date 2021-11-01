/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Apps;
using Azos.Data;
using Azos.Text;

namespace Azos.Security.ConfigForest
{
  /// <summary>
  /// Defines access levels for conf forest tree access
  /// </summary>
  public enum TreeAccessLevel
  {
    Denied = AccessLevel.DENIED,
    Read   = AccessLevel.VIEW,
    Setup  = AccessLevel.VIEW_CHANGE_DELETE
  }


  /// <summary>
  /// Grants the assignee to have desired access level to a tree node in a forest
  /// </summary>
  public sealed class TreePermission : TypedPermission
  {
    /// <summary>
    /// If this flag is set on the current call scope, relaxes the permission check
    /// for system-use cases, when the system needs to access data for internal purposes, vs
    /// disclosing the data directly to the calling user
    /// </summary>
    public static readonly Atom SYSTEM_USE_FLAG = Atom.Encode("SYS-USE");

    public const string ALLOW_SECT = "allow";
    public const string DENY_SECT = "deny";
    public const string PATH_ATTR = "path";

    public TreePermission(TreeAccessLevel level, EntityId target) : base((int)level)
    {
      Target = target;
    }

    /// <summary>
    /// Target subject path protected by permission
    /// </summary>
    public readonly EntityId Target;

    public override string Description => $"Grants the assignee to have desired access level to a tree node in a forest";

    protected override bool DoCheckAccessLevel(ISecurityManager secman, ISession session, AccessLevel access)
    {
      //Bypass security checks if the data is needed for system use
      if (SecurityFlowScope.CheckFlag(SYSTEM_USE_FLAG)) return true;

      if (!base.DoCheckAccessLevel(secman, session, access)) return false;

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
