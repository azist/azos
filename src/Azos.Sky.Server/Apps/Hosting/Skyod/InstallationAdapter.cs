/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.Server.Apps.Hosting.Skyod
{
  /// <summary>
  /// Outlines protocol for activities related to software component installation
  /// </summary>
  public abstract class InstallationAdapter : ApplicationComponent<SetComponent>
  {
    protected InstallationAdapter(SetComponent director) : base(director)
    {

    }

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_SKYOD;

    /// <summary>
    /// Currently installed package or `!Assigned`
    /// </summary>
    public abstract PackageInfo CurrentPackage { get; }

    public abstract IEnumerable<PackageInfo> GetLocalPackageList();

    public abstract IEnumerable<PackageInfo> GetRepositoryPackageList();

    public abstract Task<PackageInfo> DownloadPackageAsync(PackageInfo package);

    public async Task InstallAsync(PackageInfo package)
    {
      ComponentDirector.IsManagedInstall.IsTrue("Support managed install");
      package.HasRequiredValue(nameof(package));
      await DoInstallAsync(package).ConfigureAwait(false);
    }

    protected abstract Task DoInstallAsync(PackageInfo package);
  }

}
