using System;

using Azos.Glue;
using Azos.Glue.Native;
using Azos.Glue.Protocol;

namespace Azos.Sky.Glue
{
  /// <summary>
  /// Provides synchronous Glue communication channel over TCP with IRemoteTerminal server using
  /// text protocol stable to runtime version changes.
  /// This binding is not expected to process many messages per second as it is used for admin purposes only,
  /// the protocol is json-based framed in Glue
  /// </summary>
  public sealed class AppTermBinding : SyncBinding
  {
    #region Static Members

      public const  int FRAME_FORMAT_APPTERM = 0x41505054;//APPT

      public static readonly TypeSpec   TYPE_CONTRACT;
      public static readonly MethodSpec METHOD_CONNECT;
      public static readonly MethodSpec METHOD_EXECUTE;
      public static readonly MethodSpec METHOD_DISCONNECT;

      //static .ctor
      static AppTermBinding()
      {
        var t = typeof(@Azos.@Sky.@Contracts.@IRemoteTerminal);
        TYPE_CONTRACT     = new TypeSpec(t);
        METHOD_CONNECT    = new MethodSpec(t.GetMethod("Connect",    new Type[]{ typeof(@System.@String) }));
        METHOD_EXECUTE    = new MethodSpec(t.GetMethod("Execute",    new Type[]{ typeof(@System.@String) }));
        METHOD_DISCONNECT = new MethodSpec(t.GetMethod("Disconnect", new Type[]{  }));
      }
    #endregion

    public AppTermBinding(IGlueImplementation glue, string name) : base(glue, name)
    {
    }

    public override int FrameFormat => FRAME_FORMAT_APPTERM;


    protected override ClientTransport MakeNewClientTransport(ClientEndPoint client)
    {
      return new AppTermClientTransport(this, client.Node);
    }

    protected internal override ServerTransport OpenServerEndpoint(ServerEndPoint epoint)
    {
      var cfg = ConfigNode.NavigateSection(CONFIG_SERVER_TRANSPORT_SECTION);
      if (!cfg.Exists) cfg = ConfigNode;

      var ipep = SyncBinding.ToIPEndPoint(epoint.Node);
      var transport = new AppTermServerTransport(this, epoint, ipep.Address, ipep.Port);
      transport.Configure(cfg);
      transport.Start();

      return transport;
    }
  }
}
