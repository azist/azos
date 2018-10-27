/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Scripting;

using Azos.IO.FileSystem;
using Azos.IO.FileSystem.Local;
using Azos.Security;

namespace Azos.Tests.Unit.IO
{
    [Runnable(TRUN.BASE)]
    public class LocalFileSystemTests : IRunnableHook
    {
        public const string LOCAL_ROOT = @"c:\NFX";

        public const string FN1 = @"fstest1";
        public const string FN2 = @"fstest2.tezt";
        public const string FN3 = @"fstest3.txt";
        public const string FN4 = @"fstest4.txt";

        public const string FN_PARALLEL_MASK = @"parallel_fstest_{0}";


        public const int PARALLEL_FROM = 0;
        public const int PARALLEL_TO = 1000;

        public const string DIR_1 = "dir1";

        void IRunnableHook.Prologue(Runner runner, FID id)
        {
          cleanup();
        }

        bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
        {
          cleanup();
          return false;
        }

        private void cleanup()
        {
          for(var i=PARALLEL_FROM; i<PARALLEL_TO; i++)
           try
           {
             deleteFile( Path.Combine(LOCAL_ROOT, FN_PARALLEL_MASK.Args(i)) );
           }
           catch{}

          deleteFile( Path.Combine(LOCAL_ROOT, FN1) );
          deleteFile( Path.Combine(LOCAL_ROOT, FN2) );
          deleteFile( Path.Combine(LOCAL_ROOT, FN3) );
          deleteFile( Path.Combine(LOCAL_ROOT, FN4) );
          try{Directory.Delete( Path.Combine(LOCAL_ROOT, DIR_1), true );} catch{}
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


        [Run]
        public void CombinePaths()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              Aver.AreEqual(@"c:\a.txt", fs.CombinePaths(@"c:\",@"a.txt"));
              Aver.AreEqual(@"c:\a.txt", fs.CombinePaths(@"c:\",@"\a.txt"));

              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books",@"fiction","saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books",@"\fiction","saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books",@"\fiction",@"saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books",@"\fiction",@"\saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books",@"fiction", @"\saga.pdf"));

              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books\",@"fiction","saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books\",@"\fiction","saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books\",@"\fiction",@"saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"\books\",@"\fiction",@"\saga.pdf"));
              Aver.AreEqual(@"c:\books\fiction\saga.pdf", fs.CombinePaths(@"c:\",@"books\",@"fiction", @"\saga.pdf"));

              Aver.AreEqual(@"\books\fiction\saga.pdf", fs.CombinePaths("",@"books",@"fiction", @"\saga.pdf"));
            }
        }


        [Run]
        public void Connect_NavigateRootDir()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              Aver.AreEqual(@"c:\", dir.ParentPath);
              Aver.AreEqual(@"c:\NFX", dir.Path);
            }
        }

        [Run]
        public void CreateFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {

              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              var file = dir.CreateFile(FN1, 1500);

              Aver.AreEqual(FN1, file.Name);

              Aver.IsTrue( File.Exists(file.Path));
              Aver.AreEqual(1500, new FileInfo(file.Path).Length);
              Aver.AreEqual(1500, file.FileStream.Length);
            }
        }

        [Run]
        public void CreateDeleteFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {


              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              var file = dir.CreateFile(FN1, 1500);



              Aver.AreEqual(FN1, file.Name);

              Aver.IsTrue( File.Exists(file.Path));
              Aver.AreEqual(1500, new FileInfo(file.Path).Length);
              Aver.AreEqual(1500, file.FileStream.Length);

              Aver.IsTrue(1500 == file.Size);

              var file2 = session[fs.CombinePaths(LOCAL_ROOT, FN1)];
              Aver.IsNotNull( file2);
              Aver.IsTrue(file2 is FileSystemFile);
              Aver.AreEqual(FN1, ((FileSystemFile)file2).Name);

              file.Dispose();

              file2.Delete();
              Aver.IsFalse( File.Exists(file2.Path));
            }
        }


        [Run]
        public void CreateWriteReadFile_1()
        {
            using(var fs = new LocalFileSystem("L1"))
            {


              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              using (var file = dir.CreateFile(FN2))
              {
                var str = file.FileStream;

                var wri = new BinaryWriter(str);

                wri.Write( "Hello!" );
                wri.Write( true );
                wri.Write( 27.4d );
                wri.Close();
              }

              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN2)] as FileSystemFile)
              {
                var str = file.FileStream;

                var rdr = new BinaryReader(str);

                Aver.AreEqual( "Hello!", rdr.ReadString());
                Aver.AreEqual( true,     rdr.ReadBoolean());
                Aver.AreEqual( 27.4d,    rdr.ReadDouble());
              }
            }
        }


