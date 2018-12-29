/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

// Configure Windows 8.1 Undefined Cable Network to be Private (PowerShell/PSH)
// 1. Get-NetConnectionProfile
// 2. Set-NetConnectionProfile -InterfaceIndex [INDEX_FROM_1.] -NetworkCategory Private

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

using System.Threading.Tasks;


using Azos;
using Azos.Data;
using Azos.Glue;
using Azos.Glue.Protocol;
using Azos.Security;
using Azos.WinForms;
using TestBusinessLogic;

namespace WinFormsTest
{
  #pragma warning disable 0649,0169,0067

  public partial class GlueForm : System.Windows.Forms.Form
  {
        public class BadContract : ClientEndPoint
        {
           public BadContract(IGlue glue, string node, Azos.Glue.Binding binding = null) : base(glue, node, binding) {}

           public override Type Contract
           {
               get { return typeof(BadContract); }
           }

          #region Contract Methods

            public string Echo(string text)
            {
              return Async_Echo(text).GetValue<string>();
            }

            public CallSlot Async_Echo(string text)
            {
              var request = new RequestAnyMsg(GetType().GetMethod("Echo"), RemoteInstance, new object[]{ text });
              return DispatchCall(request);
            }

            public void Notify(string text)
            {
              var call = Async_Notify(text);
              if (call.CallStatus!= CallStatus.Dispatched)
                throw new ClientCallException(call.CallStatus);
            }

            public CallSlot Async_Notify(string text)
            {
              var request = new RequestAnyMsg(GetType().GetMethod("Notify"), RemoteInstance, new object[]{ text });

              return DispatchCall(request);
            }

          #endregion
        }


        public GlueForm()
        {
            InitializeComponent();
            cbo.SelectedIndex = 0;
        }


        public IGlue Glue => FormsAmbient.App.Glue;

        private void warmup()
        {
          var lst = new List<Task>();
          for(var ti=0; ti<Environment.ProcessorCount*8; ti++)
           lst.Add( Task.Factory.StartNew(
             () =>
             {
               long sum = 0;
               for(var k=0; k<500000000; k++)
                sum +=k;
              return sum;
             }
             ));

          Task.WaitAll(lst.ToArray());
          GC.Collect(2);
        }


        int ECHO_COUNT = 0;

        private void button1_Click(object sender, EventArgs ea)
        {
          ECHO_COUNT++;

          var client = new JokeContractClient(Glue, cbo.Text);

          client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );


          try
          {
            var echoed = chkUnsecureEcho.Checked ? client.UnsecureEcho("Hello!") : client.Echo("Hello!");
            Text = echoed + "  " + ECHO_COUNT.ToString() + " times";
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }

          client.Dispose();
        }

        private void button2_Click(object sender, EventArgs ea)
        {
          var CNT = edRepeat.Text.AsInt();

          var client = new JokeContractClient(Glue, cbo.Text);

          client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

        //  client.ReserveTransport = true;
          var w = Stopwatch.StartNew();

          try
          {
            if (chkUnsecureEcho.Checked)
            {
                for(int i=0; i<CNT; i++)
                 client.UnsecureEcho("Hello!");
            }
            else
            {
               for(int i=0; i<CNT; i++)
                 client.Echo("Hello!");
            }

            w.Stop();
            Text = "Echoed  "+CNT.ToString()+" in " + w.ElapsedMilliseconds + " ms";
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }

          client.Dispose();
        }

        int NOTIFY_COUNT = 0;

        private void button3_Click(object sender, EventArgs ea)
        {
          NOTIFY_COUNT++;

          var client = new JokeContractClient(Glue, cbo.Text);

          try
          {
            client.Notify("Notify!");
            Text = "Notified " + NOTIFY_COUNT.ToString() + " times";
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }

          client.Dispose();
        }

