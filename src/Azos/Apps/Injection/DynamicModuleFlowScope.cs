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

    public static bool Register(IModule module)
    {
      module.NonNull(nameof(module));
      var reg = ats_FlowModules.Value;
      if (reg==null)
      {
        reg = new OrderedRegistry<IModule>();
        ats_FlowModules.Value = reg;
      }

      return reg.Register(module);
    }

    public static bool Unregister(IModule module)
    {
      module.NonNull(nameof(module));
      var reg = ats_FlowModules.Value;
      if (reg == null) return false;

      var result = reg.Unregister(module);
      if (reg.Count==0) ats_FlowModules.Value = null;
      return result;
    }

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
