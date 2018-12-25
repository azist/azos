/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Scripting;

using Azos.Conf;
using Azos.Apps;
using Azos.Data.Access.Cache;

namespace Azos.Tests.Unit.Cache
{
    [Runnable]
    public class StoreTests
    {
 private const string CONF_SRC =@"
 nfx
 {
   cache
   {
     store //this is default store
     {
       parallel-sweep=true
       instrumentation-enabled=true
       table{ name='doctor' bucket-count=1234567 rec-per-page=7 lock-count=19 max-age-sec=193 parallel-sweep=true}
       table{ name='patient' bucket-count=451000000 rec-per-page=17 lock-count=1025 max-age-sec=739 parallel-sweep=true}
     }
     store
     {
       name='banking'
       table{ name='account' bucket-count=789001 rec-per-page=23 lock-count=149 max-age-sec=12000 parallel-sweep=true}
       table{ name='balance' bucket-count=1023 rec-per-page=3 lock-count=11 max-age-sec=230000}

     }
   }
 }
 ";

        [Run]
        public void Configuration_NamedStore()
        {

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                using(var store = new CacheStore(NOPApplication.Instance, "BANKING"))
                {
                  store.Configure(null);

                  Aver.AreEqual(789001, store.TableOptions["Account"].BucketCount);
                  Aver.AreEqual(23,     store.TableOptions["Account"].RecPerPage);
                  Aver.AreEqual(149,    store.TableOptions["Account"].LockCount);
                  Aver.AreEqual(12000,  store.TableOptions["Account"].MaxAgeSec);
                  Aver.AreEqual(true,   store.TableOptions["Account"].ParallelSweep);

                  Aver.AreEqual(1023,   store.TableOptions["BaLaNCE"].BucketCount);
                  Aver.AreEqual(3,      store.TableOptions["BaLaNCE"].RecPerPage);
                  Aver.AreEqual(11,     store.TableOptions["BaLaNCE"].LockCount);
                  Aver.AreEqual(230000, store.TableOptions["BaLaNCE"].MaxAgeSec);
                  Aver.AreEqual(false,  store.TableOptions["BaLaNCE"].ParallelSweep);

                  var tbl = store["AccoUNT"];
                  Aver.AreEqual(789001,    tbl.BucketCount);
                  Aver.AreEqual(23,        tbl.RecPerPage);
                  Aver.AreEqual(789001*23, tbl.Capacity);
                  Aver.AreEqual(149,       tbl.LockCount);
                  Aver.AreEqual(12000,     tbl.MaxAgeSec);
                  Aver.AreEqual(true,      tbl.ParallelSweep);
                }
            }
        }

        [Run]
        public void Configuration_UnNamedStore()
        {

            var conf = LaconicConfiguration.CreateFromString(CONF_SRC);
            using( var app = new AzosApplication(null, conf.Root))
            {
                using(var store = new CacheStore(NOPApplication.Instance, "SomeStoreThat will be configured from default store without name"))
                {
                  store.Configure(null);

                  Aver.AreEqual(true, store.ParallelSweep);
                  Aver.AreEqual(true, store.InstrumentationEnabled);

                  Aver.AreEqual(1234567, store.TableOptions["doctor"].BucketCount);
                  Aver.AreEqual(7,       store.TableOptions["doctor"].RecPerPage);
                  Aver.AreEqual(19,      store.TableOptions["doctor"].LockCount);
                  Aver.AreEqual(193,     store.TableOptions["doctor"].MaxAgeSec);
                  Aver.AreEqual(true,    store.TableOptions["doctor"].ParallelSweep);

                  Aver.AreEqual(451000000, store.TableOptions["PATIENT"].BucketCount);
                  Aver.AreEqual(17,        store.TableOptions["PATIENT"].RecPerPage);
                  Aver.AreEqual(1025,      store.TableOptions["PATIENT"].LockCount);
                  Aver.AreEqual(739,       store.TableOptions["PATIENT"].MaxAgeSec);
                  Aver.AreEqual(true,      store.TableOptions["PATIENT"].ParallelSweep);

                }
            }
        }


        [Run]
        public void Basic_Put_Get()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              var tbl1 = store["t1"];

