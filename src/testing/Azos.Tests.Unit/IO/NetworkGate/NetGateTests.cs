/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Scripting;

using Azos.IO.Net.Gate;

namespace Azos.Tests.Unit.IO.NetworkGate
{
    [Runnable]  
    public class NetGateTests
    {
       const string CONFIG_DEFAULT_DENY=
@"
gate
{
    //type='may inject type' 
    name='Default Gate'             
    enabled = true
            
    incoming
    {
        default-action=deny
                
        rule{name='AdminAccess'  order=1 action=allow from-addrs='170.12.14.12;170.12.14.13'}
        rule{name='Workgroups'    order=2 action=allow from-groups='home,work'}
                
        group
        {
          name='home'  
          address{name='Alex Berg Home' patterns='14.2.1*'}
          address{name='Denis Rozotoks Home' patterns='3.118.2.12,3.118.2.13,3.118.2.14'}
        } 
        group
        {
          name='work'  
          address{name='Alex Berg Office' patterns='45.2.2.12,45.2.2.75'}
          address{name='Denis Rozotoks Office' patterns='77.123.1.14'}
        } 
    }
}
";        



        const string CONFIG_DEFAULT_ALLOW=
@"
gate
{
    //type='may inject type' 
    name='Default Gate'             
    enabled = true
            
    incoming
    {
        default-action=allow
                
        rule{name='AdminAccess'   order=1 action=deny from-addrs='170.12.14.12;170.12.14.13'}
        rule{name='Workgroups'    order=2 action=deny from-groups='home,work'}
                
        group
        {
          name='home'  
          address{name='Alex Berg Home' patterns='14.2.1*'}
          address{name='Denis Rozotoks Home' patterns='3.118.2.12,3.118.2.13,3.118.2.14'}
        } 
        group
        {
          name='work'  
          address{name='Alex Berg Office' patterns='45.2.2.12,45.2.2.75'}
          address{name='Denis Rozotoks Office' patterns='77.123.1.14'}
        } 
    }
}
";        

       


        
        
        [Run]
        public void DefaultDeny()
        {
          using(var gate = new NetGate(null))
          {
              gate.Configure( CONFIG_DEFAULT_DENY.AsLaconicConfig() );  
              gate.Start();

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="123.0.0.1"}) );

              Rule rule;

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("AdminAccess", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.13"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("AdminAccess", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.1.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.1.46"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);


              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.1.18"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.2.18"}, out rule) );
              Aver.IsNull(rule);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.13"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.14"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);


              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.15"}, out rule) );
              Aver.IsNull(rule);


              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="77.123.1.14"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="45.2.2.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);


              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="45.2.2.75"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              
          }
        }



        [Run]
        public void DefaultAllow()
        {
          using(var gate = new NetGate(null))
          {
              gate.Configure( CONFIG_DEFAULT_ALLOW.AsLaconicConfig() );  
              gate.Start();

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="123.0.0.1"}) );

              Rule rule;

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("AdminAccess", rule.Name);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.13"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("AdminAccess", rule.Name);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.1.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.1.46"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);


              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.1.18"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="14.2.2.18"}, out rule) );
              Aver.IsNull(rule);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.13"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.14"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);


              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="3.118.2.15"}, out rule) );
              Aver.IsNull(rule);


              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="77.123.1.14"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="45.2.2.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);


              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="45.2.2.75"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Workgroups", rule.Name);

              
          }
        }


        [Run]
        public void SpeedSingleThread()
        {
          const int CNT = 2000000;

          using(var gate = new NetGate(null))
          {
              gate.Configure( CONFIG_DEFAULT_DENY.AsLaconicConfig() );  
              gate.Start();

              var sw = System.Diagnostics.Stopwatch.StartNew();
              for(int i=0;i<CNT;i++)
              {
                Rule rule;

                Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="45.2.2.75"}, out rule) );
                Aver.IsNotNull(rule);
                Aver.AreEqual("Workgroups", rule.Name);
              }

              var elapsed = sw.ElapsedMilliseconds;
              Console.WriteLine("{0} in {1}ms at {2} ops/sec".Args(CNT,elapsed, CNT / ((double)elapsed/1000)) );

          }
        }

        [Run]
        public void SpeedManyThreads()
        {
          const int CNT = 2000000;

          using(var gate = new NetGate(null))
          {
              gate.Configure( CONFIG_DEFAULT_DENY.AsLaconicConfig() );  
              gate.Start();
            
              var sw = System.Diagnostics.Stopwatch.StartNew();
              System.Threading.Tasks.Parallel.For(0,CNT,
                (i)=>
                {
                  Rule rule;

                  Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="45.2.2.75"}, out rule) );
                  Aver.IsNotNull(rule);
                  Aver.AreEqual("Workgroups", rule.Name);
                });


               

              var elapsed = sw.ElapsedMilliseconds;
              Console.WriteLine("{0} in {1}ms at {2} ops/sec".Args(CNT,elapsed, CNT / ((double)elapsed/1000)) );

          }
        }

        const string CONFIG_SESSION=
