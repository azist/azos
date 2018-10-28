/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Data;
using Azos.IO.FileSystem;
using Azos.IO.FileSystem.SVN;
using Azos.Scripting;
using FS = Azos.IO.FileSystem.FileSystem;

namespace Azos.Tests.Integration.IO.FileSystem.SVN
{
  [Runnable]
  public class SVNFileSystemTest: ExternalCfg, IRunnableHook
  {
    private static SVNFileSystemSessionConnectParams CONN_PARAMS, CONN_PARAMS_TIMEOUT;

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      CONN_PARAMS = FileSystemSessionConnectParams.Make<SVNFileSystemSessionConnectParams>(
        "svn{{ name='aaa' server-url='{0}' user-name='{1}' user-password='{2}' }}".Args(SVN_ROOT, SVN_UNAME, SVN_UPSW));

      CONN_PARAMS_TIMEOUT = FileSystemSessionConnectParams.Make<SVNFileSystemSessionConnectParams>(
        "svn{{ name='aaa' server-url='{0}' user-name='{1}' user-password='{2}' timeout-ms=1 }}".Args(SVN_ROOT, SVN_UNAME, SVN_UPSW));
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error) { return false; }

    [Run]
    public void NavigateRootDir()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        using(var fs = new SVNFileSystem("Azos-SVN"))
        {
          var session = fs.StartSession(CONN_PARAMS);

          var dir = session[string.Empty] as FileSystemDirectory;

          Aver.IsNotNull(dir);
          Aver.AreEqual("/", dir.Path);
        }
      }
    }

    [Run]
    public void Size()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["Azos-SVN"];

        using(var session = fs.StartSession())
        {
          var dir = session["trunk/Source/Testing/NUnit/Azos.Tests.Integration/IO/FileSystem/SVN/Esc Folder+"] as FileSystemDirectory;

          var file1 = session["trunk/Source/Testing/NUnit/Azos.Tests.Integration/IO/FileSystem/SVN/Esc Folder+/Escape.txt"] as FileSystemFile;
          var file2 = session["trunk/Source/Testing/NUnit/Azos.Tests.Integration/IO/FileSystem/SVN/Esc Folder+/NestedFolder/Escape.txt"] as FileSystemFile;

          Aver.AreEqual(19UL, file1.Size);
          Aver.AreEqual(11UL, file2.Size);

          Aver.AreEqual(30UL, dir.Size);
        }
      }
    }

    [Run]
    public void NavigateChildDir()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["Azos-SVN"];

        using(var session = fs.StartSession())
        {
          {
            var dir = session["trunk"] as FileSystemDirectory;

            Aver.IsNotNull(dir);
            Aver.AreEqual("/trunk", dir.Path);
            Aver.AreEqual("/", dir.ParentPath);
          }

          {
            var dir = session["trunk/Source"] as FileSystemDirectory;

            Aver.IsNotNull(dir);
            Aver.AreEqual("/trunk/Source", dir.Path);
            Aver.AreEqual("/trunk", dir.ParentPath);
          }
        }
      }
    }

    [Run]
    public void NavigateChildFile()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["Azos-SVN"];
        using (var session = fs.StartSession())
        {
          var file = session["/trunk/Source/Azos/LICENSE.txt"] as FileSystemFile;

          Aver.IsNotNull(file);
          Aver.AreEqual("/trunk/Source/Azos/LICENSE.txt", file.Path);
        }
      }
    }

    [Run]
    public void GetFileContent()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["Azos-SVN"];
        using (var session = fs.StartSession())
        {
          var file = session["/trunk/Source/Azos/LICENSE.txt"] as FileSystemFile;

          Aver.IsTrue(file.ReadAllText().IsNotNullOrEmpty());
        }
      }
    }

    [Run]
    public void GetVersions()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["Azos-SVN"];
        using (var session = fs.StartSession())
        {
          var currentVersion = session.LatestVersion;

          var preVersions = session.GetVersions(currentVersion, 5);

          Aver.AreEqual(5, preVersions.Count());
          Aver.AreEqual(preVersions.Last().Name.AsInt() + 1, currentVersion.Name.AsInt());
        }
      }
    }

    [Run]
    public void GetVersionedFiles()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        IList<WebDAV.Version> versions = WebDAV.GetVersions(SVN_ROOT, SVN_UNAME, SVN_UPSW).ToList();

        WebDAV.Version v192 = versions.First(v => v.Name == "192");
        WebDAV.Version v110 = versions.First(v => v.Name == "110");

        var fs = FS.Instances["Azos-SVN"];
        using (var session = fs.StartSession())
        {
          session.Version = v192;
          var file192 = session["trunk/Source/Azos.Wave/Templatization/StockContent/Embedded/script/wv.js"] as FileSystemFile;
          string content1530 = file192.ReadAllText();

          session.Version = v110;
          var file110 = session["trunk/Source/Azos.Wave/Templatization/StockContent/Embedded/script/wv.js"] as FileSystemFile;
          string content1531 = file110.ReadAllText();

          Aver.AreNotEqual(content1530, content1531);
        }
      }
    }

    [Run]
    [Aver.Throws(typeof(System.Net.WebException), Message = "timed out", MsgMatch = Aver.ThrowsAttribute.MatchType.Contains)]
    public void FailedFastTimeout()
    {
      using(new ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.Instances["Azos-SVN"];
        using (var session = fs.StartSession(CONN_PARAMS_TIMEOUT))
        {
          var dir = session[string.Empty] as FileSystemDirectory;
        }
      }
    }
  }
}
