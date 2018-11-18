/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Log;
using Azos.Conf;

namespace Azos.Apps.Volatile
{
  /// <summary>
  /// Defines a base provider that stores objects for ObjectStoreDaemon class
  /// </summary>
  public abstract class ObjectStoreProvider : Daemon<ObjectStoreDaemon>
  {
    protected ObjectStoreProvider(ObjectStoreDaemon store) : base(store)
    {
    }

    public abstract IEnumerable<ObjectStoreEntry> LoadAll();
    public abstract void Write(ObjectStoreEntry entry);
    public abstract void Delete(ObjectStoreEntry entry);
    public override string ComponentLogTopic => CoreConsts.OBJSTORE_TOPIC;
  }
}
