/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Scripting;


using Azos.Apps;
using Azos.Log;

namespace Azos.Tests.Unit.Config
{
    [Runnable(TRUN.BASE)]
    public class Behavior
    {

    static string conf1 = @"
 <root>

    <log>
     <behaviors>
       <behavior type='Azos.Tests.Unit.Config.AlwaysLogBehavior, Azos.Tests.Unit' />
     </behaviors>
                   <!-- Notice the behavior is defined at log level where it is applied by injecting destination -->
    </log>

 </root>
";

 static string conf2 = @"
 <root>
     <behaviors>
       <behavior type='Azos.Tests.Unit.Config.AlwaysLogBehavior, Azos.Tests.Unit' cascade='true'/>
     </behaviors>

    <log>
                 <!-- Notice the behavior is defined at application level, but it is still applied here because it cascades down the tree -->
    </log>

 </root>
";

static string conf3 = @"
 <root>
     <behaviors>
       <behavior type='Azos.Tests.Unit.Config.AlwaysLogBehavior, Azos.Tests.Unit' cascade='false'/>
     </behaviors>

    <log>
             <!-- Notice the behavior is defined at application level, but it is NOT applied here because it does not cascade down the tree -->
    </log>

 </root>
";


        [Run]
        public void Case1_LogLevel()
        {
            var root = Azos.Conf.XMLConfiguration.CreateFromXML(conf1).Root;
            using(var app = new AzosApplication(new string[0], root ))
            {
                app.Log.Write(Log.MessageType.Info, "Khello!");

                Aver.AreEqual(1, ((LogDaemon)app.Log).Sinks.Count());
                System.Threading.Thread.Sleep(1000);//wait for flush
                Aver.IsNotNull( ((listSink)((LogDaemon)app.Log).Sinks.First()).List.FirstOrDefault(m=> m.Text == "Khello!") );
            }

        }

        [Run]
        public void Case2_CascadeFromAppLevel()
        {
            var root = Azos.Conf.XMLConfiguration.CreateFromXML(conf2).Root;
            using(var app = new AzosApplication(new string[0],  root ))
            {
                app.Log.Write(Log.MessageType.Info, "Khello!");

                Aver.AreEqual(1, ((LogDaemon)app.Log).Sinks.Count());
                System.Threading.Thread.Sleep(1000);//wait for flush
                Aver.IsNotNull( ((listSink)((LogDaemon)app.Log).Sinks.First()).List.FirstOrDefault(m=> m.Text == "Khello!") );
            }

        }

        [Run]
        [Aver.Throws(typeof(AzosException), Message="No log destinations registered", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
        public void Case3_ExistsOnAppLevelButDoesNotCascade()
        {
            var root = Azos.Conf.XMLConfiguration.CreateFromXML(conf3).Root;
            using(var app = new AzosApplication(new string[0],  root ))
            {

            }

        }

    }



        internal class listSink : Log.Sinks.Sink
        {
            public listSink(LogDaemon owner): base(owner, "test-sink", -1) { }
            public MessageList List = new MessageList();

            protected internal override void DoSend(Message entry)
            {
              List.Add(entry);
            }
        }


        public class AlwaysLogBehavior : Conf.Behavior
        {
            public AlwaysLogBehavior() : base() {}

            public override void Apply(object target)
            {
              if (target is LogDaemon dlog) (new listSink(dlog)).Start();
            }
        }
}
