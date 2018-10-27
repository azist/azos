/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S3V4FSH = Azos.IO.FileSystem.S3.V4.S3V4FileSystem.S3V4FSH;

using Azos;
using Azos.Environment;
using Azos.IO.FileSystem;
using Azos.IO.FileSystem.S3.V4;
using Azos.Security;
using FS = Azos.IO.FileSystem;

using Azos.Scripting;

namespace Azos.Tests.Integration.IO.FileSystem.S3.V4
{
  [Runnable]
  public class S3V4FileSystemTests: ExternalCfg, IRunnableHook
  {
    private const string S3_ROOT_FS = "/nfx-root";
    private const string LOCAL_ROOT_FS = @"c:\Azos";

    private const string DIR_1 = "dir1";

    private const string FN1_FS = "nfxtest01.txt";
    private const string FN2_FS = "nfxtest02.txt";
    private const string FN3_FS = "nfxtest03.txt";
    private const string FN4_FS = "nfxtest04.txt";

    private static string FN2_FS_FULLPATH = Path.Combine(LOCAL_ROOT_FS, FN2_FS);

    private const string CONTENT2 = "The entity tag is a hash of the object. The ETag only reflects changes to the contents of an object, not its metadata. The ETag is determined when an object is created. For objects created by the PUT Object operation and the POST Object operation, the ETag is a quoted, 32-digit hexadecimal string representing the MD5 digest of the object data. For other objects, the ETag may or may not be an MD5 digest of the object data. If the ETag is not an MD5 digest of the object data, it will contain one or more non-hexadecimal characters and/or will consist of less than 32 or more than 32 hexadecimal digits.";
    private static byte[] CONTENT2_BYTES = Encoding.UTF8.GetBytes(CONTENT2);

    private const int PARALLEL_FROM = 0;
    private const int PARALLEL_TO = 10;
    private const string FN_PARALLEL_MASK = @"parallel_fstest_{0}";

    private static S3V4FileSystemSessionConnectParams CONN_PARAMS, CONN_PARAMS_TIMEOUT;

    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      CONN_PARAMS = FileSystemSessionConnectParams.Make<S3V4FileSystemSessionConnectParams>(
        "s3 {{ name='s3v4' bucket='{0}' region='{1}' access-key='{2}' secret-key='{3}' }}"
          .Args(S3_BUCKET, S3_REGION, S3_ACCESSKEY, S3_SECRETKEY));

      CONN_PARAMS_TIMEOUT = FileSystemSessionConnectParams.Make<S3V4FileSystemSessionConnectParams>(
        "s3 {{ name='s3v4' bucket='{0}' region='{1}' access-key='{2}' secret-key='{3}' timeout-ms=1 }}"
          .Args(S3_BUCKET, S3_REGION, S3_ACCESSKEY, S3_SECRETKEY));

      cleanUp();
      initialize();
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      cleanUp();
      return false;
    }

