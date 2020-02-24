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
    public const string DELIMITER = ";";

    private string[] m_Includes;
    private string[] m_Excludes;
    private bool m_SenseCase;


    protected virtual bool SenseCase => m_SenseCase;

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

      if (m_Includes != null && m_Includes.Length > 0)
      {
        if (!m_Includes.Any(p => value.MatchPattern(p, senseCase: SenseCase))) return false;
      }

      if (m_Excludes != null && m_Excludes.Length > 0)
      {
        if (m_Excludes.Any(p => value.MatchPattern(p, senseCase: SenseCase))) return false;
      }

      return true;
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      m_SenseCase = node.Of(CONFIG_CASE_ATTR).ValueAsBool();

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


}
