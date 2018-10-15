

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.Log.Sinks
{
    /// <summary>
    /// Provides an abstraction of a wrap around another destinations
    /// </summary>
    public class CompositeSink : Sink
    {
       #region .ctor

            public CompositeSink() : base()
            {
            }

            public CompositeSink(params CompositeSink[] inner) : this(null, inner)
            {
            }

            public CompositeSink(string name, params CompositeSink[] inner) : base (name)
            {
              if (inner==null) return;
              foreach(var d in inner)
                RegisterSink(d);
            }

            protected override void Destructor()
            {
              base.Destructor();
            }

       #endregion


       #region Pvt/Protected Fields

            private LogServiceBase.SinkList m_Sinks = new LogServiceBase.SinkList();

       #endregion


       #region Properties
        /// <summary>
        /// Returns destinations that this destination wraps. This call is thread safe
        /// </summary>
        public IEnumerable<Sink> Sinks
        {
          get { lock(m_Sinks) return m_Sinks.ToList(); }
        }

       #endregion

       #region Public

            public override void Open()
            {
                base.Open();

                lock(m_Sinks)
                 foreach(var dest in m_Sinks)
                    dest.Open();
            }

            public override void Close()
            {
               lock(m_Sinks)
                 foreach(var sink in m_Sinks)
                    sink.Close();
                base.Close();
            }

            /// <summary>
            /// Adds a destination to this wrapper
            /// </summary>
            public void RegisterSink(Sink sink)
            {
              lock (m_Sinks)
              {
                if (!m_Sinks.Contains(sink))
                {
                  m_Sinks.Add(sink);
                  sink.m_Owner = this;
                }
              }
            }

            /// <summary>
            /// Removes a destiantion from this wrapper, returns true if destination was found and removed
            /// </summary>
            public bool UnRegisterSink(Sink sink)
            {
              lock (m_Sinks)
              {
                bool r = m_Sinks.Remove(sink);
                if (r) sink.m_Owner = null;
                return r;
              }
            }

       #endregion


      #region Protected

            protected override void DoConfigure(Conf.IConfigSectionNode node)
            {
                base.DoConfigure(node);

                  foreach (var dnode in node.Children.Where(n => n.Name.EqualsIgnoreCase(LogServiceBase.CONFIG_SINK_SECTION)))
                  {
                    var dest = FactoryUtils.MakeAndConfigure(dnode) as Sink;
                    this.RegisterSink(dest);
                  }

            }



            protected internal override void DoSend(Message entry)
            {
                lock(m_Sinks)
                   foreach(var sink in m_Sinks)
                        sink.DoSend(entry);
            }

      #endregion

    }
}
