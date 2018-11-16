/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Defines a base provider for InstrumentationDaemon
  /// </summary>
  public abstract class InstrumentationProvider : Daemon<InstrumentationDaemon>, IExternallyParameterized
  {
    #region .ctor
      protected InstrumentationProvider(InstrumentationDaemon director) : base(director.NonNull().App, director) {}
    #endregion

    #region Public
    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public virtual bool ExternalGetParameter(string name, out object value, params string[] groups) { return ExternalParameterAttribute.GetParameter(this, name, out value, groups); }

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public virtual bool ExternalSetParameter(string name, object value, params string[] groups) { return ExternalParameterAttribute.SetParameter(this, name, value, groups); }
    #endregion

    #region Protected
    protected internal virtual object BeforeBatch() { return null; }
    protected internal virtual void AfterBatch(object batchContext) { }

    protected internal virtual object BeforeType(Type type, object batchContext) { return null; }
    protected internal virtual void AfterType(Type type, object batchContext, object typeContext) { }

    protected internal abstract void Write(Datum aggregatedDatum, object batchContext, object typeContext);

    protected override void DoConfigure(IConfigSectionNode node) { base.DoConfigure(node); }

    protected void WriteLog(MessageType type, string message, string parameters = null, string from = null)
    {
      App.Log.Write(
        new Log.Message
        {
          Text = message ?? string.Empty,
          Type = type,
          Topic = CoreConsts.INSTRUMENTATIONSVC_PROVIDER_TOPIC,
          From = from,
          Parameters = parameters ?? string.Empty
        });
    }
    #endregion

    #region Properties

    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParameters { get { return ExternalParameterAttribute.GetParameters(this); } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups) { return ExternalParameterAttribute.GetParameters(this, groups); }
    #endregion
  }
}
