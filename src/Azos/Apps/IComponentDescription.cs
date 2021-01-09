/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Apps
{
  /// <summary>
  /// Provides service information: The description and status.
  /// This interface is used by for component management
  /// </summary>
  public interface IComponentDescription
  {
    /// <summary>
    /// Provides short textual description of this component service which is typically used by hosting apps
    /// to describe the services provided to callers.
    /// Usually this lists end-points that the server is listening on etc..
    /// </summary>
    string ServiceDescription { get; }

    /// <summary>
    /// Provides short textual current component status description, e.g. Daemons report their Status property
    /// </summary>
    string StatusDescription { get; }
  }
}
