/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Text;

using System.Linq;

namespace Azos.Scripting.Expressions
{
  /// <summary>
  /// Represents a general purpose predicate
  /// </summary>
  public abstract class Predicate<TContext> : Expression<TContext, bool>
  {
  }

  /// <summary>
  /// Derive your filters from here, e.g. Log.Message Filter
  /// </summary>
  public abstract class BoolFilter<TContext> : Predicate<TContext>
  {
  }

  /// <summary>
  /// Base for filters with pattern sets having include/exclude patterns
  /// </summary>
  public abstract class PatternSetFilter<TContext> : BoolFilter<TContext>
  {
    public const string CONFIG_DELIMITER_ATTR = "delimiter";
    public const string CONFIG_INCLUDE_ATTR = "include";
    public const string CONFIG_EXCLUDE_ATTR = "exclude";
    public const string CONFIG_CASE_ATTR = "case";
    public const string CONFIG_DEFAULT_ATTR = "default";
    public const string DELIMITER = ";";

    protected string[] m_Includes;
    protected string[] m_Excludes;
    protected bool m_SenseCase;
    protected bool m_Default = true;


    protected virtual bool SenseCase => m_SenseCase;
    protected virtual bool Default => m_Default;

    /// <summary>
    /// Override to extract string value out of TContext
    /// </summary>
    protected abstract string GetValue(TContext context);

    /// <summary>
    /// Default implementation of include/exclude pattern search
    /// </summary>
    public override bool Evaluate(TContext context)
    {
      var value = GetValue(context);
      var result = Default;

      if (m_Includes != null && m_Includes.Length > 0)
      {
        if (!m_Includes.Any(p => value.MatchPattern(p, senseCase: SenseCase))) return false;
        result = true;
      }

      if (m_Excludes != null && m_Excludes.Length > 0)
      {
        if (m_Excludes.Any(p => value.MatchPattern(p, senseCase: SenseCase))) return false;
        result = true;
      }

      return result;
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      m_SenseCase = node.Of(CONFIG_CASE_ATTR).ValueAsBool();
      m_Default = node.Of(CONFIG_DEFAULT_ATTR).ValueAsBool(true);

      var d = node.Of(CONFIG_DELIMITER_ATTR).ValueAsString(DELIMITER)[0];

      m_Includes = node.Of(CONFIG_INCLUDE_ATTR)
                       .Value
                       .Default()
                       .Split(d)
                       .Select(v => v.Trim())
                       .Where(s => s.IsNotNullOrWhiteSpace())
                       .ToArray();

      m_Excludes = node.Of(CONFIG_EXCLUDE_ATTR)
                        .Value
                        .Default()
                        .Split(d)
                        .Select(v => v.Trim())
                        .Where(s => s.IsNotNullOrWhiteSpace())
                        .ToArray();
    }
  }

  /// <summary>
  /// Base for filters with atom sets having include/exclude patterns
  /// </summary>
  public abstract class AtomSetFilter<TContext> : BoolFilter<TContext>
  {
    public const string CONFIG_DELIMITER_ATTR = PatternSetFilter<TContext>.CONFIG_DELIMITER_ATTR;
    public const string CONFIG_INCLUDE_ATTR = PatternSetFilter<TContext>.CONFIG_INCLUDE_ATTR;
    public const string CONFIG_EXCLUDE_ATTR = PatternSetFilter<TContext>.CONFIG_EXCLUDE_ATTR;
    public const string CONFIG_DEFAULT_ATTR = PatternSetFilter<TContext>.CONFIG_DEFAULT_ATTR;
    public const string DELIMITER = PatternSetFilter<TContext>.DELIMITER;

    protected Atom[] m_Includes;
    protected Atom[] m_Excludes;
    protected bool m_Default = true;

    protected virtual bool Default => m_Default;

    /// <summary>
    /// Override to extract Atom value out of TContext
    /// </summary>
    protected abstract Atom GetValue(TContext context);

    /// <summary>
    /// Default implementation of include/exclude pattern search
    /// </summary>
    public override bool Evaluate(TContext context)
    {
      var value = GetValue(context);

      var result = Default;

      if (m_Includes != null && m_Includes.Length > 0)
      {
        if (!m_Includes.Any(p => value == p)) return false;
        result = true;
      }

      if (m_Excludes != null && m_Excludes.Length > 0)
      {
        if (m_Excludes.Any(p => value == p)) return false;
        result = true;
      }

      return result;
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      m_Default = node.Of(CONFIG_DEFAULT_ATTR).ValueAsBool(true);

      var d = node.Of(CONFIG_DELIMITER_ATTR).ValueAsString(DELIMITER)[0];

      m_Includes = node.Of(CONFIG_INCLUDE_ATTR)
                       .Value
                       .Default()
                       .Split(d)
                       .Select(v => v.Trim())
                       .Where(s => s.IsNotNullOrWhiteSpace())
                       .Select(v =>
                       {
                         Atom.TryEncodeValueOrId(v, out var atm).IsTrue("Valid atom");
                         return atm;
                       })
                       .ToArray();

      m_Excludes = node.Of(CONFIG_EXCLUDE_ATTR)
                        .Value
                       .Default()
                       .Split(d)
                       .Select(v => v.Trim())
                       .Where(s => s.IsNotNullOrWhiteSpace())
                       .Select(v =>
                       {
                         Atom.TryEncodeValueOrId(v, out var atm).IsTrue("Valid atom");
                         return atm;
                       })
                       .ToArray();
    }
  }
}
