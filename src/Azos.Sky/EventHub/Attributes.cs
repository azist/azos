/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

using Azos.Platform;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Decorates EventDocument-derived classes, providing event routing info: Namespace and Queue atoms
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class EventAttribute : Attribute
  {
    /// <summary>
    /// Decorates EventDocument-derived classes, providing event routing info: Namespace and Queue atoms
    /// </summary>
    public EventAttribute(string ns, string queue, DataLossMode mode)
    {
      Namespace = Atom.Encode(ns.NonBlank(nameof(ns)));
      Queue = Atom.Encode(ns.NonBlank(nameof(queue)));
      LossMode = mode;
    }

    public readonly Atom Namespace;
    public readonly Atom Queue;
    public readonly DataLossMode LossMode;

    /// <summary> returns (Namespace:Queue) tuple </summary>
    public Route Route => new Route(Namespace, Queue);

    private static readonly FiniteSetLookup<Type, EventAttribute> s_Cache = new FiniteSetLookup<Type, EventAttribute>(
                                                                                  t => t.GetCustomAttribute<EventAttribute>(false));

    /// <summary> Retrieves EventAttribute for EventDocument-derivative or null if not decorated</summary>
    public static EventAttribute TryGetFor(Type t)
      => s_Cache[t.IsOfType<EventDocument>()];

    /// <summary> Retrieves EventAttribute for EventDocument-derivative or throws if not decorated</summary>
    public static EventAttribute GetFor(Type t)
      => TryGetFor(t).NonNull("[{0}] on `{1}`".Args(nameof(EventAttribute), t.DisplayNameWithExpandedGenericArgs()));
  }
}
