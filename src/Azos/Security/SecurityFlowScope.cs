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
  /// Implements a logical context set around certain operations, establishing a scope by allocating and disposing the
  /// instance of this class.
  /// The context can be logically nested
  /// </summary>
  public sealed class SecurityFlowScope : DisposableObject
  {
    //ambient flow context
    private static AsyncLocal<SecurityFlowScope> ats_Scope = new AsyncLocal<SecurityFlowScope>();

    public SecurityFlowScope()
    {
      m_Flags = new HashSet<Atom>();
      m_Parent = ats_Scope.Value;
      ats_Scope.Value = this;
    }

    protected override void Destructor()
    {
      base.Destructor();
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
    /// Returns true if the scope has the specified Atom set.
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
