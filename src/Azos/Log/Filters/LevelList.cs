using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Log.Filters
{
  /// <summary>
  /// Represents a list of log message ranges
  /// </summary>
  public sealed class LevelsList : List<(MessageType from, MessageType to)>
  {
    /// <summary>
    /// Parses levels into a tuple list of level ranges
    /// </summary>
    /// <param name="levels">String representation of levels using ',' or ';' or '|'
    /// as range group delimiters, and '-' as range indicators.  If first/second bound of the range
    /// is empty, the min/max value of that bound is assumed.
    /// Examples: "Debug-DebugZ | Error", "-DebugZ | Info | Warning", "Info-", "DebugB-DebugC, Error"</param>
    public static LevelsList Parse(string levels)
    {
      var result = new LevelsList();

      if (!string.IsNullOrWhiteSpace(levels))
        foreach (var p in levels.Split(',', ';', '|'))
        {
          var minmax = p.Split(new char[] { '-' }, 2).Select(s => s.Trim()).ToArray();

          if (minmax.Length == 0)
            throw new LogException(StringConsts.ARGUMENT_ERROR + "levels: " + p);

          MessageType min, max;

          if (string.IsNullOrWhiteSpace(minmax[0]))
            min = MessageType.Debug;
          else if (!Enum.TryParse(minmax[0], true, out min))
            throw new LogException(StringConsts.ARGUMENT_ERROR +
                "levels: {0} (error parsing: {1})".Args(p, minmax[0]));

          if (minmax.Length < 2)
            max = min;
          else if (string.IsNullOrWhiteSpace(minmax[1]))
            max = MessageType.CatastrophicError;
          else if (!Enum.TryParse(minmax[1], true, out max))
            throw new LogException(StringConsts.ARGUMENT_ERROR +
                "levels: {0} (error parsing: {1})".Args(p, minmax[1]));

          result.Add((min, max));
        }

      return result;
    }

  }
}
