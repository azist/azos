using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Text
{
  /// <summary>
  /// Provides helpers for working with basic markdown constructs
  /// </summary>
  public static class MarkdownUtils
  {

    private static readonly char[] ENDINGS = new [] { '\r', '\n' };

    private static readonly char[] TRIM = new [] { '\r', '\n', ' ' };

    /// <summary>
    /// Returns content of the first H1 level'#' tag
    /// </summary>
    public static string GetTitle(string markdown)
    {
      if (markdown==null) return string.Empty;
      var i = markdown.IndexOf("# ");
      if (i<0) return string.Empty;
      var j = markdown.IndexOfAny(ENDINGS, i+1);
      if (j<0) return markdown.Substring(i+1).Trim();
      return markdown.Substring(i+1, j-i-1).Trim();
    }

    /// <summary>
    /// Gets content between The first title H1 and any subsequent H*
    /// </summary>
    public static string GetTitleDescription(string markdown)
    {
      if (markdown == null) return string.Empty;
      var i = markdown.IndexOf("# ");
      if (i < 0) return string.Empty;
      var j = markdown.IndexOfAny(ENDINGS, i + 1);
      if (j < 0) return string.Empty;

      var k = markdown.IndexOf("# ", j+1);
      if (k < 0) return markdown.Substring(j + 1).Trim(TRIM);
      return markdown.Substring(j + 1, k - j - 2).Trim(TRIM);
    }
  }
}
