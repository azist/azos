/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Scripting;


using Azos.Log;
using Azos.Log.Sinks;
using Azos.Conf;
using Azos.Apps;



namespace Azos.Tests.Unit.Logging
{
    [Runnable(TRUN.BASE, 5)]
    public class VariousDestinations
    {
 private const string CONF_SRC1 =@"
 nfx
 {
  log
  {
    destination{ name='mem1' type='Azos.Log.Destinations.MemoryBufferDestination, NFX'}
  }
 }
 ";

        [Run]
        public void Configed_MemoryBufferDestination()
        {

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC1);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var mbd = ((LogDaemon)app.Log).Sinks.First() as MemoryBufferSink;

                System.Threading.Thread.Sleep( 3000 );
                mbd.ClearBuffer();


                app.Log.Write( new Message{ Type = Log.MessageType.Info, From = "test", Text = "Hello1"});
                System.Threading.Thread.Sleep( 1000 );
                app.Log.Write( new Message{ Type = Log.MessageType.Info, From = "test", Text = "Hello2"});

                System.Threading.Thread.Sleep( 3000 );

                Aver.AreEqual(2, mbd.Buffered.Count());

                Aver.AreEqual("Hello1", mbd.BufferedTimeAscending.First().Text);
                Aver.AreEqual("Hello2", mbd.BufferedTimeAscending.Last().Text);

                Aver.AreEqual("Hello2", mbd.BufferedTimeDescending.First().Text);
                Aver.AreEqual("Hello1", mbd.BufferedTimeDescending.Last().Text);
            }
        }

        [Run]
        public void Configed_MemoryBufferDestinationCapacity()
        {

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC1);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var mbd = ((LogDaemon)app.Log).Sinks.First() as MemoryBufferSink;

                System.Threading.Thread.Sleep( 3000 );
                mbd.BufferSize = 10;

                for(int i=0; i<100; i++)
                    app.Log.Write( new Message{Type = Log.MessageType.Info, From = "test", Text = "i={0}".Args(i)} );
                System.Threading.Thread.Sleep( 3000 );

                Aver.AreEqual(10, mbd.Buffered.Count());

                Aver.AreEqual("i=99", mbd.BufferedTimeDescending.First().Text);
                Aver.AreEqual("i=90", mbd.BufferedTimeDescending.Last().Text);
            }
        }


    }
}