using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.Slim;

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


    [Run]
    public void Test02_Batch()
    {
      var doc1 = new Doc1 { S1 = "shr", S2 = "01", I1 = 15 };
      var got = doc1.Validate(new ValidState(null, ValidErrorMode.Batch));
      got.See();

      Aver.IsTrue(got.IsAssigned);
      Aver.IsTrue(got.HasErrors);
      Aver.IsFalse(got.NoErrors);
      Aver.AreEqual(1, got.ErrorCount);
      Aver.IsTrue(got.ShouldContinue);
      Aver.IsFalse(got.ShouldStop);
      Aver.IsNotNull(got.Error);

      var fve = got.Error as FieldValidationException;
      Aver.IsNotNull(fve);
      Aver.IsTrue(fve.Message.Contains("length of 5"));
    }


    [Run]
    public void Test03_Batch()
    {
      var doc = new Doc2 { Name = "Dogman", List = new List<Doc1>{
        new Doc1 { S1 = "1", S2 = "01", I1 = 15 },
        new Doc1 { S1 = "2", S2 = "sf01", I1 = 1555 },
        new Doc1 { S1 = "323456", S2 = "gdfg01", I1 = -1 }
      }};

      var got = doc.Validate(new ValidState(null, ValidErrorMode.Batch));
      got.See();

      Aver.IsTrue(got.IsAssigned);
      Aver.IsTrue(got.HasErrors);
      Aver.IsFalse(got.NoErrors);
      Aver.AreEqual(7, got.ErrorCount);
      Aver.IsTrue(got.ShouldContinue);
      Aver.IsFalse(got.ShouldStop);
      Aver.IsNotNull(got.Error);

      var batch = got.Error as ValidationBatchException;
      Aver.IsNotNull(batch);
      Aver.IsNotNull(batch.Batch);
      Aver.AreEqual(7, batch.Batch.Count);
    }


    [Run]
    public void ValidationBatchException_Concat_01()
    {
      Aver.IsNull(ValidationBatchException.Concatenate(null, null));
    }

    [Run]
    public void ValidationBatchException_Concat_02()
    {
      var got = ValidationBatchException.Concatenate(new ArgumentException("error1"), null);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ArgumentException);
    }

    [Run]
    public void ValidationBatchException_Concat_03()
    {
      var got = ValidationBatchException.Concatenate(null, new ArgumentException("error1"));
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ArgumentException);
    }

    [Run]
    public void ValidationBatchException_Concat_04()
    {
      var got = ValidationBatchException.Concatenate(new AzosException("error1"), new ArgumentException("error2"));
      Aver.IsNotNull(got);
      var be = got as ValidationBatchException;
      Aver.IsNotNull(be);

      Aver.IsNotNull(be.Batch);
      Aver.AreEqual(2, be.Batch.Count);
      Aver.IsTrue(be.Batch[0] is AzosException);
      Aver.IsTrue(be.Batch[1] is ArgumentException);
    }

    [Run]
    public void ValidationBatchException_Concat_05()
    {
      var error1 = new AzosException("error1");
      var error2 = new ArgumentException("error2");

      var got = ValidationBatchException.Concatenate(error1, error2);
      Aver.IsNotNull(got);
      var be = got as ValidationBatchException;
      Aver.IsNotNull(be);

      Aver.IsNotNull(be.Batch);
      Aver.AreEqual(2, be.Batch.Count);
      Aver.AreSameRef(error1, be.Batch[0]);
      Aver.AreSameRef(error2, be.Batch[1]);

      var got2 = ValidationBatchException.Concatenate(error2, got);
      var be2 = got2 as ValidationBatchException;
      Aver.IsNotNull(be2);

      Aver.IsNotNull(be2.Batch);
      Aver.AreEqual(2, be2.Batch.Count);
      Aver.AreSameRef(error2, be2.Batch[0]);
      Aver.AreSameRef(got, be2.Batch[1]);
    }

    [Run]
    public void ValidationBatchException_Concat_06()
    {
      var error1 = new AzosException("error1");
      var error2 = new ArgumentException("error2");

      var got = ValidationBatchException.Concatenate(error1, error2);
      Aver.IsNotNull(got);
      var be = got as ValidationBatchException;
      Aver.IsNotNull(be);

      Aver.IsNotNull(be.Batch);
      Aver.AreEqual(2, be.Batch.Count);
      Aver.AreSameRef(error1, be.Batch[0]);
      Aver.AreSameRef(error2, be.Batch[1]);

      var got2 = ValidationBatchException.Concatenate(got, error2);
      var be2 = got2 as ValidationBatchException;
      Aver.IsNotNull(be2);

      Aver.IsNotNull(be2.Batch);
      Aver.AreEqual(3, be2.Batch.Count);
      Aver.AreSameRef(error1, be2.Batch[0]);
      Aver.AreSameRef(error2, be2.Batch[1]);
      Aver.AreSameRef(error2, be2.Batch[2]);
    }

    [Run]
    public void ValidationBatchException_Serialization()
    {
      var error1 = new AzosException("error1");
      var error2 = new ArgumentException("error2");

      var got = ValidationBatchException.Concatenate(error1, error2);
      Aver.IsNotNull(got);
      var ser = new SlimSerializer();
      var ms = new MemoryStream();
      ser.Serialize(ms, got);
      ms.Position=0;

      var deser = ser.Deserialize(ms) as ValidationBatchException;
      Aver.IsNotNull(deser);
      Aver.AreNotSameRef(got, deser);
      Aver.IsNotNull(deser.Batch);
      Aver.AreEqual(2, deser.Batch.Count);

      Aver.IsTrue(deser.Batch[0] is AzosException);
      Aver.IsTrue(deser.Batch[1] is ArgumentException);

      Aver.AreEqual("error1", deser.Batch[0].Message);
      Aver.AreEqual("error2", deser.Batch[1].Message);
    }


    public class Doc1 : TypedDoc
    {
      [Field(required: true, minLength: 5)] public string S1{  get; set;}
      [Field(required: true, minLength: 2, valueList: "01:fire;02:police")] public string S2 { get; set; }
      [Field(required: true, min: 10, max: 20)] public int? I1 { get; set; }
    }

    public class Doc2 : TypedDoc
    {
      [Field(required: true, minLength: 5)] public string Name { get; set; }

      [Field(required: true, minLength: 4)]
      public List<Doc1> List { get; set; }
    }

  }
}
