/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;

namespace Azos.Wave.Instrumentation
{
  /// <summary>
  /// Provides base for Wave long gauges
  /// </summary>
  [Serializable]
  public abstract class WaveLongGauge : LongGauge, IWebInstrument
  {
    protected WaveLongGauge(string src, long value) : base(src, value) { }
  }

  /// <summary>
  /// Provides base for Wave double gauges
  /// </summary>
  [Serializable]
  public abstract class WaveDoubleGauge : DoubleGauge, IWebInstrument
  {
    protected WaveDoubleGauge(string src, double value) : base(src, value) { }
  }

  /// <summary>
  /// Provides base for Wave events
  /// </summary>
  [Serializable]
  public abstract class WaveEvent : Event, IWebInstrument
  {
    protected WaveEvent(string src) : base(src) { }
  }

  /// <summary>
  /// Provides request count received by server
  /// </summary>
  [Serializable]
  [Arow("6D5E45F2-31AE-4E39-959D-F6731149D861")]
  public class ServerRequest : WaveLongGauge, INetInstrument
  {
    internal ServerRequest(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides request count received by server"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_REQUEST; } }

    protected override Datum MakeAggregateInstance() { return new ServerRequest(this.Source, 0); }
  }

  /// <summary>
  /// Provides request count received by server portal
  /// </summary>
  [Serializable]
  [Arow("80C63B05-7C30-4CAC-932D-770D498264D9")]
  public class ServerPortalRequest : WaveLongGauge, INetInstrument
  {
    internal ServerPortalRequest(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides request count received by server portal"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_REQUEST; } }

    protected override Datum MakeAggregateInstance() { return new ServerPortalRequest(this.Source, 0); }
  }


  /// <summary>
  /// Provides request count that were denied by the server gate
  /// </summary>
  [Serializable]
  [Arow("EEC0AAE6-5F8C-4150-B800-0D499F40B430")]
  public class ServerGateDenial : WaveLongGauge, INetInstrument, ISecurityInstrument, IWarningInstrument
  {
    internal ServerGateDenial(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides request count that were denied by the server gate"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_REQUEST; } }

    protected override Datum MakeAggregateInstance() { return new ServerGateDenial(this.Source, 0); }
  }


  /// <summary>
  /// Provides the count of exceptions that server had to handle as no other member of processing chain did
  /// </summary>
  [Serializable]
  [Arow("84BA5853-260C-4CB8-9300-CEE62314EE48")]
  public class ServerHandleException : WaveLongGauge, IErrorInstrument
  {
    internal ServerHandleException(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides the count of exceptions that server had to handle as no other member of processing chain did"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_EXCEPTION; } }

    protected override Datum MakeAggregateInstance() { return new ServerHandleException(this.Source, 0); }
  }


  /// <summary>
  /// Provides the count of exceptions that filter handled
  /// </summary>
  [Serializable]
  [Arow("5D23F85E-0247-48CC-9794-D70B46AE96C8")]
  public class FilterHandleException : WaveLongGauge, IErrorInstrument
  {
    internal FilterHandleException(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides the count of exceptions that filter handled"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_EXCEPTION; } }

    protected override Datum MakeAggregateInstance() { return new FilterHandleException(this.Source, 0); }
  }


  /// <summary>
  /// The current level of taken request accept semaphore slots
  /// </summary>
  [Serializable]
  [Arow("4E8824D5-1333-424F-A4E0-B6380186F9BF")]
  public class ServerAcceptSemaphoreCount : WaveLongGauge, INetInstrument, ICPUInstrument
  {
    internal ServerAcceptSemaphoreCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "The current level of taken request accept semaphore slots"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_SLOT; } }

    protected override Datum MakeAggregateInstance() { return new ServerAcceptSemaphoreCount(this.Source, 0); }
  }

  /// <summary>
  /// The current level of taken request work semaphore slots
  /// </summary>
  [Serializable]
  [Arow("6701E768-5B30-49F3-8664-334BACCC4086")]
  public class ServerWorkSemaphoreCount : WaveLongGauge, INetInstrument, ICPUInstrument
  {
    internal ServerWorkSemaphoreCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "The current level of taken request work semaphore slots"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_SLOT; } }

    protected override Datum MakeAggregateInstance() { return new ServerWorkSemaphoreCount(this.Source, 0); }
  }


  /// <summary>
  /// How many response object were written into
  /// </summary>
  [Serializable]
  [Arow("564A6D4C-8811-4E7E-B81D-A3ED1969BF92")]
  public class WorkContextWrittenResponse : WaveLongGauge, INetInstrument
  {
    internal WorkContextWrittenResponse(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many response object were written into"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_RESPONSE; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextWrittenResponse(this.Source, 0); }
  }

  /// <summary>
  /// How many response object were buffered
  /// </summary>
  [Serializable]
  [Arow("2B73D508-3129-4434-8CA9-4CF9E08A5C1D")]
  public class WorkContextBufferedResponse : WaveLongGauge, INetInstrument, IMemoryInstrument
  {
    internal WorkContextBufferedResponse(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many response object were buffered"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_RESPONSE; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextBufferedResponse(this.Source, 0); }
  }


