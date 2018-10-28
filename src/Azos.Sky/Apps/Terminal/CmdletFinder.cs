using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Azos.Sky.Apps.Terminal
{
  /// <summary>
  /// Provides enumerations of well-known cmdlets and facilities to find process-specific cmdlets
  /// </summary>
  public static class CmdletFinder
  {
    /// <summary>
    /// Provides common cmdlets supported by all servers
    /// </summary>
    public static IEnumerable<Type> Common
    {
      get
      {
        return FindByNamespace(typeof(AppRemoteTerminal), "Azos.Sky.Apps.Terminal.Cmdlets");
      }
    }

    /// <summary>
    /// Provides cmdlets supported by host governor
    /// </summary>
    public static IEnumerable<Type> HGov
    {
      get
      {
        return FindByNamespace(typeof(AppRemoteTerminal), "Azos.Sky.Apps.HostGovernor.Cmdlets");
      }
    }

    /// <summary>
    /// Provides cmdlets supported by zone governor
    /// </summary>
    public static IEnumerable<Type> ZGov
    {
      get
      {
        return FindByNamespace(typeof(AppRemoteTerminal), "Azos.Sky.Apps.ZoneGovernor.Cmdlets");
      }
    }


    /// <summary>
    /// Finds cmdlets in assembly that contains the specified type, optionally filtering the cmdlet types
    /// </summary>
    public static IEnumerable<Type> FindByNamespace(Type assemblyContainingType, string nsFilter = null)
    {
      return nsFilter.IsNullOrWhiteSpace() ? Find(assemblyContainingType)
                                           : Find(assemblyContainingType, (t) => t.Namespace.EqualsSenseCase(nsFilter));
    }

    /// <summary>
    /// Finds cmdlets in assembly that contains the specified type, optionally filtering the cmdlet types
    /// </summary>
    public static IEnumerable<Type> Find(Type assemblyContainingType, Func<Type, bool> filter = null)
    {
      if (assemblyContainingType==null) assemblyContainingType = typeof(CmdletFinder);
      var asm = Assembly.GetAssembly(assemblyContainingType);
      var cmdlets = asm.GetTypes()
                       .Where(t => !t.IsAbstract && typeof(Cmdlet).IsAssignableFrom(t));

      return filter!=null? cmdlets.Where( t => filter(t)) : cmdlets;
    }

  }
}
