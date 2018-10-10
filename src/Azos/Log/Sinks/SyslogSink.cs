
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Log.Syslog;

namespace Azos.Log.Sinks
{
    /// <summary>
    /// Implements destination that sends messages to UNIX syslog using UDP datagrams
    /// </summary>
    public class SyslogSink : Sink
    {

        #region .ctor

        /// <summary>
        /// Creates a new instance of destination that sends messages to .nix SYSLOG
        /// </summary>
        public SyslogSink() : base(null)
        {
          m_Client = new SyslogClient();
        }

        /// <summary>
        /// Creates a new instance of destination that sends messages to .nix SYSLOG
        /// </summary>
        public SyslogSink(string name, string host, int port) : base(name)
        {
          m_Client = new SyslogClient(host, port);
        }

        protected override void Destructor()
        {
          m_Client.Dispose();
          base.Destructor();
        }
      #endregion


      #region Private Fields

        private SyslogClient m_Client;

      #endregion


      #region Properties

       /// <summary>
       /// References the underlying syslog client instance
       /// </summary>
       public SyslogClient Client
       {
         get { return m_Client; }
       }

      #endregion

      #region Public


       public override void Close()
       {
           m_Client.Close();
           base.Close();
       }

      #endregion


      #region Protected


        protected override void DoConfigure(Environment.IConfigSectionNode node)
        {
            base.DoConfigure(node);
            m_Client.Configure(node);
        }

        protected internal override void DoSend(Message entry)
        {
           m_Client.Send(new SyslogMessage(entry));
        }

      #endregion

    }
}
