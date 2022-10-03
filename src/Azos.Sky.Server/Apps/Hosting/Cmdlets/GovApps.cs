/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Diagnostics;
using System.Runtime;
using System.Text;

using Azos.Apps.Terminal;
using Azos.Conf;

namespace Azos.Apps.Hosting.Cmdlets
{
  public sealed class GovApps : Cmdlet
  {
    public GovApps(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var gov = App.Singletons.Get<GovernorDaemon>();
      gov.NonNull(nameof(GovernorDaemon));
      gov.Running.IsTrue("Running gov");

      var result = new StringBuilder();

      gov.Applications.OrderedValues.ForEach(app =>
      {
        result.AppendFormat("* {0}  {1}\n", app.ServiceDescription, app.StatusDescription);
      });

      return result.ToString();
    }

    public override string GetHelp()
    {
      return @"Lists all apps managed by governor";
    }
  }

}
