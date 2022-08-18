/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Collections;
using Azos.Conf;

namespace Azos.Sky.Cms
{
  /// <summary>
  /// Provides information about a language: an iso code atom and a name
  /// </summary>
  [Serializable]
  public struct LangInfo : INamed
  {
    public const string CONFIG_LANG_INFO_SECTION = "lang-info";

    public static readonly LangInfo GENERIC_ENGLISH = new LangInfo(CoreConsts.ISOA_LANG_ENGLISH, "English");

    /// <summary>
    /// Returns enumerable of LangInfo structures made of "lang-info" config sections
    /// </summary>
    public static IEnumerable<LangInfo> MakeManyFromConfig(IConfigSectionNode cfg)
     =>  cfg.NonNull(nameof(cfg))
            .Children
            .Where(c => c.IsSameName(CONFIG_LANG_INFO_SECTION))
            .Select(c => new LangInfo(c) )
            .ToArray();

    /// <summary>
    /// Defaults the value if it is null or not assigned
    /// </summary>
    public static LangInfo Default(LangInfo? value, LangInfo? dflt = null)
    {
      dflt = dflt ?? GENERIC_ENGLISH;
      if (!dflt.Value.IsAssigned) dflt = GENERIC_ENGLISH;
      var result = value.HasValue ? value.Value : dflt.Value;
      return result.IsAssigned ? result : dflt.Value;
    }

    public LangInfo(Atom iso, string name)
    {
      ISO = iso;
      Name = name.NonBlank(nameof(name));
    }

    public LangInfo(IConfigSectionNode cfg) : this(
      Atom.Encode(cfg.NonNull(nameof(cfg)).AttrByName(nameof(ISO)).Value.NonBlank(nameof(ISO))),//throws on bad atom
      cfg.NonNull(nameof(cfg)).AttrByName(nameof(Name)).Value)
    {

    }

    /// <summary>
    /// Language ISO code atom.
    /// </summary>
    /// <remarks>
    /// Since language codes are a limited set, they are stored as atoms for efficiency and consistency.
    /// You can use Atom.Encode(string) to encode a string.
    /// See: https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
    /// </remarks>
    public readonly Atom ISO;

    /// <summary>
    /// Language name
    /// </summary>
    public readonly string Name;
    string INamed.Name => this.Name;

    /// <summary>
    /// Returns true if this is an assigned structure
    /// </summary>
    public bool IsAssigned => !ISO.IsZero;

    public override string ToString() => $"[{ISO}]'{Name}'";
  }
}
