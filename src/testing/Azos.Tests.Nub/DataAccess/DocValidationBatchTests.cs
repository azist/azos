using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DocValidationBatchTests
  {
    [Run]
    public void Test01_Batch()
    {
      var doc1 = new Doc1{  S1 = "shr", S2 = "none", I1 = 120};
      var got = doc1.Validate(new ValidState(null, ValidErrorMode.Batch));
      got.See();

      Aver.IsTrue(got.IsAssigned);
      Aver.IsTrue(got.HasErrors);
      Aver.IsFalse(got.NoErrors);
      Aver.AreEqual(3, got.ErrorCount);
      Aver.IsTrue(got.ShouldContinue);
      Aver.IsFalse(got.ShouldStop);
      Aver.IsNotNull(got.Error);

      var batch = got.Error as ValidationBatchException;
      Aver.IsNotNull(batch);
      Aver.IsNotNull(batch.Batch);
      Aver.AreEqual(3, batch.Batch.Count);

      Aver.IsTrue(batch.Batch.OfType<FieldValidationException>().Any( e => e.FieldName=="I1" ));
      Aver.IsTrue(batch.Batch.OfType<FieldValidationException>().Any(e => e.FieldName == "S1"));
      Aver.IsTrue(batch.Batch.OfType<FieldValidationException>().Any(e => e.FieldName == "S2"));

      Aver.IsTrue(batch.Batch.Any(e => e.Message.Contains("length of 5")));
      Aver.IsTrue(batch.Batch.Any(e => e.Message.Contains("list of allowed")));
      Aver.IsTrue(batch.Batch.Any(e => e.Message.Contains("max bound")));
    }

    [Run]
    public void Test01_Single()
    {
      var doc1 = new Doc1 { S1 = "shr", S2 = "none", I1 = 120 };
      var got = doc1.Validate(new ValidState(null, ValidErrorMode.Single));
      got.See();

      Aver.IsTrue(got.IsAssigned);
      Aver.IsTrue(got.HasErrors);
      Aver.IsFalse(got.NoErrors);
      Aver.AreEqual(1, got.ErrorCount);
      Aver.IsFalse(got.ShouldContinue);
      Aver.IsTrue(got.ShouldStop);
      Aver.IsNotNull(got.Error);

      var fve = got.Error as FieldValidationException;
      Aver.IsNotNull(fve);
      Aver.IsTrue(fve.Message.Contains("length of 5"));
    }

    public class Doc1 : TypedDoc
    {
      [Field(required: true, minLength: 5)] public string S1{  get; set;}
      [Field(required: true, minLength: 2, valueList: "01:fire;02:police")] public string S2 { get; set; }
      [Field(required: true, min: 10, max: 20)] public int? I1 { get; set; }
    }

  }
}