        [Run]
        public void CreateWriteReadFile_2()
        {
            using(var fs = new LocalFileSystem("L1"))
            {


              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              using (var file = dir.CreateFile(FN4))
              {
                file.WriteAllText("This is what it takes!");
              }

              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN4)] as FileSystemFile)
              {
                Aver.AreEqual( "This is what it takes!", file.ReadAllText());
              }
            }
        }

        [Run]
        public void CreateWriteReadFile_3()
        {
            using(var fs = new LocalFileSystem("L1"))
            {


              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              using (var file = dir.CreateFile(FN4))
              {
                file.WriteAllText("Hello,");
                file.WriteAllText("this will overwrite");
              }

              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN4)] as FileSystemFile)
              {
                Aver.AreEqual( "this will overwrite", file.ReadAllText());
              }
            }
        }



        [Run]
        public void CreateDeleteDir()
        {
            using(var fs = new LocalFileSystem("L1"))
            {

              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              var dir2 = dir[DIR_1] as FileSystemDirectory;
              Aver.IsNull( dir2 );

              dir2 = dir.CreateDirectory(DIR_1);

              Aver.AreEqual(DIR_1, ((FileSystemDirectory)dir[DIR_1]).Name);

              Aver.AreEqual(DIR_1, ((FileSystemDirectory)dir2[""]).Name);

              Aver.IsTrue( Directory.Exists(dir2.Path));

              Aver.AreEqual(DIR_1, dir2.Name);

              dir2.Delete();
              Aver.IsFalse( Directory.Exists(dir2.Path));

            }
        }

        [Run]
        public void CreateDirCreateFileDeleteDir()
        {
            using(var fs = new LocalFileSystem("L1"))
            {

              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;
 Aver.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor
              var dir2 = dir[DIR_1] as FileSystemDirectory;
              Aver.IsNull( dir2 );
 Aver.AreEqual(1, session.Items.Count());//checking item registation via .ctor/.dctor
              dir2 = dir.CreateDirectory(DIR_1);
 Aver.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
              Aver.IsTrue( Directory.Exists(dir2.Path));

              Aver.AreEqual(DIR_1, dir2.Name);

              var f = dir2.CreateFile(FN1, 237);
Aver.AreEqual(3, session.Items.Count());//checking item registation via .ctor/.dctor
              Aver.IsTrue( File.Exists(f.Path));

              Aver.IsTrue(237 == dir2.Size);

              Aver.IsTrue( dir.SubDirectoryNames.Contains(DIR_1) );

              Aver.IsTrue( dir2.FileNames.Contains(FN1) );


              dir2.Delete();
Aver.AreEqual(2, session.Items.Count());//checking item registation via .ctor/.dctor
              Aver.IsFalse( Directory.Exists(dir2.Path));
              Aver.IsFalse( File.Exists(f.Path));
Aver.AreEqual(1, fs.Sessions.Count());//checking item registation via .ctor/.dctor
              session.Dispose();
Aver.AreEqual(0, fs.Sessions.Count());//checking item registation via .ctor/.dctor
Aver.AreEqual(0, session.Items.Count());//checking item registation via .ctor/.dctor
            }
        }



        [Run]
        public void CreateWriteCopyReadFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              var session = fs.StartSession();

              var dir = session[LOCAL_ROOT] as FileSystemDirectory;

              using (var file = dir.CreateFile(FN2))
              {
                var str = file.FileStream;

                var wri = new BinaryWriter(str);

                wri.Write( "Hello!" );
                wri.Write( true );
                wri.Write( 27.4d );
                wri.Close();
              }

              //FN3 copied from FN2 and made readonly
              Aver.IsNotNull( dir.CreateFile(FN3, Path.Combine(LOCAL_ROOT, FN2), true) );


              using (var file = session[fs.CombinePaths(LOCAL_ROOT, FN3)] as FileSystemFile)
              {
                Aver.IsTrue( file.ReadOnly);
                Aver.IsTrue( file.IsReadOnly);

                var str = file.FileStream;

                var rdr = new BinaryReader(str);

                Aver.AreEqual( "Hello!", rdr.ReadString());
                Aver.AreEqual( true,     rdr.ReadBoolean());
                Aver.AreEqual( 27.4d,    rdr.ReadDouble());

                file.ReadOnly = false;

                Aver.IsFalse( file.ReadOnly);
                Aver.IsFalse( file.IsReadOnly);

              }
            }
        }


        [Run]
        public void Parallel_CreateWriteReadFile()
        {
            using(var fs = new LocalFileSystem("L1"))
            {
              System.Threading.Tasks.Parallel.For(PARALLEL_FROM, PARALLEL_TO,
               (i)=>
               {
                    var fn = FN_PARALLEL_MASK.Args(i);
                    var session = fs.StartSession();

                    var dir = session[LOCAL_ROOT] as FileSystemDirectory;

                    using (var file = dir.CreateFile(fn))
                    {
                      file.WriteAllText("Hello, {0}".Args(i));
                    }

                    using (var file = session[fs.CombinePaths(LOCAL_ROOT, fn)] as FileSystemFile)
                    {
                      Aver.AreEqual( "Hello, {0}".Args(i), file.ReadAllText());
                      file.Delete();
                    }
                    Aver.IsNull( session[fs.CombinePaths(LOCAL_ROOT, fn)] );

               });//Parallel.For

            }
        }





    }
}
