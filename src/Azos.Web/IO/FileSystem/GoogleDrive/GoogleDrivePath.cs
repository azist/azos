/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

namespace Azos.IO.FileSystem.GoogleDrive
{
  /// <summary>
  /// Methods for working with paths
  /// </summary>
  static class GoogleDrivePath
  {
    private static char PATH_SEPARATOR = '/';
    private static char[] PATH_SEPARATORS = new[] { PATH_SEPARATOR };
    private static char[] TRIMMERS = new[] { PATH_SEPARATOR, ' ', '\t', '\r', '\n' };

    public static string[] Split(string path)
    {
      return path.Split(PATH_SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string GetParentPath(string path)
    {
      if (path.IsNullOrWhiteSpace())
      {
        return null;
      }

      path = path.Trim(TRIMMERS);

      var segments = Split(path);

      if (segments.Length < 2)
      {
        return null;
      }

      var parentBuilder = new StringBuilder();

      for (int i = 0; i < segments.Length - 1; i++)
      {
        if (i > 0)
        {
          parentBuilder.Append(PATH_SEPARATOR);
        }

        parentBuilder.Append(segments[i]);
      }

      return parentBuilder.ToString();
    }
  }
}
