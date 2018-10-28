/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Pile;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{
    /// <summary>
    /// Pile Cache
    /// </summary>
    public class Pilc : Cmdlet
    {
        public const string CONFIG_TABLE_ATTR = "table";
        public const string CONFIG_PURGE_ATTR = "purge";

        public Pilc(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var cache = CMan.GetApplicationComponentBySIDorName(m_Args) as ICache;

            if (cache==null)
             return "The specified component is not of ICache type";

            var tName = m_Args.AttrByName(CONFIG_TABLE_ATTR).ValueAsString();
            var purge = m_Args.AttrByName(CONFIG_PURGE_ATTR).ValueAsBool();

            var sb = new StringBuilder(1024);
            sb.AppendLine(AppRemoteTerminal.MARKUP_PRAGMA);
            sb.Append("<push><f color=gray>");
            sb.AppendLine("Pile Cache");
            sb.AppendLine("----------------------");

            if (purge)
             sb.AppendLine("<f color=red>Purging:");

            foreach(var tbl in cache.Tables.Where(t => tName==null || t.Name.EqualsOrdIgnoreCase(tName)))
            {
              sb.AppendFormatLine("<f color=yellow>{0,-48}|<f color=white> {1,8:n0}(<f color=cyan>{2,8:n0}<f color=white>)| <f color=darkgray>{3,4:n0}%", tbl.Name, tbl.Count, tbl.Capacity, tbl.LoadFactor*100d);
              if (purge) tbl.Purge();
            }

            sb.AppendLine("<pop>");
            return sb.ToString();
        }

        public override string GetHelp()
        {
            return
@"Pile Cache Manager
           Parameters:
            <f color=yellow>sid=id<f color=gray> - PileCache component SID
             or
            <f color=yellow>name=name<f color=gray> - PileCache component common name


            <f color=yellow>table=name<f color=gray> - optional specific table
            <f color=yellow>purge=bool<f color=gray> - if true, purges the table/s
";
        }


    }

}
