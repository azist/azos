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

    /// <summary>
    /// Returns content between the specified header and the next header of this or higher rank,
    /// e.g. "## Endpoint" will return content until the next H2 or H1 header
    /// </summary>
    public static string GetSectionContent(string markdown, string section)
    {
      if (markdown == null) return string.Empty;
      if (section.IsNullOrWhiteSpace()) return string.Empty;

      markdown = '\n'+markdown+'\n';

      var start = '\n' + section;
      var istart = markdown.IndexOf(start);

      if (istart<0) return string.Empty;
      if (istart+start.Length+2 >= markdown.Length) return string.Empty;

      var level = 0;//level of heading H1/H2 etc..
      for(var i =0; i<start.Length; i++)
      {
        if (start[i]=='#') level++;
        if (start[i]!='#' && level>0) break;
      }

      while(level>0)
      {
        var end = '\n' + new string('#', level) + ' ';
        var iend = markdown.IndexOf(end, istart+start.Length+1);
        if (iend>istart)
          return markdown.Substring(istart, iend-istart-1).Trim(TRIM);

        level--;//increase the header size
      }
      return markdown.Substring(istart).Trim(TRIM);
    }


    /// <summary>
    /// Evaluates variables by applying a transform function to each value placed in between `{...}`.
    /// It is up to the eval function to interpret escapes
    /// </summary>
    public static string EvaluateVariables(string content, Func<string, string> eval)
    {
      if (content.IsNullOrWhiteSpace()) return string.Empty;
      if (eval==null) return null;

      var result = new StringBuilder(content.Length);
      var span = new StringBuilder(64);
      var isSpan = false;
      var i=0;
      for(;i<content.Length-1; i++)
      {
        var ch = content[i];
        var nch = content[i+1];
        if (ch=='`' && nch == '{')
        {
          isSpan = true;
          i++;
          continue;
        }

        if (ch=='}' && nch == '`')
        {
          isSpan = false;
          var evaluated = eval(span.ToString());
          result.Append(evaluated);
          span.Clear();
          i++;
          continue;
        }

        if (isSpan)
          span.Append(ch);
        else
          result.Append(ch);
      }

      if (i<content.Length) result.Append(content[i]);//last char

      return result.ToString();
    }

  }
}
