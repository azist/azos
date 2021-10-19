/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Parses the forest tree path into list of normalized segment names (trimmed and lower-case-converted).
  /// This is a utility class mostly used by servers parsing <see cref="Data.EntityId.Address"/>.
  /// The `/` character delimits the segments. There may be maximum of PATH_SEGMENT_MAX_COUNT(0xff) segments, each may not be longer than SEGMENT_MAX_LEN(64).
  /// You can escape characters using `%xx` syntax, for example you can create a segment `a/b` like so `a%2fb` where `%2f` is ASCII for forward slash.
  /// The `%` can be escaped as `%25`.
  /// </summary>
  public sealed class TreePath : List<string>
  {
    [ThreadStatic] private static StringBuilder ts_Buffer;

    /// <summary>
    /// True when this path is the very root path and has zero segments
    /// </summary>
    public bool IsRoot => Count == 0;


    /// <summary>
    /// Creates the path object which is a list of segments
    /// </summary>
    public TreePath(string path) : base(Constraints.PATH_SEGMENT_MAX_COUNT)
    {
      path.NonBlank(nameof(path));

      var buf = ts_Buffer;
      if (buf == null)
      {
        buf = new StringBuilder(Constraints.SEGMENT_MAX_LEN);
        ts_Buffer = buf;//cache
      }

      buf.Clear();
      var len = path.Length;
      for(var i=0; i < len; i++)
      {
        var c = Char.ToLowerInvariant(path[i]);

        if (c == '/')//flush
        {
          var line = buf.ToString().Trim();

          if (line.IsNotNullOrWhiteSpace())
          {
            this.Add(line);
            if (Count == Constraints.PATH_SEGMENT_MAX_COUNT) throw new ConfigException(StringConsts.CONFIG_FOREST_MAX_SEGMENT_COUNT_ERROR.Args(Constraints.PATH_SEGMENT_MAX_COUNT));
          }

          buf.Clear();
          continue;
        }

        if (c == '%')
        {
          i += 2;
          if (i >= len) throw new ConfigException(StringConsts.CONFIG_FOREST_PATH_ESCAPE_ERROR.Args("<eol>"));

          var hex = path.Substring(i - 1, 2);
          if (!byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var ascii))
            throw new ConfigException(StringConsts.CONFIG_FOREST_PATH_ESCAPE_ERROR.Args(hex));

          c = (char)ascii;
        }

        buf.Append(c);
        if (buf.Length > Constraints.SEGMENT_MAX_LEN) throw new ConfigException(StringConsts.CONFIG_FOREST_MAX_SEGMENT_LEN_ERROR.Args(Constraints.SEGMENT_MAX_LEN));
      }

      //tail
      if (buf.Length > 0)
      {
        var line = buf.ToString().Trim();
        if (line.IsNotNullOrWhiteSpace()) this.Add(line);
        buf.Clear();
      }
    }


  }
}
