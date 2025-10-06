/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using Azos.Conf;
using Azos.Log.Sinks;

namespace Azos.Apps
{
  /// <summary>
  /// Extensions methods for building azos applications in a fluent manner
  /// </summary>
  public static class AppBuilderPatternExtensions
  {

    public static IAzosAppChassisBuilder AddModule<T>(this IAzosAppChassisBuilder builder, string name, IConfigSectionNode details = null) where T : IModule
    {
      var root = builder.NonNull(nameof(builder)).AppConfigRoot;
      var modules = root[CommonApplicationLogic.CONFIG_MODULES_SECTION];
      if (!modules.Exists)
      {
        modules = root.AddChildNode(CommonApplicationLogic.CONFIG_MODULES_SECTION);
      }

      var module = modules.AddChildNode(CommonApplicationLogic.CONFIG_MODULE_SECTION);
      module.AddAttributeNode(Configuration.CONFIG_NAME_ATTR, name.NonBlank(nameof(name)));
      module.AddAttributeNode(FactoryUtils.CONFIG_TYPE_ATTR, typeof(T).AssemblyQualifiedName);
      if (details != null && details.Exists) module.OverrideBy(details);

      return builder;
    }


    /// <summary>
    /// Adds console logging
    /// </summary>
    public static IAzosAppChassisBuilder AddConsoleLogging(this IAzosAppChassisBuilder builder, string name = null, bool colored = false)
    {
      var root = builder.NonNull(nameof(builder)).AppConfigRoot;
      var log = root[CommonApplicationLogic.CONFIG_LOG_SECTION];
      if (!log.Exists)
      {
        log = root.AddChildNode(CommonApplicationLogic.CONFIG_LOG_SECTION);
      }

      var sink = log.AddChildNode(Log.LogDaemonBase.CONFIG_SINK_SECTION);
      if (name.IsNotNullOrWhiteSpace()) sink.AddAttributeNode(Configuration.CONFIG_NAME_ATTR, name);
      sink.AddAttributeNode(FactoryUtils.CONFIG_TYPE_ATTR, "Azos.Log.Sinks.ConsoleLogSink, Azos");
      sink.AddAttributeNode(nameof(ConsoleSink.Colored), colored);

      //todo: Add kubernetes format

      return builder;
    }

    public static IAzosAppChassisBuilder AddKubeConsoleLogging(this IAzosAppChassisBuilder builder, string name = null)
    {
      var root = builder.NonNull(nameof(builder)).AppConfigRoot;
      var log = root[CommonApplicationLogic.CONFIG_LOG_SECTION];
      if (!log.Exists)
      {
        log = root.AddChildNode(CommonApplicationLogic.CONFIG_LOG_SECTION);
      }

      var sink = log.AddChildNode(Log.LogDaemonBase.CONFIG_SINK_SECTION);
      if (name.IsNotNullOrWhiteSpace()) sink.AddAttributeNode(Configuration.CONFIG_NAME_ATTR, name);
      sink.AddAttributeNode(FactoryUtils.CONFIG_TYPE_ATTR, "Azos.Log.Sinks.KubeConsoleLogSink, Azos");

      return builder;
    }

  }
}