@"
gate
{
    incoming
    {
        default-action=allow
                
        rule{name='Session Flood'   order=1 action=deny from-expression='$newSession>3'}
        var-def{name='newSession' decay-by='1' interval-sec='1'} 
    }
}
";        

        [Run]
        public void Variables_SessionFlood_1()
        {
          using(var gate = new NetGate(null))
          {
              gate.Configure( CONFIG_SESSION.AsLaconicConfig() );  
              gate.Start();

              
              Rule rule;

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNull(rule);

              gate.IncreaseVariable(TrafficDirection.Incoming, "170.12.14.12", "newSession", 5);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Session Flood", rule.Name);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.1.99.144"}, out rule) );
              Aver.IsNull(rule);
             
              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Session Flood", rule.Name);

              System.Threading.Thread.Sleep(5000);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNull(rule);
          }
        }


        [Run]
        public void Variables_SessionFlood_2()
        {
          using(var gate = new NetGate(null))
          {
              gate.Configure( CONFIG_SESSION.AsLaconicConfig() );  
              gate.Start();

              
              Rule rule;

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNull(rule);

              gate.IncreaseVariable(TrafficDirection.Incoming, "5.5.5.5", "newSession", 5);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNull(rule);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.1.99.144"}, out rule) );
              Aver.IsNull(rule);
             
              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="5.5.5.5"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Session Flood", rule.Name);

              System.Threading.Thread.Sleep(5000);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="5.5.5.5"}, out rule) );
              Aver.IsNull(rule);

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.1.99.144"}, out rule) );
              Aver.IsNull(rule);
          }
        }



        
        [Run]
        public void Variables_SessionFlood_Parallel_Create_Decay()
        {
          using(var gate = new NetGate(null))
          {
              gate.Configure( CONFIG_SESSION.AsLaconicConfig() );  
              gate.Start();

              
              Rule rule;

              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="170.12.14.12"}, out rule) );
              Aver.IsNull(rule);

              gate.IncreaseVariable(TrafficDirection.Incoming, "5.5.5.5", "newSession", 8);

              Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress="5.5.5.5"}, out rule) );
              Aver.IsNotNull(rule);
              Aver.AreEqual("Session Flood", rule.Name);

              const int CNT = 10000;
              System.Threading.Tasks.Parallel.For(0, CNT,
                 (i)=>
                 {    
                      var address = "addr-{0}".Args(i);
                      gate.IncreaseVariable(TrafficDirection.Incoming, address, "newSession", 8);
                     
                      Rule lr;
                      Aver.IsTrue(GateAction.Deny == gate.CheckTraffic( new GeneralTraffic{FromAddress=address}, out lr) );
                      Aver.IsNotNull(lr);
                      Aver.AreEqual("Session Flood", lr.Name);
                      System.Threading.Thread.Sleep(App.Random.NextScaledRandomInteger(1,5));
                 });
             
              Aver.AreEqual(CNT+1,  gate[TrafficDirection.Incoming].NetState.Count);
              System.Threading.Thread.Sleep(12000);
              Aver.AreEqual( 0, gate[TrafficDirection.Incoming].NetState.Count);
           
              Aver.IsTrue(GateAction.Allow == gate.CheckTraffic( new GeneralTraffic{FromAddress="5.5.5.5"}, out rule) );
              Aver.IsNull(rule);
          }
        }



        
    }
}
