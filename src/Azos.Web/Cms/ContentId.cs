/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;

namespace Azos.Sky.Cms
{
  /// <summary>
  /// Identifies a piece of content by PORTAL:NS:BLOCK pair.
  /// Portals identify logical web sites, a namespace is used to identify a part of the site/area/template,
  /// a block represents a concrete piece of content on a page/document/interface
  /// </summary>
  [Serializable]
  public struct ContentId : IEquatable<ContentId>, IRequiredCheck
  {
    /// <summary>
    /// Imposes a limit on the content id segment identifier
    /// </summary>
    public const int MAX_ID_LEN = 64;

    /// <summary>
    /// Create the content identifier. The segments values must be an ASCII char or digit,
    /// hyphen, dots or underscore only of at most 64 chars in total length
    /// </summary>
    /// <param name="portal">Portal id</param>
    /// <param name="ns">Namespace name within portal</param>
    /// <param name="block">Block name within namespace</param>
    public ContentId(string portal,
                     string ns,
                     string block)
    {
      if (!ValidateIdentifier(portal)) throw new CmsException(StringConsts.CMS_ID_ERROR.Args(nameof(portal), portal, MAX_ID_LEN));
      if (!ValidateIdentifier(ns))     throw new CmsException(StringConsts.CMS_ID_ERROR.Args(nameof(ns), ns, MAX_ID_LEN));
      if (!ValidateIdentifier(block))  throw new CmsException(StringConsts.CMS_ID_ERROR.Args(nameof(block), block, MAX_ID_LEN));
      Portal =  portal;
      Namespace = ns;
      Block = block;
    }

    /// <summary>
    /// Portal ID - the identifier of the target site instance aka. "portal". The value must be an ASCII char or digit,
    /// hyphen or underscore only of at most 64 chars in total length
    /// </summary>
    public readonly string Portal;

    /// <summary>
    /// Identifies a named area within the Portal. The value must be an ASCII char or digit,
    /// hyphen or underscore only of at most 64 chars in total length
    /// </summary>
    public readonly string Namespace;

    /// <summary>
    /// Identifies the content block within the namespace/portal pair. The value must be an ASCII char or digit,
    /// hyphen or underscore only of at most 64 chars in total length
    /// </summary>
    public readonly string Block;

    /// <summary>
    /// Returns true if this is an assigned structure
    /// </summary>
    public bool IsAssigned => Portal != null;

    public bool CheckRequired(string targetName) => IsAssigned;

    public bool Equals(ContentId other)
      => this.Block.EqualsOrdIgnoreCase(other.Block) && //most specific
         this.Namespace.EqualsOrdIgnoreCase(other.Namespace) &&
         this.Portal.EqualsOrdIgnoreCase(other.Portal);

    public override bool Equals(object obj)
      => (obj is ContentId cid) ? Equals(cid) : false;

    public override int GetHashCode() => Block?.GetHashCode() ?? 0;//has the maximum specificity

    public override string ToString() => $"@{Portal}/{Namespace}#{Block}";

    public static bool operator ==(ContentId lhs, ContentId rhs) =>  lhs.Equals(rhs);
    public static bool operator !=(ContentId lhs, ContentId rhs) => !lhs.Equals(rhs);

    /// <summary>
    /// Returns true if the specified string represents a valid identifier segment for portal content identification,
    /// that is: is only contains `0`..`9`|`A`..`Z`|`a`..`z`|`-`|`_`|`.` characters and does NOT start with `.`.
    /// The length must be between 1 and MAX_ID_LEN = 64 aforementioned characters
    /// </summary>
    public static bool ValidateIdentifier(string id)
    {
      if (id == null) return false;
      if (id.Length < 1 || id.Length > MAX_ID_LEN) return false;

      for(var i=0; i<id.Length; i++)
      {
        var c = id[i];
        if (c >= '0' && c <= '9') continue;
        if (c >= 'a' && c <= 'z') continue;
        if (c >= 'A' && c <= 'Z') continue;
        if (c == '-') continue;
        if (c == '_') continue;
        if (c == '.' && i > 0) continue;
        return false;
      }
      return true;
    }

  }
}
