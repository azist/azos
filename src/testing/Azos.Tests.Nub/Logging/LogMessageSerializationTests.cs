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
using Azos.Serialization.JSON;
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
    public void Test_Arow()
    {
      var msg = withError();
      var writer = SlimFormat.Instance.MakeWritingStreamer();
      var reader = SlimFormat.Instance.MakeReadingStreamer();

      using (var ms = new MemoryStream())
      {
        writer.BindStream(ms);
        reader.BindStream(ms);

        ArowSerializer.Serialize(msg, writer);
        ms.Position = 0;

        var got = new Message();
        ArowSerializer.Deserialize(got, reader);

        Console.WriteLine(got.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap));

        Aver.AreEqual(msg.Gdid , got.Gdid);
        Aver.AreEqual(msg.Guid, got.Guid);
        Aver.AreEqual(msg.RelatedTo, got.RelatedTo);
        Aver.AreEqual(msg.Text, got.Text);
        Aver.AreEqual(msg.App, got.App);
        Aver.AreEqual(msg.Channel, got.Channel);
        Aver.AreEqual(msg.ArchiveDimensions, got.ArchiveDimensions);
        Aver.AreEqual(msg.Topic, got.Topic);
        Aver.AreEqual(msg.From, got.From);
        Aver.AreEqual(msg.Parameters, got.Parameters);

        Aver.IsNotNull(got.ExceptionData);
        Aver.AreSameRef(got.ExceptionData, ((WrappedException)got.Exception).Wrapped);

        Aver.AreEqual("Azos.AzosException", got.ExceptionData.TypeName);
        Aver.AreEqual("System.Exception", got.ExceptionData.InnerException.InnerException.InnerException.TypeName);
      }
    }


    [Run("cnt=500000 error=false")]
    [Run("cnt=10000 error=true")]
    public void Benchmark_Arow(int cnt, bool error)
    {
      var msg = error ? withError() : withoutError();
      var writer = SlimFormat.Instance.MakeWritingStreamer();

      using(var ms = new MemoryStream())
      {
        writer.BindStream(ms);

        var sw = Stopwatch.StartNew();
        for(var i=0; i<cnt; i++)
        {
          ArowSerializer.Serialize(msg, writer);
        }

        sw.Stop();
        Console.WriteLine("Wrote {0:n2} AROW bytes for {1:n0} in {2:n0}ms at {3:n0} ops/sec".Args(ms.Position, cnt, sw.ElapsedMilliseconds, cnt / (sw.ElapsedMilliseconds / 1000d)));
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
       ArchiveDimensions = "1, 45, yes",
       RelatedTo = Guid.NewGuid(),
       Parameters = "{a: 1, b: 2, c: 3}",
     };

    private Message withError()
    {
       var result = withoutError();

      try
      {
        a();
      }
      catch (Exception e)
      {
        result.Exception = e;
      }

      return result;
    }

    private void a()
    {
      try
      {
        b();
      }
      catch(Exception e)
      {
        throw new AzosException("Was thrown: "+e.ToMessageWithType(), e);
      }
    }

    private void b()
    {
      try
      {
        c();
      }
      catch (Exception e)
      {
        throw new AzosException("Was thrown: " + e.ToMessageWithType(), e);
      }
    }

    private void c()
    {
      try
      {
        throw new Exception("aaaaa");
      }
      catch (Exception e)
      {
        throw new AzosException("Was thrown: " + e.ToMessageWithType(), e);
      }
    }
  }
}