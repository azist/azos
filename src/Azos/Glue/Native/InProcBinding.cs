
namespace Azos.Glue.Native
{
    /// <summary>
    /// Provides synchronous communication pattern based on in-memory message exchange without serialization.
    /// This binding is usable for interconnection between Azos-native components in the same app domain
    /// </summary>
    public class InProcBinding  : Binding
    {

       #region .ctor
            public InProcBinding(string name) : base (name)
            {

            }

            public InProcBinding(IGlueImplementation glue, string name) : base(glue, name)
            {

            }

        #endregion


        #region Fields


        #endregion

        #region Properties
            /// <summary>
            /// InProc binding is synchronous by definition
            /// </summary>
            public override OperationFlow OperationFlow
            {
                get { return OperationFlow.Synchronous; }
            }

            public override string EncodingFormat
            {
              get { return "memory"; }
            }

        #endregion

        #region Public
            public override bool AreNodesIdentical(Node left, Node right)
            {
               return  left.Assigned && right.Assigned && left.ConnectString.EqualsIgnoreCase(right.ConnectString);
            }
        #endregion

        #region Protected


            protected override ClientTransport AcquireClientTransportForCall(ClientEndPoint client, Protocol.RequestMsg request)
            {
                var tr = new InProcClientTransport(this, client.Node);
                tr.Start();
                return tr;
            }


            protected override ClientTransport MakeNewClientTransport(ClientEndPoint client)
            {
             return null;
            }


            protected internal override void ReleaseClientTransportAfterCall(ClientTransport transport)
            {
               transport.Dispose();
            }

            protected internal override ServerTransport OpenServerEndpoint(ServerEndPoint epoint)
            {
                return new InProcServerTransport(this, epoint);
            }

            protected internal override void CloseServerEndpoint(ServerEndPoint epoint)
            {
               var t = epoint.Transport;
               if (t!=null) t.Dispose();
            }


       #endregion

    }
}
