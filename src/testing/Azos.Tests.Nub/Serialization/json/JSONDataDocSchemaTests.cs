/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;

using Azos.Serialization.JSON;
using Azos.Data;
using System.Collections.Generic;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JSONDataDocSchemaTests
  {
    public class AddressDoc : AmorphousTypedDoc
    {
      [Field] public string Address { get; set; }
      [Field] public string City { get; set; }
      [Field] public string State { get; set; }
      [Field] public string Zip { get; set; }
    }

    public class Doc1 : AmorphousTypedDoc
    {
      [Field]
      public long? ID { get; set; }

      [Field]
      public string CustomerName { get; set; }

      [Field]
      public AddressDoc Address { get; set; }

      [Field]
      public Doc1 Relative { get; set; }

      [Field]
      public AddressDoc[] AddressArray { get; set; }

      [Field]
      public List<AddressDoc> AddressList { get; set; }
    }

    [Run]
    public void FlatTest()
    {
      var d1 = new Doc1 { ID = 1234, CustomerName = "Snake Lam" };
      var json = d1.Schema.ToJson(JsonWritingOptions.PrettyPrint);
      json.See();
      //todo This needs to be redesigned per Schema JSOn writable re-design
    }

  }//class
}//namespace
