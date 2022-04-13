/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using Azos.Conf;
using Azos.Scripting.Steps;

namespace Azos.Tools.Srun
{

  public sealed class ScriptSource : Multisource<StepRunner>
  {
    public ScriptSource(IApplication app, string rootFilePath) : base(app, rootFilePath) { }
    public ScriptSource(IApplication app, IConfigSectionNode rootSource) : base(app, rootSource) { }

    protected override StepRunner MakeRunner(IConfigSectionNode rootSource) => new StepRunner(App, rootSource);
  }
}
