/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Instrumentation;

namespace Azos.Data.Access.Subscriptions
{
  /// <summary>
  /// Base type for externally parametrized app components that are used throughout Subscription implementation
  /// </summary>
  public abstract class SubscriptionAppComponent : ApplicationComponent, IExternallyParameterized
  {
    protected SubscriptionAppComponent(IApplicationComponent director) : base(director)
    {

    }

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

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
