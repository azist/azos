
/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Instrumentation;

namespace Azos.Apps
{
  /// <summary>
  /// Provides base implementation for Service with IInstrumentable logic
  /// </summary>
  public abstract class DaemonWithInstrumentation<TDirector> : Daemon<TDirector>, IInstrumentable
                  where TDirector : IApplicationComponent
  {

    protected DaemonWithInstrumentation(IApplication application) : base(application) { }
    protected DaemonWithInstrumentation(TDirector director) : base(director) { }

    /// <summary>
    /// Turns instrumentation on/off
    /// </summary>
    public abstract bool InstrumentationEnabled
    {
      get;
      set;
    }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }

    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public virtual bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
        return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
    }

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public virtual bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(this, name, value, groups);
    }
  }
}
