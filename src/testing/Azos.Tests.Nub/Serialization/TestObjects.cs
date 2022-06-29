/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Azos.IO;
using Azos.Serialization.POD;

namespace Azos.Tests.Nub.Serialization
{
#pragma warning disable 0649, 0169, 0659

  public struct DataStruct
  {
    public int fInt;
    public int? fNullableInt;
    public string fString;
    public object fObject;
    public object[] fObjectArray1D;
    public object[,] fObjectArray2D;
  }


  public class DataNode
  {
    public string fValue;
    public DataNode fParent;
    public DataNode[] fChildren;

    public DataNode fLeft;
    public DataNode fRight;

    public override bool Equals(object obj)
    {
      var other = obj as DataNode;
      if (other == null) return false;

      if (fValue != other.fValue) return false;

      if (fChildren != null && other.fChildren != null)
        return fChildren.SequenceEqual(other.fChildren);

      return true;
    }
  }


  public class DataObject : IEquatable<DataObject>
  {
    public string fString;

    public int fInt;
    public uint fUInt;
    public int? fIntNullable;
    public uint? fUIntNullable;

    public long fLong;
    public ulong fULong;
    public long? fLongNullable;
    public ulong? fULongNUllable;

    public short fShort;
    public ushort fUShort;
    public short? fShortNullable;
    public ushort? fUShortNullable;

    public byte fByte;
    public sbyte fSByte;
    public byte? fByteNullable;
    public sbyte? fSByteNullable;

    public float fFloat;
    public double fDouble;
    public decimal fDecimal;
    public float? fFloatNullable;
    public double? fDoubleNullable;
    public decimal? fDecimalNullable;

    public TimeSpan fTimeSpan;
    public DateTime fDateTime;
    public TimeSpan? fTimeSpanNullable;
    public DateTime? fDateTimeNullable;

    public bool fBool;
    public char fChar;
    public bool? fBoolNullable;
    public char? fCharNullable;

    public byte[] fByteArray;
    public char[] fCharArray;
    public string[] fStringArray;

    public MetaHandle fMetaHandle;

    public override bool Equals(object obj)
    {
      return Equals(obj as DataObject);
    }

    public virtual bool Equals(DataObject other)
    {
      if (other == null) return false;
      return
        (fString == other.fString) &&
        (fInt == other.fInt) &&
        (fUInt == other.fUInt) &&
        (fIntNullable == other.fIntNullable) &&
        (fUIntNullable == other.fUIntNullable) &&

        (fLong == other.fLong) &&
        (fULong == other.fULong) &&
        (fLongNullable == other.fLongNullable) &&
        (fULongNUllable == other.fULongNUllable) &&

        (fShort == other.fShort) &&
        (fUShort == other.fUShort) &&
        (fShortNullable == other.fShortNullable) &&
        (fUShortNullable == other.fUShortNullable) &&

        (fByte == other.fByte) &&
        (fSByte == other.fSByte) &&
        (fByteNullable == other.fByteNullable) &&
        (fSByteNullable == other.fSByteNullable) &&

        (fFloat == other.fFloat) &&
        (fDouble == other.fDouble) &&
        (fDecimal == other.fDecimal) &&
        (fFloatNullable == other.fFloatNullable) &&
        (fDoubleNullable == other.fDoubleNullable) &&
        (fDecimalNullable == other.fDecimalNullable) &&

        (fTimeSpan == other.fTimeSpan) &&
        (fDateTime == other.fDateTime) &&
        (fTimeSpanNullable == other.fTimeSpanNullable) &&
        (fDateTimeNullable == other.fDateTimeNullable) &&

        (fBool == other.fBool) &&
        (fChar == other.fChar) &&
        (fBoolNullable == other.fBoolNullable) &&
        (fCharNullable == other.fCharNullable) &&

        (fByteArray.SequenceEqual(other.fByteArray)) &&
        (fCharArray.SequenceEqual(other.fCharArray)) &&
        (fStringArray.SequenceEqual(other.fStringArray)) &&

        (fMetaHandle.Equals(other.fMetaHandle));
    }
  }


  public class DataObjectWithList : DataObject
  {
    public List<DataObject> OtherObjects = new List<DataObject>();

    public override bool Equals(DataObject other)
    {
      var ao = other as DataObjectWithList;
      if (ao == null) return false;
      if (!base.Equals(ao)) return false;

      return OtherObjects.SequenceEqual(ao.OtherObjects);
    }
  }


