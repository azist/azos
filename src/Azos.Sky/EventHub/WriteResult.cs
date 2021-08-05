/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Data;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Describes result of event post into queue
  /// </summary>
  public struct WriteResult
  {
    public WriteResult(IEnumerable<ChangeResult> changes, int total)
    {
      Changes = changes;
      Total = total;
    }

    /// <summary> Changes returned by server </summary>
    public readonly IEnumerable<ChangeResult> Changes;

    /// <summary> Total number of nodes in cluster </summary>
    public readonly int Total;

    /// <summary> How many nodes out of Tried returned OK </summary>
    public int Ok => Changes.Count( c => c.Change != ChangeResult.ChangeType.Undefined);

    /// <summary> How many nodes out of Total were tried </summary>
    public int Tried => Changes.Count();

    /// <summary> Throws when expectation is not met </summary>
    public void EnsureSuccessOfAtLeast(double level)
     => Success.IsTrue( v => v >= level, "Success >= {0:n4}".Args(level));

    public double Success => Tried == 0 ? 0 : Ok / (double)Tried;
    public double Error => Tried == 0 ? 0 : (Tried - Ok) / (double)Tried;
  }
}
