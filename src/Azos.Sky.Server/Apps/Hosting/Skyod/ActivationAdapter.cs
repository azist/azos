/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.Server.Apps.Hosting.Skyod
{

  /// <summary>
  /// Outlines protocol for activities related to software component activation and lifetime management
  /// </summary>
  public abstract class ActivationAdapter : ApplicationComponent<SetComponent>
  {
    protected ActivationAdapter(SetComponent director) : base(director)
    {
    }


    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_SKYOD;

    DateTime? LastOk { get; }

    DateTime? LastError { get; }

    public void Start()
    {
      ComponentDirector.IsManagedActivation.IsTrue("Support managed activation");
    }
    public void Stop()
    {
      ComponentDirector.IsManagedActivation.IsTrue("Support managed activation");
    }
  }


}
