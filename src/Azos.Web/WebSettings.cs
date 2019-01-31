/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Web
{
  /// <summary>
  /// Facilitates fast access to important web-related config settings
  /// </summary>
  public static class WebSettings
  {
    public const string CONFIG_WEBSETTINGS_SECTION = "web-settings";
    public const string CONFIG_SERVICEPOINTMANAGER_SECTION = "service-point-manager";

    //public const string CONFIG_LOGTYPE_ATTR = "log-type";
    //public const string CONFIG_DEFAULT_TIMEOUT_MS_ATTR = "default-timeout-ms";



    /// <summary>
    /// Ensures that ServicePointManager class gets configured via the ServicePointManagerConfigurator instance.
    /// ServicePointManager is .NET-provided class with static methods which configure global Web call/service point properties
    /// such as timeouts and keep-alive.
    /// </summary>
    public static ServicePointManagerConfigurator GetServicePointManagerConfigurator(this IApplication app) =>
      app.NonNull(nameof(app)).Singletons.GetOrCreate<ServicePointManagerConfigurator>(() =>
      {
        var result = new ServicePointManagerConfigurator(app);
        ((IConfigurable)result).Configure(app.ConfigRoot[CONFIG_WEBSETTINGS_SECTION]);
        return result;
      }).instance;

    /// <summary>
    /// Ensures that ServicePointManager class gets configured via the ServicePointManagerConfigurator instance.
    /// ServicePointManager is .NET-provided class with static methods which configure global Web call/service point properties
    /// such as timeouts and keep-alive.
    /// </summary>
    public static void RequireInitilizedServicePointManager(this IApplication app) => app.GetServicePointManagerConfigurator();

  }
}
