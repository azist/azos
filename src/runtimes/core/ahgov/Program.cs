/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace ahgov
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      //   Azos.Sky.Hosts.ahgov.ProgramBody.Main(args);
      refactoring_main(args);
    }

    //07/07/2021
    static void refactoring_main(string[] args)
    {
      var bargs = new Azos.Apps.Hosting.BootArgs(args);

      if (bargs.IsGoverned)
        Azos.Apps.Hosting.ApplicationHostProgramBody.GovernedConsoleMain(bargs);
      else
        Azos.Apps.Hosting.ApplicationHostProgramBody.InteractiveConsoleMain(bargs);
    }

  }
}
