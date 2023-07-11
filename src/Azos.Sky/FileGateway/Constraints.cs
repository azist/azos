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
  /// <summary>
  /// Imposes limits/constraints on file gateway related functionality like paths length etc..
  /// </summary>
  public static class Constraints
  {
    public const int MAX_PATH_SEGS = 8;
    public const int MAX_PATH_SEG_LEN = 24;
    public const int MAX_PATH_TOTAL_LEN = 150;

    public const int MAX_FILE_CHUNK_SIZE = 8 * 1024 * 1024;

    public static EntityId SanitizePath(EntityId path, bool isFile)
    {
      path.HasRequiredValue(nameof(path), putExternalDetails: true);
      path.Type.HasRequiredValue(nameof(path.Type), putExternalDetails: true);
      var addr = SanitizePath(path.Address, isFile);
      return new EntityId(path.System, path.Type, Atom.ZERO, addr);
    }

    public static string SanitizePath(string path, bool isFile)
    {
      path.NonBlankMax(MAX_PATH_TOTAL_LEN, nameof(path), putExternalDetails: true);

      var segs = path.Split('/', '\\');

      var result = new StringBuilder();
      var cnt = 0;
      string last = null;
      foreach(var seg in segs)
      {
        if (seg.IsNullOrWhiteSpace()) continue;
        (cnt++ < MAX_PATH_SEGS).IsTrue($"Total seg count < {MAX_PATH_SEGS}", putExternalDetails: true);
        var one = seg.Trim();
        one.NonBlankMax(MAX_PATH_SEG_LEN, "path segment", putExternalDetails: true);

      //https://stackoverflow.com/questions/1976007/what-characters-are-forbidden-in-windows-and-linux-directory-names
        seg.IsValidWindowsOrNixPathSegment().IsTrue("Valid path seg", putExternalDetails: true);

        if (result.Length > 0) result.Append('/');

        result.Append(one);
        last = one;
      }

      (result.Length < MAX_PATH_TOTAL_LEN).IsTrue($"Total len < {MAX_PATH_TOTAL_LEN}", putExternalDetails: true);

      if (isFile)
      {
        (result.Length > 0).IsTrue("Non empty file path", putExternalDetails: true);
        last.IsValidWindowsOrNixFileName().IsTrue("Valid file name", putExternalDetails: true);
      }

      return result.ToString();
    }
  }
}
