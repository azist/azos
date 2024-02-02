/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using Azos.Data;

namespace Azos.Scripting.Expressions.Data
{
  /// <summary>
  /// Context under which data scripting expressions execute.
  /// You can derive and extend this class, passing the derived instance into "RunScript()"
  /// family of methods
  /// </summary>
  public class ScriptCtx
  {
    public ScriptCtx(Doc doc)
    {
      Data = doc.NonNull(nameof(doc));
    }

    /// <summary>
    /// Primary Data document which this script operates on
    /// </summary>
    public readonly Doc Data;

    private Exception m_Error;

    /// <summary>
    /// Returns current error or null if none
    /// </summary>
    public Exception Error => m_Error;

    /// <summary>
    /// Sets the current error object
    /// </summary>
    public void SetError(Exception err) => m_Error = err;

    /// <summary>
    /// Clears current error
    /// </summary>
    public void ClearError(){ m_Error = null; }

  }
}