    [Run]
    public void CombinePaths()
    {
      using(var fs = new S3V4FileSystem("S3-1"))
      {
        Aver.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/a.txt", fs.CombinePaths("https://dxw.s3-us-west-2.amazonaws.com/", "a.txt"));
        Aver.AreEqual("https://dxw.s3-us-west-2.amazonaws.com/a.txt", fs.CombinePaths("https://dxw.s3-us-west-2.amazonaws.com/", "/a.txt"));


        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "books", "saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "/books", "saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx", "/books", "saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx", "/books", "/saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "books", "/saga.pdf"));


        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx/", "books", "saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx/", "/books", "saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx/", "/books", "saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx/", "/books", "/saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "/nfx/", "/books", "/saga.pdf"));

        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx/", "books", "/saga.pdf"));


        Aver.AreEqual("https://dxw.s3-us-west-1.amazonaws.com/nfx/books/saga.pdf",
        fs.CombinePaths("https://dxw.s3-us-west-1.amazonaws.com/", "nfx", "books", "/saga.pdf"));
      }
    }

    [Run]
    public void Connect_NavigateRootDir()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        using (var fs = new S3V4FileSystem("S3-1"))
        {
          using(var session = fs.StartSession(CONN_PARAMS))
          {
            var dir = session[S3_ROOT_FS] as FileSystemDirectory;

            Aver.AreEqual("/", dir.ParentPath);
            Aver.AreEqual(S3_ROOT_FS, dir.Path);
          }
        }
      }
    }

    [Run]
    public void CreateFile()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          var file = dir.CreateFile(FN1_FS, 1500);

          Aver.AreEqual(FN1_FS, file.Name);

          Aver.IsTrue(S3V4.FileExists(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0));
          Aver.AreEqual(1500, S3V4.GetItemMetadata(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0)["Content-Length"].AsLong());
          Aver.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [Run]
    public void CreateFileAsync()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var task = session.GetItemAsync(S3_ROOT_FS);
          //task.Start();
          var dir = task.Result as FileSystemDirectory;

          var task1 = dir.CreateFileAsync(FN1_FS, 1500);
          //task1.Start();
          var file = task1.Result;

          Aver.AreEqual(FN1_FS, file.Name);

          Aver.IsTrue(S3V4.FileExists(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0));
          Aver.AreEqual(1500, S3V4.GetItemMetadata(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0)["Content-Length"].AsLong());
          Aver.AreEqual(1500, file.FileStream.Length);
        }
      }
    }

    [Run]
    public void CreateDeleteFile()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;
          var file = dir.CreateFile(S3_FN1, 1500);

          Aver.AreEqual(S3_FN1, file.Name);

          IDictionary<string, string> headersFN1 = S3V4.GetItemMetadata(file.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0);
          Aver.AreEqual(1500, headersFN1["Content-Length"].AsInt());
          Aver.AreEqual(1500, file.FileStream.Length);

          Aver.AreEqual(1500UL, file.Size);

          var file2 = session[fs.CombinePaths(S3_ROOT_FS, S3_FN1)];
          Aver.IsNotNull(file2);
          Aver.IsTrue(file2 is FileSystemFile);
          Aver.AreEqual(S3_FN1, ((FileSystemFile)file2).Name);

          file.Dispose();

          file2.Delete();
          Aver.IsFalse(fileExists((FileSystemFile)file2));
        }
      }
    }

    [Run]
    public void CreateWriteCopyReadFile()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (Stream s = new FileStream(FN2_FS_FULLPATH, FileMode.Create, FileAccess.Write))
          {
            var wri = new BinaryWriter(s);

            wri.Write("Hello!");
            wri.Write(true);
            wri.Write(27.4d);
            wri.Close();
          }

          //FN3 copied from FN2 and made readonly

          Aver.IsNotNull(dir.CreateFile(FN3_FS, Path.Combine(LOCAL_ROOT_FS, FN2_FS), true));


          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN3_FS)] as FileSystemFile)
          {
            var str = file.FileStream;

            var rdr = new BinaryReader(str);

            Aver.AreEqual("Hello!", rdr.ReadString());
            Aver.AreEqual(true, rdr.ReadBoolean());
            Aver.AreEqual(27.4d, rdr.ReadDouble());
          }
        }
      }
    }

    [Run]
    public void CreateWriteReadFile_1()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (var file = dir.CreateFile(FN2_FS))
          {
            var str = file.FileStream;

            var wri = new BinaryWriter(str);

            wri.Write("Hello!");
            wri.Write(true);
            wri.Write(27.4d);
            wri.Close();
          }

          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN2_FS)] as FileSystemFile)
          {
            var str = file.FileStream;

            var rdr = new BinaryReader(str);

            Aver.AreEqual("Hello!", rdr.ReadString());
            Aver.AreEqual(true, rdr.ReadBoolean());
            Aver.AreEqual(27.4d, rdr.ReadDouble());
          }
        }
      }
    }

    [Run]
    public void CreateWriteReadFile_2()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (var file = dir.CreateFile(FN4_FS))
          {
            file.WriteAllText("This is what it takes!");
          }

          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN4_FS)] as FileSystemFile)
          {
            Aver.AreEqual("This is what it takes!", file.ReadAllText());
          }
        }
      }
    }

    [Run]
    public void CreateWriteReadFile_3()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          using (var file = dir.CreateFile(FN4_FS))
          {
            file.WriteAllText("Hello,");
            file.WriteAllText("this will overwrite");
          }

          using (var file = session[fs.CombinePaths(S3_ROOT_FS, FN4_FS)] as FileSystemFile)
          {
            Aver.AreEqual("this will overwrite", file.ReadAllText());
          }
        }
      }
    }

    [Run]
    public void CreateWriteReadFile_3_Async()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dirTask = session.GetItemAsync(S3_ROOT_FS);
          dirTask.ContinueWith(t => {

            var dir = t.Result as FileSystemDirectory;

            var createFileTask = dir.CreateFileAsync(FN4_FS);
            createFileTask.ContinueWith(t1 => {

              using (var file = t1.Result)
              {
                var writeTask1 = file.WriteAllTextAsync("Hello,");
                writeTask1.Wait();
                var writeTask2 = file.WriteAllTextAsync("this will overwrite");
                writeTask2.ContinueWith(t2 => {

                  var readTask = getFileText(session, fs.CombinePaths(S3_ROOT_FS, FN4_FS));

                  Aver.AreEqual("this will overwrite", readTask.Result);

                });
              }

            });

          });
        }
      }
    }

            private async Task<string> getFileText(FileSystemSession session, string path)
            {
              using (var file = await session.GetItemAsync(path) as FileSystemFile)
              {
                return await file.ReadAllTextAsync();
              }
            }

    [Run]
    public void CreateDeleteDir()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Aver.IsNull(dir2);

          dir2 = dir.CreateDirectory(DIR_1);

          Aver.AreEqual(DIR_1, ((FileSystemDirectory)dir[DIR_1]).Name);

          Aver.AreEqual(DIR_1, ((FileSystemDirectory)dir2[""]).Name);

          Aver.IsTrue(folderExists(dir2));

          Aver.AreEqual(DIR_1, dir2.Name);

          dir2.Delete();
          Aver.IsFalse(folderExists(dir2));
        }
      }
    }

    [Run]
    public void CreateDirCreateFileDeleteDir()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;
          Aver.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor
          var dir2 = dir[DIR_1] as FileSystemDirectory;
          Aver.IsNull(dir2);
          Aver.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor
          dir2 = dir.CreateDirectory(DIR_1);
          Aver.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
          Aver.IsTrue(folderExists(dir2));

          Aver.AreEqual(DIR_1, dir2.Name);

          var f = dir2.CreateFile(S3_FN1, 237);
          Aver.AreEqual(3, session.Items.Count());//checking item registation via .ctor/.dctor
          Aver.IsTrue(fileExists(f));

          Aver.AreEqual(237UL, dir2.Size);

          Aver.IsTrue(dir.SubDirectoryNames.Contains(DIR_1));

          Aver.IsTrue(dir2.FileNames.Contains(S3_FN1));


          dir2.Delete();
          Aver.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
          Aver.IsFalse(folderExists(dir2));
          Aver.IsFalse(fileExists(f));
          Aver.AreEqual(1, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          session.Dispose();
          Aver.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor
          Aver.AreEqual(0, session.Items.Count());//checking item registation via .ctor/.dctor
        }
      }
    }

    [Run]
    public void CreateRefreshDeleteDirAsync()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;
          dir.CreateDirectoryAsync(DIR_1).ContinueWith(t => {
            var dir1 = t.Result;
            dir1.RefreshAsync().Wait();
            dir1.DeleteAsync().Wait();
          });
        }
      }
    }

    [Run]
    public void Parallel_CreateWriteReadFile()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);
            using(var session = fs.StartSession())
            {
              var dir = session[S3_ROOT_FS] as FileSystemDirectory;

              using (var file = dir.CreateFile(fn))
              {
                file.WriteAllText("Hello, {0}".Args(i));
              }

              using (var file = session[fs.CombinePaths(S3_ROOT_FS, fn)] as FileSystemFile)
              {
                Aver.AreEqual("Hello, {0}".Args(i), file.ReadAllText());
                file.Delete();
              }
              Aver.IsNull(session[fs.CombinePaths(S3_ROOT_FS, fn)]);
            }

          });//Parallel.For
      }
    }

    [Run]
    public void Parallel_CreateWriteReadFile_Async()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        var tasks = new List<Task>();

        System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
          (i) =>
          {
            var fn = FN_PARALLEL_MASK.Args(i);

            var session = fs.StartSession();

            FileSystemDirectory dir = null;
            FileSystemFile file = null;

            var t = session.GetItemAsync(S3_ROOT_FS)
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
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(S3_ROOT_FS, fn)) )
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
            .OnOk(() => session.GetItemAsync(fs.CombinePaths(S3_ROOT_FS, fn)) )
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
    [Aver.Throws(typeof(System.Net.WebException), MsgMatch = Aver.ThrowsAttribute.MatchType.Contains)]
    public void FailedFastTimeout()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession(CONN_PARAMS_TIMEOUT))
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          Aver.AreEqual("/", dir.ParentPath);
          Aver.AreEqual(S3_ROOT_FS, dir.Path);
        }
      }
    }

    private void cleanUp()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        deleteFile(FN2_FS_FULLPATH);

        var fs = FS.FileSystem.Instances[NFX_S3];

        using(var session = fs.StartSession())
        {
          var dir = session[S3_ROOT_FS] as FileSystemDirectory;

          if (dir != null)
            dir.Delete();
        }
      }
    }

    private void initialize()
    {
      using(new Azos.ApplicationModel.ServiceBaseApplication(null, LACONF.AsLaconicConfig()))
      {
        using (Stream s = new FileStream(FN2_FS_FULLPATH, FileMode.Create, FileAccess.Write))
        {
          s.Write(CONTENT2_BYTES, 0, CONTENT2_BYTES.Length);
        }

        S3V4.PutFolder(S3_ROOT_FS, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0);
      }
    }

    private void deleteFile(string name)
    {
      try
      {
        var fi = new FileInfo(name);
        if (fi.IsReadOnly) fi.IsReadOnly = false;

        fi.Delete();
      }
      catch{}
    }

    private bool fileExists(FileSystemFile file)
    {
      var handle = (S3V4FSH)file.Handle;
      return S3V4.FileExists(handle.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0);
    }

    private bool folderExists(FileSystemDirectory folder)
    {
      var handle = (S3V4FSH)folder.Handle;
      return S3V4.FolderExists(handle.Path, S3_ACCESSKEY, S3_SECRETKEY, S3_BUCKET, S3_REGION, 0);
    }

    private IConfigSectionNode getFSNode(string name)
    {
      return LACONF.AsLaconicConfig()[Azos.IO.FileSystem.FileSystem.CONFIG_FILESYSTEMS_SECTION].Children.First(c => c.IsSameNameAttr(name));
    }
  }
}