        private void button4_Click(object sender, EventArgs ea)
        {
          var CNT = edRepeat.Text.AsInt();

          var client = new JokeContractClient(Glue, cbo.Text);

         // client.ReserveTransport = true;
          var w = Stopwatch.StartNew();
          int i = 0;
          try
          {
            for(; i<CNT; i++)
              client.Notify("Notify!");

            w.Stop();
            Text = "Notified  "+CNT.ToString()+" in " + w.ElapsedMilliseconds + " ms";
          }
          catch (Exception e)
          {
            Text = string.Format("After {0} times: {1}", i, e.ToMessageWithType());
          }

          client.Dispose();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private JokeCalculatorClient calc = null;

        private void btnInit_Click(object sender, EventArgs e)
        {
            calc = new JokeCalculatorClient(Glue, cbo.Text);
            calc.Init(tbCalc.Text.AsInt());
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Text = calc.Add(tbCalc.Text.AsInt()).ToString();
        }

        private void btnSub_Click(object sender, EventArgs e)
        {
            Text = calc.Sub(tbCalc.Text.AsInt()).ToString();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Text = calc.Done().ToString();
            calc.Dispose();
        }

        private void edRepeat_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void btnBadMsg_Click(object sender, EventArgs ea)
        {
            var client = new BadContract(Glue, cbo.Text);

            try
            {
                var echoed = client.Echo("Hello!");
                Text = echoed + "  " + ECHO_COUNT.ToString() + " times";
            }
            catch (Exception e)
            {
                Text = e.ToMessageWithType();
            }

            client.Dispose();
        }


            private class BadClass
            {
                public event EventHandler Click;

                public BadClass()
                {
                }
            }

        private void btnBadPayload_Click(object sender, EventArgs ea)
        {
          var client = new JokeContractClient(Glue, cbo.Text);

          try
          {
            var bad = new BadClass();
            bad.Click += btnBadMsg_Click;
            var echoed = client.ObjectWork(bad);
            Text = echoed.ToString();
          }
          catch (Exception e)
          {
            Text = e.ToMessageWithType();
          }

          client.Dispose();
        }


        private void tmrAuto_Tick(object sender, EventArgs e)
        {
          tmrAuto.Enabled = false;

        }

        private void button6_Click(object sender, EventArgs e)
        {
           warmup();
           var unsecure = chkUnsecureEcho.Checked;
            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();

                var client = new JokeContractClient(Glue, cbo.Text);
 client.DispatchTimeoutMs = 5 * 1000;
 client.TimeoutMs = 40 * 1000;
                if (!unsecure && chkImpersonate.Checked)
                    client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

                var totalCalls = tbCallsPerReactor.Text.AsInt();

                    System.Threading.Tasks.Parallel.For(0, totalCalls,
                        (i) =>
                        {
                           client.Async_Notify("aa");
                        });
              var allFinished = w.ElapsedMilliseconds;
              Text = "Placed {0:n2} One Way calls in {1:n2} ms,  @ {2:n2} calls/sec "
                   .Args
                   (
                     totalCalls,
                     allFinished,
                     totalCalls / (allFinished/1000d)
                   );
        }

        private void btnSimple_Click(object sender, EventArgs e)
        {
            warmup();
            var unsecure = chkUnsecureEcho.Checked;
            Text = "Working...";
            tbNote.Text = "Started...";
            var w = Stopwatch.StartNew();

                var client = new JokeContractClient(Glue, cbo.Text);
 client.DispatchTimeoutMs = 5 * 1000;
 client.TimeoutMs = 40 * 1000;
                if (!unsecure && chkImpersonate.Checked)
                    client.Headers.Add( new AuthenticationHeader( new IDPasswordCredentials(tbID.Text, tbPwd.Text)) );

                var totalCalls = tbCallsPerReactor.Text.AsInt();

                var totalErrors = 0;


                    System.Threading.Tasks.Parallel.For(0, totalCalls,
                        (i) =>
                        {
                            try
                            {
                              if (unsecure)
                                client.UnsecureEcho("Call number {0} ".Args(i));
                              else
                                client.Echo("Call number {0} ".Args(i));
                            }
                            catch (Exception)
                            {
                              Interlocked.Increment(ref totalErrors);
                            }
                        });


            var allFinished = w.ElapsedMilliseconds;

            Text = "Placed {0:n2} calls in {1:n2} ms total time {2:n2} ms @ {3:n2} calls/sec; totalErrors={4:n2} "
                   .Args
                   (
                     totalCalls,
                     allFinished,
                     allFinished,
                     totalCalls / (allFinished/1000d),
                     totalErrors
                   );
        }

        private void btGCCollect_Click(object sender, EventArgs e)
        {
          var m1 = GC.GetTotalMemory(false);
          GC.Collect();
          var m2 = GC.GetTotalMemory(false);
          Text = "Collected {0} bytes".Args(m1 - m2);
        }

        private void chkDumpMessages_CheckedChanged(object sender, EventArgs e)
        {
          var en = chkDumpMessages.Checked;

          Glue.Bindings["sync"].ClientDump = en ? DumpDetail.Message : DumpDetail.None;
          Glue.Bindings["mpx"].ClientDump = en ? DumpDetail.Message : DumpDetail.None;
        }



    }
}
