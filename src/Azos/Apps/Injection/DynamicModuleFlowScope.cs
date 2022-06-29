/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;

using Azos.Collections;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Establishes a scope of module presence for the purpose of DI per calling flow which may be asynchronous.
  /// Modules in the scope are visible by the DI
  /// </summary>
  public static class DynamicModuleFlowScope
  {
    private static AsyncLocal<OrderedRegistry<IModule>> ats_FlowModules = new AsyncLocal<OrderedRegistry<IModule>>();

    /// <summary>
    /// Starts a logical scope where dynamic modules may register themselves for
    /// dynamic dependency injection.
    /// The call is re-entrant, if a scope region is already enabled then second call will return false
    /// </summary>
    public static bool Begin()
    {
      var reg = ats_FlowModules.Value;
      if (reg == null)
      {
        reg = new OrderedRegistry<IModule>();
        ats_FlowModules.Value = reg;
        return true;
      }

      return false;
    }

    /// <summary>
    /// Clears the scope started with Begin().
    /// IF the scope was not begun then return false.
    /// A scope has to have zero dependencies on exit - that is all
    /// dependencies must be unregistered before this call
    /// </summary>
    public static bool End()
    {
      var reg = ats_FlowModules.Value;
      if (reg == null) return false;
      (reg.Count == 0).IsTrue("empty scope");
      ats_FlowModules.Value = null;
      return true;
    }


    /// <summary>
    /// Registers a dependency. A scope should have started with Begin().
    /// Returns true when dependency was registered, false if it was already registered
    /// </summary>
    public static bool Register(IModule module)
    {
      module.NonNull(nameof(module));
      var reg = ats_FlowModules.Value;
      reg.NonNull("{0} scope which has begun".Args(nameof(DynamicModuleFlowScope)));
      return reg.Register(module);
    }

    /// <summary>
    /// Unregisters a dependency. A scope should have started with Begin().
    /// Returns true when dependency was unregistered, false if it was already unregistered
    /// </summary>
    public static bool Unregister(IModule module)
    {
      module.NonNull(nameof(module));
      var reg = ats_FlowModules.Value;
      reg.NonNull("{0} scope which has begun".Args(nameof(DynamicModuleFlowScope)));

      var result = reg.Unregister(module);
      return result;
    }


    public static IOrderedRegistry<IModule> Scope => ats_FlowModules.Value;

    /// <summary>
    /// Returns a module with requested type and optional name or null
    /// </summary>
    public static IModule Find(Type tModule, string name)
    {
      var reg = ats_FlowModules.Value;
      if (reg == null) return null;

      if (name.IsNotNullOrWhiteSpace())
      {
        var result = reg[name];
        if (result==null) return null;
        if (!tModule.IsAssignableFrom(result.GetType())) return null;
        return result;
      }
      else
      {
        //todo: Optimize linear search with a trie
        var result = reg.OrderedValues
                        .FirstOrDefault(m => tModule.IsAssignableFrom(m.GetType()));
        return result;
      }
    }

  }
}
