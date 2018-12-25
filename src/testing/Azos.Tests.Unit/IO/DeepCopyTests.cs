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
using Azos.IO.FileSystem.Packaging;
using Azos.Security;
using Azos.Apps;

namespace Azos.Tests.Unit.IO
{
    [Runnable(TRUN.BASE)]
    public class DeepCopyTests
    {

        [Run]
        public void DeepCopy_1()
        {
            var p1 = ManifestUtilsTests.Get_TEZT_PATH();
            var p2 = p1+"_2";

            IOUtils.EnsureDirectoryDeleted(p2);
            Directory.CreateDirectory(p2);

            using(var fs = new LocalFileSystem(NOPApplication.Instance))
            {
              using(var session = fs.StartSession())
              {
                var fromDir = session[p1] as FileSystemDirectory;
                var manifest1 = ManifestUtils.GeneratePackagingManifest(fromDir);

                var toDir = session[p2] as FileSystemDirectory;

                fromDir.DeepCopyTo(toDir, FileSystemDirectory.DirCopyFlags.Directories | FileSystemDirectory.DirCopyFlags.Files);
                var manifest2 = ManifestUtils.GeneratePackagingManifest(toDir);


                Console.WriteLine(manifest1.Configuration.ContentView);

                Aver.IsTrue( ManifestUtils.HasTheSameContent(manifest1, manifest2) );
              }
            }
        }


    }
}
