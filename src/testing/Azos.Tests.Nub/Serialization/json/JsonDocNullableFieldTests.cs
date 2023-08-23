/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Reflection;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JsonDocNullableFieldTests
  {

    public class DocWithNullables : AmorphousTypedDoc
    {
      public override bool AmorphousDataEnabled => true;

      [Field(required: false, Description = "Tree path for org unit. So the user list may be searched by it")]
      public EntityId? OrgUnit{ get; set; }
    }




    [Run]
    public void Test01()
    {
      var json = @"{""OrgUnit"": ""path@sys::addr""}";

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.OrgUnit);
      Aver.AreEqual("addr", got.OrgUnit.Value.Address);

      got.See();
    }
  }
}
