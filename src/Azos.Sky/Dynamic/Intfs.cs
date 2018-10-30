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
