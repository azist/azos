/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Diagnostics;
using System.Runtime;
using System.Text;
using Azos.Conf;
using Azos.Security;
using Azos.Serialization.JSON;

namespace Azos.Apps.Terminal.Cmdlets
{
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public sealed class Env : Cmdlet
  {
    public Env(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var result = new JsonDataMap();

      result["Is64BitOperatingSystem"] = System.Environment.Is64BitOperatingSystem;
      result["Is64BitProcess"] = System.Environment.Is64BitProcess;
      result["OSVersion"] = System.Environment.OSVersion;
      result["HasShutdownStarted"] = System.Environment.HasShutdownStarted;

      result["ExitCode"] = System.Environment.ExitCode;
      result["CurrentManagedThreadId"] = System.Environment.CurrentManagedThreadId;
      result["CurrentDirectory"] = System.Environment.CurrentDirectory;
      result["CommandLine"] = System.Environment.CommandLine;
      result["ProcessPath"] = System.Environment.ProcessPath;
      result["MachineName"] = System.Environment.MachineName;
      result["ProcessorCount"] = System.Environment.ProcessorCount;
      result["SystemDirectory"] = System.Environment.SystemDirectory;
      result["SystemPageSize"] = System.Environment.SystemPageSize;
      result["TickCount64"] = System.Environment.TickCount64;
      result["UserDomainName"] = System.Environment.UserDomainName;
      result["UserInteractive"] = System.Environment.UserInteractive;
      result["UserName"] = System.Environment.UserName;
      result["Version"] = System.Environment.Version;
      result["WorkingSet"] = System.Environment.WorkingSet;
      result["StackTrace"] = System.Environment.StackTrace;

      result["NewLine"] = System.Environment.NewLine;

      result["args"] = System.Environment.GetCommandLineArgs();

      result["vars"] = System.Environment.GetEnvironmentVariables();


      result["az:hostname"] = Azos.Platform.Computer.HostName;
      result["az:osfam"]    = Azos.Platform.Computer.OSFamily;
      result["az:netsig"]   = Azos.Platform.Computer.UniqueNetworkSignature;
      result["az:memmodel"] = Ambient.MemoryModel;


      return result.ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII);
    }

    public override string GetHelp()
    {
      return
@"Fetches current process environment.
  <f color=cyan> NOTE: 
    Requires  <f color=red>SystemAdministratorPermission(AccessLevel.ADVANCED)<f color=cyan> grant 
";
    }
  }

}
