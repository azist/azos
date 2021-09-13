/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DataReflectionUtilsTests
  {
    [Run]
    public void Target_ANY()
    {
      var sut = DataReflectionUtils.GetFieldDescriptorsFor(typeof(Doc1), null);
      // sut.See();
      Aver.AreEqual(5, sut.Count);

      Aver.AreEqual("Field1", sut["Field1"].TargetFieldName);
      Aver.AreEqual("Field2", sut["Field2"].TargetFieldName);
      Aver.AreEqual("Field3", sut["Field3"].TargetFieldName);
      Aver.AreEqual("Field4", sut["Field4"].TargetFieldName);
      Aver.AreEqual("field-five", sut["field-five"].TargetFieldName);

      Aver.AreEqual("field-five", sut["field-five"].Attr.BackendName);
      Aver.AreEqual(98000, sut["field-five"].Attr.Metadata.Of(DataReflectionUtils.META_FIELD_ORDER).ValueAsInt());
      Aver.AreEqual("Field5", sut["field-five"].FieldDef.Name);

      Aver.AreEqual("Field1", sut[0].TargetFieldName);
      Aver.AreEqual("Field2", sut[1].TargetFieldName);
      Aver.AreEqual("Field3", sut[2].TargetFieldName);
      Aver.AreEqual("Field4", sut[3].TargetFieldName);
      Aver.AreEqual("field-five", sut[4].TargetFieldName);
    }

    [Run]
    public void Target_ANY_Exact()
    {
      var sut = DataReflectionUtils.GetFieldDescriptorsExactlyFor(typeof(Doc1), null);
      Aver.AreEqual(5, sut.Count);

      Aver.AreEqual("Field1", sut["Field1"].TargetFieldName);
      Aver.AreEqual("Field2", sut["Field2"].TargetFieldName);
      Aver.AreEqual("Field3", sut["Field3"].TargetFieldName);
      Aver.AreEqual("Field4", sut["Field4"].TargetFieldName);
      Aver.AreEqual("field-five", sut["field-five"].TargetFieldName);

      Aver.AreEqual("field-five", sut["field-five"].Attr.BackendName);
      Aver.AreEqual(98000, sut["field-five"].Attr.Metadata.Of(DataReflectionUtils.META_FIELD_ORDER).ValueAsInt());
      Aver.AreEqual("Field5", sut["field-five"].FieldDef.Name);

      Aver.AreEqual("Field1", sut[0].TargetFieldName);
      Aver.AreEqual("Field2", sut[1].TargetFieldName);
      Aver.AreEqual("Field3", sut[2].TargetFieldName);
      Aver.AreEqual("Field4", sut[3].TargetFieldName);
      Aver.AreEqual("field-five", sut[4].TargetFieldName);
    }

    [Run]
    public void Target_A()
    {
      var sut = DataReflectionUtils.GetFieldDescriptorsFor(typeof(Doc1), "system-a");
      Aver.AreEqual(5, sut.Count);

      Aver.AreEqual("Field1", sut["Field1"].TargetFieldName);
      Aver.AreEqual("f2", sut["f2"].TargetFieldName);
      Aver.AreEqual("Field3", sut["Field3"].TargetFieldName);
      Aver.AreEqual("Field4", sut["Field4"].TargetFieldName);
      Aver.AreEqual("field-five", sut["field-five"].TargetFieldName);

      Aver.AreEqual("field-five", sut["field-five"].Attr.BackendName);
      Aver.AreEqual(98000, sut["field-five"].Attr.Metadata.Of(DataReflectionUtils.META_FIELD_ORDER).ValueAsInt());
      Aver.AreEqual("Field5", sut["field-five"].FieldDef.Name);

      Aver.AreEqual("Field1", sut[0].TargetFieldName);
      Aver.AreEqual("f2", sut[1].TargetFieldName);
      Aver.AreEqual("Field3", sut[2].TargetFieldName);
      Aver.AreEqual("Field4", sut[3].TargetFieldName);
      Aver.AreEqual("field-five", sut[4].TargetFieldName);
    }

    [Run]
    public void Target_A_Exact()
    {
      var sut = DataReflectionUtils.GetFieldDescriptorsExactlyFor(typeof(Doc1), "system-a");
      Aver.AreEqual(1, sut.Count);

      Aver.AreEqual("f2", sut["f2"].TargetFieldName);

      Aver.AreEqual("f2", sut["f2"].Attr.BackendName);
      Aver.AreEqual(1, sut["f2"].Attr.Metadata.Of(DataReflectionUtils.META_FIELD_ORDER).ValueAsInt());
      Aver.AreEqual("Field2", sut["f2"].FieldDef.Name);

      Aver.AreEqual("f2", sut[0].TargetFieldName);
    }

    [Run]
    public void Target_B()
    {
      var sut = DataReflectionUtils.GetFieldDescriptorsFor(typeof(Doc1), "system-b");
      Aver.AreEqual(5, sut.Count);

      Aver.AreEqual("Field1", sut["field1"].TargetFieldName);
      Aver.AreEqual("field-five", sut["field-five"].TargetFieldName);

      Aver.AreEqual("f-4", sut["f-4"].TargetFieldName);
      Aver.AreEqual("f-2", sut["f-2"].TargetFieldName);
      Aver.AreEqual("f-3", sut["f-3"].TargetFieldName);

      Aver.AreEqual("f-3", sut["f-3"].Attr.BackendName);
      Aver.AreEqual(-10, sut["f-3"].Attr.Metadata.Of(DataReflectionUtils.META_FIELD_ORDER).ValueAsInt());
      Aver.AreEqual("Field3", sut["f-3"].FieldDef.Name);

      Aver.AreEqual("f-4", sut[0].TargetFieldName);
      Aver.AreEqual("f-3", sut[1].TargetFieldName);
      Aver.AreEqual("Field1", sut[2].TargetFieldName);
      Aver.AreEqual("f-2", sut[3].TargetFieldName);
      Aver.AreEqual("field-five", sut[4].TargetFieldName);
    }

    [Run]
    public void Target_B_Exact()
    {
      var sut = DataReflectionUtils.GetFieldDescriptorsExactlyFor(typeof(Doc1), "system-b");
      Aver.AreEqual(3, sut.Count);

      Aver.AreEqual("f-4", sut["f-4"].TargetFieldName);
      Aver.AreEqual("f-2", sut["f-2"].TargetFieldName);
      Aver.AreEqual("f-3", sut["f-3"].TargetFieldName);

      Aver.AreEqual("f-3", sut["f-3"].Attr.BackendName);
      Aver.AreEqual(-10, sut["f-3"].Attr.Metadata.Of(DataReflectionUtils.META_FIELD_ORDER).ValueAsInt());
      Aver.AreEqual("Field3", sut["f-3"].FieldDef.Name);

      Aver.AreEqual("f-4", sut[0].TargetFieldName);
      Aver.AreEqual("f-3", sut[1].TargetFieldName);
      Aver.AreEqual("f-2", sut[2].TargetFieldName);
    }

    [Run]
    public void Target_C_Exact()
    {
      var sut = DataReflectionUtils.GetFieldDescriptorsExactlyFor(typeof(Doc1), "system-c");
      Aver.AreEqual(2, sut.Count);

      Aver.AreEqual("f-four", sut["f-four"].TargetFieldName);
      Aver.AreEqual("five", sut["five"].TargetFieldName);

      Aver.AreEqual("five", sut["five"].Attr.BackendName);
      Aver.AreEqual(-100, sut["five"].Attr.Metadata.Of(DataReflectionUtils.META_FIELD_ORDER).ValueAsInt());
      Aver.AreEqual("Field5", sut["five"].FieldDef.Name);

      Aver.AreEqual("five", sut[0].TargetFieldName);
      Aver.AreEqual("f-four", sut[1].TargetFieldName);
    }

    public class Doc1 : TypedDoc
    {
      [Field(metadata: "ord=1")]
      public string Field1 { get; set; }

      [Field(metadata: "ord=2")]
      [Field("system-a", BackendName = "f2", MetadataContent = "ord=1")]
      [Field("system-b", BackendName = "f-2", MetadataContent = "ord=20")]
      public string Field2 { get; set; }

      [Field(metadata: "ord=3")]
      [Field("system-b", BackendName = "f-3", MetadataContent = "ord=-10")]
      public string Field3 { get; set; }

      //Even though this field does not have ANY_TARGET, it gets added automatically
      [Field("system-c", BackendName = "f-four", MetadataContent = "ord=1")]
      [Field("system-b", BackendName = "f-4", MetadataContent = "ord=-20")]
      public string Field4 { get; set; }

      [Field(backendName: "field-five", metadata: "ord=98000")]
      [Field("system-c", BackendName = "five", MetadataContent = "ord=-100")]
      public string Field5 { get; set; }
    }

  }
}
