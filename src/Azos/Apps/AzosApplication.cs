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
  public class AzosApplication : CommonApplicationLogic
  {
    //framework internal, called by derivatives
    protected AzosApplication() : base() { } //keep this ctor for clarity to signify absence of behavior
                                             //the true object construction is delegated to Constructor()
                                             //because of CLR's inability to invoke inherited .ctor in the middle of another


    /// <summary>
    /// Takes optional application args[] and root configuration.
    /// The args are parsed into CommandArgsConfiguration. If configuration is null then
    /// application is configured from a file co-located with entry-point assembly and
    ///  called the same name as assembly with '.config' extension, unless args are specified and "/config file"
    ///   switch is used in which case 'file' has to be locatable and readable.
    /// </summary>
    public AzosApplication(string[] args, ConfigSectionNode rootConfig)
      : this(false, args, rootConfig)
    {}

    /// <summary>
    /// Takes optional application args[] and root configuration.
    /// The args are parsed into CommandArgsConfiguration. If configuration is null then
    /// application is configured from a file co-located with entry-point assembly and
    ///  called the same name as assembly with '.config' extension, unless args are specified and "/config file"
    ///   switch is used in which case 'file' has to be locatable and readable.
    /// Pass allowNesting=true to nest other app container instances
    /// </summary>
    public AzosApplication(bool allowNesting, string[] args, ConfigSectionNode rootConfig)
      : this(allowNesting, args == null ? null : new CommandArgsConfiguration(args), rootConfig)
    {}

    /// <summary>
    /// Takes optional command-line configuration args and root configuration. If configuration is null then
    ///  application is configured from a file co-located with entry-point assembly and
    ///   called the same name as assembly with '.config' extension, unless args are specified and "/config file"
    ///   switch is used in which case 'file' has to be locatable and readable.
    /// Pass allowNesting=true to nest other app container instances
    /// </summary>
    public AzosApplication(bool allowNesting, Configuration cmdLineArgs, ConfigSectionNode rootConfig) : base()//do not call this ctor from derived class
    {
      try
      {
        Constructor(allowNesting, cmdLineArgs, rootConfig);
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
       m_ShutdownStarted = true;
       CleanupApplication();
       base.Destructor();
    }
  }

}
