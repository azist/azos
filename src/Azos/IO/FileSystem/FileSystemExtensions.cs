/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos.IO.FileSystem
{
  /// <summary>
  ///File System extensions
  /// </summary>
  public static class FileSystemExtensions
  {
    /// <summary>
    /// Ensures that all directories of a fullPath exist creating them as needed.
    /// Returns false if fullPath did not supply valid path
    /// </summary>
    public static bool EnsureDirectoryPath(this FileSystemSession session, string fullPath)
    {
      var segments = session.NonNull(nameof(session)).FileSystem.SplitPathSegments(fullPath);

      if (segments == null || segments.Length == 0) return false;

      //set to root
      var dir = session[session.FileSystem.GetPathRoot(fullPath)] as FileSystemDirectory;
      if (dir == null) return false;

      var toClean = new List<FileSystemSessionItem>{ dir };

      try
      {
        foreach (var seg in segments)
        {
          var sub = dir.GetSubDirectory(seg);

          if (sub == null)
            sub = dir.CreateDirectory(seg);

          toClean.Add(sub);
          dir = sub;
        }
      }
      finally
      {
        toClean.ForEach( i => i.Dispose());
      }

      return true;
    }

    /// <summary>
    /// Extracts file path and file name from a full file path string
    /// </summary>
    public static (string path, string fn) ExtractFilePathAndName(this IFileSystem fileSystem, string fullPath)
    {
      var segs = fileSystem.NonNull(nameof(fileSystem))
                           .SplitPathSegments(fullPath);

      var root = fileSystem.GetPathRoot(fullPath) ?? string.Empty;

      if (segs == null || segs.Length == 0) return (string.Empty, string.Empty);
      if (segs.Length == 1) return (string.Empty, segs[0]);
      if (segs.Length == 2) return (segs[0], segs[1]);

      var path = root + fileSystem.CombinePaths(segs[0], segs.Skip(1).Take(segs.Length - 2).ToArray());
      var fn = segs[segs.Length - 1];
      return (path, fn);
    }

    /// <summary>
    /// Opens existing or creates a new file
    /// </summary>
    public static FileSystemFile OpenOrCreateFile(this FileSystemSession session, string fullPath)
    {
      var (path, fileName) = session.NonNull(nameof(session))
                        .FileSystem
                        .ExtractFilePathAndName(fullPath.NonBlank(nameof(fullPath)));

      var dir = session[path] as FileSystemDirectory;
      if (dir == null)
        throw new FileSystemException(StringConsts.FS_DIRECTORY_DOES_NOT_EXIST_ERROR.Args(path.TakeLastChars(32, "...")));

      var file = dir[fileName] as FileSystemFile;

      if (file == null)
      {
        file = dir.CreateFile(fileName);
      }

      return file;
    }
  }
}
