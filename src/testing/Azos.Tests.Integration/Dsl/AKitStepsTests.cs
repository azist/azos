/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Conf.Forest.Dsl;
using Azos.Scripting;
using Azos.Scripting.Dsl;

namespace Azos.Tests.Integration.Dsl
{
  [Runnable]
  public class AKitStepsTests
  {
    public static string INSTALL_DB = @"
script
{
  do
  {
    type='Azos.AuthKit.Server.MySql.Dsl.InstallUserDatabase, Azos.AuthKit.Server.MySql'
    my-sql-connect-string='SERVER=localhost;uid=root;pwd=thejake'
    db-name='dildo-idp'
  }
}
    ";

    [Run]
    public async Task InstallAkitDatabase()
    {
       var runner = new StepRunner(NOPApplication.Instance, INSTALL_DB.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
       await runner.RunAsync();
    }

    public static string DCTX = @"
script
{
  do
  {
    type='Azos.Scripting.Dsl.SetDataContextName, Azos'
    data-context='MEDUZA-GARGONNA'
  }
}
    ";

    [Run]
    public async Task SetDataContext()
    {
      var runner = new StepRunner(NOPApplication.Instance, DCTX.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));

      await runner.RunAsync();
      Ambient.CurrentCallSession.GetType().DisplayNameWithExpandedGenericArgs().See();
      Aver.AreEqual("MEDUZA-GARGONNA", Ambient.CurrentCallSession.DataContextName);
    }
  }
}
