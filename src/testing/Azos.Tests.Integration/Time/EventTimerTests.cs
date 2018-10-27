/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

using Azos;
using Azos.ApplicationModel;
using Azos.Scripting;
using Azos.Time;

namespace Azos.Tests.Integration.Time
{
                           public class TeztHandler : IEventHandler
                           {
                             public static List<string> s_List = new List<string>();

                             public TeztHandler(string name, bool podonok)
                             {
                               Name = "{0}.{1}".Args(name, podonok);
                             }

                             public readonly string Name;

                             public void EventHandlerBody(Event sender)
                             {
                                var who = "{0}::{1}".Args(sender.Context, Name);
                                lock(s_List)
                                 s_List.Add(who);
                             }

                             public void EventStatusChange(Event sender, EventStatus priorStatus)
                             {
                             }

                             public void EventDefinitionChange(Event sender, string parameterName)
                             {
                             }
                           }



  /// <summary>
  /// To perform tests below MySQL server instance is needed.
  /// Look at CONNECT_STRING constant
  /// </summary>
  [Runnable]
  public class EventTimerTests
  {

        public const string CONFIG1=@"
nfx
{
  event-timer
  {
    resolution-ms=150
  }
}";

        public const string CONFIG2_ARZAMAS=@"
nfx
{
  time-location {utc-offset='04:45:10' description='Arzamas-7'}

  event-timer
  {
    resolution-ms=150
  }
}";

    public const string CONFIG3_HANDLERS=@"
nfx
{
  event-timer
  {
    resolution-ms=150
    event{ name='A' interval='0:0:1' Context='ItonTV' handler{type='Azos.Tests.Integration.Time.TeztHandler, Azos.Tests.Integration' arg0='Gorin' arg1='true'} }
    event{ name='B' interval='0:0:3' Context='Nativ'  handler{type='Azos.Tests.Integration.Time.TeztHandler, Azos.Tests.Integration' arg0='Kedmi' arg1='false'} }
  }
}";

       public static readonly TimeSpan ARZAMAS_OFFSET = TimeSpan.Parse("04:45:10");


        [Run]
        public void T1()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Aver.AreEqual(150, app.EventTimer.ResolutionMs);

              var lst = new List<string>();

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), Context = 123 };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), Context = 234 };

              Aver.AreEqual(2, app.EventTimer.Events.Count);
              Aver.AreObjectsEqual(123, app.EventTimer.Events["A"].Context);
              Aver.AreObjectsEqual(234, app.EventTimer.Events["B"].Context);


              Thread.Sleep(10000);

              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Aver.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Aver.AreEqual(10, lst.Count(s=>s=="a"));
              Aver.AreEqual(4, lst.Count(s=>s=="b"));
            }
        }

        [Run]
        public void T2()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Aver.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1)};
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), Enabled = false};

              Thread.Sleep(10000);

              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Aver.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Aver.AreEqual(10, lst.Count(s=>s=="a"));
              Aver.AreEqual(0, lst.Count(s=>s=="b"));
            }
        }


        [Run]
        public void T3()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Aver.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();

              var appNow = app.LocalizedTime;

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), StartDate = appNow.AddSeconds(3.0)};
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), StartDate = appNow.AddSeconds(-100.0)};

              Thread.Sleep(10000);

              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Aver.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Aver.AreEqual(7, lst.Count(s=>s=="a"));
              Aver.AreEqual(4, lst.Count(s=>s=="b"));
            }
        }


        [Run]
        public void T4()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              Aver.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();

              var limpopoTime = new TimeLocation(new TimeSpan(0, 0, 2), "Limpopo Time is 2 seconds ahead of UTC");

              var utcNow = app.TimeSource.UTCNow;

              Console.WriteLine( utcNow.Kind );

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}, new TimeSpan(0,0,1)) { TimeLocation = limpopoTime, StartDate = utcNow.AddSeconds(-2) };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}, new TimeSpan(0,0,3)) { TimeLocation = TimeLocation.UTC, StartDate = utcNow };

              Thread.Sleep(10000);

              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Aver.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Aver.AreEqual(10, lst.Count(s=>s=="a"));
              Aver.AreEqual(4, lst.Count(s=>s=="b"));
            }
        }


        [Run]
        public void T5()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG2_ARZAMAS.AsLaconicConfig()))
            {
              Aver.AreEqual(150, app.EventTimer.ResolutionMs);


              var lst = new List<string>();

              var utcNow = app.TimeSource.UTCNow;

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), StartDate = DateTime.SpecifyKind(utcNow + ARZAMAS_OFFSET, DateTimeKind.Local) };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), StartDate = utcNow };

              Thread.Sleep(10000);

              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Aver.AreEqual(0, app.EventTimer.Events.Count);

              Console.WriteLine(string.Join(" , ", lst));
              Aver.AreEqual(10, lst.Count(s=>s=="a"));
              Aver.AreEqual(4, lst.Count(s=>s=="b"));
            }
        }


        [Run]
        public void T6()
        {
            var lst = TeztHandler.s_List;
            lst.Clear();

            using(var app = new ServiceBaseApplication(null, CONFIG3_HANDLERS.AsLaconicConfig()))
            {
              Thread.Sleep(10000);
            }

            Console.WriteLine(string.Join(" , ", lst));
            Aver.AreEqual(14, lst.Count);
            Aver.AreEqual(10, lst.Count(s=>s=="ItonTV::Gorin.True"));
            Aver.AreEqual(4, lst.Count(s=>s=="Nativ::Kedmi.False"));
        }



        [Run]
        public void T7()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {

              new Event(app.EventTimer, "A")
              {
                Interval = new TimeSpan(0,0,1),
                Context = 123,
                StartDate = new DateTime(2079, 12, 12),
                EndDate = new DateTime(1980, 1, 1)
              };

              Aver.IsTrue(EventStatus.NotStarted == app.EventTimer.Events["A"].Status);
              Thread.Sleep(2000);
              Aver.IsTrue(EventStatus.Invalid == app.EventTimer.Events["A"].Status);

              app.EventTimer.Events["A"].StartDate = new DateTime(1979, 1,1);
              Thread.Sleep(2000);
              Aver.IsTrue(EventStatus.Expired == app.EventTimer.Events["A"].Status);

              app.EventTimer.Events["A"].EndDate = new DateTime(2979, 1,1);
              Thread.Sleep(2000);
              Aver.IsTrue(EventStatus.Started == app.EventTimer.Events["A"].Status);
            }
        }



         [Run]
        public void T8()
        {
            using(var app = new ServiceBaseApplication(null, CONFIG1.AsLaconicConfig()))
            {
              var lst = new List<string>();

              new Event(app.EventTimer, "A", (e) => {lock(lst) lst.Add("a");}) { Interval = new TimeSpan(0,0,1), Context = 123, MaxCount=2 };
              new Event(app.EventTimer, "B", (e) => {lock(lst) lst.Add("b");}) { Interval = new TimeSpan(0,0,3), Context = 234 };

              Thread.Sleep(10000);

              app.EventTimer.Events["A"].Dispose();
              app.EventTimer.Events["B"].Dispose();

              Console.WriteLine(string.Join(" , ", lst));
              Aver.AreEqual(2, lst.Count(s=>s=="a"));
              Aver.AreEqual(4, lst.Count(s=>s=="b"));
            }
        }


  }
}
