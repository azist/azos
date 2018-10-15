
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.Log.Sinks
{
    /// <summary>
    /// Implements a destination that is based on another instance of LogService, which provides asynchronous buffering and failover capabilities.
    /// </summary>
    public class LogServiceSink : Sink
    {
       #region .ctor
        public LogServiceSink() : base(null)
        {

        }

        public LogServiceSink(string name) : base(name)
        {
        }

        protected override void Destructor()
        {
          base.Destructor();
        }
      #endregion

       #region Pvt Fields

        private LogService  m_Service = new LogService(null);
      #endregion


      #region Properties


      #endregion

      #region Public


           public override void Open()
           {
               base.Open();
               m_Service.Start();
           }

           public override void Close()
           {
               m_Service.WaitForCompleteStop();
               base.Close();
           }

      #endregion



        #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
            base.DoConfigure(node);
            m_Service.Configure(node);
        }


        protected internal override void DoSend(Message msg)
        {
           m_Service.Write(msg);
        }

      #endregion
    }
}
