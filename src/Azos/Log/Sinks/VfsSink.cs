/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.IO.FileSystem;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Implements base for sinks which write into Virtual File System
  /// </summary>
  public abstract class VfsSink : FileSink
  {
    protected VfsSink(ISinkOwner owner) : base(owner) { }
    protected VfsSink(ISinkOwner owner, string name, int order) : base(owner, name, order) { }

    private IFileSystemImplementation m_Fs;
    private FileSystemSession m_Session;

    protected override void CheckPath(string path)
    {
      //check directory exists
      base.CheckPath(path);
    }

    protected sealed override string CombinePaths(string p1, string p2)
      => m_Fs.CombinePaths(p1, p2);

    protected override Stream MakeStream(string fileName)
      => (m_Session[fileName] as FileSystemFile).NonNull("no file").FileStream;
  }
}
