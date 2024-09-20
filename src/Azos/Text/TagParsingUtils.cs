/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Conf;
using Azos.Data;

namespace Azos.Text
{
  /// <summary>
  /// Provides extensions for parsing a stream of characters into delimited segments
  /// which later can be interpreted as tags containing structured content.
  /// The functionality is mostly used for various templating engines like mail-merge and web messaging temlates
  /// pre processing HTML content used in rich ineternet messaging
  /// </summary>
  public static class TagParsingUtils
  {

    /// <summary>
    /// Delimites a segment of char stream into segments.
    /// If <see cref="IsTag"/> true then the segment is a part inside of delimited content, such as an HTML tag,
    /// otherwise it is just a simple content whoch should be treated verbatim
    /// </summary>
    public readonly struct Segment
    {
      internal Segment(bool isTag, int idxs, int idxe, string content)
      {
        IsTag = isTag;
        IdxStart = idxs;
        IdxEnd = idxe;
        Content = content;
      }

      public readonly bool IsTag;
      public readonly int IdxStart;
      public readonly int IdxEnd;
      public int Length => IdxEnd - IdxStart;

      public readonly string Content;
    }

    /// <summary>
    /// Parses a stream of chars into a stream of <see cref="Segment"/> instances.
    /// Empty enum is returned for null input
    /// </summary>
    public static IEnumerable<Segment> ParseSegments(this IEnumerable<char> source,
                                                      char segStart = '<',
                                                      char segEnd = '>')
    {
      if (source == null) yield break;

      var isTag = false;
      var buf = new StringBuilder(0xff);
      var idxs = 0;
      var idx = -1;
      foreach(var one in source)
      {
        idx++;
        if (isTag)
        {
          if (one == segEnd)
          {
            if (buf.Length > 0)
            {
              yield return new Segment(true, idxs, idx, buf.ToString());
              buf.Clear();
            }
            buf.Clear();
            isTag = false;
            idxs = idx + 1;
            continue;
          }
        }
        else
        {
          if (one == segStart)
          {
            if (buf.Length > 0)
            {
              yield return new Segment(false, idxs, idx, buf.ToString());
              buf.Clear();
            }
            isTag = true;
            idxs = idx;
            continue;
          }
        }

        buf.Append(one);
      }//foreach

      //flush
      if (buf.Length > 0) yield return new Segment(isTag, idxs, idx, buf.ToString());
    }//parseSegments


    /// <summary>
    /// Builds upon <see cref="Segment"/> by adding a parsed tag representation.
    /// A tag is a tagges segment which starts with a special tag start pragma, when it is found in tag body
    /// it gets excised and the rest of tag contet ineterpeted as Laconic config vector
    /// </summary>
    /// <example>
    ///  &lt;p&gt; Hello dear &lt;@ path="/customer/name"&gt;    &lt;/p&gt;
    /// </example>
    public readonly struct Tag
    {
      internal Tag(Segment segment)
      {
        Segment = segment;
        Data = null;
      }

      internal Tag(Segment segment, IConfigSectionNode data)
      {
        Aver.IsTrue(segment.IsTag);
        Segment = segment;
        Data = data;
      }

      public readonly Segment Segment;
      public readonly IConfigSectionNode Data;
    }

    public static IEnumerable<Tag> ParseTags(this IEnumerable<Segment> source, string tagPragma, params KeyValuePair<string, string>[] escapes)
    {
      if (source == null) yield break;

      tagPragma.NonBlank(nameof(tagPragma));

      foreach(var one in source)
      {
        if (!one.IsTag)
        {
          yield return new Tag(one);
          continue;
        }

        if (!one.Content.StartsWith(tagPragma))
        {
          yield return new Tag(one);
          continue;
        }

        var content = one.Content;
        content = content.Substring(tagPragma.Length);

        foreach(var escape in escapes.Where(e => e.Key.IsNotNullOrWhiteSpace() && e.Value.IsNotNullOrWhiteSpace()))
         content = content.Replace(escape.Key, escape.Value);

        var data = content.AsLaconicConfig(null, "r", ConvertErrorHandling.Throw);
        yield return new Tag(one, data);
      }
    }

    /// <summary>
    /// Expands HTML content by invoking a function to build tag content into a string builder
    /// </summary>
    public static StringBuilder ExpandHtmlTags(this IEnumerable<char> source, Action<Tag, StringBuilder> fTagExpander, string tagPragma = "@")
    {
      fTagExpander.NonNull(nameof(fTagExpander));
      var spans = source.ParseSegments('<', '>');
      var tags = spans.ParseTags(tagPragma, new KeyValuePair<string, string>("&lt;", "<"), new KeyValuePair<string, string>("&gt;", ">"), new KeyValuePair<string, string>("&amp;", "&"));

      var result = new StringBuilder(1024);
      foreach (var tag in tags)
      {
        if (tag.Segment.IsTag)
          fTagExpander(tag, result);
        else
          result.Append($"<{tag.Segment.Content}>");
      }

      return result;
    }

  }
}
