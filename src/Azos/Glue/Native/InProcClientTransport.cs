
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Glue;
using Azos.Glue.Protocol;

namespace Azos.Glue.Native
{
    /// <summary>
    /// Provides client-side functionality for synchronous communication pattern based on
    ///  in-memory message exchange without serialization
    /// </summary>
    public class InProcClientTransport : ClientTransport<InProcBinding>
    {
        #region .ctor

           public InProcClientTransport(InProcBinding binding, Node node) : base(binding)
           {
             Node = node;
           }


        #endregion

        #region Fields/Props


        #endregion

        #region Protected

                protected override CallSlot DoSendRequest(ClientEndPoint endpoint, RequestMsg request, CallOptions options)
                {

                  request.__setServerTransport(findServerTransport(endpoint));
                  //notice: no serialization because we are in the same address space
                  ResponseMsg response;
                  try
                  {
                    response = Glue.ServerHandleRequest(request);
                  }
                  catch (Exception e)
                  {
                    response = Glue.ServerHandleRequestFailure(request.RequestID, request.OneWay, e, request.BindingSpecificContext);
                  }

                  if (request.OneWay)
                   return new CallSlot(endpoint, this, request, CallStatus.Dispatched, options.TimeoutMs);

                  var result = new CallSlot(endpoint, this, request, CallStatus.ResponseOK);
                  result.DeliverResponse(response);

                  return result;
                }

        #endregion


        #region .pvt

          private ServerTransport findServerTransport(ClientEndPoint client)
          {
            var srv = Glue.Servers.FirstOrDefault(s => s.Binding == this.Binding && Binding.AreNodesIdentical(client.Node, s.Node));
            if (srv!=null)
              return srv.Transport;

            throw new ClientCallException(CallStatus.DispatchError, StringConsts.GLUE_NO_INPROC_MATCHING_SERVER_ENDPOINT_ERROR + client.Node.ToString());
          }


        #endregion

    }
}
