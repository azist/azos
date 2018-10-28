/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Log;

namespace Azos.Sky.Apps.Terminal.Cmdlets
{



    public class Log : Cmdlet
    {
        public const string CONFIG_TYPE_ATTR = "type";
        public const string CONFIG_ON_ATTR = "on";
        public const string CONFIG_BINDING_ATTR = "binding";

        public Log(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var type = m_Args.AttrByName(CONFIG_TYPE_ATTR).ValueAsEnum<InstrType>(InstrType.View);
            var on = m_Args.AttrByName(CONFIG_ON_ATTR).ValueAsBool();
            var bname = m_Args.AttrByName(CONFIG_BINDING_ATTR).Value;

            var msg = new Message
            {
              From     = m_Args.AttrByName("from").Value,
              Source   = m_Args.AttrByName("source").ValueAsInt(0),
              Type     = m_Args.AttrByName("type").ValueAsEnum<MessageType>(MessageType.Info),
              Topic    = m_Args.AttrByName("topic").ValueAsString("App Terminal"),
              Text     = m_Args.AttrByName("text").ValueAsString("-none-"),
              Parameters = m_Args.AttrByName("parameters").Value
            };
            App.Log.Write( msg );

            return msg.ToString();
        }

        public override string GetHelp()
        {
            return
@"Writes to application log
           Parameters:
            <f color=yellow>from = string<f color=gray> - name of code/component
            <f color=yellow>source=int<f color=gray> - int source
            <f color=yellow>type=MessageType<f color=gray> - standard log MessageType enum
            <f color=yellow>topic=string<f color=gray> - what msg relates to
            <f color=yellow>text=string<f color=gray> - msg text
            <f color=yellow>parameters=string<f color=gray> - msg parameters

";
        }


    }

}
