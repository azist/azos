/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

namespace Azos.Apps.Volatile
{
  /// <summary>
  /// Defines a provider that does not do anything - does not store object anywhere but memory
  /// </summary>
  public class NOPObjectStoreProvider : ObjectStoreProvider
  {
    internal NOPObjectStoreProvider(ObjectStoreDaemon director) : base(director)
    {

    }

    public override IEnumerable<ObjectStoreEntry> LoadAll()
    {
      return Enumerable.Empty<ObjectStoreEntry>();
    }

    public override void Write(ObjectStoreEntry entry)
    {
    }

    public override void Delete(ObjectStoreEntry entry)
    {
    }
  }
}
