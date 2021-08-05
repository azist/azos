/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.Time;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Measures request processing latency via ApiLatency/ms instrument and optionally emits ApiCallEvent
  /// </summary>
  public sealed class LatencyInstrumentationFilter : WorkFilter
  {
    public LatencyInstrumentationFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) { }
    public LatencyInstrumentationFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode) { ConfigAttribute.Apply(this, confNode); }
    public LatencyInstrumentationFilter(WorkHandler handler, string name, int order) : base(handler, name, order) { }
    public LatencyInstrumentationFilter(WorkHandler handler, IConfigSectionNode confNode) : base(handler, confNode) { ConfigAttribute.Apply(this, confNode); }


    /// <summary>
    /// When set, emits API call event instrument
    /// </summary>
    [Config(Default = true), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public bool EmitApiCallEvent { get; set; } = true;

    /// <summary>
    /// Stops latency measurement if set to false
    /// </summary>
    [Config(Default = true), ExternalParameter(CoreConsts.EXT_PARAM_GROUP_WEB, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public bool Enabled { get; set; } = true;

    protected override void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
    {
      var bypass = !this.Enabled || !App.Instrumentation.Enabled;

      if (bypass)
      {
        InvokeNextWorker(work, filters, thisFilterIndex);
        return;
      }

      var time = Timeter.StartNew();
      try
      {
        InvokeNextWorker(work, filters, thisFilterIndex);
      }
      finally
      {
        var uri = work.Request.Url.AbsolutePath;
        var ctx = work.Session?.DataContextName;
        Instrumentation.ApiLatency.EmitApiCall(App.Instrumentation, uri, ctx, time.ElapsedMs, EmitApiCallEvent);
      }
    }

  }
}

