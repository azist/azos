/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Data.Heap
{
  public static class Extensions
  {
    public static long SetReplicationNode(this long mask, byte nodeId) => mask | (1L << nodeId);
    public static bool IsReplicationNodeSet(this long mask, byte nodeId) => 0 != (mask & (1L << nodeId));
  }
}
