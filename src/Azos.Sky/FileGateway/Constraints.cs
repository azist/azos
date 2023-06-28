/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Azos.Data;

namespace Azos.Sky.FileGateway
{
  public static class Constraints
  {
    public const int MAX_PATH_SEGS = 8;
    public const int MAX_PATH_SEG_LEN = 24;
    public const int MAX_PATH_TOTAL_LEN = 150;

    public static EntityId SanitizePath(EntityId path, bool isFile)
    {
      path.HasRequiredValue(nameof(path));
      path.Type.HasRequiredValue(nameof(path.Type));
      var addr = SanitizePath(path.Address, isFile);
      return new EntityId(path.System, path.Type, Atom.ZERO, addr);
    }

    public static string SanitizePath(string path, bool isFile)
    {
      path.NonBlankMax(MAX_PATH_TOTAL_LEN, nameof(path));

      var segs = path.Split('/', '\\');

      var result = new StringBuilder();
      var cnt = 0;
      string last = null;
      foreach(var seg in segs)
      {
        if (seg.IsNullOrWhiteSpace()) continue;
        (cnt++ < MAX_PATH_SEGS).IsTrue($"Total seg count < {MAX_PATH_TOTAL_LEN}");
        var one = seg.Trim();
        one.NonBlankMax(MAX_PATH_SEG_LEN, "path segment");

      //https://stackoverflow.com/questions/1976007/what-characters-are-forbidden-in-windows-and-linux-directory-names
        seg.IsValidWindowsOrNixPathSegment().IsTrue("Valid path seg");

        if (result.Length > 0) result.Append('/');

        result.Append(one);
        last = one;
      }

      (result.Length > 0).IsTrue("Non empty path");
      (result.Length < MAX_PATH_TOTAL_LEN).IsTrue($"Total len < {MAX_PATH_TOTAL_LEN}");

      if (isFile)
      {
        last.IsValidWindowsOrNixFileName().IsTrue("Valid file name");
      }

      return result.ToString();
    }
  }
}
