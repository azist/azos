/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data.Idgen;

namespace Azos.Data
{
  /// <summary>
  /// Represents a special value that signifies the absence of any entity/state/value/data.
  /// The instances may be stored in a cache to indicate that the key exists, but points to a non-existing "absent" entity.
  /// This is useful for DB lookups, not to touch the backend for values that don't exist.
  /// Use AbsentValue.Instance singleton
  /// </summary>
  [Serializable]
  public sealed class AbsentValue : IDistributedStableHashProvider
  {
    public static readonly AbsentValue Instance = new AbsentValue();

    private AbsentValue() { }

    public override int GetHashCode()       => 0;
    public override bool Equals(object obj) =>  obj is AbsentValue;
    public override string ToString()       => "[Absent]";

    public ulong GetDistributedStableHash() => 0ul;
  }
}
