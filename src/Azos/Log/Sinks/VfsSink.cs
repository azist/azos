/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;

using Azos.Conf;
using Azos.IO.FileSystem;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Implements base for sinks which write into Virtual File System
  /// </summary>
  public abstract class VfsSink : FileSink
  {
    public const string CONFIG_CONTENT_FS_SECTION = "file-system";
    public const string CONFIG_FS_CONNECT_PARAMS_SECTION = "connect-params";

    protected VfsSink(ISinkOwner owner) : base(owner) { }
    protected VfsSink(ISinkOwner owner, string name, int order) : base(owner, name, order) { }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Fs);
      DisposeIfDisposableAndNull(ref m_FsConnectParams);
    }

    private IFileSystemImplementation m_Fs;
    private FileSystemSessionConnectParams m_FsConnectParams;
    private FileSystemSession m_Session;//not thread safe

    private FileSystemFile m_File;//not thread safe

    public override int ExpectedShutdownDurationMs => 3570;//since there is VFS involved

    protected override void CheckPath(string path)
     => m_Session.EnsureDirectoryPath(path);

    protected sealed override string CombinePaths(string p1, string p2)
     => m_Fs.CombinePaths(p1, p2);

    protected override Stream MakeStream(string fileName)
    {
       DisposeAndNull(ref m_File);
       m_File = m_Session.OpenOrCreateFile(fileName);
       return m_File.FileStream;
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node == null || !node.Exists) return;

      //Make File System
      var fsNode = node[CONFIG_CONTENT_FS_SECTION];

      DisposeAndNull(ref m_Session);
      DisposeAndNull(ref m_Fs);

      m_Fs = FactoryUtils.MakeAndConfigureComponent<FileSystem>(App, fsNode, typeof(IO.FileSystem.Local.LocalFileSystem));

      var pnode = fsNode[CONFIG_FS_CONNECT_PARAMS_SECTION];

      DisposeIfDisposableAndNull(ref m_FsConnectParams);

      if (pnode.Exists)
        m_FsConnectParams = FileSystemSessionConnectParams.Make<FileSystemSessionConnectParams>(pnode);
      else
        m_FsConnectParams = new FileSystemSessionConnectParams() { User = Security.User.Fake };


      m_Session = m_Fs.StartSession(m_FsConnectParams);
    }

    protected override void DoStart()
    {
      m_Session.NonNull("Not configured");
      base.DoStart();
    }

  }
}
