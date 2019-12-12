/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class FieldAttrTargetInheritTests
  {
    [Run]
    public void Basic()
    {
      var schema = Schema.GetForTypedDoc<BasicDoc>();
      Aver.AreEqual(1, schema.FieldCount);
      var def = schema["Data"];
      Aver.IsNotNull(def);

      def.Attrs.See();

      Aver.AreEqual(5, def.Attrs.Count());
      var atrANY = def[null];
      Aver.AreEqual(FieldAttribute.ANY_TARGET, atrANY.TargetName);
      Aver.IsNull(atrANY.CloneFromTargetName);
      Aver.AreEqual("Common description", atrANY.Description);
      Aver.IsFalse(atrANY.Required);


      var atrL1 = def["L1"];
      Aver.AreEqual("L1", atrL1.TargetName);
      Aver.AreEqual(FieldAttribute.ANY_TARGET, atrL1.CloneFromTargetName);
      Aver.AreEqual("Common description", atrL1.Description);
      Aver.IsTrue(atrL1.Required);
      Aver.AreEqual(0, atrL1.MaxLength);
      Aver.AreEqual(0, atrL1.MinLength);

      var atrL2 = def["L2"];
      Aver.AreEqual("L2", atrL2.TargetName);
      Aver.AreEqual("L1", atrL2.CloneFromTargetName);
      Aver.AreEqual("Common description", atrL2.Description);
      Aver.IsTrue(atrL2.Required);
      Aver.AreEqual(190, atrL2.MaxLength);
      Aver.AreEqual(0, atrL2.MinLength);

      var atrL3 = def["L3"];
      Aver.AreEqual("L3", atrL3.TargetName);
      Aver.AreEqual("L2", atrL3.CloneFromTargetName);
      Aver.AreEqual("Palm tree 3", atrL3.Description);
      Aver.IsTrue(atrL3.Required);
      Aver.AreEqual(190, atrL3.MaxLength);
      Aver.AreEqual(1, atrL3.MinLength);

      var atrALT = def["ALT"];
      Aver.AreEqual("ALT", atrALT.TargetName);
      Aver.AreEqual(FieldAttribute.ANY_TARGET, atrALT.CloneFromTargetName);
      Aver.AreEqual("Description override", atrALT.Description);
      Aver.IsFalse(atrALT.Required);
      Aver.AreEqual(0, atrALT.MaxLength);
      Aver.AreEqual(0, atrALT.MinLength);

    }

    public class BasicDoc : TypedDoc
    {
      [Field(description: "Common description")]
      [Field(null, "L1", Required = true)]
      [Field("L1", "L2", MaxLength = 190)]
      [Field("L2", "L3", MinLength = 1, Description = "Palm tree 3")]
      [Field(null, "ALT", Description = "Description override")]//same as ANY_TARGET
      public string Data{ get; set;}
    }



  }
}
