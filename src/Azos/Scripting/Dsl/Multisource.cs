/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  /// Handy base class for building various script harnesses which feed from multiple sources,
  /// such as installation scripts
  /// </summary>
  public abstract class Multisource
  {
    /// <summary>
    /// Builds a unified root source by including the referenced sources
    /// </summary>
    public static IConfigSectionNode DefaultBuildSourcesFromFile(string rootFilePath)
    {
      rootFilePath.NonBlank(nameof(rootFilePath));

      var cfg = Configuration.ProviderLoadFromFile(rootFilePath);

      var includePragma = cfg.Root.AttrByName(Apps.CommonApplicationLogic.CONFIG_PROCESS_INCLUDES).Value;
      if (includePragma.IsNotNullOrWhiteSpace())
      {
        cfg.Root.ProcessAllExistingIncludes(nameof(Multisource), includePragma);
      }

      return cfg.Root;
    }


    public Multisource(IApplication app)
    {
      m_App = app.NonNull(nameof(app));
    }

    private IApplication m_App;


    public IApplication App => m_App;

    /// <summary>
    /// Accesses the runner which executes the steps
    /// </summary>
    public abstract StepRunner GenericRunner { get; }

    /// <summary>
    /// Runs the script
    /// </summary>
    public async Task<JsonDataMap> RunAsync(EntryPoint ep = null) => ep == null ? await GenericRunner.RunAsync().ConfigureAwait(false)
                                                                                : await GenericRunner.RunAsync(ep).ConfigureAwait(false);
  }

  /// <summary>
  /// Handy base class for building various script harnesses which feed from multiple sources,
  /// such as installation scripts
  /// </summary>
  public abstract class Multisource<TRunner> : Multisource where TRunner : StepRunner
  {
    public Multisource(IApplication app, string rootFilePath) : base(app)
    {
      var rootSource = BuildSourcesFromFile(rootFilePath);
      m_Runner = MakeRunner(rootSource);
    }

    public Multisource(IApplication app, IConfigSectionNode rootSource) : base(app) => m_Runner = MakeRunner(rootSource);


    protected virtual IConfigSectionNode BuildSourcesFromFile(string rootFilePath)
    {
      return DefaultBuildSourcesFromFile(rootFilePath);
    }

    protected abstract TRunner MakeRunner(IConfigSectionNode rootSource);


    private TRunner m_Runner;

    /// <summary>
    /// Accesses the runner which executes the steps
    /// </summary>
    public override StepRunner GenericRunner => m_Runner;

    /// <summary>
    /// Accesses the runner which executes the steps
    /// </summary>
    public TRunner Runner => m_Runner;
  }

}
