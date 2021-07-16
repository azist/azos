/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Describes result of event post into queue
  /// </summary>
  public struct WriteResult
  {
    public WriteResult(int ok, int tried, int total)
    {
      Ok = ok;
      Tried = tried;
      Total = total;
    }

    /// <summary> How many nodes out of Tried returned OK </summary>
    public readonly int Ok;

    /// <summary> How many nodes out of Total were tried </summary>
    public readonly int Tried;

    /// <summary> Total number of nodes in cluster </summary>
    public readonly int Total;

    public double Success => Tried == 0 ? 0 : Ok / (double)Tried;
    public double Error => Tried == 0 ? 0 : (Tried - Ok) / (double)Tried;
  }
}
