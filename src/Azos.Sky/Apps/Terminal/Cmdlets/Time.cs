/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Text;

using Azos.Conf;


namespace Azos.Apps.Terminal.Cmdlets
{

  public sealed class Time : Cmdlet
  {
    public Time(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var result = new StringBuilder(0xff);
      result.AppendLine("System Time:");
      result.AppendLine(" Machine Now:     " + DateTime.Now);
      result.AppendLine(" Machine UTCNow:  " + DateTime.UtcNow);
      result.AppendLine(" App Localized:   " + App.LocalizedTime);
      result.AppendLine(" App Location:    " + App.TimeLocation);
      result.AppendLine(" App Time Source:        " + App.TimeSource.GetType().FullName);
      result.AppendLine(" App Time Source Now:    " + App.TimeSource.Now);
      result.AppendLine(" App Time Source UTCNow: " + App.TimeSource.UTCNow);
      result.AppendLine(" App Time Location:   " + App.TimeSource.TimeLocation);
      result.AppendLine(" App Start Time:         " + App.StartTime );
      result.AppendLine(" App Running Time:       " + (App.LocalizedTime - App.StartTime).ToString() );

      return result.ToString();
    }

    public override string GetHelp()
    {
        return "Returns app container time";
    }
  }

}
