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

namespace Azos.Wave.Cms
{
  /// <summary>
  /// Provides information about a language: an iso code and a name
  /// </summary>
  [Serializable]
  public struct LangInfo : INamed
  {
    public const string CONFIG_LANG_INFO_SECTION = "lang-info";

    public static readonly LangInfo GENERIC_ENGLISH = new LangInfo(CoreConsts.ISO_LANG_ENGLISH, "English");

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

    public LangInfo(string iso, string name)
    {
      ISO = iso.NonBlankMinMax(2, 3, nameof(iso));
      Name = name.NonBlank(nameof(name));
    }

    public LangInfo(IConfigSectionNode cfg) : this(
      cfg.NonNull(nameof(cfg)).AttrByName(nameof(ISO)).Value,
      cfg.NonNull(nameof(cfg)).AttrByName(nameof(Name)).Value)
    {

    }

    /// <summary>
    /// Language ISO code
    /// </summary>
    /// <remarks>
    /// See: https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
    /// </remarks>
    public readonly string ISO;

    /// <summary>
    /// Language name
    /// </summary>
    public readonly string Name;
    string INamed.Name => this.Name;

    /// <summary>
    /// Returns true if this is an assigned structure
    /// </summary>
    public bool IsAssigned => ISO != null;

    public override string ToString() => $"[{ISO}]'{Name}'";
  }
}
