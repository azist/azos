/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class GetOnlyFieldTests
  {
    public class Doc1 : TypedDoc
    {
      [Field]
      public string F1 { get; set; }

      [Field]
      public string F2 => F1 + "+two";
    }


    [Run]
    public void Test001()
    {
      var doc = new Doc1 { F1 = "one" };

      Aver.AreEqual("one+two", doc.F2);

      doc.Schema.See();

      var json = doc.ToJson();

      json.See();

      var doc2 = JsonReader.ToDoc<Doc1>(json);

      doc2.See();

    }
  }
}
