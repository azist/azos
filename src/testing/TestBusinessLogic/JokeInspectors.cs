/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos;
using Azos.Glue;
using Azos.Glue.Protocol;

namespace TestBusinessLogic
{
  public class TextInfoReporter : IClientMsgInspector
  {
    public RequestMsg ClientDispatchCall(ClientEndPoint endpoint, RequestMsg request)
    {
      request.Headers.Add(new TextInfoHeader { Text = "Moscow time is " + App.LocalizedTime.ToString(), Info = @"/\EH|/|H  }|{|/|B!" });
      return request;
    }

    public ResponseMsg ClientDeliverResponse(CallSlot callSlot, ResponseMsg response)
    {
      if (response.ReturnValue is string)
        return new ResponseMsg(response, (string)(response.ReturnValue) + " Added by Client Inspector");
      else
        return response;
    }

    public string Name { get { return "Marazm"; } }

    public int Order { get { return 0; } }

    public void Configure(Azos.Conf.IConfigSectionNode node) { }
  }

  public class ServerInspector : IServerMsgInspector
  {
    public RequestMsg ServerDispatchRequest(ServerEndPoint endpoint, RequestMsg request)
    {
      Azos.Apps.ExecutionContext.Application.Log.Write(new Azos.Log.Message { Type = Azos.Log.MessageType.TraceA, From = "ServeInspector", Text = "Received " + request.ServerTransport.StatBytesReceived.ToString() + " bytes" });
      return request;
    }

    public ResponseMsg ServerReturnResponse(ServerEndPoint endpoint, RequestMsg request, ResponseMsg response)
    {
      response.Headers.Add(new TextInfoHeader { Text = "Response generated at " + App.LocalizedTime.ToString(), Info = "Serve Node: " + endpoint.Node });
      return response;
    }

    public string Name { get { return "MarazmOnServer"; } }

    public int Order { get { return 0; } }

    public void Configure(Azos.Conf.IConfigSectionNode node) { }
  }
}
