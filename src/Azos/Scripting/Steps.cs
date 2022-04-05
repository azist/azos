/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Scripting
{
  /// <summary>
  /// Custom steps get derived from this class
  /// </summary>
  public abstract class Step
  {
    public Step(StepRunner runner, IConfigSectionNode cfg)
    {
      m_Runner = runner.NonNull(nameof(runner));
      m_Runner.App.InjectInto(this);
      m_Config = cfg.NonEmpty(nameof(cfg));
      ConfigAttribute.Apply(this, m_Config);
    }

    private readonly StepRunner m_Runner;
    private readonly IConfigSectionNode m_Config;

    public StepRunner         Runner => m_Runner;
    public IApplication       App    => Runner.App;
    public IConfigSectionNode Config => m_Config;

    /// <summary>
    /// State is maintained between steps during a run instance as Step instances are not retained
    /// You can also use "Runner.GlobalState" for global storage
    /// </summary>
    public void Run(JsonDataMap state)
    {
      try
      {
        DoRun(state);
      }
      catch(Exception error)
      {
        var handled = DoError(state, error);
        if (!handled) throw;
      }
    }

    protected abstract void DoRun(JsonDataMap state);
    protected abstract bool DoError(JsonDataMap state, Exception error);
  }

}
