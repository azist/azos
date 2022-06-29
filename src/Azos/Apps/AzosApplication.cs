/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Apps
{
  /// <summary>
  /// Provides base implementation of IApplication for applications that have no forms like services and console apps. This class IS thread safe
  /// </summary>
  public sealed class AzosApplication : CommonApplicationLogic
  {
    /// <summary>
    /// Takes optional application args[] and root configuration.
    /// The args are parsed into CommandArgsConfiguration. If configuration is null then
    /// application is configured from a file co-located with entry-point assembly and
    ///  called the same name as assembly with '.config' extension, unless args are specified and "/config file"
    ///   switch is used in which case 'file' has to be locatable and readable.
    /// </summary>
    public AzosApplication(string[] cmdArgs, ConfigSectionNode rootConfig = null)
      : this(false, cmdArgs, rootConfig)
    {}

    /// <summary>
    /// Takes optional command-line configuration args and root configuration. If configuration is null then
    ///  application is configured from a file co-located with entry-point assembly and
    ///   called the same name as assembly with '.config' extension, unless args are specified and "/config file"
    ///   switch is used in which case 'file' has to be locatable and readable.
    /// Pass allowNesting=true to nest other app container instances
    /// </summary>
    public AzosApplication(bool allowNesting, string[] cmdArgs, ConfigSectionNode rootConfig = null) : base()//do not call this ctor from derived class
    {
      try
      {
        Constructor(allowNesting, cmdArgs, rootConfig);
        InitApplication();
      }
      catch
      {
        Destructor();
        throw;
      }
    }

    protected override void Destructor()
    {
       SetShutdownStarted();
       CleanupApplication();
       base.Destructor();
    }
  }

}
