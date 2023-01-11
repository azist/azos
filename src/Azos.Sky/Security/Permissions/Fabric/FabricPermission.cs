/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using System.Linq;

namespace Azos.Security.Fabric
{
  /// <summary>
  /// Controls the access level to chronicle service
  /// </summary>
  public enum FabricAccessLevel
  {
    Denied  = AccessLevel.DENIED,

    //VIEW does not apply
    //AccessLevel.VIEW,

    /// <summary>
    /// Can start, run, set result, send signals. Can not Pause/Suspend/Abort
    /// </summary>
    Operate  = AccessLevel.VIEW_CHANGE,

    /// <summary>
    /// Everything above <see cref="Operate"/>  plus: List, Pause/Suspend/Abort
    /// </summary>
    Manage = AccessLevel.VIEW_CHANGE_DELETE
  }

  /// <summary>
  /// Controls whether users can access chronicle functionality
  /// </summary>
  /// <example>
  /// Sample ACL:
  /// <code>
  /// fabric
  /// {
  ///   level=0
  ///
  ///   case
  ///   {
  ///     of="play,db,biz"
  ///     level=2
  ///   }
  ///
  ///   case
  ///   {
  ///     of= "sys"
  ///     level = 3
  ///   }
  /// }
  /// </code>
  /// </example>
  public sealed class FabricPermission : TypedPermission
  {
    public const string CASE_SECT = "case";
    public const string OF_ATTR = "of";


    public FabricPermission() : this(FabricAccessLevel.Operate) { }
    public FabricPermission(FabricAccessLevel level) : base((int)level) { }
    public FabricPermission(FabricAccessLevel level, Atom runspace) : base((int)level)
    {
      Runspace = runspace.HasRequiredValue(nameof(runspace));
    }

    /// <summary>
    /// Optional runspace subject protected by permission
    /// </summary>
    public readonly Atom Runspace;

    public override string Description => Azos.Sky.StringConsts.PERMISSION_DESCRIPTION_FabricPermission;

    protected sealed override bool DoCheckAccessLevel(ISecurityManager secman, ISession session, AccessLevel access)
    {
      var level = access.Level;

      if (!Runspace.IsZero)
      {
        var rs = Runspace.Value;

        var ncase = access.Data
                          .ChildrenNamed(CASE_SECT)
                          .FirstOrDefault(n => n.ValOf(OF_ATTR)
                                                .Split(',').Any(one => one.Trim().EqualsOrdSenseCase(rs)));
        if (ncase != null)
        {
          level = ncase.Of(AccessLevel.CONFIG_LEVEL_ATTR).ValueAsInt(AccessLevel.DENIED);
        }
      }

      return level >= Level;
    }
  }
}
