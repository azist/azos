using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// e.g. "## Endpoints" will return content until the next H2 or H1 header.
    /// </summary>
    public static string GetSectionContent(string markdown, string section)
     => GetSectionContentEx(markdown, section).content;

    /// <summary>
    /// Returns content between the specified header and the next header of this or higher rank,
    /// e.g. "## Endpoints" will return content until the next H2 or H1 header.
    /// This method returns the start and end indexes in the original markdown content taken by the section
    /// </summary>
    public static (string content, int istart, int iend) GetSectionContentEx(string markdown, string section)
    {
      if (markdown == null || section.IsNullOrWhiteSpace()) return (string.Empty, -1, -1);

      markdown = '\n'+markdown+'\n';//content is padded +1

      var start = '\n' + section;
      var istart = markdown.IndexOf(start);

      if (istart<0) return (string.Empty, -1, -1);
      if (istart+start.Length+2 >= markdown.Length) return (string.Empty, istart-1, -1);

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
          return (markdown.Substring(istart, iend-istart-1).Trim(TRIM), istart-1, iend-1);

        level--;//increase the header size
      }
      return (markdown.Substring(istart).Trim(TRIM), istart-1, -1);
    }

    /// <summary>
    /// Returns content of markdown without the content between the specified header and the next header of this or higher rank,
    /// also returning the index where the excision was made.
    /// Example: "## Endpoints" will return all content but the content starting with "## Endpoints" until the next H2 or H1 header.
    /// </summary>
    public static (string content, int iexcision) ExciseSection(string markdown, string section)
    {
      var span = GetSectionContentEx(markdown, section);
      if (span.istart<0) return (markdown, -1);
      var result = markdown.Substring(0, span.istart);
      if (span.iend>0)
       result += markdown.Substring(span.iend);

      return (result, span.istart);
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


    private static readonly string[] HX = new[]{"# ", "## ", "### ", "#### ", "##### ", "###### ", "####### "};

    /// <summary>
    /// Performs simple markdown to HTML conversion
    /// </summary>
    public static string MarkdownToHtml(string content)
    {
      if (content.IsNullOrWhiteSpace()) return string.Empty;
      var result = new StringBuilder(content.Length);
      var lineBuffer = new StringBuilder(64);

      var isPar = false;
      var isCodeBlock = false;
      var isUL = false;
      var isOL = false;

      void closeSpan()
      {
        if (isPar)
        {
          isPar = false;
          result.AppendLine();
          result.AppendLine("</p>");
          return;
        }

        if (isCodeBlock)
        {
          isCodeBlock = false;
          result.AppendLine("</pre>");
          return;
        }

        if (isUL)
        {
          isUL = false;
          result.AppendLine("</ul>");
          return;
        }

        if (isOL)
        {
          isOL = false;
          result.AppendLine("</ol>");
          return;
        }
      }

      void processLine()
      {
        var lineFull = lineBuffer.ToString();

        if (lineFull.Length==0 && isPar)//paragraph delimiter
        {
          closeSpan();
          return;
        }

        var line = lineFull.Trim();
        lineBuffer.Clear();
        if (line.IsNullOrWhiteSpace()) return;

        if (line.StartsWith("```"))
        {
          if (!isCodeBlock)
          {
            closeSpan();
            result.AppendLine("<pre>");
            isCodeBlock = true;
          }
          else
          {
            closeSpan();
          }
          return;
        }

        if (isCodeBlock)
        {
          result.AppendLine(WebUtility.HtmlEncode(lineFull));//<--- escape HTML
          return;
        }

        for(var j=0; j<HX.Length; j++)
        {
          if (line.StartsWith(HX[j]))
          {
            closeSpan();
            result.AppendLine("<h{0}>{1}</h{0}>".Args(j+1, line.Substring(j+1)));
            return;
          }
        }

        if (line.All(c => c=='-'))
        {
          closeSpan();
          result.AppendLine("<hr />");
          return;
        }

        if (line.StartsWith("- ") || line.StartsWith("* "))
        {
          if (!isUL)
          {
            closeSpan();
            isUL = true;
            result.AppendLine("<ul>");
          }
          result.AppendLine("  <li>{0}</li>".Args(line.Substring(1)));
          return;
        }

        var idot = line.IndexOf('.');
        if (idot>0 && idot<4 && int.TryParse(line.Substring(0,idot), out var _))
        {
          if (!isOL)
          {
            closeSpan();
            isOL = true;
            result.AppendLine("<ol>");
          }
          result.AppendLine("  <li>{0}</li>".Args(line.Substring(idot + 1)));
          return;
        }

        if (!isPar)
        {
          closeSpan();
          isPar = true;
          result.AppendLine();
          result.AppendLine("<p>");
        }
        result.Append(line);
      }




      //loop by lines
      for(var i=0; i<content.Length; i++ )
      {
        var ch = content[i];
        if (ch=='\n' || ch=='\r')
        {
          processLine();
          continue;
        }
        lineBuffer.Append(ch);
      }
      processLine();
      closeSpan();

      return result.ToString();
    }


  }
}
