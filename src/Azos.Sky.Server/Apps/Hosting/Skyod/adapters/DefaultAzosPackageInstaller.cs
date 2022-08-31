/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Server.Apps.Hosting.Skyod
{
  public sealed class DefaultAzosPackageInstaller : InstallationAdapter
  {
    public DefaultAzosPackageInstaller(SetComponent director) : base(director)
    {
    }

    public override PackageInfo CurrentPackage => throw new NotImplementedException();

    public override Task<PackageInfo> DownloadPackageAsync(PackageInfo package)
    {
      throw new NotImplementedException();
    }

    public override IEnumerable<PackageInfo> GetLocalPackageList()
    {
      throw new NotImplementedException();
    }

    public override IEnumerable<PackageInfo> GetRepositoryPackageList()
    {
      throw new NotImplementedException();
    }

    protected override Task DoInstallAsync(PackageInfo package)
    {
      throw new NotImplementedException();
    }
  }
}
