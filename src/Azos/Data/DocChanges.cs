/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Data
{
  /// <summary>
  /// Describes what kind of modification was done
  /// </summary>
  public enum DocChangeType { Insert, Upsert, Update, Delete }

  /// <summary>
  /// Describes data document modification
  /// </summary>
  public struct DocChange
  {
    public DocChange(DocChangeType type, Doc doc, Access.IDataStoreKey key)
    {
        ChangeType = type;
        Doc = doc;
        Key = key;
    }

    public readonly DocChangeType ChangeType;
    public readonly Doc Doc;
    public readonly Access.IDataStoreKey Key;
  }
}
