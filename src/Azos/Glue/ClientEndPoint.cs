/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Glue.Protocol;

namespace Azos.Glue
{
    /// <summary>
    /// Represents an ancestor for client classes that make calls to server endpoints.
    /// This and descendant classes are thread safe ONLY for making non-constructing/destructing remote calls, unless ReserveTransport is set to true
    /// in which case no operation is thread safe
    /// </summary>
    /// <remarks>
    /// This class is not thread safe in general, however Glue allows for concurrent remote calls via the same endpoint instance
    /// if the following conditions are met:
    ///  1). The endpoint instance has not reserved its transport (ReserveTransport=false)
    ///  2). Either remote contract is stateless OR none of the concurrent calls are constructing/destructing remote instance
    /// The second condition ensures that stateful remote instance is consistent, otherwise operations may get executed
    ///  out-of-order in the multi-threaded scenario
    /// </remarks>
    public abstract class ClientEndPoint : EndPoint
    {
        public ClientEndPoint(IGlue glue, string node, Binding binding = null) : base(glue, new Node(node), binding) { ctor(); }
        public ClientEndPoint(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }


        private void ctor()
        {
            if (Binding==null)
                throw new ClientCallException(CallStatus.DispatchError, StringConsts.GLUE_CLIENT_CALL_NO_BINDING_ERROR.Args(GetType().FullName));

            m_DispatchTimeoutMs = Glue.DefaultDispatchTimeoutMs;
            m_TimeoutMs = Glue.DefaultTimeoutMs;
        }

        protected override void Destructor()
        {
            if (m_ReservedTransport != null)
            {
                Binding.ReleaseClientTransportAfterCall(m_ReservedTransport);
                m_ReservedTransport = null;
            }

            base.Destructor();
        }



        private Guid? m_RemoteInstance; protected internal void __setRemoteInstance(Guid id) { m_RemoteInstance = id; }

        private int m_DispatchTimeoutMs;
        private int m_TimeoutMs;
        private Headers m_Headers = new Headers();
        private bool m_ReserveTransport;
        internal ClientTransport m_ReservedTransport;

        private OrderedRegistry<IClientMsgInspector> m_MsgInspectors = new OrderedRegistry<IClientMsgInspector>();



        /// <summary>
        /// Returns a contract type of this endpoint. "C" component of the "ABC" rule
        /// </summary>
        public abstract Type Contract { get; }


        /// <summary>
        /// Returns client message inspectors for this instance
        /// </summary>
        public OrderedRegistry<IClientMsgInspector> MsgInspectors { get { return m_MsgInspectors; } }

        /// <summary>
        /// Returns headers that get attached in every call
        /// </summary>
        public Headers Headers
        {
            get { return m_Headers;}
        }

        /// <summary>
        /// Returns a reference to remote instance or null if service is stateless(no instance created)
        /// </summary>
        public Guid? RemoteInstance
        {
            get { return m_RemoteInstance; }
        }


        /// <summary>
        /// Indicates whether transport instance should not be released after a call and be reserved per this endpoint instance.
        /// Be careful when setting this property to true as this action can really impede the system performance as transport is reserved
        ///  until this property is either reset to false or endpoint instance is disposed. Transport reservation reduces call latency
        ///   and is mostly beneficial in synchronous bindings. Warning: client endpoint with reserved transport is NOT THREAD SAFE for making
        ///  parallel calls! Set this property to true only when latency is very critical and only 1 dedicated thread is working with this
        ///   client endpoint instance
        /// </summary>
        public bool ReserveTransport
        {
            get { return m_ReserveTransport; }
            set
            {
                m_ReserveTransport = value;
                if (!value && m_ReservedTransport != null)
                {
                    Binding.ReleaseClientTransportAfterCall(m_ReservedTransport);
                    m_ReservedTransport = null;
                }
            }
        }

        /// <summary>
        /// Specifies timeout for call invocation
        /// </summary>
        public int DispatchTimeoutMs
        {
            get { return m_DispatchTimeoutMs; }
            set { m_DispatchTimeoutMs = value>0 ? value : 0; }
        }

        /// <summary>
        /// Specifies timeout for the whole call
        /// </summary>
        public int TimeoutMs
        {
            get { return m_TimeoutMs; }
            set { m_TimeoutMs = value>0 ? value : 0; }
        }


        public override string ToString()
        {
            return string.Format("{0} -> {1}", GetType().Name, m_Node);
        }

        /// <summary>
        /// Sets RemoteInstance to null. This method is needed when the same instance of client endpoint is used to make subsequent stateful calls
        ///  to different server instances. Call this method before calling [Constructor]-decorated remote method or making the first call to
        ///   InstanceLifetime.AutoconstructedStateful servers.
        /// </summary>
        /// <remarks>
        /// The remote instance ID is retained locally even after a call to [Destructor]-decorated remote method. This is needed because
        ///  a call to destructor may be asynchronous and it may be necessary to know the ID of the instance (that has already died on remote host) after call returns.
        /// Call ForgetRemoteInstance() to deterministically nullify the local cached ID.
        /// </remarks>
        public void ForgetRemoteInstance()
        {
            m_RemoteInstance = null;
        }


        /// <summary>
        /// Dispatches a call into binding passing message through client inspectors on this endpoint
        /// </summary>
        protected CallSlot DispatchCall(RequestMsg request)
        {
          if (m_Headers.Count>0)
           request.Headers.AddRange(m_Headers);

          foreach(var insp in m_MsgInspectors.OrderedValues)
            request = insp.ClientDispatchCall(this, request);

          return Binding.DispatchCall(this, request);
        }

    }

}
