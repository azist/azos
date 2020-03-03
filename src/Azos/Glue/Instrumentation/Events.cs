/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Glue.Instrumentation
{
  [Serializable]
  public abstract class GlueEvent : Event
  {
    protected GlueEvent(string src) : base(src) { }
  }

  [Serializable]
  public abstract class ServerEvent : GlueEvent
  {
    protected ServerEvent(string src) : base(src) { }
  }

  [Serializable]
  public abstract class ClientEvent : GlueEvent
  {
    protected ClientEvent(string src) : base(src) { }
  }

  [Serializable]
  public abstract class ServerTransportErrorEvent : ServerEvent, IErrorInstrument, INetInstrument
  {
    protected ServerTransportErrorEvent(string src) : base(src) { }
  }

  [Serializable]
  public abstract class ClientTransportErrorEvent : ClientEvent, IErrorInstrument, INetInstrument
  {
    protected ClientTransportErrorEvent(string src) : base(src) { }
  }

  [Serializable]
  [Arow("4B7A0BF9-2CAF-4941-9F9B-BF4AAB58A648")]
  [BSONSerializable("AB53F9A7-06B9-4157-A3F2-77E679C844BB")]
  public class ServerDeserializationErrorEvent : ServerTransportErrorEvent
  {
    protected ServerDeserializationErrorEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ServerDeserializationErrorEvent(node.ToString()));
    }

    public override string Description { get { return "Server-side errors while deserializing messages"; } }


    protected override Datum MakeAggregateInstance() { return new ServerDeserializationErrorEvent(this.Source); }
  }

  [Serializable]
  [Arow("1A168669-6786-4FAD-96D7-7C1947C80EC0")]
  [BSONSerializable("90B561AB-83BD-4A60-B8FF-D6E57137018C")]
  public class ClientDeserializationErrorEvent : ClientTransportErrorEvent
  {
    protected ClientDeserializationErrorEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ClientDeserializationErrorEvent(node.ToString()));
    }

    public override string Description { get { return "Client-side errors while deserializing messages"; } }

    protected override Datum MakeAggregateInstance() { return new ClientDeserializationErrorEvent(this.Source); }
  }

  [Serializable]
  [Arow("DDBBE2D2-46C4-475D-B6A2-57E70E0D228A")]
  [BSONSerializable("7FAA2D5D-E458-4A25-8BEF-5FFB2E645FE3")]
  public class ServerGotOverMaxMsgSizeErrorEvent : ServerTransportErrorEvent
  {
    protected ServerGotOverMaxMsgSizeErrorEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ServerGotOverMaxMsgSizeErrorEvent(node.ToString()));
    }

    public override string Description { get { return "Server-side errors getting messages with sizes over limit"; } }

    protected override Datum MakeAggregateInstance() { return new ServerGotOverMaxMsgSizeErrorEvent(this.Source); }
  }

  [Serializable]
  [Arow("54A4AAAE-FFEF-4F85-8952-71DC52582B2C")]
  [BSONSerializable("BB4CE13E-26FC-47E5-AF84-6A4C77F01E39")]
  public class ClientGotOverMaxMsgSizeErrorEvent : ClientTransportErrorEvent
  {
    protected ClientGotOverMaxMsgSizeErrorEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ClientGotOverMaxMsgSizeErrorEvent(node.ToString()));
    }


    public override string Description { get { return "Client-side errors getting messages with sizes over limit"; } }

    protected override Datum MakeAggregateInstance() { return new ClientGotOverMaxMsgSizeErrorEvent(this.Source); }
  }

  [Serializable]
  [Arow("AF11F455-719B-4617-BCD5-78E6298A99FF")]
  [BSONSerializable("F80C902A-991F-4266-AE2A-E816058507E7")]
  public class ServerSerializedOverMaxMsgSizeErrorEvent : ServerTransportErrorEvent
  {
    protected ServerSerializedOverMaxMsgSizeErrorEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ServerSerializedOverMaxMsgSizeErrorEvent(node.ToString()));
    }


    public override string Description { get { return "Server-side errors serializing messages with sizes over limit"; } }

    protected override Datum MakeAggregateInstance() { return new ServerSerializedOverMaxMsgSizeErrorEvent(this.Source); }
  }

  [Serializable]
  [Arow("B9C5AE14-F5B0-4BA0-9B30-EA04AF5CBC01")]
  [BSONSerializable("32EAEDAC-EA29-4B13-B83F-2B3D8D9243A8")]
  public class ClientSerializedOverMaxMsgSizeErrorEvent : ClientTransportErrorEvent
  {
    protected ClientSerializedOverMaxMsgSizeErrorEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ClientSerializedOverMaxMsgSizeErrorEvent(node.ToString()));
    }

    public override string Description { get { return "Client-side errors serializing messages with sizes over limit"; } }

    protected override Datum MakeAggregateInstance() { return new ClientSerializedOverMaxMsgSizeErrorEvent(this.Source); }
  }

  [Serializable]
  [Arow("9110677F-E84A-4445-9654-9F35F1E66FC6")]
  [BSONSerializable("19B225C3-BA39-4A3C-A63E-E53C6B6B40E8")]
  public class ServerListenerErrorEvent : ServerTransportErrorEvent
  {
    protected ServerListenerErrorEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ServerListenerErrorEvent(node.ToString()));
    }

    public override string Description { get { return "Server-side listener errors"; } }

    protected override Datum MakeAggregateInstance() { return new ServerListenerErrorEvent(this.Source); }
  }

  [Serializable]
  [Arow("1359F373-6ABF-4106-8AC8-F9D2DA748BD5")]
  [BSONSerializable("2176DBDF-2B0C-4D15-B1EB-E8B7A1250A32")]
  public class InactiveClientTransportClosedEvent : ClientEvent, INetInstrument
  {
    protected InactiveClientTransportClosedEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new InactiveClientTransportClosedEvent(node.ToString()));
    }

    public override string Description { get { return "Client closed inactive transport"; } }

    protected override Datum MakeAggregateInstance() { return new InactiveClientTransportClosedEvent(this.Source); }
  }

  [Serializable]
  [Arow("E10F4790-A0C5-44B4-B254-3D2BC8D0B59D")]
  [BSONSerializable("0A617BA7-2092-42A7-9D85-CD6A990C70C3")]
  public class InactiveServerTransportClosedEvent : ServerEvent, INetInstrument
  {
    protected InactiveServerTransportClosedEvent(string src) : base(src) { }

    public static void Happened(IInstrumentation inst, Node node)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new InactiveServerTransportClosedEvent(node.ToString()));
    }

    public override string Description { get { return "Server closed inactive transport"; } }

    protected override Datum MakeAggregateInstance() { return new InactiveServerTransportClosedEvent(this.Source); }
  }

  [Serializable]
  [Arow("12EE5843-FEF4-47E7-8391-E5CF8E1D9FE5")]
  [BSONSerializable("23550EEF-9040-4470-BE3E-C5EA147F8D90")]
  public class CallSlotNotFoundErrorEvent : ClientEvent, IErrorInstrument, INetInstrument
  {
    protected CallSlotNotFoundErrorEvent() : base(Datum.UNSPECIFIED_SOURCE) { }

    public static void Happened(IInstrumentation inst)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new CallSlotNotFoundErrorEvent());
    }

    public override string Description { get { return "Client could not find call slot"; } }

    protected override Datum MakeAggregateInstance() { return new CallSlotNotFoundErrorEvent(); }
  }

  [Serializable]
  [Arow("0AEAAADB-EDC4-4134-BA62-2CEB42CB056E")]
  [BSONSerializable("DC2C447B-9D77-4B3D-8440-F5F3B20EEB0B")]
  public class ClientConnectedEvent : ServerEvent, INetInstrument
  {
    protected ClientConnectedEvent(string from) : base(from ?? Datum.UNSPECIFIED_SOURCE) { }

    public static void Happened(IInstrumentation inst, string from)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ClientConnectedEvent(from));
    }

    public override string Description { get { return "Client connected to server"; } }

    protected override Datum MakeAggregateInstance() { return new ClientConnectedEvent(this.Source); }
  }

  [Serializable]
  [Arow("345090BA-7753-4D36-9D9E-3A28ACBA6869")]
  [BSONSerializable("B3C5D9EC-2914-4973-9047-0D8C7DCC2860")]
  public class ClientDisconnectedEvent : ServerEvent, INetInstrument
  {
    protected ClientDisconnectedEvent(string from) : base(from ?? Datum.UNSPECIFIED_SOURCE) { }

    public static void Happened(IInstrumentation inst, string from)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new ClientDisconnectedEvent(from));
    }

    public override string Description { get { return "Client disconnected from server"; } }

    protected override Datum MakeAggregateInstance() { return new ClientDisconnectedEvent(this.Source); }
  }
}
