/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Sky.Identification.Instrumentation
{
  /// <summary>
  /// Provides base for GDID events
  /// </summary>
  [Serializable]
  public abstract class GDIDEvent : Event, IGDIDInstrument
  {
    protected GDIDEvent(string src) : base(src) { }

    public override string Description { get { return "Provides info about GDID events"; } }

    public override string ValueUnitName { get { return StringConsts.UNIT_NAME_TIME; } }

  }

  /// <summary>
  /// Provides base for GDID events that happen in generator (client side)
  /// </summary>
  [Serializable]
  public abstract class GDIDGeneratorEvent : GDIDEvent
  {
    protected GDIDGeneratorEvent(string src) : base(src) { }
  }

  /// <summary>
  /// Provides base for GDID events that happen in authority (server side)
  /// </summary>
  [Serializable]
  public abstract class GDIDAuthorityEvent : GDIDEvent, INetInstrument
  {
    protected GDIDAuthorityEvent(string src) : base(src) { }
  }

  /// <summary>
  /// Generator requested new block allocation
  /// </summary>
  [Serializable]
  [Arow("855252F5-A3CE-4A4A-99E1-27CE27D03C0F")]
  [BSONSerializable("31A3C469-8A8B-43CE-9765-C538AC0DD574")]
  public class AllocBlockRequestedEvent : GDIDGeneratorEvent, INetInstrument
  {
    protected AllocBlockRequestedEvent(string src) : base(src) { }

    public override string Description { get { return "Generator requested new block allocation"; } }

    public static void Happened(IInstrumentation inst, string scope, string seq)
    {
      if (inst!=null && inst.Enabled)
        inst.Record(new AllocBlockRequestedEvent("{0}::{1}".Args(scope, seq)));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AllocBlockRequestedEvent(this.Source);
    }
  }

  /// <summary>
  /// Generator requested new block allocation completely failed
  /// </summary>
  [Serializable]
  [Arow("0F73FFA4-9FBE-42A6-BA74-595D077EC510")]
  [BSONSerializable("1DA4AEF0-7D9A-4E12-856D-01E0BE61DA84")]
  public class AllocBlockRequestFailureEvent : GDIDGeneratorEvent, INetInstrument, IErrorInstrument, ICatastropyInstrument
  {
    protected AllocBlockRequestFailureEvent(string src) : base(src) { }

    public override string Description { get { return "Generator requested new block allocation completely failed"; } }

    public static void Happened(IInstrumentation inst, string scope, string seq)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AllocBlockRequestFailureEvent("{0}::{1}".Args(scope, seq)));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AllocBlockRequestFailureEvent(this.Source);
    }
  }

  /// <summary>
  /// Generator successfully allocated new block
  /// </summary>
  [Serializable]
  [Arow("8D129B96-C214-45B5-ACC0-B99A92021482")]
  [BSONSerializable("9724C9E3-2CBF-45AD-861C-3FF818209232")]
  public class AllocBlockSuccessEvent : GDIDGeneratorEvent, INetInstrument
  {
    protected AllocBlockSuccessEvent(string src) : base(src) { }

    public override string Description { get { return "Generator successfully allocated new block"; } }

    public static void Happened(IInstrumentation inst, string scope, string seq, string authority)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AllocBlockSuccessEvent("{0}::{1}::{2}".Args(scope, seq, authority)));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AllocBlockSuccessEvent(this.Source);
    }
  }

  /// <summary>
  /// Generator block allocation attempt failed
  /// </summary>
  [Serializable]
  [Arow("6144885A-5544-4DAC-A27F-41BAF8CA4425")]
  [BSONSerializable("A963B984-11EB-4333-A01F-F73EAF84D126")]
  public class AllocBlockFailureEvent : GDIDGeneratorEvent, INetInstrument, IErrorInstrument
  {
    protected AllocBlockFailureEvent(string src) : base(src) { }

    public override string Description { get { return "Generator block allocation attempt failed"; } }

    public static void Happened(IInstrumentation inst, string scope, string seq, int block, string authority)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AllocBlockFailureEvent("{0}::{1}::{2}".Args(scope, seq, authority)));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AllocBlockFailureEvent(this.Source);
    }
  }

  /// <summary>
  /// Authority received block allocation call
  /// </summary>
  [Serializable]
  [Arow("B27465F8-6563-4ABA-B586-D006A9BA603A")]
  [BSONSerializable("88D561E4-32C6-47A0-ACA1-E7E735D3D84E")]
  public class AuthAllocBlockCalledEvent : GDIDAuthorityEvent, INetInstrument
  {
    protected AuthAllocBlockCalledEvent(string src) : base(src) { }

    public override string Description { get { return "Authority received block allocation call"; } }

    public static void Happened(IInstrumentation inst, string scope, string seq)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AuthAllocBlockCalledEvent("{0}.{1}".Args(scope, seq)));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AuthAllocBlockCalledEvent(this.Source);
    }
  }

  /// <summary>
  /// Authority location write failed
  /// </summary>
  [Serializable]
  [Arow("BEFDDF37-CB7D-4264-AD7E-E93E5D78A402")]
  [BSONSerializable("6DB69416-5CFF-4378-9448-D887923540B5")]
  public class AuthLocationWriteFailureEvent : GDIDAuthorityEvent, IErrorInstrument
  {
    protected AuthLocationWriteFailureEvent(string location) : base(location) { }

    public override string Description { get { return "Authority location write failed"; } }

    public static void Happened(IInstrumentation inst, string location)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AuthLocationWriteFailureEvent(location));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AuthLocationWriteFailureEvent(this.Source);
    }
  }

  /// <summary>
  /// Authority sequence write to all locations failed
  /// </summary>
  [Serializable]
  [Arow("6FBC9C82-9CA2-4917-B647-6B260E0797A7")]
  [BSONSerializable("BF0FE93A-5969-4A0C-9FEA-FDB8EBA86798")]
  public class AuthLocationWriteTotalFailureEvent : GDIDAuthorityEvent, ICatastropyInstrument, IErrorInstrument
  {
    protected AuthLocationWriteTotalFailureEvent() : base(string.Empty) { }

    public override string Description { get { return "Authority sequence write to all locations failed"; } }

    public static void Happened(IInstrumentation inst)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AuthLocationWriteTotalFailureEvent());
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AuthLocationWriteTotalFailureEvent();
    }
  }

  /// <summary>
  /// Authority sequence read from location failed
  /// </summary>
  [Serializable]
  [Arow("478B9FE8-D0BF-44BE-BA6F-05C074AC1F69")]
  [BSONSerializable("E29A3788-1372-47B7-B1EB-32740F93C9FF")]
  public class AuthLocationReadFailureEvent : GDIDAuthorityEvent, IErrorInstrument
  {
    protected AuthLocationReadFailureEvent(string location) : base(location) { }

    public override string Description { get { return "Authority sequence read from location failed"; } }

    public static void Happened(IInstrumentation inst, string location)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AuthLocationReadFailureEvent(location));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AuthLocationReadFailureEvent(this.Source);
    }
  }

  /// <summary>
  /// Authority sequence read failed for all locations
  /// </summary>
  [Serializable]
  [Arow("7C4DEB2E-0052-4B0E-B4EE-3F7A36C3B723")]
  [BSONSerializable("A3CFF2DB-4D72-41EA-8F46-27B3BDE3B3B0")]
  public class AuthLocationReadTotalFailureEvent : GDIDAuthorityEvent, ICatastropyInstrument, IErrorInstrument
  {
    protected AuthLocationReadTotalFailureEvent() : base(string.Empty) { }

    public override string Description { get { return "Authority sequence read failed for all locations"; } }

    public static void Happened(IInstrumentation inst)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AuthLocationReadTotalFailureEvent());
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AuthLocationReadTotalFailureEvent();
    }
  }

  /// <summary>
  /// Authority ERA promoted +1
  /// </summary>
  [Serializable]
  [Arow("378043D3-DBD0-4628-B9B0-B1BCEF2800CB")]
  [BSONSerializable("065480FD-6198-45D2-AA15-F54EE71E3D16")]
  public class AuthEraPromotedEvent : GDIDAuthorityEvent
  {
    protected AuthEraPromotedEvent(string src) : base(src) { }

    public override string Description { get { return "Authority ERA promoted +1"; } }

    public static void Happened(IInstrumentation inst, string scope, string seq)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new AuthEraPromotedEvent("{0}:{1}".Args(scope, seq)));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new AuthEraPromotedEvent(this.Source);
    }
  }
}
