/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

namespace Azos.Apps.Hosting.Skyod.Adapters
{
  public sealed class DefaultAzosPackageInstaller : InstallationAdapter
  {
    public DefaultAzosPackageInstaller(SetComponent director) : base(director)
    {
    }

    private async Task<InstallationGetCurrentPackageResponse> doGetCurrentPackageRequest(InstallationGetCurrentPackageRequest request)
    {
      var result =  AdapterResponse.MakeNew<InstallationGetCurrentPackageResponse>(request);
      result.PackageInfo = new PackageInfo();
      return result;
    }

    private Task<InstallationGetPackageListResponse> doGetPackageListRequest(InstallationGetPackageListRequest request)
    {
      return null;
    }

    private Task<InstallationGetRepositoryPackageListResponse> doGetRepositoryPackageListRequest(InstallationGetRepositoryPackageListRequest request)
    {
      return null;
    }


    private Task<InstallationDownloadPackageResponse> doDownloadPackageRequest(InstallationDownloadPackageRequest request)
    {
      return null;
    }

    private Task<InstallationInstallPackageResponse> doInstallPackageRequest(InstallationInstallPackageRequest request)
    {
      return null;
    }
  }
}
