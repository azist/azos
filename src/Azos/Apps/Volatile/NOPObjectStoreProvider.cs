/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;


using Azos.Log;
using Azos.Conf;
using Azos.Serialization;
using Azos.Serialization.Slim;

namespace Azos.Apps.Volatile
{
  /// <summary>
  /// Defines a provider that does not do anything - does not store object anywhere but memory
  /// </summary>
  public class NOPObjectStoreProvider : ObjectStoreProvider
  {
    #region .ctor
        public NOPObjectStoreProvider() : base(null)
        {

        }

        public NOPObjectStoreProvider(ObjectStoreService director) : base(director)
        {

        }
    #endregion


    #region Public

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


    #endregion

  }
}
