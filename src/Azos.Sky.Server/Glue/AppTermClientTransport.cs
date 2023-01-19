/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.IO;
using System.Text;

using Azos.Data;
using Azos.Glue;
using Azos.Glue.Native;
using Azos.Glue.Protocol;
using Azos.Serialization.JSON;

namespace Azos.Sky.Glue
{
  /// <summary>
  /// Implements client transport for application terminal contract.
  /// READ THIS: this binding processes a few messages a second at best, no need to implement complex optimizations
  /// like copy-free code etc.
  /// </summary>
  public sealed class AppTermClientTransport : SyncClientTransport
  {
    public AppTermClientTransport(SyncBinding binding, Node node) : base(binding, node)
    {
    }



    protected override void DoEncodeRequest(MemoryStream ms, RequestMsg msg)
    {
      if (msg.Contract!=typeof(Contracts.IRemoteTerminal))
       throw new ProtocolException(ServerStringConsts.GLUE_BINDING_UNSUPPORTED_FUNCTION_ERROR.Args(nameof(AppTermBinding),
                                                                                             nameof(Contracts.IRemoteTerminal),
                                                                                             msg.Contract.FullName));

      var request = msg as RequestAnyMsg;
      if (request==null)
        throw new ProtocolException(ServerStringConsts.GLUE_BINDING_UNSUPPORTED_FUNCTION_ERROR.Args(nameof(AppTermBinding),
                                                                                              nameof(RequestAnyMsg),
                                                                                              msg.GetType().FullName));


      var data = new JsonDataMap();
      data["id"] = request.RequestID.ID;
      data["instance"] = request.RemoteInstance?.ToString("D");
      data["method"] = request.MethodName;
      data["one-way"] = false;//reserved for future use

      if (request.Arguments!=null && request.Arguments.Length>0)
        data["command"] = request.Arguments[0];
      else
        data["command"] = null;

      //Handle headers, this binding allows ONLY for AuthenticationHeader with supported tokens/credentials
      if (request.HasHeaders)
      {
        if (request.Headers.Count>1)
         throw new ProtocolException(ServerStringConsts.GLUE_BINDING_UNSUPPORTED_FUNCTION_ERROR.Args(nameof(AppTermBinding),
                                                                                              "1 AuthenticationHeader",
                                                                                              request.Headers.Count));
        var ahdr = request.Headers[0] as AuthenticationHeader;
        if (ahdr==null)
          throw new ProtocolException(ServerStringConsts.GLUE_BINDING_UNSUPPORTED_FUNCTION_ERROR.Args(nameof(AppTermBinding),
                                                                                              "1 AuthenticationHeader",
                                                                                              request.Headers[0].GetType().FullName));

        if (ahdr.Token.Assigned)
         data["auth-token"] = ahdr.Token.ToString();

        if (ahdr.Credentials!=null)
        {
          var src = ahdr.Credentials as Azos.Security.IStringRepresentableCredentials;
          if (src==null)
            throw new ProtocolException(ServerStringConsts.GLUE_BINDING_UNSUPPORTED_FUNCTION_ERROR.Args(nameof(AppTermBinding),
                                                                                              "IStringRepresentableCredentials",
                                                                                              ahdr.Credentials.GetType().FullName));
          data["auth-cred"] = src.RepresentAsString();
        }
      }

      var json = data.ToJson(JsonWritingOptions.Compact);
      var utf8 = Encoding.UTF8.GetBytes(json);
      ms.Write(utf8, 0, utf8.Length);
    }

    protected override ResponseMsg DoDecodeResponse(WireFrame frame, MemoryStream ms)
    {
      var utf8 = ms.GetBuffer();
      var json = Encoding.UTF8.GetString(utf8, (int)ms.Position, (int)ms.Length - (int)ms.Position);
      var data = json.JsonToDataObject() as JsonDataMap;

      if (data==null)
        throw new ProtocolException(ServerStringConsts.GLUE_BINDING_RESPONSE_ERROR.Args(nameof(AppTermBinding),"data==null"));


      var reqID = new FID( data["request-id"].AsULong(handling: ConvertErrorHandling.Throw) );
      var instance = data["instance"].AsNullableGUID(handling: ConvertErrorHandling.Throw);

      object returnValue = data["return"];
      if (returnValue==null || returnValue is string)
      {
        //return as-is
      } else if (returnValue is JsonDataMap map)//error or Remote Terminal
      {
        var errorContent = map["error-content"].AsString();
        if (errorContent!=null)
          returnValue =  JsonReader.ToDoc<WrappedExceptionData>(errorContent);
        else
          returnValue = new Contracts.RemoteTerminalInfo(map);

      } else throw new ProtocolException(ServerStringConsts.GLUE_BINDING_RESPONSE_ERROR.Args(nameof(AppTermBinding), "data.return is "+returnValue.GetType().FullName));


      var result = new ResponseMsg(reqID, instance, returnValue);

      return result;
    }
  }
}
