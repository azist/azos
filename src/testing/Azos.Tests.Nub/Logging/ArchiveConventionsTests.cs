/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using Azos.Data;
using Azos.Log;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Logging
{
  [Runnable]
  public class ArchiveConventionsTests
  {
    [Run]
    public void Ad_Test01()
    {
      var got = ArchiveConventions.DecodeArchiveDimensionsMap((string)null);
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeArchiveDimensionsMap("       ");
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeArchiveDimensionsMap((IArchiveLoggable)null);
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeArchiveDimensionsMap("not a content produced by convention");
      Aver.IsNull(got);
    }

    [Run]
    public void Ad_Test02()
    {
      var encoded = ArchiveConventions.EncodeArchiveDimensions( new { a = 1, b = 3 });
      encoded.See();

      var decoded = ArchiveConventions.DecodeArchiveDimensionsMap(encoded);
      Aver.IsNotNull(decoded);
      Aver.AreEqual(2, decoded.Count);

      Aver.AreObjectsEqual(1, decoded["a"]);
      Aver.AreObjectsEqual(3, decoded["b"]);
    }

    [Run]
    public void Ad_Test03()
    {
      var encoded1 = ArchiveConventions.EncodeArchiveDimensions(new { a = 1, b = 3 });
      var encoded2 = ArchiveConventions.EncodeArchiveDimensions(new { b = 3, a = 1, c = (string)null });//notice a different sequence of keys
      encoded1.See();
      encoded2.See();
      Aver.AreEqual(encoded1, encoded2);//however the strings are equal, because keys are sorted and nulls are skipped

      var decoded = ArchiveConventions.DecodeArchiveDimensionsMap(encoded1);
      Aver.IsNotNull(decoded);
      Aver.AreEqual(2, decoded.Count);

      Aver.AreObjectsEqual(1, decoded["a"]);
      Aver.AreObjectsEqual(3, decoded["b"]);
    }

    [Run]
    public void SD_Test01()
    {
      var got = ArchiveConventions.DecodeStructuredDataMap((string)null);
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeStructuredDataMap("       ");
      Aver.IsNull(got);

      got = ArchiveConventions.DecodeStructuredDataMap("not a content produced by convention");
      Aver.IsNull(got);
    }

    [Run]
    public void SD_Test02()
    {
      var encoded = ArchiveConventions.EncodeStructuredData(new { a = 1, b = 3 });
      encoded.See();

      var decoded = ArchiveConventions.DecodeStructuredDataMap(encoded);
      Aver.IsNotNull(decoded);
      Aver.AreEqual(2, decoded.Count);

      Aver.AreObjectsEqual(1, decoded["a"]);
      Aver.AreObjectsEqual(3, decoded["b"]);
    }
  }
}