/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.IO.Sipc;
using Azos.Log;


namespace Azos.Apps.Hosting
{
  /// <summary>
  /// Marker interface for objects which keep activator-specific state (such as process, or container instance handle)
  /// per application
  /// </summary>
  public interface IAppActivatorContext
  {

  }

  public interface IAppActivator : IApplicationComponent
  {
    void StartApplication(App app);
    void StopApplication(App app);
  }



  /// <summary>
  /// Provides an abstraction for activation/deactivation of application process.
  /// The default implementation uses System.Process to run/terminate applications
  /// You can create another activator which spawns apps in their on container runtime, such as Docker
  /// </summary>
  public sealed class ProcessAppActivator : ApplicationComponent<GovernorDaemon>, IAppActivator
  {
    public ProcessAppActivator(GovernorDaemon gov, IConfigSectionNode cfg) : base(gov)
    {
      ConfigAttribute.Apply(this, cfg);
    }

    public override string ComponentLogTopic => Sky.SysConsts.LOG_TOPIC_HOST_GOV;

    public void StartApplication(App app)
    {
    }

    public void StopApplication(App app)
    {
    }
  }
}
