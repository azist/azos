/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azos.Scripting;
using Azos.IO.FileSystem;
using Azos.IO.FileSystem.GoogleDrive;
using Azos.IO.FileSystem.GoogleDrive.V2;

namespace Azos.Tests.Integration.IO.FileSystem.GoogleDrive.V2
{
  using FS = Azos.IO.FileSystem;
  using System.Collections.Generic;
  using System;

  [Runnable]
  class GoogleDriveTests : ExternalCfg, IRunnableHook
  {
    private const string ROOT = "/nfx-root";
    private const string ROOT_ID = "root";
    private const string LOCAL_ROOT = @"c:\Azos";
    private static string FILE2_FULLPATH = Path.Combine(LOCAL_ROOT, FILE2);

    private const string DIR_1 = "dir1";
    private const string FILE1 = "nfxtest01.txt";
    private const string FILE2 = "nfxtest02.txt";
    private const string FILE3 = "nfxtest03.txt";
    private const string FILE4 = "nfxtest04.txt";

    private const int PARALLEL_FROM = 0;
    private const int PARALLEL_TO = 10;
    private const string FN_PARALLEL_MASK = @"parallel_fstest_{0}";

    private static GoogleDriveParameters CONN_PARAMS, CONN_PARAMS_TIMEOUT;

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      var csDefault = "google-drive{{ email='{0}' cert-path=$'{1}' }}"
        .Args(GOOGLE_DRIVE_EMAIL, GOOGLE_DRIVE_CERT_PATH);

      CONN_PARAMS = FileSystemSessionConnectParams.Make<GoogleDriveParameters>(csDefault);

      var csTimeout = "google-drive{{ email='{0}' cert-path=$'{1}' timeout-ms=1 }}"
        .Args(GOOGLE_DRIVE_EMAIL, GOOGLE_DRIVE_CERT_PATH);

      CONN_PARAMS_TIMEOUT = FileSystemSessionConnectParams.Make<GoogleDriveParameters>(csTimeout);

      cleanUp();
      initialize();
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      cleanUp();
      return false;
    }

