/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Apps;

namespace Azos.Security.FileGateway
{
  /// <summary>
  /// Controls the access level to chronicle service
  /// </summary>
  public enum FileGatewayAccessLevel
  {
    Denied  = AccessLevel.DENIED,

    /// <summary>
    /// Can read files and directories
    /// </summary>
    Read = AccessLevel.VIEW,

    /// <summary>
    /// Can create directories and write files, rename dir and files
    /// </summary>
    Write  = AccessLevel.VIEW_CHANGE,

    /// <summary>
    /// Everything above <see cref="Write"/>  plus: Delete files and directories
    /// </summary>
    Full = AccessLevel.VIEW_CHANGE_DELETE
  }

  /// <summary>
  /// Controls whether users can access FileGateway functionality
  /// </summary>
  /// <example>
  /// Sample ACL:
  /// <code>
  /// FileGateway
  /// {
  ///   level=0
  ///
  ///   case
  ///   {
  ///     of="man:vision2, man:fin"
  ///     level=2
  ///   }
  ///
  ///   case
  ///   {
  ///     of= "sys:*"
  ///     level=3
  ///   }
  /// }
  /// </code>
  /// </example>
  public sealed class FileGatewayPermission : TypedPermission
  {
    public const string CASE_SECT = "case";
    public const string OF_ATTR = "of";
    public const char SYS_VOL_SEPARATOR = ':';
    public const string ANY = "*";


    public FileGatewayPermission() : this(FileGatewayAccessLevel.Read) { }
    public FileGatewayPermission(FileGatewayAccessLevel level) : base((int)level) { }
    public FileGatewayPermission(FileGatewayAccessLevel level, Atom system, Atom volume) : base((int)level)
    {
      System = system.HasRequiredValue(nameof(system));
      Volume = volume.HasRequiredValue(nameof(volume));
    }

    /// <summary>
    /// Optional runspace subject protected by permission
    /// </summary>
    public readonly Atom System;

    /// <summary>
    /// Optional runspace subject protected by permission
    /// </summary>
    public readonly Atom Volume;

    public override string Description => Azos.Sky.StringConsts.PERMISSION_DESCRIPTION_FileGatewayPermission;

    protected sealed override bool DoCheckAccessLevel(ISecurityManager secman, ISession session, AccessLevel access)
    {
      var level = access.Level;

      if (!System.IsZero)
      {
        var sys = System.Value;
        var vol = Volume.Value;

        var ncase = access.Data
                          .ChildrenNamed(CASE_SECT)
                          .FirstOrDefault(n => n.ValOf(OF_ATTR)
                                                .Split(',').Any(one =>
                                                {
                                                  var kvp = one.SplitKVP(SYS_VOL_SEPARATOR);
                                                  if (kvp.Key.Trim().EqualsOrdSenseCase(sys)) return false;
                                                  var v = kvp.Value.Trim();
                                                  if (v != ANY && !v.EqualsOrdSenseCase(vol)) return false;
                                                  return true;
                                                }));
        if (ncase != null)
        {
          level = ncase.Of(AccessLevel.CONFIG_LEVEL_ATTR).ValueAsInt(AccessLevel.DENIED);
        }
      }

      return level >= Level;
    }
  }
}
