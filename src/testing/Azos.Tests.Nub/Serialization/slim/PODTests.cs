/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Scripting;
using Azos.Serialization.POD;
using System.Collections.Concurrent;

namespace Azos.Tests.Nub.Serialization
{

#pragma warning disable 659

  [Runnable]
  public class PODTests
  {
    [Run]
    public void RootPrimitive1_int()
    {
      var data = 5;

      var doc = new PortableObjectDocument(data);

      Aver.AreObjectsEqual(typeof(MetaPrimitiveType), doc.RootMetaType.GetType());
      Aver.AreObjectsEqual(5, doc.Root);
    }

    [Run]
    public void RootPrimitive2_bool()
    {
      var data = true;

      var doc = new PortableObjectDocument(data);

      Aver.AreObjectsEqual(typeof(MetaPrimitiveType), doc.RootMetaType.GetType());
      Aver.AreObjectsEqual(true, doc.Root);
    }

    [Run]
    public void RootPrimitive3_string()
    {
      var data = "test string";

      var doc = new PortableObjectDocument(data);

      Aver.AreObjectsEqual(typeof(MetaPrimitiveType), doc.RootMetaType.GetType());
      Aver.AreObjectsEqual("test string", doc.Root);
    }

    [Run]
    public void RootComposite1()
    {
      var data = new Tuple<int, string>(5, "yes");

      var doc = new PortableObjectDocument(data);

      Aver.AreObjectsEqual(typeof(MetaComplexType), doc.RootMetaType.GetType());
      Aver.AreObjectsEqual(typeof(CompositeReflectedData), doc.Root.GetType());

      var crd = (CompositeReflectedData)doc.Root;

      Aver.AreEqual(2, crd.FieldData.Length);
      Aver.AreObjectsEqual(5, crd.FieldData[0]);
      Aver.AreObjectsEqual("yes", crd.FieldData[1]);
    }

    [Run]
    public void RootPrimitiveWriteRead_int()
    {
      var originalData = 5;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_int_nullable1()
    {
      int? originalData = 5;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_int_nullable2()
    {
      int? originalData = null;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_bool()
    {
      var originalData = true;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_string()
    {
      var originalData = "hello testing";

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_string_null()
    {
      string originalData = null;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_double()
    {
      var originalData = 10 / 3.01d;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_datetime()
    {
      var originalData = DateTime.Now;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootPrimitiveWriteRead_timespan()
    {
      var originalData = TimeSpan.FromDays(12.4321);

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject();

      Aver.AreObjectsEqual(originalData, convertedData);
    }

    [Run]
    public void RootCompositeWriteRead_tuple()
    {
      var originalData = new Tuple<int, string>(5, "yes");

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject() as Tuple<int, string>;

      Aver.IsFalse(object.ReferenceEquals(originalData, convertedData));

      Aver.AreEqual(5, convertedData.Item1);
      Aver.AreEqual("yes", convertedData.Item2);
    }

    [Run]
    public void RootCompositeWriteRead_Person()
    {
      var originalData = new TestPerson { Name = "Kolyan", DOB = DateTime.Now, Assets = 2000000, IsRegistered = true, Luck = 150.89 };

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject() as TestPerson;

      Aver.IsFalse(object.ReferenceEquals(originalData, convertedData));

      Aver.IsTrue(originalData.Equals(convertedData));
    }

    [Run]
    public void RootCompositeWriteRead_Family()
    {
      var originalData =
               new TestFamily
               {
                 Husband = new TestPerson { Name = "Kolyan", DOB = DateTime.Now, Assets = 2000000, IsRegistered = true, Luck = 150.5489 },
                 Wife = new TestPerson { Name = "Feiga", DOB = DateTime.Now, Assets = 578, IsRegistered = false, Luck = 250.489 },
                 Kid = new TestPerson { Name = "Yasha", DOB = DateTime.Now, Assets = 12, IsRegistered = true, Luck = 350.189 },
               };


      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject() as TestFamily;

      Aver.IsFalse(object.ReferenceEquals(originalData, convertedData));

      Aver.IsTrue(originalData.Equals(convertedData));
    }

    [Run]
    public void RootCompositeWriteRead_BusinessFamily()
    {
      var originalData =
               new TestBusinessFamily
               {
                 Husband = new TestPerson { Name = "Kolyan Zver'", DOB = DateTime.Now, Assets = 2000000, IsRegistered = true, Luck = 150.5489 },
                 Wife = new TestPerson { Name = "Feiga Pozman", DOB = DateTime.Now, Assets = 578, IsRegistered = false, Luck = 250.489 },
                 Kid = new TestPerson { Name = "Mykola Zver'", DOB = DateTime.Now, Assets = 12, IsRegistered = true, Luck = 350.189 },
                 Assets = 9000000000,
                 IsCertified = true
               };


      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject() as TestFamily;

      Aver.IsFalse(object.ReferenceEquals(originalData, convertedData));

      Aver.IsTrue(originalData.Equals(convertedData));
    }

    [Run]
    public void RootCompositeWriteRead_PersonList()
    {
      var originalData = new List<TestPerson>
                                        {
                                            new TestPerson{ Name = "Kolyan", DOB = DateTime.Now, Assets=2000000, IsRegistered=true, Luck=150.89},
                                            new TestPerson{ Name = "Zmeyan", DOB = DateTime.Now.AddYears(-25), Assets=50000000, IsRegistered=false, Luck=283.4},

                                        };
      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject() as List<TestPerson>;

      Aver.IsFalse(object.ReferenceEquals(originalData, convertedData));

      Aver.AreEqual(originalData.Count, convertedData.Count);

      Aver.IsTrue(originalData[0].Equals(convertedData[0]));

      Aver.IsTrue(originalData[1].Equals(convertedData[1]));
    }

    [Run]
    public void RootDictionary()
    {
      var originalData = new Dictionary<string, int>
                                {
                                    {"x",10},
                                    {"y",-20}
                                };
      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject() as Dictionary<string, int>;

      Aver.IsFalse(object.ReferenceEquals(originalData, convertedData));

      Aver.AreEqual(originalData.Count, convertedData.Count);
      Aver.AreEqual(10, convertedData["x"]);
      Aver.AreEqual(-20, convertedData["y"]);
    }

    [Run]
    public void RootConcurrentDictionary()
    {
      var originalData = new ConcurrentDictionary<string, int>();

      originalData["x"] = 10;
      originalData["y"] = -20;

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject() as ConcurrentDictionary<string, int>;

      Aver.IsFalse(object.ReferenceEquals(originalData, convertedData));

      Aver.AreEqual(originalData.Count, convertedData.Count);
      Aver.AreEqual(10, convertedData["x"]);
      Aver.AreEqual(-20, convertedData["y"]);
    }

    [Run]
    public void DeserializationTransform1()
    {
      var originalData = new PODTest_Ver1
      {
        Name = "Xerson Person",
        Description = "Some description",
        Age = 25
      };

      var doc = new PortableObjectDocument(originalData);

      var convertedData = doc.ToOriginalObject(new PODTestVersionUpgradeStrategy());

      Aver.IsTrue(convertedData is PODTest_Ver2);

      var ver2 = convertedData as PODTest_Ver2;

      Aver.AreEqual(originalData.Name, ver2.Name);
      Aver.AreEqual(originalData.Description, ver2.Description);
      Aver.AreEqual(originalData.Age, ver2.AgeAsOfToday);
      Aver.AreEqual(DateTime.Now.AddYears(-originalData.Age).Year, ver2.DOB.Year);
    }

  }
}