    [Run]
    public void GoogleDrive_NavigateToRoot()
    {
      using(CreateApplication())
      {
        using (var fs = new GoogleDriveFileSystem("GoogleDrive"))
        {
          using(var session = StartSession(fs))
          {
            var dir = session[ROOT] as FileSystemDirectory;

            Aver.AreEqual("/", dir.ParentPath);
            Aver.AreEqual(ROOT, dir.Path);
          }
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateFile()
    {
      using(CreateApplication())
      {
        using(var session = StartSession())
        {
          var dir = session[ROOT] as FileSystemDirectory;

          var file = dir.CreateFile(FILE1, 1500);

          Aver.AreEqual(FILE1, file.Name);

          Aver.IsTrue(session.Client.FileExists(file.Path));
          Aver.AreEqual(1500UL, file.Size);
          Aver.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateFileAsync()
    {
      using(CreateApplication())
      {
        using(var session = StartSession())
        {
          var task = session.GetItemAsync(ROOT);

          var dir = task.Result as FileSystemDirectory;

          var task1 = dir.CreateFileAsync(FILE1, 1500);

          var file = task1.Result;

          Aver.AreEqual(FILE1, file.Name);

          var client = GetClient(session);

          Aver.IsTrue(client.FileExists(file.Path));
          Aver.AreEqual(1500UL, file.Size);
          Aver.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateDeleteFile()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;
          var file = dir.CreateFile(FILE1, 1500);

          Aver.AreEqual(FILE1, file.Name);

          var fileId = (file.Handle as GoogleDriveHandle).Id;

          var item = session.Client.GetItemInfoById(fileId);
          Aver.AreEqual(1500UL, item.Size);
          Aver.AreEqual(1500, file.FileStream.Length);

          var file2 = session[fs.CombinePaths(ROOT, FILE1)];
          Aver.IsNotNull(file2);
          Aver.IsTrue(file2 is FileSystemFile);
          Aver.AreEqual(FILE1, file2.Name);

          file.Dispose();

          file2.Delete();
          Aver.IsFalse(session.Client.FileExists(file2.Path));
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateWriteCopyReadFile()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var stream = new FileStream(FILE2_FULLPATH, FileMode.Create, FileAccess.Write))
          {
            var writer = new BinaryWriter(stream);

            writer.Write("Hello!");
            writer.Write(true);
            writer.Write(27.4d);
            writer.Close();
          }

          // FILE3 copied from FILE2 and made readonly.
          Aver.IsNotNull(dir.CreateFile(FILE3, Path.Combine(LOCAL_ROOT, FILE2), true));

          using (var file = session[fs.CombinePaths(ROOT, FILE3)] as FileSystemFile)
          {
            var str = file.FileStream;

            var reader = new BinaryReader(str);

            Aver.AreEqual("Hello!", reader.ReadString());
            Aver.AreEqual(true, reader.ReadBoolean());
            Aver.AreEqual(27.4d, reader.ReadDouble());
          }
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateWriteReadFile_1()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var file = dir.CreateFile(FILE2))
          {
            var str = file.FileStream;

            var writer = new BinaryWriter(str);

            writer.Write("Hello!");
            writer.Write(true);
            writer.Write(27.4d);
            writer.Close();
          }

          using (var file = session[fs.CombinePaths(ROOT, FILE2)] as FileSystemFile)
          {
            var str = file.FileStream;

            var reader = new BinaryReader(str);

            Aver.AreEqual("Hello!", reader.ReadString());
            Aver.AreEqual(true, reader.ReadBoolean());
            Aver.AreEqual(27.4d, reader.ReadDouble());
          }
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateWriteReadFile_2()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var file = dir.CreateFile(FILE4))
          {
            file.WriteAllText("This is what it takes!");
          }

          using (var file = session[fs.CombinePaths(ROOT, FILE4)] as FileSystemFile)
          {
            Aver.AreEqual("This is what it takes!", file.ReadAllText());
          }
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateWriteReadFile_3()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          using (var file = dir.CreateFile(FILE4))
          {
            file.WriteAllText("Hello,");
            file.WriteAllText("this will overwrite");
          }

          using (var file = session[fs.CombinePaths(ROOT, FILE4)] as FileSystemFile)
          {
            Aver.AreEqual("this will overwrite", file.ReadAllText());
          }
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateWriteReadFile_3_Async()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dirTask = session.GetItemAsync(ROOT);
          dirTask.ContinueWith(t => {

            var dir = t.Result as FileSystemDirectory;

            var createFileTask = dir.CreateFileAsync(FILE4);
            createFileTask.ContinueWith(t1 => {

              using (var file = t1.Result)
              {
                var writeTask1 = file.WriteAllTextAsync("Hello,");
                writeTask1.Wait();
                var writeTask2 = file.WriteAllTextAsync("this will overwrite");
                writeTask2.ContinueWith(t2 => {

                  var readTask = GetFileText(session, fs.CombinePaths(ROOT, FILE4));

                  Aver.AreEqual("this will overwrite", readTask.Result);

                });
              }

            });

          });
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateDeleteDir()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Aver.IsNull(dir2);

          dir2 = dir.CreateDirectory(DIR_1);

          Aver.AreEqual(DIR_1, ((FileSystemDirectory)dir[DIR_1]).Name);

          Aver.AreEqual(DIR_1, ((FileSystemDirectory)dir2[""]).Name);

          Aver.IsTrue(session.Client.FolderExists(dir2.Path));

          Aver.AreEqual(DIR_1, dir2.Name);

          dir2.Delete();
          Aver.IsFalse(session.Client.FolderExists(dir2.Path));
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateDirCreateFileDeleteDir()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;
          Aver.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor
          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Aver.IsNull(dir2);
          Aver.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor
          dir2 = dir.CreateDirectory(DIR_1);
          Aver.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
          Aver.IsTrue(session.Client.FolderExists(dir2.Path));

          Aver.AreEqual(DIR_1, dir2.Name);

          var f = dir2.CreateFile(FILE1, 237);
          Aver.AreEqual(3, session.Items.Count());//checking item registation via .ctor/.dctor
          Aver.IsTrue(session.Client.FileExists(f.Path));

          Aver.AreEqual(237UL, dir2.Size);

          Aver.IsTrue(dir.SubDirectoryNames.Contains(DIR_1));

          Aver.IsTrue(dir2.FileNames.Contains(FILE1));

          dir2.Delete();
          Aver.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
          Aver.IsFalse(session.Client.FolderExists(dir2.Path));
          Aver.IsFalse(session.Client.FileExists(f.Path));
          Aver.AreEqual(1, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          session.Dispose();
          Aver.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          Aver.AreEqual(0, session.Items.Count());//checking item registation via .ctor/.dctor
        }
      }
    }

    [Run]
    public void GoogleDrive_CreateRefreshDeleteDirAsync()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          var task = dir.CreateDirectoryAsync(DIR_1).ContinueWith(t => {
            var dir1 = t.Result;
            dir1.RefreshAsync().Wait();
            dir1.DeleteAsync().Wait();
          });

          task.Wait();
        }
      }
    }

    [Run]
    public void GoogleDrive_Parallel_CreateWriteReadFile()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);
            using(var session = StartSession(fs))
            {
              var dir = session[ROOT] as FileSystemDirectory;

              using (var file = dir.CreateFile(fn))
              {
                file.WriteAllText("Hello, {0}".Args(i));
              }

              using (var file = session[fs.CombinePaths(ROOT, fn)] as FileSystemFile)
              {
                Aver.AreEqual("Hello, {0}".Args(i), file.ReadAllText());
                file.Delete();
              }
              Aver.IsNull(session[fs.CombinePaths(ROOT, fn)]);
            }

          });//Parallel.For
      }
    }

    [Run]
    public void GoogleDrive_Parallel_CreateWriteReadFile_Async()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        var tasks = new List<Task>();

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);

            var session = StartSession(fs);

            FileSystemDirectory dir = null;
            FileSystemFile file = null;

            var t = session.GetItemAsync(ROOT)
            .OnOk(item =>
            {
              dir = item as FileSystemDirectory;
              return dir.CreateFileAsync(fn);

            }).OnOk(f => {
              Console.WriteLine("file '{0}' created", f.Name);
              file = f;
              return file.WriteAllTextAsync("Hello, {0}".Args(i));
            })
            .OnOkOrError(_ => {
              Console.WriteLine("text written into '{0}'", file.Name);
              if (file != null && !file.Disposed) {
                  file.Dispose();
                  Console.WriteLine("file '{0}' disposed", file.Name);
              }
            })
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(ROOT, fn)) )
            .OnOk(item => {
              file = item as FileSystemFile;
              Console.WriteLine("file {0} got", file.Name);
              return file.ReadAllTextAsync();
            })
            .OnOk(txt => {
              Console.WriteLine("file '{0}' red {1}", file.Name, txt);
              Aver.AreEqual("Hello, {0}".Args(i), txt);
              return file.DeleteAsync();
            })
            .OnOkOrError(_ => {
              Console.WriteLine("file '{0}' deleted", file.Name);
              if (file != null && !file.Disposed) {
                file.Dispose();
                Console.WriteLine("file '{0}' disposed", file.Name);
              }
             })
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(ROOT, fn)) )
            .OnOk(item => { Aver.IsNull(item); })
            .OnOkOrError(_ => { if (!session.Disposed) session.Dispose(); } );

            tasks.Add(t);
          });//Parallel.For

          Console.WriteLine("all tasks created");

          Task.WaitAll(tasks.ToArray());

          Aver.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor

          Console.WriteLine("done");
      }
    }

    [Run]
    [Aver.Throws(typeof(FileSystemException), MsgMatch = Aver.ThrowsAttribute.MatchType.Contains)]
    public void GoogleDrive_FailedFastTimeout()
    {
      using(CreateApplication())
      {
        var fs = GetFileSystem();

        using(var session = StartSession(fs, CONN_PARAMS_TIMEOUT))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          Aver.AreEqual("/", dir.ParentPath);
          Aver.AreEqual(ROOT, dir.Path);
        }
      }
    }

    private async Task<string> GetFileText(FileSystemSession session, string path)
    {
      using (var file = await session.GetItemAsync(path) as FileSystemFile)
      {
        return await file.ReadAllTextAsync();
      }
    }

    private Apps.ServiceBaseApplication CreateApplication()
    {
      return new Apps.ServiceBaseApplication(null, LACONF.AsLaconicConfig());
    }

    private GoogleDriveFileSystem GetFileSystem()
    {
      return FS.FileSystem.Instances[NFX_GOOGLE_DRIVE] as GoogleDriveFileSystem;
    }

    private GoogleDriveSession StartSession(GoogleDriveFileSystem fs = null, GoogleDriveParameters connParams = null)
    {
      if (fs == null)
      {
        fs = GetFileSystem();
      }

      return fs.StartSession(connParams ?? CONN_PARAMS) as GoogleDriveSession;
    }

    private void cleanUp()
    {
      using(CreateApplication())
      {
        var fs = FS.FileSystem.Instances[NFX_GOOGLE_DRIVE];

        using(var session = fs.StartSession(CONN_PARAMS))
        {
          var dir = session[ROOT] as FileSystemDirectory;

          if (dir != null)
          {
            dir.Delete();
          }
        }
      }
    }

    private void initialize()
    {
      var client = new GoogleDriveClient(GOOGLE_DRIVE_EMAIL, GOOGLE_DRIVE_CERT_PATH);

      client.CreateDirectory(ROOT_ID, ROOT.Trim('/'));
    }

    private GoogleDriveClient GetClient(FileSystemSession session)
    {
      return (session as GoogleDriveSession).Client;
    }
  }
}
