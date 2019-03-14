/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Linq;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Reflection;


using Azos.Log;
using Azos.Serialization.BSON;
using Azos.Serialization.Arow;
using Azos.Scripting;
using Azos.IO;

namespace Azos.Tests.Nub.Logging
{
  [Runnable]
  public class LogMessageSerializationTests : IRunHook
  {

    public bool Prologue(Runner runner, FID id, MethodInfo method, RunAttribute attr, ref object[] args)
    {
      ArowSerializer.RegisterTypeSerializationCores(Assembly.GetCallingAssembly());
      return false;
    }

    public bool Epilogue(Runner runner, FID id, MethodInfo method, RunAttribute attr, Exception error)
     => false;

    [Run]
    public void Benchmark_WOError_Arow()
    {
      var CNT = 500_000;

      var msg = withoutError();
      var writer = SlimFormat.Instance.MakeWritingStreamer();

      using(var ms = new MemoryStream())
      {
        writer.BindStream(ms);

        var sw = Stopwatch.StartNew();
        for(var i=0; i<CNT; i++)
        {
          ArowSerializer.Serialize(msg, writer);
        }

        sw.Stop();
        Console.WriteLine("Wrote {0:n2} AROW bytes for {1:n0} in {2:n0}ms at {3:n0} ops/sec".Args(ms.Position, CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d)));
      }
    }

    [Run]
    public void Benchmark_WOError_BSON()
    {
      var CNT = 500_000;

      var msg = withoutError();
      var ser = new BSONSerializer();

      var writer = SlimFormat.Instance.MakeWritingStreamer();

      using (var ms = new MemoryStream())
      {
        writer.BindStream(ms);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < CNT; i++)
        {
          var doc = ser.Serialize(msg);
          doc.WriteAsBSON(ms);
        }

        sw.Stop();
        Console.WriteLine("Wrote {0:n2} BSON bytes for {1:n0} in {2:n0}ms at {3:n0} ops/sec".Args(ms.Position, CNT, sw.ElapsedMilliseconds, CNT / (sw.ElapsedMilliseconds / 1000d)));
      }
    }



    private Message withoutError()
     => new Message
     {
       Topic = "my topic",
       From = "here",
       App = Atom.Encode("testing"),
       Channel = Atom.Encode("security"),
       Host = "/world/us/local/a/i/med0001",
       Source = 78324,
       Type = MessageType.CatastrophicError,
       Text = "This is a text for this message",
       Gdid = new Data.GDID(1, 1, 1),
     };


  }
}