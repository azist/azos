/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


using Azos;

namespace TestBusinessLogic.Server
{
    /// <summary>
    /// Provides IJokeContract implementation used for testing
    /// </summary>
    public class JokeServer : IJokeContract
    {
        public string Echo(string text)
        {

          //StringBuilder sb = null;
          //TextInfoHeader th = null;
          //foreach(var h in Azos.Glue.ServerCallContext.Request.Headers)
          //{
          //  if (sb==null) sb = new StringBuilder();
          //  if (h is TextInfoHeader) th = (TextInfoHeader)h;
          //  sb.Append(h.ToString());
          //  sb.Append(" ");
          //}


          //ExecutionContext.Application.Log.Write(MessageType.Info, "MarazmServer.Echo. Headers: " + sb.ToString(), from: text + (th==null?string.Empty : th.Text + th.Info));


          //Azos.Glue.ServerCallContext.ResponseHeaders.Add( new MyHeader());
          return "Server echoed " + text;// + "' on "+App.LocalizedTime.ToString();
        }

        public string UnsecureEcho(string text)
        {
          return "Server echoed " + text;
        }

        public string UnsecEchoMar(string text)
        {
          return "Server echoed " + text;
        }

        public object DBWork(string id, int recCount, int waitMs)
        {
          var result = new SimplePersonDoc[recCount];// new List<SimplePersonRow>();

          if (recCount<0) recCount = 0;
          for(var i=0; i<recCount; i++)
          {
           result[i] = new SimplePersonDoc
           {
              ID = new Azos.Data.GDID(0, (ulong) i),
              Age = i,
               Name = "abuxazn"+i,// Azos.Text.NaturalTextGenerator.Generate(10),
                Date = DateTime.Now,
                 Bool1 = i % 18 ==0,
                Str1 = "jsaudhasuhdasiuhduhd", // Azos.Text.NaturalTextGenerator.Generate(25),
                Str2 = "dsadas sdas ",//Azos.Text.NaturalTextGenerator.Generate(25),
                Salary = 1234d * i
            };
          }

          //emulate DB Access
          if (waitMs>0)
           System.Threading.Thread.Sleep(App.Random.NextScaledRandomInteger(waitMs, waitMs));

          return result;
        }
        //{
        //  var result = new List<object[]>();

        //  if (recCount<0) recCount = 0;
        //  for(var i=0; i<recCount; i++)
        //  {
        //    var rec = new object[8];
        //    rec[0] = id;
        //    rec[1] = i;
        //    rec[2] = Azos.Text.NaturalTextGenerator.Generate(10);
        //    rec[3] = i % 18 ==0;
        //    rec[4] = Azos.Text.NaturalTextGenerator.Generate(25);
        //    rec[5] = Azos.Text.NaturalTextGenerator.Generate(25);
        //    rec[6] = DateTime.Now;
        //    rec[7] = 1234d * i;
        //    result.Add( rec );
        //  }

        //  //emulate DB Access
        //  if (waitMs>0)
        //   System.Threading.Thread.Sleep(ExternalRandomGenerator.Instance.NextScaledRandomInteger(waitMs, waitMs));

        //  return result;
        //}


        public void Notify(string text)
        {
            NotifyEvent.Happened();
            //ExecutionContext.Application.Log.Write(MessageType.Info, from: "MarazmServer.Notify", text: text);
        }

        public object ObjectWork(object dummy)
        {
            if (dummy is string)
                dummy = dummy.ToString() + " server work done on " + App.LocalizedTime.ToString();
            return dummy;
        }


        public string SimpleWorkAny(string s, int i1, int i2, bool b, double d)
        {
          return s + i1.ToString() + i2.ToString() + b.ToString() + d.ToString();
        }

        public string SimpleWorkMar(string s, int i1, int i2, bool b, double d)
        {
          return s + i1.ToString() + i2.ToString() + b.ToString() + d.ToString();
        }

    }
}
