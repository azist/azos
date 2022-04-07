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
using Azos.Serialization.JSON;

namespace Azos.MySql.ConfForest
{
  /// <summary>
  /// Facilitates the config forest setup functionality, such as running DDL backend store script,
  /// inserting seed data etc.
  /// </summary>
  public class ForestInstaller
  {
    public static ForestInstaller FromFile(IApplication app, string rootFilePath)
    {
      var cfg = Configuration.ProviderLoadFromFile(rootFilePath);

      var includePragma = cfg.Root.AttrByName(Apps.CommonApplicationLogic.CONFIG_PROCESS_INCLUDES).Value;
      if (includePragma.IsNotNullOrWhiteSpace())
      {
        cfg.Root.ProcessAllExistingIncludes(nameof(ForestInstaller), includePragma);
      }

      return new ForestInstaller(app, cfg.Root);
    }

    public ForestInstaller(IApplication app, IConfigSectionNode rootSource)
    {
      m_App = app.NonNull(nameof(app));
      m_Runner = new StepRunner(m_App, null);
    }

    private IApplication m_App;

    //will be used on StepRunner form Azos.Scripting
    private StepRunner m_Runner;

    public IApplication App => m_App;

    public IEnumerable<EntryPoint> EntryPoints => m_Runner.EntryPoints;

    public JsonDataMap Run(EntryPoint ep = null) => ep == null ? m_Runner.Run() : m_Runner.Run(ep);
  }
}
