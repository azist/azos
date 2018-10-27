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

using Azos.Conf;
using Azos.IO.FileSystem;
using Azos.IO.FileSystem.Local;
using Azos.IO.FileSystem.Packaging;
using Azos.Security;

namespace Azos.Tests.Unit.IO
{
    [Runnable(TRUN.BASE)]
    public class LocalInstallationTests
    {

        [Run]
        public void Install_1()
        {
            var source = ManifestUtilsTests.Get_TEZT_PATH();
            var target = source+"_INSTALLED";

            using(var fs = new LocalFileSystem("L1"))
            {
              var session = fs.StartSession();
              var sourceDir = session[source] as FileSystemDirectory;

              var manifest = ManifestUtils.GeneratePackagingManifest(sourceDir, packageName: "Package1");
              var fn = Path.Combine(source, ManifestUtils.MANIFEST_FILE_NAME);
              try
              {
                  manifest.Configuration.ToLaconicFile(fn);


                  var set = new List<LocalInstallation.PackageInfo>
                  {
                      new LocalInstallation.PackageInfo("Package1", sourceDir, null)//no relative path
                  };


                  using(var install = new LocalInstallation(target))
                  {
                    Console.WriteLine("Installed anew: "+ install.CheckLocalAndInstallIfNeeded(set));
                  }
              }
              finally
              {
                IOUtils.EnsureFileEventuallyDeleted(fn);
              }

            }
        }



    }
}
