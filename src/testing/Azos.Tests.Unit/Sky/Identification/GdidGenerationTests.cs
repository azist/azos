/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Scripting;
using Azos.Sky.Identification;

namespace Azos.Tests.Unit.Sky.Identification
{
  [Runnable]
  public class GdidGenerationtests
  {
#warning Hard-coded path in code replace with access via TestSources
      const string ROOT_DIR=@"c:\Azos\gdid-utest";

      const string CONFIG =
@"
app
{
   log
   {
     sink
     {
       type='Azos.Log.Sinks.ConsoleSink, Azos'
     }
   }

   gdid-authority
   {
      authority-ids='0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f'
      persistence
      {
        location{name='Primary' order=0 path=$'"+ROOT_DIR+@"'}
      }
   }


  glue
  {
    bindings
    {
      binding
      {
        name='sync'
        type='Azos.Glue.Native.SyncBinding, Azos'
      }
    }

    servers
    {
      server
      {
        name='IGdidAuthorityAsync'
        node='sync://*:9999'
        contract-servers='Azos.Sky.Identification.GdidAuthority, Azos.Sky'
      }
    }
  }
}
";
      private void prepdirs()
      {
          try{ Directory.Delete(ROOT_DIR, true);  }
          catch(Exception error) { Console.WriteLine(error.ToMessageWithType());}

          System.Threading.Thread.Sleep(2000);

          try{ Directory.CreateDirectory(ROOT_DIR); }
          catch(Exception error) { Console.WriteLine(error.ToMessageWithType());}

          System.Threading.Thread.Sleep(2000);
      }

      [Run]
      public void GD_ParallelGeneration_SameSequence()
      {
          prepdirs();

          var conf  = LaconicConfiguration.CreateFromString(CONFIG);
          using(var app =  new AzosApplication(null, conf.Root))
           using ( var authority = new GdidAuthorityService(app))
           {
              authority.Configure(null);
              authority.Start();
              _parallelGenerationSame();
           }

      }

            private void _parallelGenerationSame()
            {
              const int PASS_CNT = 5000;

              int TOTAL = 0;

              var gen = new GdidGenerator(NOPApplication.Instance);
              gen.AuthorityHosts.Register(new GdidGenerator.AuthorityHost(NOPApplication.Instance, "sync://127.0.0.1:9999"));

              var lst = new List<ulong>();
              var rnd = new Random();

              var sw = Stopwatch.StartNew();
              Parallel.For(0, PASS_CNT,
                        (_)=>
                        {
                          var BATCH = 32 + rnd.Next(255); //introduces randomness in threads
                          ulong[] arr = new ulong[BATCH];

                          for(int i=0; i<BATCH; i++)
                          {
                           arr[i] = gen.GenerateOneGdid("a", "aseq", 1024).ID;
                          }

                          lock(lst)
                            foreach(var id in arr)
                              lst.Add(id);

                          System.Threading.Interlocked.Add(ref TOTAL, BATCH);
                        });

              var elapsed = sw.ElapsedMilliseconds;


              Aver.AreEqual(lst.Count, lst.Distinct().Count());//all values are distinct

              Console.WriteLine("Processed {0} in {1} ms. at {2}/sec.".Args(TOTAL, elapsed, TOTAL / (elapsed / 1000d)));

            }


      [Run]
      public void GD_ParallelGeneration_DifferentSequences()
      {
          prepdirs();

          var conf  = LaconicConfiguration.CreateFromString(CONFIG);
          using(var app =  new AzosApplication(null, conf.Root))
           using ( var authority = new GdidAuthorityService(app))
           {
              authority.Configure(null);
              authority.Start();
              _parallelGenerationDifferent();
           }

      }


            private void _parallelGenerationDifferent()
            {
              const int PASS_CNT = 5000;

              int TOTAL = 0;

              var gen = new GdidGenerator(NOPApplication.Instance);
              gen.AuthorityHosts.Register(new GdidGenerator.AuthorityHost(NOPApplication.Instance, "sync://127.0.0.1:9999"));

              var dict = new Dictionary<string, List<ulong>>();
              dict.Add("aseq", new List<ulong>());
              dict.Add("bseq", new List<ulong>());
              dict.Add("cseq", new List<ulong>());
              dict.Add("dseq", new List<ulong>());
              dict.Add("eseq", new List<ulong>());
              dict.Add("fseq", new List<ulong>());
              dict.Add("gseq", new List<ulong>());


              var rnd = new Random();

              var sw = Stopwatch.StartNew();
              Parallel.For(0, PASS_CNT,
                        (n)=>
                        {
                          var seq = dict.Keys.ToList()[n % dict.Keys.Count];

                          var BATCH = 32 + rnd.Next(255); //introduces randomness in threads
                          ulong[] arr = new ulong[BATCH];

                          for(int i=0; i<BATCH; i++)
                          {
                           arr[i] = gen.GenerateOneGdid("a", seq, 1024).ID;
                          }

                          lock(dict[seq])
                            foreach(var id in arr)
                              dict[seq].Add(id);

                          System.Threading.Interlocked.Add(ref TOTAL, BATCH);
                        });

              var elapsed = sw.ElapsedMilliseconds;

              foreach(var kvp in dict)
              {
                Aver.AreEqual(kvp.Value.Count, kvp.Value.Distinct().Count());//all values are distinct
                Console.WriteLine("{0} = {1} ids".Args(kvp.Key, kvp.Value.Count));
              }

              Console.WriteLine("Processed {0} in {1} ms. at {2}/sec.".Args(TOTAL, elapsed, TOTAL / (elapsed / 1000d)));

            }

  }
}