              for(int i=0; i<1000; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Froemson-{0}".Args(i), IsGood = i%7==0};
                  Aver.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Aver.AreEqual( 1000, tbl1.Count);

              Aver.AreEqual( "Froemson-12",  tbl1.Get(12)     .ValueAs<businessObject>().Name );
              Aver.AreEqual( "Froemson-115",  store["t1", 115].ValueAs<businessObject>().Name );
              Aver.AreEqual( "Froemson-999", tbl1.Get(999)    .ValueAs<businessObject>().Name );
              Aver.AreEqual( true, tbl1.Get(7).ValueAs<businessObject>().IsGood );
            }
        }

        [Run]
        public void Basic_Put_Get_HitCount()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              var tbl1 = store["t1"];

              for(int i=0; i<100; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Froemson-{0}".Args(i), IsGood = i%7==0};
                  Aver.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Aver.AreEqual( 100, tbl1.Count);

              Aver.AreEqual( "Froemson-12",  tbl1.Get(12)     .ValueAs<businessObject>().Name );
              Aver.AreEqual( "Froemson-12",  store["t1", 12]  .ValueAs<businessObject>().Name );
              Aver.AreEqual( true, tbl1.Get(7).ValueAs<businessObject>().IsGood );

              Aver.AreEqual(1, tbl1.Get(0).HitCount);
              Aver.AreEqual(3, tbl1.Get(12).HitCount);
              Aver.AreEqual(2, tbl1.Get(7).HitCount);
            }
        }

        [Run]
        public void Basic_Put_Get_Remove()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              var tbl1 = store["t1"];

              for(int i=0; i<1000; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Xroemson-{0}".Args(i), IsGood = i%7==0};
                  Aver.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Aver.AreEqual( 1000, tbl1.Count);

              Aver.IsTrue( tbl1.Remove(115) );

               Aver.AreEqual( 999, tbl1.Count);

              Aver.AreEqual( "Xroemson-12",  tbl1.Get(12)  .ValueAs<businessObject>().Name );
              Aver.IsNull(tbl1.Get(115) );
              Aver.AreEqual( "Xroemson-999", tbl1.Get(999) .ValueAs<businessObject>().Name );
              Aver.AreEqual( true, tbl1.Get(7).ValueAs<businessObject>().IsGood );
            }
        }


        [Run]
        public void Put_Expire_Get()
        {
            using( var store = new CacheStore(NOPApplication.Instance))
            {
              var tbl1 = store["t1"];

              for(int i=0; i<1000; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Suvorov-{0}".Args(i), IsGood = i%7==0};
                  Aver.IsTrue( tbl1.Put((ulong)i, bo, i<500 ? 5 : 100 ) );
              }

              Aver.AreEqual( 1000, tbl1.Count);

              //warning: this timeout depends on how fast the store will purge garbage
              for(var spin=0; spin<20 && tbl1.Count!=500; spin++)
              {
                  System.Threading.Thread.Sleep( 1000 );
              }

              Aver.AreEqual( 500, tbl1.Count);

              Aver.IsNull(tbl1.Get(1) );
              Aver.IsNull(tbl1.Get(499) );
              Aver.AreEqual( "Suvorov-500",  tbl1.Get(500).ValueAs<businessObject>().Name );
              Aver.AreEqual( "Suvorov-999",  tbl1.Get(999).ValueAs<businessObject>().Name );
            }
        }

        [Run]
        public void Get_Does_Not_See_Expired()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              var tbl1 = store["t1"];

              for(int i=0; i<200; i++)
              {
                  var bo = new businessObject { ID = i, Name = "Suvorov-{0}".Args(i), IsGood = i%7==0};
                  Aver.IsTrue( tbl1.Put((ulong)i, bo) );
              }

              Aver.AreEqual( 200, tbl1.Count);

              Aver.IsNotNull( tbl1.Get(123, 3) );//3 sec old

              System.Threading.Thread.Sleep( 15000 );// this timeout depends on store sweep thread speed, 15 sec span should be enough

              Aver.IsNull( tbl1.Get(123, 3) );//3 sec old
              Aver.AreEqual( "Suvorov-123",  tbl1.Get(123, 25).ValueAs<businessObject>().Name ); //25 sec old
            }
        }


        [Run]
        public void Get_Does_Not_See_AbsoluteExpired()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              var tbl1 = store["t1"];

              var utcNow = Ambient.UTCNow;
              var utcExp = utcNow.AddSeconds(5);


              var bo1 = new businessObject { ID = 1, Name = "Suvorov-1", IsGood = true};
              Aver.IsTrue( tbl1.Put(1, bo1, maxAgeSec: int.MaxValue, absoluteExpirationUTC: utcExp) );

              var bo2 = new businessObject { ID = 2, Name = "Meduzer-2", IsGood = false};
              Aver.IsTrue( tbl1.Put(2, bo2) );



              Aver.AreEqual( 2, tbl1.Count);

              System.Threading.Thread.Sleep( 15000 );// this timeout depends on store sweep thread speed, 15 sec span should be enough

              Aver.AreEqual( 1, tbl1.Count);

              Aver.IsNull( tbl1.Get(1) );// expired
              Aver.AreEqual( "Meduzer-2",  tbl1.Get(2).ValueAs<businessObject>().Name ); //still there
            }
        }


        [Run]
        public void Collision()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              store.TableOptions.Register( new TableOptions("t1",3 ,7) );
              var tbl1 = store["t1"];

              Aver.AreEqual( 21, tbl1.Capacity);

              var obj1 = new businessObject { ID = 0, Name = "Suvorov-1", IsGood = true};
              var obj2 = new businessObject { ID = 21, Name = "Kozloff-21", IsGood = false};

              //because of the table size 3 x 7 both 0 and 21 map onto the same slot causing collision
              Aver.IsTrue( tbl1.Put(0, obj1) );
              Aver.IsFalse( tbl1.Put(21, obj2) );

              Aver.IsNull( tbl1.Get(0) );
              Aver.AreEqual( "Kozloff-21",  tbl1.Get(21).ValueAs<businessObject>().Name );
            }
        }

        [Run]
        public void Collision_Prevented_Priority()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              store.TableOptions.Register( new TableOptions("t1",3 ,7) );
              var tbl1 = store["t1"];

              Aver.AreEqual( 21, tbl1.Capacity);

              //because of the table size 3 x 7 both 0 and 21 map onto the same slot causing collision
              var obj1 = new businessObject { ID = 0,  Name = "Suvorov-1", IsGood = true};
              var obj2 = new businessObject { ID = 21, Name = "Kozloff-21", IsGood = false};
              Aver.IsTrue( tbl1.Put(0, obj1, priority: 10) ); //Suvorov has higher priority than Kozloff
              Aver.IsFalse( tbl1.Put(21, obj2) );             //so collision does not happen

              Aver.AreEqual( "Suvorov-1",  tbl1.Get(0).ValueAs<businessObject>().Name );
              Aver.IsNull( tbl1.Get(21) );
            }
        }


        [Run]
        public void ComplexKeyHashingStrategy_1()
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              var strat = new ComplexKeyHashingStrategy(store);

              strat.Put("t1", "A", new businessObject { ID = 000,  Name = "Adoyan",   IsGood = true});
              strat.Put("t1", "B", new businessObject { ID = 234,  Name = "Borisenko", IsGood = false});
              strat.Put("t1", "C", new businessObject { ID = 342,  Name = "Carov",    IsGood = true});
              strat.Put("t2", "A", new businessObject { ID = 2000,  Name = "2Adoyan",   IsGood = true});
              strat.Put("t2", "B", new businessObject { ID = 2234,  Name = "2Borisenko", IsGood = false});
              strat.Put("t2", "C", new businessObject { ID = 2342,  Name = "2Carov",    IsGood = true});

              Aver.AreEqual("Adoyan",    ((businessObject)strat.Get("t1", "A")).Name);
              Aver.AreEqual("Borisenko", ((businessObject)strat.Get("t1", "B")).Name);
              Aver.AreEqual("Carov",     ((businessObject)strat.Get("t1", "C")).Name);

              Aver.AreEqual("2Adoyan",    ((businessObject)strat.Get("t2", "A")).Name);
              Aver.AreEqual("2Borisenko", ((businessObject)strat.Get("t2", "B")).Name);
              Aver.AreEqual("2Carov",     ((businessObject)strat.Get("t2", "C")).Name);
            }
        }

        [Run("cnt=1000    k=0.1")]
        [Run("cnt=10000   k=0.25")]
        [Run("cnt=100000  k=0.5")]
        public void ComplexKeyHashingStrategy_2(int CNT, double k)
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {

              var strat = new ComplexKeyHashingStrategy(store);

              for(var i=0; i<CNT; i++)
                strat.Put("t1", "Key-"+i, new businessObject { ID = i,  Name = "Gagarin-"+i,   IsGood = true});


              var collision = 0;
              for(var i=0; i<CNT; i++)
              {
                Aver.IsNull( strat.Get("t1", "KeI-"+i) );
                Aver.IsNull( strat.Get("t1", "Ke y-"+i) );
                Aver.IsNull( strat.Get("t1", "Key "+i) );

                var o = strat.Get("t1", "Key-"+i);
                if (o!=null)
                 Aver.AreEqual(i, ((businessObject)o).ID);
                else
                 collision++;
              }

              Console.WriteLine("Did {0}, collision {1}", CNT, collision);
              Aver.IsTrue(collision < (int)(CNT*k));
            }
        }



        [Run("cnt=1000    k=0.1")]
        [Run("cnt=10000   k=0.25")]
        [Run("cnt=100000  k=0.5")]
        public void ComplexKeyHashingStrategy_3(int CNT, double k)
        {
            using(var store = new CacheStore(NOPApplication.Instance))
            {
              var strat = new ComplexKeyHashingStrategy(store);

              for(var i=0; i<CNT; i++)
                strat.Put("t1", "ThisIsAKeyLongerThanEight-"+i, new businessObject { ID = i,  Name = "Gagarin-"+i,   IsGood = true});

              var collision = 0;
              for(var i=0; i<CNT; i++)
              {
                Aver.IsNull( strat.Get("t1", "ThisAKeyLongerThanEight-"+i) );
                Aver.IsNull( strat.Get("t1", "ThusIsAKeyLongerThanEight-"+i) );
                Aver.IsNull( strat.Get("t1", "ThisIsAKeyLongerEightThan-"+i) );
                var o =  strat.Get("t1", "ThisIsAKeyLongerThanEight-"+i);

                if (o!=null)
                {
                  Aver.AreEqual(i, ((businessObject)strat.Get("t1", "ThisIsAKeyLongerThanEight-"+i)).ID);
                  Aver.AreEqual(i, ((businessObject)strat.Get("t1", "ThisIsAKeyLongerThanEight-"+i)).ID);
                }
                else
                 collision++;
              }

              Console.WriteLine("Did {0}, collision {1}", CNT, collision);
              Aver.IsTrue(collision < (int)(CNT*k));

            }
        }

        [Run]
        public void Hashing_StringKey_1()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("123");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("223");
           Console.WriteLine( h2 );
        }

        [Run]
        public void Hashing_StringKey_2()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("abc");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("忁曨曣");
           Console.WriteLine( h2 );
           Aver.AreNotEqual(h1, h2);

           var h3 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("忁曣曣");
           Console.WriteLine( h3 );
           Aver.AreNotEqual(h3, h2);

           var h4 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("Abc");
           Console.WriteLine( h4 );
           Aver.AreNotEqual(h1, h4);
        }



        [Run]
        public void Hashing_StringKey_3()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("    012345678");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Console.WriteLine( h2 );
           Aver.AreNotEqual(h1, h2);
        }

        [Run]
        public void Hashing_StringKey_4()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Aver.AreEqual(h1, h2);
        }

        [Run]
        public void Hashing_StringKey_5()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678 and also some text follows that makes this much longer than others in the same test group");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678");
           Console.WriteLine( h2 );
           Aver.AreNotEqual(h1, h2);
        }

        [Run]
        public void Hashing_StringKey_6()
        {
           var h1 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678 and also some text follows that makes this much longer than others in the same test group");
           Console.WriteLine( h1 );

           var h2 = ComplexKeyHashingStrategy.DefaultStringKeyToCacheKey("1234012345678 and also some text follows that makes this much longer than others in the same test group");
           Aver.AreEqual(h1, h2);
        }



                private class businessObject
                {
                    public int ID;
                    public string Name;
                    public bool IsGood;
                }


    }



}