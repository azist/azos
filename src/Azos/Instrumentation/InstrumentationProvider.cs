/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Defines a base provider for InstrumentationDaemon
  /// </summary>
  public abstract class InstrumentationProvider : DaemonWithInstrumentation<InstrumentationDaemon>, IExternallyParameterized
  {
    protected InstrumentationProvider(InstrumentationDaemon director) : base(director) {}

    protected internal virtual object BeforeBatch() { return null; }
    protected internal virtual void AfterBatch(object batchContext) { }

    protected internal virtual object BeforeType(Type type, object batchContext) { return null; }
    protected internal virtual void AfterType(Type type, object batchContext, object typeContext) { }

    protected internal abstract void Write(Datum aggregatedDatum, object batchContext, object typeContext);

    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled { get; set; }
  }
}
