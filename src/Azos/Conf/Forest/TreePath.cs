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
    public const int PATH_SEGMENT_MAX_COUNT = 0xff;
    public const int SEGMENT_MAX_LEN = 64;

    [ThreadStatic] private static StringBuilder ts_Buffer;

    /// <summary>
    /// Creates the path object which is a list of segments
    /// </summary>
    public TreePath(string path)
    {
      path.NonBlank(nameof(path));

      var buf = ts_Buffer;
      if (buf == null)
      {
        buf = new StringBuilder(SEGMENT_MAX_LEN, SEGMENT_MAX_LEN);
        ts_Buffer = buf;//cache
      }

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
            if (Count == PATH_SEGMENT_MAX_COUNT) throw new ConfigException("Maximum path segment count of {0} is exceeded".Args(PATH_SEGMENT_MAX_COUNT));
          }

          buf.Clear();
          continue;
        }

        if (c == '%')
        {
          i += 3;
          if (i > len) throw new ConfigException("Invalid escape sequence in forest path");

          var hex = path.Substring(i - 2, 2);

          if (!byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var ascii))
            throw new ConfigException("Invalid escape sequence in forest path");

          c = (char)ascii;
        }

        buf.Append(c);
        if (buf.Length > SEGMENT_MAX_LEN) throw new ConfigException("Maximum path segment length of {0} is exceeded".Args(SEGMENT_MAX_LEN));
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
