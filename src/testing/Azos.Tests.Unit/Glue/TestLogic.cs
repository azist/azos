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
using Azos.Conf;
using Azos.Glue.Native;
using Azos.Glue;
using Azos.IO;
using System.Threading.Tasks;

namespace Azos.Tests.Unit.Glue
{
    public static class TestLogic
    {
        private static void dumpBindingTransports(Binding binding)
        {
           Console.WriteLine("Client transport count: {0}".Args( binding.ClientTransports.Count()));
           Console.WriteLine("Server transport count: {0}".Args( binding.ServerTransports.Count()));
        }


        public static void TestContractA_TwoWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);

                var result = cl.Method1(12);
                Aver.AreEqual( "12", result);
                Aver.AreEqual(12, TestServerA.s_Accumulator);
            }
        }

        public static void TASK_TestContractA_TwoWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);

                var call = cl.Async_Method1(12);
                var task = call.AsTask;

                var result = task.Result.GetValue<string>();

                Aver.AreEqual( "12", result);
                Aver.AreEqual(12, TestServerA.s_Accumulator);
            }
        }

        public static void TASKReturning_TestContractA_TwoWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);

                var call = cl.Async_Method1(12);
                var task = call.AsTaskReturning<string>();

                var result = task.Result;

                Aver.AreEqual( "12", result);
                Aver.AreEqual(12, TestServerA.s_Accumulator);
            }
        }

        public static async Task ASYNC_AWAIT_CALL_TestContractA_TwoWayCall(string CONF_SRC)
        {
          TestServerA.s_Accumulator = 0;

          var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
          using (var app = new AzosApplication(null, conf.Root))
          {
            var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);

            var call = cl.Async_Method1(12);

            await call;//this will wait and come back on either timeout or result delivery

            Aver.IsTrue(call.Available);

            Aver.AreEqual("12", call.GetValue<string>());
            Aver.AreEqual(12, TestServerA.s_Accumulator);
          }
        }

        public static async Task ASYNC_MANY_AWAITS_TestContractA_TwoWayCall(string CONF_SRC)
        {
          TestServerA.s_Accumulator = 0;

          var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
          using (var app = new AzosApplication(null, conf.Root))
          {
            var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);

            var call = cl.Async_Method1(234);

            var result = await call.AsTaskReturning<string>();

            Aver.IsTrue(call.Available);

            Aver.AreEqual("234", result);

            Aver.AreEqual("234", (await call).GetValue<string>());//this will instantly return as it is completed already
            Aver.AreEqual("234", (await call).GetValue<string>());//so will this
            Aver.AreEqual("234", (await call).GetValue<string>());//and this... and so on.... can do many awaits

            Aver.AreEqual(234, TestServerA.s_Accumulator);
          }
        }





        public static void TestContractA_TwoWayCall_Timeout(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);
                cl.TimeoutMs = 2000;

                try
                {
                  cl.Sleeper(5000);
                }
                catch(ClientCallException err)
                {
                  Aver.IsTrue(CallStatus.Timeout == err.Status);
                  return;
                }
                catch(System.IO.IOException err) //sync binding throws IO exception
                {
                  Aver.IsTrue( err.Message.Contains("after a period of time") );
                  return;
                }

                Aver.Fail("Invalid Call status");
            }
        }


        public static void TASK_TestContractA_TwoWayCall_Timeout(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);
                cl.TimeoutMs = 2000;

                System.Threading.Tasks.Task<CallSlot> task = null;
                try
                {
                  task = cl.Async_Sleeper(10000).AsTask;
                  task.Result.GetValue<int>();
                }
                catch(ClientCallException err)
                {
                  Aver.IsTrue(CallStatus.Timeout == err.Status);
                  return;
                }
                catch(System.IO.IOException err) //sync binding throws IO exception
                {
                  Aver.IsTrue( err.Message.Contains("after a period of time") );
                  return;
                }

                Aver.Fail("Invalid Call status: " + (task!=null ? task.Result.CallStatus.ToString() : "task==null"));
            }
        }

        public static async Task ASYNC_TestContractA_TwoWayCall_Timeout(string CONF_SRC)
        {
          TestServerA.s_Accumulator = 0;

          var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
          using (var app = new AzosApplication(null, conf.Root))
          {
            var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);
            cl.TimeoutMs = 2000;

            CallSlot call = null;
            try
            {
              call = await cl.Async_Sleeper(10000);
              call.GetValue<int>();
            }
            catch (ClientCallException err)
            {
              Aver.IsTrue(CallStatus.Timeout == err.Status);
              return;
            }
            catch (System.IO.IOException err) //sync binding throws IO exception
            {
              Aver.IsTrue(err.Message.Contains("after a period of time"));
              return;
            }

            Aver.Fail("Invalid Call status: " + (call != null ? call.CallStatus.ToString() : "call==null"));
          }
        }



        public static void TestContractA_OneWayCall(string CONF_SRC)
        {
            TestServerA.s_Accumulator = 0;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);

                cl.Method2(93);

                //since this is one-way call, we wait 10 sec tops for server-side state becoming 93
                for(var cnt=0; cnt<10 && TestServerA.s_Accumulator!=93 ; cnt++)
                    System.Threading.Thread.Sleep(1000);

                Aver.AreEqual(93,TestServerA.s_Accumulator);
            }
        }

        public static async Task ASYNC_TestContractA_OneWayCall(string CONF_SRC)
        {
          TestServerA.s_Accumulator = 0;

          var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
          using (var app = new AzosApplication(null, conf.Root))
          {
            var cl = new TestContractAClient(app.ConfigRoot.AttrByName("cs").Value);

            await cl.Async_Method2(93);//this should instantly return as call already dispatched

            //since this is one-way call, we wait 10 sec tops for server-side state becoming 93
            for (var cnt = 0; cnt < 10 && TestServerA.s_Accumulator != 93; cnt++)
              System.Threading.Thread.Sleep(1000);

            Aver.AreEqual(93, TestServerA.s_Accumulator);
          }
        }


        public static void TestContractB_1(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var person = new PersonData{ID = 10, FirstName="Joe", LastName="Tester" };

                cl.SetPersonOfTheDay( person );

                var ret = cl.GetPersonOfTheDay();

                Aver.IsTrue( 10 == ret.ID);
                Aver.AreEqual( "Joe", ret.FirstName);
                Aver.AreEqual( "Tester", ret.LastName);

                dumpBindingTransports( cl.Binding );
            }

        }


        public static void TestContractB_1_Async(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var person = new PersonData{ID = 10, FirstName="Joe", LastName="Tester" };

                var slot = cl.Async_SetPersonOfTheDay( person );

                slot.CheckVoidValue();

                slot = cl.Async_GetPersonOfTheDay();

                var ret = slot.GetValue<PersonData>();

                Aver.IsTrue( 10 == ret.ID);
                Aver.AreEqual( "Joe", ret.FirstName);
                Aver.AreEqual( "Tester", ret.LastName);

                dumpBindingTransports( cl.Binding );
            }

        }




        public static void TestContractB_2(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var person = new PersonData{ID = 10, FirstName="Joe", LastName="Tester" };

                cl.SetPersonOfTheDay( person );

                var ret = cl.GetPersonOfTheDay();

                Aver.IsTrue( 10 == ret.ID);
                Aver.AreEqual( "Joe", ret.FirstName);
                Aver.AreEqual( "Tester", ret.LastName);

                var sum = cl.SummarizeAndFinish(); //destructor

                Aver.AreEqual("That is all! for the person Tester", sum);

                cl.ForgetRemoteInstance();

                Aver.AreEqual("Felix", cl.GetName()); //this will allocate the new instance

                sum = cl.SummarizeAndFinish(); // this will kill the instance again
                Aver.AreEqual("That is all! but no person of the day was set", sum);

                dumpBindingTransports( cl.Binding );
            }
        }


        public static void TestContractB_3(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                Aver.AreEqual( "Felix", cl.GetName());

                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_4(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                //Testing overloaded calls
                Aver.AreEqual( "Felix", cl.GetName());
                Aver.AreEqual( "Felix23", cl.GetName(23));
                Aver.AreEqual( "Felix42", cl.GetName(42));
                Aver.AreEqual( "Felix", cl.GetName());

                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_4_Async(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                Aver.AreEqual( "Felix", cl.GetName());//alloc first

                //Testing overloaded calls via CallSlot
                Aver.AreEqual( "Felix",   cl.Async_GetName()  .GetValue<string>());
                Aver.AreEqual( "Felix23", cl.Async_GetName(23).GetValue<string>());
                Aver.AreEqual( "Felix42", cl.Async_GetName(42).GetValue<string>());
                Aver.AreEqual( "Felix",   cl.Async_GetName()  .GetValue<string>());

                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_4_AsyncReactor(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var reactor = new CallReactor(
                                new Call( cl.Async_GetName(),   (r,c)=> Aver.AreEqual( "Felix", c.CallSlot.GetValue<string>()   ) ),
                                new Call( cl.Async_GetName(23), (r,c)=> Aver.AreEqual( "Felix23", c.CallSlot.GetValue<string>() ) ),
                                new Call( cl.Async_GetName(42), (r,c)=> Aver.AreEqual( "Felix42", c.CallSlot.GetValue<string>() ) ),
                                new Call( cl.Async_GetName(2, DateTime.Now), (r,c)=> Aver.IsTrue( c.CallSlot.GetValue<string>().StartsWith("Felix2") ) )
                              );

                reactor.Wait();

                dumpBindingTransports( cl.Binding );
                Aver.IsTrue(reactor.Finished);
            }

        }


        public static void TestContractB_4_Parallel(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;


            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                //Use the same client.....
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);

                Aver.AreEqual( "Felix1223", cl.GetName(1223));//alloc server

                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );

                                                //Testing overloaded calls
                                                var result = cl.GetName(i);
                                                Aver.AreEqual( "Felix{0}".Args(i), result);
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Glue Test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( cl.Binding );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }

        public static void TestContractB_4_Marshalling_Parallel(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;


            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                //Use the same client.....
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);

                Aver.AreEqual( "Felix1223", cl.GetNameMar(1223));//alloc server

                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );

                                                //Testing overloaded calls
                                                var result = cl.GetNameMar(i);
                                                Aver.AreEqual( "Felix{0}".Args(i), result);
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Glue Test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( cl.Binding );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }





        public static void TASK_TestContractB_4_Parallel(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;


            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                //Use the same client.....
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);

                Aver.AreEqual( "Felix1223", cl.GetName(1223));//alloc server

                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );

                                                //Testing overloaded calls USING asTASK property
                                                var result = cl.Async_GetName(i).AsTask.Result.GetValue<string>();
                                                Aver.AreEqual( "Felix{0}".Args(i), result);
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Glue Test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( cl.Binding );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }






        public static void TestContractB_4_Parallel_ManyClients(string CONF_SRC, bool threadSafe)
        {
            const int CNT = 10000;
            const int CLCNT = 157;

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var rnd = new Random();
                var rndBound = (int)(CLCNT * 1.3751d);
                var clients = new List<TestContractBClient>();

                for(var i=0; i<CLCNT; i++)
                {
                  var cl = new TestContractBClient(app.ConfigRoot.AttrByName(threadSafe?"cs":"cs2").Value);
                  Aver.AreEqual( "Felix1223", cl.GetName(1223));//alloc server
                  clients.Add(cl);
                }


                var set = new HashSet<int>();
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //.....for making many parallel calls
                System.Threading.Tasks.Parallel.For(0, CNT,
                                              (i, loop)=>
                                              {
                                                var id = System.Threading.Thread.CurrentThread.ManagedThreadId;

                                                lock(set)
                                                 set.Add( id );

                                                var idx = rnd.Next( rndBound );
                                                if (idx>=clients.Count) idx = clients.Count-1;
                                                var cl = clients[idx];

                                                //Testing overloaded calls
                                                Aver.AreEqual( "Felix{0}".Args(i), cl.GetName(i));
                                              });
                var elps = watch.ElapsedMilliseconds;

                Console.WriteLine("Parallel Many Clients Glue test made {0} calls in {1} ms at {2} call/sec and was done by these threads:".Args(CNT, elps, CNT / (elps / 1000d)) );
                dumpBindingTransports( app.Glue.Bindings.First() );
                var cnt = 0;
                foreach(var id in set)
                {
                   Console.Write( id+", " );
                   cnt++;
                }
                Console.WriteLine( cnt + " total");

            }

        }





        public static void TestContractB_5(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var ret = cl.GetPersonalData(new int[]{1,23,97});

                Aver.AreEqual( 3, ret.Count);

                Aver.IsTrue(  1 == ret[0].ID);
                Aver.IsTrue( 23 == ret[1].ID);
                Aver.IsTrue( 97 == ret[2].ID);

                Aver.AreEqual( "Oleg1",  ret[0].FirstName);
                Aver.AreEqual( "Oleg23", ret[1].FirstName);
                Aver.AreEqual( "Oleg97", ret[2].FirstName);

                Aver.AreEqual( "Popov1",  ret[0].LastName);
                Aver.AreEqual( "Popov23", ret[1].LastName);
                Aver.AreEqual( "Popov97", ret[2].LastName);

                Aver.AreEqual( false, ret[0].Certified);
                Aver.AreEqual( false, ret[1].Certified);
                Aver.AreEqual( false, ret[2].Certified);

                dumpBindingTransports( cl.Binding );
            }

        }

        public static void TestContractB_6(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var ret = cl.GetPersonalData(new int[]{1,23,97}, true, 127000m);

                Aver.AreEqual( 3, ret.Count);

                Aver.IsTrue(  1 == ret[0].ID);
                Aver.IsTrue( 23 == ret[1].ID);
                Aver.IsTrue( 97 == ret[2].ID);

                Aver.AreEqual( "Oleg1",  ret[0].FirstName);
                Aver.AreEqual( "Oleg23", ret[1].FirstName);
                Aver.AreEqual( "Oleg97", ret[2].FirstName);

                Aver.AreEqual( "Popov1",  ret[0].LastName);
                Aver.AreEqual( "Popov23", ret[1].LastName);
                Aver.AreEqual( "Popov97", ret[2].LastName);

                Aver.AreEqual( true, ret[0].Certified);
                Aver.AreEqual( true, ret[1].Certified);
                Aver.AreEqual( true, ret[2].Certified);

                Aver.AreEqual( 127000m, ret[0].Salary);
                Aver.AreEqual( 127000m, ret[1].Salary);
                Aver.AreEqual( 127000m, ret[2].Salary);

                dumpBindingTransports( cl.Binding );
            }

        }


        public static void TestContractB_7(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var ret = cl.GetDailyStatuses(7);

                Aver.AreEqual( 7, ret.Count);
                var dt = new DateTime(1980,1,1);

                Aver.AreEqual( 100,        ret[dt].Count);
                Aver.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Aver.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Aver.AreEqual( "Popov99",  ret[dt][99].LastName);
                Aver.AreEqual( 99000m,     ret[dt][99].Salary);

                dt = dt.AddSeconds(ret.Count-1);

                Aver.AreEqual( 100,        ret[dt].Count);
                Aver.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Aver.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Aver.AreEqual( "Popov99",  ret[dt][99].LastName);
                Aver.AreEqual( 99000m,     ret[dt][99].Salary);

                dumpBindingTransports( cl.Binding );
            }

        }


        public static void TestContractB_8(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                var ret = cl.GetDailyStatuses(150);

                Aver.AreEqual( 150, ret.Count);
                var dt = new DateTime(1980,1,1);

                Aver.AreEqual( 100,        ret[dt].Count);
                Aver.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Aver.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Aver.AreEqual( "Popov99",  ret[dt][99].LastName);
                Aver.AreEqual( 99000m,     ret[dt][99].Salary);

                dt = dt.AddSeconds(ret.Count-1);

                Aver.AreEqual( 100,        ret[dt].Count);
                Aver.AreEqual( "Oleg0",    ret[dt][0].FirstName);
                Aver.AreEqual( "Oleg99",   ret[dt][99].FirstName);
                Aver.AreEqual( "Popov99",  ret[dt][99].LastName);
                Aver.AreEqual( 99000m,     ret[dt][99].Salary);

                dumpBindingTransports( cl.Binding );
            }

        }


        //this will throw
        public static void TestContractB_9(string CONF_SRC)
        {
            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                var cl = new TestContractBClient(app.ConfigRoot.AttrByName("cs").Value);

                Exception err = null;
                try
                {
                  cl.GetDailyStatuses(1);//this is needed to init type registry for sync binding
                                         //because otherwise it will abort the channel instead of marshalling exception back
                  cl.GetDailyStatuses(550);
                }
                catch(Exception error)
                {
                  err = error;
                }
                Aver.IsNotNull( err );
                Aver.AreObjectsEqual( typeof(RemoteException), err.GetType());

                Aver.IsTrue( err.Message.Contains("MessageSizeException"));
                Aver.IsTrue( err.Message.Contains("exceeds limit"));
            }

        }





    }
}
