/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using Azos.Conf;

namespace Azos.Apps
{
  /// <summary>
  /// Defines a builder interface for constructing an application chassis. <br/><br/>
  /// NOTE: this interface is provided for ASP-like applications only as Azos applications
  /// auto build from config files and do not require programmatic building.
  /// </summary>
  /// <remarks>This interface provides a mechanism to create and configure an application chassis instance.
  /// Implementations of this interface are responsible for assembling the necessary components and dependencies
  /// required by the application.</remarks>
  public interface IAzosAppChassisBuilder
  {
    /// <summary>
    /// Process entry point args
    /// </summary>
    string[] AppArgs {  get; }

    /// <summary>
    /// Builds the root configuration section node for the application chassis.
    /// The "Build()" methods uses this method already to get the config.
    /// </summary>
    ConfigSectionNode AppConfigRoot { get; }

    /// <summary>
    /// Builds the application chassis instance.
    /// </summary>
    IApplicationImplementation Build();
  }


  /// <summary>
  /// Global control and building of Azos application chassis - <see cref="IApplication"/>.<br/><br/>
  /// Note: this class is provided for ASP-like applications only as Azos applications
  /// auto build from config files and do not require programmatic building.
  /// </summary>
  public static class AzosAppChassis
  {
    /// <summary>
    /// Factory method which creates an appropriate builder object which allows us to build app objects programmatically
    /// </summary>
    /// <param name="appArgs">App process start args</param>
    /// <returns></returns>
    public static IAzosAppChassisBuilder CreateBuilder(string [] appArgs)
    {
      //as of now we do not need to make this complex, use one builder
      var builder = new AzosAppChassisDefaultBuilder(appArgs);
      return builder;
    }
  }

  /// <summary>
  /// Facilitates building of Azos application chassis - <see cref="IApplication"/>.<br/><br/>
  /// NOTE: this class is provided for ASP-like applications only as Azos applications
  /// auto build from config files and do not require programmatic building.
  /// </summary>
  public class AzosAppChassisDefaultBuilder : IAzosAppChassisBuilder
  {
    public AzosAppChassisDefaultBuilder(string[] appArgs)
    {
      m_AppArgs = appArgs ?? new string[0];
      m_Config = Configuration.NewEmptyRoot("app");
    }

    protected readonly ConfigSectionNode m_Config;
    protected readonly string[] m_AppArgs;

    /// <inheritdoc/>
    public string[] AppArgs => m_AppArgs;

    /// <inheritdoc/>
    public ConfigSectionNode AppConfigRoot => m_Config;

    /// <inheritdoc/>
    public IApplicationImplementation Build()
    {
      var app = new AzosApplication(AppArgs, AppConfigRoot);
      return app;
    }
  }
}
