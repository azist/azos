/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

using Azos.Sky.Metabase;

namespace Azos.Sky.Dynamic
{
  /// <summary>
  /// Represents contract for working with dynamic cloud runtimes
  /// </summary>
  public interface IHostManager
  {
    Contracts.DynamicHostID Spawn(Metabank.SectionHost host, string id);
    string GetHostName(Contracts.DynamicHostID hid);
  }

  public interface IHostManagerImplementation : IHostManager, IApplicationComponent, IDisposable, IConfigurable, IInstrumentable
  {
  }
}
