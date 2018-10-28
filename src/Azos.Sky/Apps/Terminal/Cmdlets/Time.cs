using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using Azos.Conf;


namespace Azos.Sky.Apps.Terminal.Cmdlets
{

    public class Time : Cmdlet
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
