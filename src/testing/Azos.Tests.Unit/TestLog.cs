/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Log;

namespace Azos.Tests.Unit
{
    /// <summary>
    /// A log class for writing messages to memory.
    /// This class is intended for testing purposes only!!!
    /// </summary>
    public class TestMemoryLog : LogServiceBase
    {
        private MessageList m_List = new MessageList();

        public override bool DestinationsAreOptional
        {
          get
          {
            return true;
          }
        }

        /// <summary>
        /// Returns a thread-safe copy of buffered messages
        /// </summary>
        public IList<Message> Messages
        {
            get { lock(m_List) { var r = new MessageList(m_List); return r; } }
        }

        public void Clear()
        {
            lock(m_List)
                m_List.Clear();
        }

        protected override void DoWrite(Message msg, bool urgent)
        {
            lock(m_List)
                m_List.Add(msg);
        }
    }

    /// <summary>
    /// A log class for writing messages synchronously to destinations.
    /// This class is intended for testing purposes only!!!
    /// </summary>
    public class TestSyncLog : LogServiceBase
    {
        protected override void DoWrite(Message msg, bool urgent)
        {
            lock (m_Destinations)
                foreach (var destination in m_Destinations)
                    destination.Send(msg);
        }
    }
}
