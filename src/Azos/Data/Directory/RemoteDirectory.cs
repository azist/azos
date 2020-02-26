/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Directory
{
  public partial class RemoteDirectory : ApplicationComponent,  IDirectoryImplementation
  {

    protected RemoteDirectory(IApplication app) : base(app)
    {
    }

    protected RemoteDirectory(IApplicationComponent director) : base(director)
    {
    }


    #region Private Fields

    private string m_TargetName;
    private bool m_InstrumentationEnabled;

    #endregion

    #region Properties

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    [Config]
    public StoreLogLevel LogLevel { get; set; }

    [Config]
    public string TargetName
    {
      get { return m_TargetName.IsNullOrWhiteSpace() ? "Directory" : m_TargetName; }
      set { m_TargetName = value; }
    }

    #endregion

    #region IInstrumentation

    // public string Name { get { return GetType().FullName; } }

    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public bool InstrumentationEnabled { get { return m_InstrumentationEnabled; } set { m_InstrumentationEnabled = value; } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters { get { return ExternalParameterAttribute.GetParameters(this); } }



    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }

    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      return ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);
    }

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(App, this, name, value, groups);
    }

    #endregion


    public virtual void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }

  }
}
