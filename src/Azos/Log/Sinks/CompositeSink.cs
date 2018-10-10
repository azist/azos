

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
                RegisterDestination(d);
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

                lock(m_Destinations)
                 foreach(var dest in m_Destinations)
                    dest.Open();
            }

            public override void Close()
            {
               lock(m_Destinations)
                 foreach(var dest in m_Destinations)
                    dest.Close();
                base.Close();
            }

            /// <summary>
            /// Adds a destination to this wrapper
            /// </summary>
            public void RegisterDestination(Destination dest)
            {
              lock (m_Destinations)
              {
                if (!m_Destinations.Contains(dest))
                {
                  m_Destinations.Add(dest);
                  dest.m_Owner = this;
                }
              }
            }

            /// <summary>
            /// Removes a destiantion from this wrapper, returns true if destination was found and removed
            /// </summary>
            public bool UnRegisterDestination(Destination dest)
            {
              lock (m_Destinations)
              {
                bool r = m_Destinations.Remove(dest);
                if (r) dest.m_Owner = null;
                return r;
              }
            }

       #endregion


      #region Protected

            protected override void DoConfigure(Environment.IConfigSectionNode node)
            {
                base.DoConfigure(node);

                  foreach (var dnode in node.Children.Where(n => n.Name.EqualsIgnoreCase(LogServiceBase.CONFIG_DESTINATION_SECTION)))
                  {
                    var dest = FactoryUtils.MakeAndConfigure(dnode) as Destination;
                    this.RegisterDestination(dest);
                  }

            }



            protected internal override void DoSend(Message entry)
            {
                lock(m_Destinations)
                   foreach(var dest in m_Destinations)
                        dest.DoSend(entry);
            }

      #endregion

    }
}
