/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Azos.Security
{
  /// <summary>
  /// Implements a logical context set around certain operations, establishing a scope by allocating
  /// and disposing the instance of this class. The context can be logically nested just by allocating
  /// an instance of another inner context - it will capture the `Parent`/outer context if one exists.
  /// </summary>
  public sealed class SecurityFlowScope : DisposableObject
  {
    //ambient flow context
    private static AsyncLocal<SecurityFlowScope> ats_Scope = new AsyncLocal<SecurityFlowScope>();


    /// <summary>
    /// Returns the ambient current inner-most scope OR NULL if nothing was set in the code which called this code
    /// </summary>
    public static SecurityFlowScope Current => ats_Scope.Value;

    /// <summary>
    /// If a scope is set then determines if that scope or any of its parents scopes have the specified flag returning true if they do.
    /// False if there is no scope set or that flag is not set in call scope chain.
    /// You can checks for flag only in the inner-most scope by calling `Current?.HasFlag(flag, onlyCurrent: true)`
    /// </summary>
    public static bool CheckFlag(Atom flag)
    {
      var current = Current;
      if (current == null) return false;
      return current.HasFlag(flag);
    }

    public SecurityFlowScope(Atom flag) : this(flag, Atom.ZERO, Atom.ZERO, Atom.ZERO){ }
    public SecurityFlowScope(Atom flag1, Atom flag2) : this(flag1, flag2, Atom.ZERO, Atom.ZERO) { }
    public SecurityFlowScope(Atom flag1, Atom flag2, Atom flag3, Atom flag4)
    {
      m_Flags = new HashSet<Atom>();
      if (!flag1.IsZero) m_Flags.Add(flag1);
      if (!flag2.IsZero) m_Flags.Add(flag2);
      if (!flag3.IsZero) m_Flags.Add(flag3);
      if (!flag4.IsZero) m_Flags.Add(flag4);
      ctor();
    }

    public SecurityFlowScope(params Atom[] flags)
    {
      m_Flags = new HashSet<Atom>(flags.NonNull(nameof(flags)));
      ctor();
    }

    private void ctor()
    {
      m_Parent = ats_Scope.Value;
      ats_Scope.Value = this;
    }

    protected override void Destructor()
    {
      ats_Scope.Value = m_Parent;
    }

    private SecurityFlowScope m_Parent;
    private HashSet<Atom> m_Flags;


    /// <summary>
    /// Returns a parent scope for this scope or null if this scope is the most outer scope
    /// </summary>
    public SecurityFlowScope Parent => m_Parent;

    /// <summary>
    /// Returns true if this scope was allocated in the parent scope
    /// </summary>
    public bool HasParent => m_Parent != null;

    /// <summary>
    /// Returns true if the scope has the specified flag Atom set.
    /// If this scope has a parent scope then it is also searched unless `onlyCurrent` is set to true
    /// in which case only the current inner-most scope is checked for flag presence
    /// </summary>
    public bool HasFlag(Atom flag, bool onlyCurrent = false)
    {
      if (m_Flags.Contains(flag)) return true;
      if (!onlyCurrent && m_Parent != null) return m_Parent.HasFlag(flag);
      return false;
    }

  }
}
