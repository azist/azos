/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Data.Adlib.Server
{
  public static class BsonConvert
  {
    public const string ADLIB_COLLECTION_PREFIX = "adl_";

    public static Atom MongoToCanonicalCollectionName(string mongoName)
    {
      mongoName.NonBlankMin(ADLIB_COLLECTION_PREFIX.Length + 1);
      var canonical = mongoName.Substring(ADLIB_COLLECTION_PREFIX.Length);
      return Atom.Encode(canonical);
    }

  }
}