  /// <summary>
  /// How many response bytes were buffered
  /// </summary>
  [Serializable]
  [Arow("1F22A5E0-8AE4-4AC2-8D76-68F38449CAD9")]
  public class WorkContextBufferedResponseBytes : WaveLongGauge, INetInstrument, IMemoryInstrument
  {
    internal WorkContextBufferedResponseBytes(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many response bytes were buffered"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_BYTE; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextBufferedResponseBytes(this.Source, 0); }
  }


  /// <summary>
  /// How many work contexts were created
  /// </summary>
  [Serializable]
  [Arow("4D6425E8-3E57-4AD5-A4EC-707D96A1D25D")]
  public class WorkContextCtor : WaveLongGauge
  {
    internal WorkContextCtor(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many work contexts were created"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextCtor(this.Source, 0); }
  }

  /// <summary>
  /// How many work contexts were destroyed
  /// </summary>
  [Serializable]
  [Arow("CB84EFF2-3F67-472D-A801-6BB45025EE22")]
  public class WorkContextDctor : WaveLongGauge
  {
    internal WorkContextDctor(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many work contexts were destroyed"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextDctor(this.Source, 0); }
  }

  /// <summary>
  /// How many times work semaphore was released
  /// </summary>
  [Serializable]
  [Arow("52904D36-E76D-4E33-A3B0-D45384143CEE")]
  public class WorkContextWorkSemaphoreRelease : WaveLongGauge, INetInstrument, ICPUInstrument
  {
    internal WorkContextWorkSemaphoreRelease(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times work semaphore was released"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextWorkSemaphoreRelease(this.Source, 0); }
  }

  /// <summary>
  /// How many work contexts got aborted
  /// </summary>
  [Serializable]
  [Arow("6B9CD963-03D0-429A-905E-0939ABA8BF7C")]
  public class WorkContextAborted : WaveLongGauge
  {
    internal WorkContextAborted(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many work contexts got aborted"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextAborted(this.Source, 0); }
  }

  /// <summary>
  /// How many work contexts got handled
  /// </summary>
  [Serializable]
  [Arow("5874A803-16C0-4590-AEC5-8594DF900D5F")]
  public class WorkContextHandled : WaveLongGauge
  {
    internal WorkContextHandled(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many work contexts got handled"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextHandled(this.Source, 0); }
  }


  /// <summary>
  /// How many work contexts requested session state
  /// </summary>
  [Serializable]
  [Arow("9C36D37E-C3D2-4308-8487-B2A42985BCDE")]
  public class WorkContextNeedsSession : WaveLongGauge
  {
    internal WorkContextNeedsSession(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many work contexts requested session state"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_WORK_CONTEXT; } }

    protected override Datum MakeAggregateInstance() { return new WorkContextNeedsSession(this.Source, 0); }
  }

  /// <summary>
  /// How many new sessions created
  /// </summary>
  [Serializable]
  [Arow("537FB7BA-D762-427D-A440-69E6382389CB")]
  public class SessionNew : WaveLongGauge
  {
    internal SessionNew(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many new sessions created"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_SESSION; } }

    protected override Datum MakeAggregateInstance() { return new SessionNew(this.Source, 0); }
  }

  /// <summary>
  /// How many existing sessions found
  /// </summary>
  [Serializable]
  [Arow("29AC4FD3-9AF2-4CF6-83AD-B16EBD339EC1")]
  public class SessionExisting : WaveLongGauge
  {
    internal SessionExisting(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many existing sessions found"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_SESSION; } }

    protected override Datum MakeAggregateInstance() { return new SessionExisting(this.Source, 0); }
  }

  /// <summary>
  /// How many sessions ended by request
  /// </summary>
  [Serializable]
  [Arow("C9534BAA-A081-478B-B32E-B673FC2041F5")]
  public class SessionEnd : WaveLongGauge
  {
    internal SessionEnd(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many sessions ended by request"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_SESSION; } }

    protected override Datum MakeAggregateInstance() { return new SessionEnd(this.Source, 0); }
  }

  /// <summary>
  /// How many sessions supplied invalid identifier (by client)
  /// </summary>
  [Serializable]
  [Arow("76FB857E-3C51-40C4-A61D-72A9EB0896AB")]
  public class SessionInvalidID : WaveLongGauge
  {
    internal SessionInvalidID(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many sessions supplied invalid identifier (by client)"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_SESSION; } }

    protected override Datum MakeAggregateInstance() { return new SessionInvalidID(this.Source, 0); }
  }

  /// <summary>
  /// How many geo lookups have been requested
  /// </summary>
  [Serializable]
  [Arow("32C5DA16-2025-4740-B82F-D161030A9DEA")]
  public class GeoLookup : WaveLongGauge
  {
    internal GeoLookup(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many geo lookups have been requested"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_GEO_LOOKUP; } }

    protected override Datum MakeAggregateInstance() { return new GeoLookup(this.Source, 0); }
  }

  /// <summary>
  /// How many geo lookups have resulted in finding the geo location
  /// </summary>
  [Serializable]
  [Arow("6BA83E49-92E7-4A23-98D0-CC5CAD9C43D5")]
  public class GeoLookupHit : WaveLongGauge
  {
    internal GeoLookupHit(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many geo lookups have resulted in finding the geo location"; } }

    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_GEO_LOOKUP_HIT; } }

    protected override Datum MakeAggregateInstance() { return new GeoLookupHit(this.Source, 0); }
  }
}