  public static class Utils
  {
    public static DataObject Populate(this DataObject obj)
    {
      obj.fString = "some string";

      obj.fInt = 892;
      obj.fUInt = 3000000000;
      obj.fIntNullable = -18909;
      obj.fUIntNullable = 3000000002;

      obj.fLong = 12;
      obj.fULong = 8000000000;
      obj.fLongNullable = -5000000000;
      obj.fULongNUllable = 8000000002;

      obj.fShort = -32767;
      obj.fUShort = 65535;
      obj.fShortNullable = -32767;
      obj.fUShortNullable = 65535;

      obj.fByte = 255;
      obj.fSByte = -127;
      obj.fByteNullable = 255;
      obj.fSByteNullable = -127;

      obj.fFloat = 12.283f;
      obj.fDouble = 0.0232123;
      obj.fDecimal = 150000.2234M;
      obj.fFloatNullable = 23.1f;
      obj.fDoubleNullable = -232132.123211001;
      obj.fDecimalNullable = 290000.0000M;

      obj.fTimeSpan = TimeSpan.FromHours(23.5);
      obj.fDateTime = new DateTime(2011, 10, 12);
      obj.fTimeSpanNullable = TimeSpan.FromHours(23.5);
      obj.fDateTimeNullable = new DateTime(2099, 10, 12);

      obj.fBool = true;
      obj.fChar = 'Z';
      obj.fBoolNullable = true;
      obj.fCharNullable = 'a';

      obj.fByteArray = new byte[4] { 12, 56, 54, 76 };
      obj.fCharArray = new char[5] { 'a', 'u', 'm', ' ', '!' };
      obj.fStringArray = new string[3] { "slava", "kpss", "ura!" };

      obj.fMetaHandle = new MetaHandle(12, new VarIntStr("my test handle"));

      return obj;
    }
  }


  internal class ClassWithAutoPropFields
  {
    public string Name { get; set; }
    public int Age { get; set; }
  }


  public class APerson
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }

    public DateTime DOB { get; set; }
  }


  internal enum PersonRespect { Normal, Low, High, Guru }


  internal class TestPerson
  {
    public string Name;
    public DateTime DOB;
    public decimal Assets;
    public double Luck;
    public bool IsRegistered;
    public PersonRespect Respect;
    public override bool Equals(object obj)
    {
      var other = obj as TestPerson;
      if (other == null) return false;
      return Name == other.Name &&
      DOB == other.DOB &&
      Assets == other.Assets &&
      Luck == other.Luck &&
      IsRegistered == other.IsRegistered &&
      Respect == other.Respect;
    }
  }


  internal class TestFamily
  {
    public TestPerson Husband;
    public TestPerson Wife;
    public TestPerson Kid;

    public override bool Equals(object obj)
    {
      var other = obj as TestFamily;
      if (other == null) return false;
      return Husband.Equals(other.Husband) &&
             Wife.Equals(other.Wife) &&
             Kid.Equals(other.Kid);

    }
  }


  internal class TestBusinessFamily : TestFamily
  {
    public decimal Assets;
    public bool IsCertified;

    public override bool Equals(object obj)
    {
      var other = obj as TestBusinessFamily;
      if (other == null) return false;
      return base.Equals(obj) && Assets == other.Assets && IsCertified == other.IsCertified;
    }
  }


  internal class PODTestBase
  {
    public string Description;
  }


  internal class PODTest_Ver1 : PODTestBase
  {
    public string Name;
    public int Age;
  }


  [PODTestVer2Transform]
  internal class PODTest_Ver2 : PODTestBase
  {
    public string Name;
    public int AgeAsOfToday;
    public DateTime DOB;
  }


  internal class PODTestVersionUpgradeStrategy : ReadingStrategy
  {
    public override Type ResolveType(MetaType metaType)
    {
      if (metaType.Name == "PODTest_Ver1")
        return typeof(PODTest_Ver2);

      return base.ResolveType(metaType);
    }
  }


  internal class PODTestVer2Transform : PortableObjectDocumentDeserializationTransform
  {

    public override object ConstructObjectInstance(CompositeData data)
    {
      return new PODTest_Ver2();
    }

    public override bool DeserializeFromCompositeCustomData(object instance, CompositeCustomData data)
    {
      return false;
    }

    public override HashSet<MetaComplexType.MetaField> DeserializeFromCompositeReflectedData(object instance, CompositeReflectedData data)
    {
      return null;
    }

    public override bool SetFieldValue(ReadingStrategy readingStrategy,
                                       object instance,
                                       FieldInfo fieldInfo,
                                       CompositeReflectedData data,
                                       MetaComplexType.MetaField mfield)
    {
      if (fieldInfo.Name == "AgeAsOfToday")
      {
        var nativeData = (int)data.Document.PortableDataToNativeData(readingStrategy, data.FieldData[mfield.Index]);
        ((PODTest_Ver2)instance).DOB = DateTime.Now.AddYears(-nativeData);
      }
      return false;
    }

    public override FieldInfo ResolveField(Type nativeType, MetaComplexType.MetaField mfield)
    {
      if (mfield.FieldName == "Age") return nativeType.GetField("AgeAsOfToday");
      return null;
    }
  }
}
