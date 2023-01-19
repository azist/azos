/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Conf;
using Azos.Sky.Coordination;
using Azos.Sky.Metabase;

namespace Azos.Sky//for convenience it is in root namespace
{
  /// <summary>
  /// Provides access to HostSetBuilder singleton
  /// </summary>
  public static class HostSetBuilderAccessor
  {
    /// <summary>
    /// Provides access to HostSetBuilder singleton instance of the app context
    /// </summary>
    public static HostSetBuilder GetHostSetBuilder(this IApplication app)
      => app.AsSky()
            .Singletons
            .GetOrCreate(() =>
            {
              var conf = "mbroot/"+Metabank.CONFIG_HOST_SET_BUILDER_SECTION;
              try
              {
                var sky = app.AsSky();

                var node = sky.Metabase.RootConfig[Metabank.CONFIG_HOST_SET_BUILDER_SECTION];
                conf = node.AsTextSnippet();
                return FactoryUtils.MakeComponent<HostSetBuilder>(app, node, typeof(HostSetBuilder), new[] { node });
              }
              catch (Exception error)
              {
                throw new CoordinationException(ServerStringConsts.HOST_SET_BUILDER_SINGLETON_CONFIG_ERROR
                                                            .Args(conf, error.ToMessageWithType()), error);
              }
            }).instance;

    /// <summary>
    /// Shortcut to HostSetBuilder.FindAndBuild()....
    /// Tries to find a named host set starting at the requested cluster level.
    /// Throws if not found.
    /// </summary>
    public static THostSet FindAndBuild<THostSet>(this IApplication app, string setName, string clusterPath, bool searchParent = true, bool transcendNoc = false)
      where THostSet : HostSet
      => app.GetHostSetBuilder().FindAndBuild<THostSet>(setName, clusterPath, searchParent, transcendNoc);

    /// <summary>
    /// Shortcut to HostSetBuilder.TryFindAndBuild()....
    /// Tries to find a named host set starting at the requested cluster level.
    /// Returns null if not found.
    /// </summary>
    public static THostSet TryFindAndBuild<THostSet>(this IApplication app, string setName, string clusterPath, bool searchParent = true, bool transcendNoc = false)
      where THostSet : HostSet
      => app.GetHostSetBuilder().TryFindAndBuild<THostSet>(setName, clusterPath, searchParent, transcendNoc);
  }
}
