/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos;
using Azos.Data;
using Azos.Log;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Bix("BF3E4485-07B9-4797-9DE6-2AF4AAED93FB")]
  [BixJsonHandler]
  public class bxonBaseDoc : AmorphousTypedDoc
  {
    [Field] public string String1 {  get; set; }
    [Field] public int Int1 { get; set; }
    [Field] public int? NInt1 { get; set; }
    [Field] public Atom Atom1 { get; set; }
    [Field] public GDID Gdid1 { get; set; }
    [Field] public JsonDataMap Jdm1 { get; set; }
    [Field] public JsonDataArray Jar1 { get; set; }

    [Field] public object Obj1 { get; set; }
    [Field] public object[] ObjArr1 { get; set; }

    protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      if (def?.Order == 0)
      {
        BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
      }

      base.AddJsonSerializerField(def, options, jsonMap, name, value);
    }
  }

  [Bix("E4C90E2E-5CB8-4632-A956-0605C56DABEC")]
  public class bxonADoc : bxonBaseDoc
  {
    [Field] public string String2 { get; set; }
  }

  [Bix("2C9B792D-BEB9-4DBD-8467-49FFAD0177B9")]
  public class bxonBDoc : bxonBaseDoc
  {
    [Field] public bool Flag1 { get; set; }
    [Field] public object Obj2 { get; set; }
  }
}