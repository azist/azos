/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Packaging.Dsl
{
  /// <summary>
  /// Sets target name
  /// </summary>
  public sealed class InstallPackage : Step
  {
    public InstallPackage(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
    [Config] public string PackagePath { get; set; }
    [Config] public string InstallRootPath { get; set; }
    [Config] public string TargetNames { get; set; }

    [Config] public string Umask { get; set; }

    [Config] public int Verbosity { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var rootPath = Eval(InstallRootPath, state);
      if (rootPath.IsNullOrWhiteSpace() || rootPath == ".") rootPath = Directory.GetCurrentDirectory();

      using (var installer = new Installer(App, null, Conout.Port.DefaultConsole))//todo: Add type resolver
      {
        installer.PackagePath = Eval(PackagePath, state).NonBlank(nameof(PackagePath));
        installer.RootPath = rootPath;
        installer.TargetNames = Eval(TargetNames, state);
        installer.Umask = Eval(Umask, state).AsEnum(Installer.UmaskType.User);
        installer.Verbosity = Verbosity;
        installer.Run();

        if (installer.State_Error != null) throw installer.State_Error;
      }


      return Task.FromResult<string>(null);
    }

    private void installer_Progress(string status)
    {
      Conout.WriteLine(status);
    }
  }
}
