/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Scripting;
using Azos.Data;
using Azos.Serialization.JSON;


namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JSONtoDocTests
  {
    #region Arrays

    [Run]
    public void ToTypedRow_Arrays_FromString_Strings()
    {
      var str = @"{name: ""Orlov"", StringArray: [""a"", null, ""b""]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual("Orlov", row.Name);

      Aver.IsNotNull(row.StringArray);
      Aver.AreEqual(3, row.StringArray.Length);

      Aver.AreEqual("a", row.StringArray[0]);
      Aver.AreEqual(null, row.StringArray[1]);
      Aver.AreEqual("b", row.StringArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Bytes()
    {
      var str = @"{UInt8: 255, UInt8Array: [1, 0, 255], UInt8NArray: [null, 0, 124]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(255, row.UInt8);

      Aver.IsNotNull(row.UInt8Array);
      Aver.AreEqual(3, row.UInt8Array.Length);
      Aver.AreEqual(1, row.UInt8Array[0]);
      Aver.AreEqual(0, row.UInt8Array[1]);
      Aver.AreEqual(255, row.UInt8Array[2]);

      Aver.IsNotNull(row.UInt8NArray);
      Aver.AreEqual(3, row.UInt8NArray.Length);
      Aver.AreEqual(null, row.UInt8NArray[0]);
      Aver.AreEqual(0, row.UInt8NArray[1]);
      Aver.AreEqual(124, row.UInt8NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_SBytes()
    {
      var str = @"{Int8: -56, Int8Array: [-1, 0, 127], Int8NArray: [null, 0, 127]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(-56, row.Int8);

      Aver.IsNotNull(row.Int8Array);
      Aver.AreEqual(3, row.Int8Array.Length);
      Aver.AreEqual(-1, row.Int8Array[0]);
      Aver.AreEqual(0, row.Int8Array[1]);
      Aver.AreEqual(127, row.Int8Array[2]);

      Aver.IsNotNull(row.Int8NArray);
      Aver.AreEqual(3, row.Int8NArray.Length);
      Aver.AreEqual(null, row.Int8NArray[0]);
      Aver.AreEqual(0, row.Int8NArray[1]);
      Aver.AreEqual(127, row.Int8NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Shorts()
    {
      var str = @"{Int16: 12345, Int16Array: [32767, 0, -32768], Int16NArray: [null, 0, 32767]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(12345, row.Int16);

      Aver.IsNotNull(row.Int16Array);
      Aver.AreEqual(3, row.Int16Array.Length);
      Aver.AreEqual(short.MaxValue, row.Int16Array[0]);
      Aver.AreEqual(0, row.Int16Array[1]);
      Aver.AreEqual(short.MinValue, row.Int16Array[2]);

      Aver.IsNotNull(row.Int16NArray);
      Aver.AreEqual(3, row.Int16NArray.Length);
      Aver.AreEqual(null, row.Int16NArray[0]);
      Aver.AreEqual(0, row.Int16NArray[1]);
      Aver.AreEqual(short.MaxValue, row.Int16NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_UShorts()
    {
      var str = @"{UInt16: 123, UInt16Array: [65535, 0, 12345], UInt16NArray: [null, 0, 65535]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123, row.UInt16);

      Aver.IsNotNull(row.UInt16Array);
      Aver.AreEqual(3, row.UInt16Array.Length);
      Aver.AreEqual(ushort.MaxValue, row.UInt16Array[0]);
      Aver.AreEqual(0, row.UInt16Array[1]);
      Aver.AreEqual(12345, row.UInt16Array[2]);

      Aver.IsNotNull(row.UInt16NArray);
      Aver.AreEqual(3, row.UInt16NArray.Length);
      Aver.AreEqual(null, row.UInt16NArray[0]);
      Aver.AreEqual(0, row.UInt16NArray[1]);
      Aver.AreEqual(ushort.MaxValue, row.UInt16NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Int()
    {
      var str = @"{Int32: 123, Int32Array: [2147483647, 0, -2147483648], Int32NArray: [null, 0, 2147483647]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123, row.Int32);

      Aver.IsNotNull(row.Int32Array);
      Aver.AreEqual(3, row.Int32Array.Length);
      Aver.AreEqual(int.MaxValue, row.Int32Array[0]);
      Aver.AreEqual(0, row.Int32Array[1]);
      Aver.AreEqual(int.MinValue, row.Int32Array[2]);

      Aver.IsNotNull(row.Int32NArray);
      Aver.AreEqual(3, row.Int32NArray.Length);
      Aver.AreEqual(null, row.Int32NArray[0]);
      Aver.AreEqual(0, row.Int32NArray[1]);
      Aver.AreEqual(int.MaxValue, row.Int32NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_UInt()
    {
      var str = @"{UInt32: 123, UInt32Array: [4294967295, 0, 124], UInt32NArray: [null, 0, 4294967295]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.IsTrue(123 == row.UInt32);

      Aver.IsNotNull(row.UInt32Array);
      Aver.AreEqual(3, row.UInt32Array.Length);
      Aver.AreEqual(uint.MaxValue, row.UInt32Array[0]);
      Aver.IsTrue(0 == row.UInt32Array[1]);
      Aver.IsTrue(124 == row.UInt32Array[2]);

      Aver.IsNotNull(row.UInt32NArray);
      Aver.AreEqual(3, row.UInt32NArray.Length);
      Aver.AreEqual(null, row.UInt32NArray[0]);
      Aver.AreEqual(0, row.UInt32NArray[1]);
      Aver.AreEqual(uint.MaxValue, row.UInt32NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Long()
    {
      var str = @"{Int64: 123, Int64Array: [9223372036854775807, 0, -9223372036854775808], Int64NArray: [null, 0, 9223372036854775807]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123, row.Int64);

      Aver.IsNotNull(row.Int64Array);
      Aver.AreEqual(3, row.Int64Array.Length);
      Aver.AreEqual(long.MaxValue, row.Int64Array[0]);
      Aver.AreEqual(0, row.Int64Array[1]);
      Aver.AreEqual(long.MinValue, row.Int64Array[2]);

      Aver.IsNotNull(row.Int64NArray);
      Aver.AreEqual(3, row.Int64NArray.Length);
      Aver.AreEqual(null, row.Int64NArray[0]);
      Aver.AreEqual(0, row.Int64NArray[1]);
      Aver.AreEqual(long.MaxValue, row.Int64NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_ULong()
    {
      var str = @"{UInt64: 123, UInt64Array: [18446744073709551615, 0, 124], UInt64NArray: [null, 0, 18446744073709551615]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.IsTrue(123 == row.UInt64);

      Aver.IsNotNull(row.UInt64Array);
      Aver.AreEqual(3, row.UInt64Array.Length);
      Aver.AreEqual(ulong.MaxValue, row.UInt64Array[0]);
      Aver.IsTrue(0 == row.UInt64Array[1]);
      Aver.IsTrue(124 == row.UInt64Array[2]);

      Aver.IsNotNull(row.UInt64NArray);
      Aver.AreEqual(3, row.UInt64NArray.Length);
      Aver.AreEqual(null, row.UInt64NArray[0]);
      Aver.AreEqual(0, row.UInt64NArray[1]);
      Aver.AreEqual(ulong.MaxValue, row.UInt64NArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Single()
    {
      var str = @"{Single: 123.456, SingleArray: [3.4028E+38, 0, -3.402E+38], SingleNArray: [null, 0, 3.4028]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123.456F, row.Single);

      Aver.IsNotNull(row.SingleArray);
      Aver.AreEqual(3, row.SingleArray.Length);
      Aver.AreEqual(3.4028E+38F, row.SingleArray[0]);
      Aver.AreEqual(0, row.SingleArray[1]);
      Aver.AreEqual(-3.402E+38F, row.SingleArray[2]);

      Aver.IsNotNull(row.SingleNArray);
      Aver.AreEqual(3, row.SingleNArray.Length);
      Aver.AreEqual(null, row.SingleNArray[0]);
      Aver.AreEqual(0, row.SingleNArray[1]);
      Aver.AreEqual(3.4028F, row.SingleNArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Double()
    {
      var str = @"{Double: 123.456, DoubleArray: [1.79769E+308, 0, -1.7976931E+308], DoubleNArray: [null, 0, 3.482347E+38]}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123.456D, row.Double);

      Aver.IsNotNull(row.DoubleArray);
      Aver.AreEqual(3, row.DoubleArray.Length);
      Aver.AreEqual(1.79769E+308, row.DoubleArray[0]);
      Aver.AreEqual(0, row.DoubleArray[1]);
      Aver.AreEqual(-1.7976931E+308, row.DoubleArray[2]);

      Aver.IsNotNull(row.DoubleNArray);
      Aver.AreEqual(3, row.DoubleNArray.Length);
      Aver.AreEqual(null, row.DoubleNArray[0]);
      Aver.AreEqual(0, row.DoubleNArray[1]);
      Aver.AreEqual(3.482347E+38, row.DoubleNArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Guid()
    {
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      var guid3 = Guid.NewGuid();
      var str = "{" + @"Guid: ""{0}"", GuidArray: [""{0}"", ""{1}"", ""{2}""], GuidNArray: [""{0}"", null, ""{2}""]".Args(guid1, guid2, guid3) + "}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(guid1, row.Guid);

      Aver.IsNotNull(row.GuidArray);
      Aver.AreEqual(3, row.GuidArray.Length);
      Aver.AreEqual(guid1, row.GuidArray[0]);
      Aver.AreEqual(guid2, row.GuidArray[1]);
      Aver.AreEqual(guid3, row.GuidArray[2]);

      Aver.IsNotNull(row.GuidNArray);
      Aver.AreEqual(3, row.GuidNArray.Length);
      Aver.AreEqual(guid1, row.GuidNArray[0]);
      Aver.AreEqual(null, row.GuidNArray[1]);
      Aver.AreEqual(guid3, row.GuidNArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_GDID()
    {
      var gdid1 = new GDID(4000000000, 8000000000);
      var gdid2 = new GDID(100002, 1, 8000000000);
      var gdid3 = new GDID(123, 123456789);
      var str = @"{" + @"GDID: ""{0}"", GDIDArray: [""{0}"", ""{1}"", ""{2}""], GDIDNArray: [""{0}"", null, ""{2}""]".Args(gdid1, gdid2, gdid3) + "}";

      var row = new RowWithArrays();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(gdid1, row.GDID);

      Aver.IsNotNull(row.GDIDArray);
      Aver.AreEqual(3, row.GDIDArray.Length);
      Aver.AreEqual(gdid1, row.GDIDArray[0]);
      Aver.AreEqual(gdid2, row.GDIDArray[1]);
      Aver.AreEqual(gdid3, row.GDIDArray[2]);

      Aver.IsNotNull(row.GDIDNArray);
      Aver.AreEqual(3, row.GDIDNArray.Length);
      Aver.AreEqual(gdid1, row.GDIDNArray[0]);
      Aver.AreEqual(null, row.GDIDNArray[1]);
      Aver.AreEqual(gdid3, row.GDIDNArray[2]);
    }

    [Run]
    public void ToTypedRow_Arrays_FromString_Row()
    {
      var str =
      @"{
            Row: { Name: ""Ivan"", Int32Array: [1, 0, -12345] },
            RowArray:
                [
                    { Name: ""John"", Int8: 123 },
                    { Name: ""Anna"", Single: 123.567 }
                ]
        }";

      var row = new RowWithArrays();
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.IsNotNull(row.Row);
      Aver.AreEqual("Ivan", row.Row.Name);
      Aver.IsNotNull(row.Row.Int32Array);
      Aver.AreEqual(3, row.Row.Int32Array.Length);
      Aver.AreEqual(1, row.Row.Int32Array[0]);
      Aver.AreEqual(0, row.Row.Int32Array[1]);
      Aver.AreEqual(-12345, row.Row.Int32Array[2]);

      Aver.IsNotNull(row.RowArray);
      Aver.AreEqual(2, row.RowArray.Length);
      Aver.IsNotNull(row.RowArray[0]);
      Aver.AreEqual("John", row.RowArray[0].Name);
      Aver.AreEqual(123, row.RowArray[0].Int8);
      Aver.IsNotNull(row.RowArray[1]);
      Aver.AreEqual("Anna", row.RowArray[1].Name);
      Aver.AreEqual(123.567F, row.RowArray[1].Single);
    }

    #endregion Arrays

    #region Lists

    [Run]
    public void ToTypedRow_Lists_FromString_Strings()
    {
      var str = @"{name: ""Orlov"", StringList: [""a"", null, ""b""]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual("Orlov", row.Name);

      Aver.IsNotNull(row.StringList);
      Aver.AreEqual(3, row.StringList.Count);

      Aver.AreEqual("a", row.StringList[0]);
      Aver.AreEqual(null, row.StringList[1]);
      Aver.AreEqual("b", row.StringList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Bytes()
    {
      var str = @"{UInt8: 255, UInt8List: [1, 0, 255], UInt8NList: [null, 0, 124]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(255, row.UInt8);

      Aver.IsNotNull(row.UInt8List);
      Aver.AreEqual(3, row.UInt8List.Count);
      Aver.AreEqual(1, row.UInt8List[0]);
      Aver.AreEqual(0, row.UInt8List[1]);
      Aver.AreEqual(255, row.UInt8List[2]);

      Aver.IsNotNull(row.UInt8NList);
      Aver.AreEqual(3, row.UInt8NList.Count);
      Aver.AreEqual(null, row.UInt8NList[0]);
      Aver.AreEqual(0, row.UInt8NList[1]);
      Aver.AreEqual(124, row.UInt8NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_SBytes()
    {
      var str = @"{Int8: -56, Int8List: [-1, 0, 127], Int8NList: [null, 0, 127]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(-56, row.Int8);

      Aver.IsNotNull(row.Int8List);
      Aver.AreEqual(3, row.Int8List.Count);
      Aver.AreEqual(-1, row.Int8List[0]);
      Aver.AreEqual(0, row.Int8List[1]);
      Aver.AreEqual(127, row.Int8List[2]);

      Aver.IsNotNull(row.Int8NList);
      Aver.AreEqual(3, row.Int8NList.Count);
      Aver.AreEqual(null, row.Int8NList[0]);
      Aver.AreEqual(0, row.Int8NList[1]);
      Aver.AreEqual(127, row.Int8NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Shorts()
    {
      var str = @"{Int16: 12345, Int16List: [32767, 0, -32768], Int16NList: [null, 0, 32767]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(12345, row.Int16);

      Aver.IsNotNull(row.Int16List);
      Aver.AreEqual(3, row.Int16List.Count);
      Aver.AreEqual(short.MaxValue, row.Int16List[0]);
      Aver.AreEqual(0, row.Int16List[1]);
      Aver.AreEqual(short.MinValue, row.Int16List[2]);

      Aver.IsNotNull(row.Int16NList);
      Aver.AreEqual(3, row.Int16NList.Count);
      Aver.AreEqual(null, row.Int16NList[0]);
      Aver.AreEqual(0, row.Int16NList[1]);
      Aver.AreEqual(short.MaxValue, row.Int16NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_UShorts()
    {
      var str = @"{UInt16: 123, UInt16List: [65535, 0, 12345], UInt16NList: [null, 0, 65535]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123, row.UInt16);

      Aver.IsNotNull(row.UInt16List);
      Aver.AreEqual(3, row.UInt16List.Count);
      Aver.AreEqual(ushort.MaxValue, row.UInt16List[0]);
      Aver.AreEqual(0, row.UInt16List[1]);
      Aver.AreEqual(12345, row.UInt16List[2]);

      Aver.IsNotNull(row.UInt16NList);
      Aver.AreEqual(3, row.UInt16NList.Count);
      Aver.AreEqual(null, row.UInt16NList[0]);
      Aver.AreEqual(0, row.UInt16NList[1]);
      Aver.AreEqual(ushort.MaxValue, row.UInt16NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Int()
    {
      var str = @"{Int32: 123, Int32List: [2147483647, 0, -2147483648], Int32NList: [null, 0, 2147483647]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123, row.Int32);

      Aver.IsNotNull(row.Int32List);
      Aver.AreEqual(3, row.Int32List.Count);
      Aver.AreEqual(int.MaxValue, row.Int32List[0]);
      Aver.AreEqual(0, row.Int32List[1]);
      Aver.AreEqual(int.MinValue, row.Int32List[2]);

      Aver.IsNotNull(row.Int32NList);
      Aver.AreEqual(3, row.Int32NList.Count);
      Aver.AreEqual(null, row.Int32NList[0]);
      Aver.AreEqual(0, row.Int32NList[1]);
      Aver.AreEqual(int.MaxValue, row.Int32NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_UInt()
    {
      var str = @"{UInt32: 123, UInt32List: [4294967295, 0, 124], UInt32NList: [null, 0, 4294967295]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.IsTrue(123 == row.UInt32);

      Aver.IsNotNull(row.UInt32List);
      Aver.AreEqual(3, row.UInt32List.Count);
      Aver.AreEqual(uint.MaxValue, row.UInt32List[0]);
      Aver.IsTrue(0 == row.UInt32List[1]);
      Aver.IsTrue(124 == row.UInt32List[2]);

      Aver.IsNotNull(row.UInt32NList);
      Aver.AreEqual(3, row.UInt32NList.Count);
      Aver.AreEqual(null, row.UInt32NList[0]);
      Aver.AreEqual(0, row.UInt32NList[1]);
      Aver.AreEqual(uint.MaxValue, row.UInt32NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Long()
    {
      var str = @"{Int64: 123, Int64List: [9223372036854775807, 0, -9223372036854775808], Int64NList: [null, 0, 9223372036854775807]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123, row.Int64);

      Aver.IsNotNull(row.Int64List);
      Aver.AreEqual(3, row.Int64List.Count);
      Aver.AreEqual(long.MaxValue, row.Int64List[0]);
      Aver.AreEqual(0, row.Int64List[1]);
      Aver.AreEqual(long.MinValue, row.Int64List[2]);

      Aver.IsNotNull(row.Int64NList);
      Aver.AreEqual(3, row.Int64NList.Count);
      Aver.AreEqual(null, row.Int64NList[0]);
      Aver.AreEqual(0, row.Int64NList[1]);
      Aver.AreEqual(long.MaxValue, row.Int64NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_ULong()
    {
      var str = @"{UInt64: 123, UInt64List: [18446744073709551615, 0, 124], UInt64NList: [null, 0, 18446744073709551615]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.IsTrue(123 == row.UInt64);

      Aver.IsNotNull(row.UInt64List);
      Aver.AreEqual(3, row.UInt64List.Count);
      Aver.AreEqual(ulong.MaxValue, row.UInt64List[0]);
      Aver.IsTrue(0 == row.UInt64List[1]);
      Aver.IsTrue(124 == row.UInt64List[2]);

      Aver.IsNotNull(row.UInt64NList);
      Aver.AreEqual(3, row.UInt64NList.Count);
      Aver.AreEqual(null, row.UInt64NList[0]);
      Aver.AreEqual(0, row.UInt64NList[1]);
      Aver.AreEqual(ulong.MaxValue, row.UInt64NList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Single()
    {
      var str = @"{Single: 123.456, SingleList: [3.4028E+38, 0, -3.402E+38], SingleNList: [null, 0, 3.4028]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123.456F, row.Single);

      Aver.IsNotNull(row.SingleList);
      Aver.AreEqual(3, row.SingleList.Count);
      Aver.AreEqual(3.4028E+38F, row.SingleList[0]);
      Aver.AreEqual(0, row.SingleList[1]);
      Aver.AreEqual(-3.402E+38F, row.SingleList[2]);

      Aver.IsNotNull(row.SingleNList);
      Aver.AreEqual(3, row.SingleNList.Count);
      Aver.AreEqual(null, row.SingleNList[0]);
      Aver.AreEqual(0, row.SingleNList[1]);
      Aver.AreEqual(3.4028F, row.SingleNList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Double()
    {
      var str = @"{Double: 123.456, DoubleList: [1.79769E+308, 0, -1.7976931E+308], DoubleNList: [null, 0, 3.482347E+38]}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123.456D, row.Double);

      Aver.IsNotNull(row.DoubleList);
      Aver.AreEqual(3, row.DoubleList.Count);
      Aver.AreEqual(1.79769E+308, row.DoubleList[0]);
      Aver.AreEqual(0, row.DoubleList[1]);
      Aver.AreEqual(-1.7976931E+308, row.DoubleList[2]);

      Aver.IsNotNull(row.DoubleNList);
      Aver.AreEqual(3, row.DoubleNList.Count);
      Aver.AreEqual(null, row.DoubleNList[0]);
      Aver.AreEqual(0, row.DoubleNList[1]);
      Aver.AreEqual(3.482347E+38, row.DoubleNList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Guid()
    {
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      var guid3 = Guid.NewGuid();
      var str = "{" + @"Guid: ""{0}"", GuidList: [""{0}"", ""{1}"", ""{2}""], GuidNList: [""{0}"", null, ""{2}""]".Args(guid1, guid2, guid3) + "}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(guid1, row.Guid);

      Aver.IsNotNull(row.GuidList);
      Aver.AreEqual(3, row.GuidList.Count);
      Aver.AreEqual(guid1, row.GuidList[0]);
      Aver.AreEqual(guid2, row.GuidList[1]);
      Aver.AreEqual(guid3, row.GuidList[2]);

      Aver.IsNotNull(row.GuidNList);
      Aver.AreEqual(3, row.GuidNList.Count);
      Aver.AreEqual(guid1, row.GuidNList[0]);
      Aver.AreEqual(null, row.GuidNList[1]);
      Aver.AreEqual(guid3, row.GuidNList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_GDID()
    {
      var gdid1 = new GDID(4000000000, 8000000000);
      var gdid2 = new GDID(100002, 1, 8000000000);
      var gdid3 = new GDID(123, 123456789);
      var str = @"{" + @"GDID: ""{0}"", GDIDList: [""{0}"", ""{1}"", ""{2}""], GDIDNList: [""{0}"", null, ""{2}""]".Args(gdid1, gdid2, gdid3) + "}";

      var row = new RowWithLists();

      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(gdid1, row.GDID);

      Aver.IsNotNull(row.GDIDList);
      Aver.AreEqual(3, row.GDIDList.Count);
      Aver.AreEqual(gdid1, row.GDIDList[0]);
      Aver.AreEqual(gdid2, row.GDIDList[1]);
      Aver.AreEqual(gdid3, row.GDIDList[2]);

      Aver.IsNotNull(row.GDIDNList);
      Aver.AreEqual(3, row.GDIDNList.Count);
      Aver.AreEqual(gdid1, row.GDIDNList[0]);
      Aver.AreEqual(null, row.GDIDNList[1]);
      Aver.AreEqual(gdid3, row.GDIDNList[2]);
    }

    [Run]
    public void ToTypedRow_Lists_FromString_Row()
    {
      var str =
      @"{
            Row: { Name: ""Ivan"", Int32List: [1, 0, -12345] },
            RowList:
                [
                    { Name: ""John"", Int8: 123 },
                    { Name: ""Anna"", Single: 123.567 }
                ]
        }";

      var row = new RowWithLists();
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.IsNotNull(row.Row);
      Aver.AreEqual("Ivan", row.Row.Name);
      Aver.IsNotNull(row.Row.Int32List);
      Aver.AreEqual(3, row.Row.Int32List.Count);
      Aver.AreEqual(1, row.Row.Int32List[0]);
      Aver.AreEqual(0, row.Row.Int32List[1]);
      Aver.AreEqual(-12345, row.Row.Int32List[2]);

      Aver.IsNotNull(row.RowList);
      Aver.AreEqual(2, row.RowList.Count);
      Aver.IsNotNull(row.RowList[0]);
      Aver.AreEqual("John", row.RowList[0].Name);
      Aver.AreEqual(123, row.RowList[0].Int8);
      Aver.IsNotNull(row.RowList[1]);
      Aver.AreEqual("Anna", row.RowList[1].Name);
      Aver.AreEqual(123.567F, row.RowList[1].Single);
    }

    #endregion Lists

    #region Amorphous

    [Run]
    public void ToAmorphousRow_FromString_Strings()
    {
      var str = @"{ Name_1: ""Orlov"", StringArray_1: [""a"", null, ""b""]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual("Orlov", row.AmorphousData["Name_1"]);

      var array = row.AmorphousData["StringArray_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual("a", array[0]);
      Aver.IsNull(array[1]);
      Aver.AreObjectsEqual("b", array[2]);
    }

    [Run]
    public void ToAmorphousRow_FromString_Bytes()
    {
      var str = @"{UInt8_1: 255, UInt8Array_1: [1, 0, 255], UInt8NArray_1: [null, 0, 124]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(255, row.AmorphousData["UInt8_1"]);

      var array = row.AmorphousData["UInt8Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(1, array[0]);
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(255, array[2]);

      var narray = row.AmorphousData["UInt8NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(124, narray[2]);
    }

    [Run]
    public void ToAmorphousRow_FromString_SBytes()
    {
      var str = @"{Int8_1: -56, Int8Array_1: [-1, 0, 127], Int8NArray_1: [null, 0, 127]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(-56, row.AmorphousData["Int8_1"]);

      var array = row.AmorphousData["Int8Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(-1, array[0]);
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(127, array[2]);

      var narray = row.AmorphousData["Int8NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(127, narray[2]);
    }

    [Run]
    public void ToAmorphousRow_FromString_Shorts()
    {
      var str = @"{Int16_1: 12345, Int16Array_1: [32767, 0, -32768], Int16NArray_1: [null, 0, 32767]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(12345, row.AmorphousData["Int16_1"]);

      var array = row.AmorphousData["Int16Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(short.MaxValue, array[0].AsShort());
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(short.MinValue, array[2].AsShort());

      var narray = row.AmorphousData["Int16NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(short.MaxValue, narray[2].AsShort());
    }

    [Run]
    public void ToAmorphousRow_FromString_UShorts()
    {
      var str = @"{UInt16_1: 123, UInt16Array_1: [65535, 0, 12345], UInt16NArray_1: [null, 0, 65535]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(123, row.AmorphousData["UInt16_1"]);

      var array = row.AmorphousData["UInt16Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(ushort.MaxValue, array[0].AsUShort());
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(12345, array[2]);

      var narray = row.AmorphousData["UInt16NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(ushort.MaxValue, narray[2].AsUShort());
    }

    [Run]
    public void ToAmorphousRow_FromString_Int()
    {
      var str = @"{Int32_1: 123, Int32Array_1: [2147483647, 0, -2147483648], Int32NArray_1: [null, 0, 2147483647]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(123, row.AmorphousData["Int32_1"]);

      var array = row.AmorphousData["Int32Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(int.MaxValue, array[0].AsInt());
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(int.MinValue, array[2].AsInt());

      var narray = row.AmorphousData["Int32NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(int.MaxValue, narray[2].AsInt());
    }

    [Run]
    public void ToAmorphousRow_FromString_UInt()
    {
      var str = @"{UInt32_1: 123, UInt32Array_1: [4294967295, 0, 124], UInt32NArray_1: [null, 0, 4294967295]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(123, row.AmorphousData["UInt32_1"]);

      var array = row.AmorphousData["UInt32Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(uint.MaxValue, array[0].AsUInt());
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(124, array[2]);

      var narray = row.AmorphousData["UInt32NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(uint.MaxValue, narray[2].AsUInt());
    }

    [Run]
    public void ToAmorphousRow_FromString_Long()
    {
      var str = @"{Int64_1: 123, Int64Array_1: [9223372036854775807, 0, -9223372036854775808], Int64NArray_1: [null, 0, 9223372036854775807]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(123, row.AmorphousData["Int64_1"]);

      var array = row.AmorphousData["Int64Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(long.MaxValue, array[0].AsLong());
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(long.MinValue, array[2]);

      var narray = row.AmorphousData["Int64NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(long.MaxValue, narray[2].AsLong());
    }

    [Run]
    public void ToAmorphousRow_FromString_ULong()
    {
      var str = @"{UInt64_1: 123, UInt64Array_1: [18446744073709551615, 0, 124], UInt64NArray_1: [null, 0, 18446744073709551615]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(123, row.AmorphousData["UInt64_1"]);

      var array = row.AmorphousData["UInt64Array_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(ulong.MaxValue, array[0]);
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(124, array[2]);

      var narray = row.AmorphousData["UInt64NArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(ulong.MaxValue, narray[2]);
    }

    [Run]
    public void ToAmorphousRow_FromString_Single()
    {
      var str = @"{Single_1: 123.456, SingleArray_1: [3.4028E+38, 0, -3.402E+38], SingleNArray_1: [null, 0, 3.4028]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(123.456F, row.AmorphousData["Single_1"].AsFloat());

      var array = row.AmorphousData["SingleArray_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreEqual(3.4028E+38F, array[0].AsFloat());
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreEqual(-3.402E+38F, array[2].AsFloat());

      var narray = row.AmorphousData["SingleNArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreEqual(0, narray[1].AsFloat());
      Aver.AreEqual(3.4028F, narray[2].AsFloat());
    }

    [Run]
    public void ToAmorphousRow_FromString_Double()
    {
      var str = @"{Double_1: 123.456, DoubleArray_1: [1.79769E+308, 0, -1.7976931E+308], DoubleNArray_1: [null, 0, 3.482347E+38]}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreObjectsEqual(123.456D, row.AmorphousData["Double_1"]);

      var array = row.AmorphousData["DoubleArray_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreObjectsEqual(1.79769E+308, array[0]);
      Aver.AreObjectsEqual(0, array[1]);
      Aver.AreObjectsEqual(-1.7976931E+308, array[2]);

      var narray = row.AmorphousData["DoubleNArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.IsNull(narray[0]);
      Aver.AreObjectsEqual(0, narray[1]);
      Aver.AreObjectsEqual(3.482347E+38, narray[2]);
    }

    [Run]
    public void ToAmorphousRow_FromString_Guid()
    {
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      var guid3 = Guid.NewGuid();
      var str = "{" + @"Guid_1: ""{0}"", GuidArray_1: [""{0}"", ""{1}"", ""{2}""], GuidNArray_1: [""{0}"", null, ""{2}""]".Args(guid1, guid2, guid3) + "}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(guid1, row.AmorphousData["Guid_1"].AsGUID(Guid.Empty));

      var array = row.AmorphousData["GuidArray_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreEqual(guid1, array[0].AsGUID(Guid.Empty));
      Aver.AreEqual(guid2, array[1].AsGUID(Guid.Empty));
      Aver.AreEqual(guid3, array[2].AsGUID(Guid.Empty));

      var narray = row.AmorphousData["GuidNArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.AreEqual(guid1, narray[0].AsGUID(Guid.Empty));
      Aver.AreEqual(null, narray[1].AsNullableGUID());
      Aver.AreEqual(guid3, narray[2].AsGUID(Guid.Empty));
    }

    [Run]
    public void ToAmorphousRow_FromString_GDID()
    {
      var gdid1 = new GDID(4000000000, 8000000000);
      var gdid2 = new GDID(100002, 1, 8000000000);
      var gdid3 = new GDID(123, 123456789);
      var str = @"{" + @"GDID_1: ""{0}"", GDIDArray_1: [""{0}"", ""{1}"", ""{2}""], GDIDNArray_1: [""{0}"", null, ""{2}""]".Args(gdid1, gdid2, gdid3) + "}";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      Aver.AreEqual(gdid1, row.AmorphousData["GDID_1"].AsGDID());

      var array = row.AmorphousData["GDIDArray_1"] as JsonDataArray;
      Aver.IsNotNull(array);
      Aver.AreEqual(3, array.Count);
      Aver.AreEqual(gdid1, array[0].AsGDID());
      Aver.AreEqual(gdid2, array[1].AsGDID());
      Aver.AreEqual(gdid3, array[2].AsGDID());

      var narray = row.AmorphousData["GDIDNArray_1"] as JsonDataArray;
      Aver.IsNotNull(narray);
      Aver.AreEqual(3, narray.Count);
      Aver.AreEqual(gdid1, narray[0].AsGDID());
      Aver.IsNull(narray[1]);
      Aver.AreEqual(gdid3, narray[2].AsGDID());
    }

    [Run]
    public void ToAmorphousRow_FromString_Row()
    {
      var str =
      @"{
            Row_1: { Name: ""Ivan"", Int32Array: [1, 0, -12345] },
            RowArray_1:
                [
                    { Name: ""John"", Int8: 123 },
                    { Name: ""Anna"", Single: 123.567 }
                ]
        }";

      var row = new AmorphousRow(Schema.GetForTypedDoc(typeof(RowWithArrays)));
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap);

      var innerRow = row.AmorphousData["Row_1"] as JsonDataMap;
      Aver.IsNotNull(innerRow);
      Aver.AreObjectsEqual("Ivan", innerRow["Name"]);

      var innerRowArray = innerRow["Int32Array"] as JsonDataArray;
      Aver.IsNotNull(innerRowArray);
      Aver.AreEqual(3, innerRowArray.Count);
      Aver.AreObjectsEqual(1, innerRowArray[0]);
      Aver.AreObjectsEqual(0, innerRowArray[1]);
      Aver.AreObjectsEqual(-12345, innerRowArray[2]);

      var innerArray = row.AmorphousData["RowArray_1"] as JsonDataArray;
      Aver.IsNotNull(innerArray);
      Aver.AreEqual(2, innerArray.Count);

      var innerArrayRow0 = innerArray[0] as JsonDataMap;
      Aver.IsNotNull(innerArrayRow0);
      Aver.AreObjectsEqual("John", innerArrayRow0["Name"]);
      Aver.AreObjectsEqual(123, innerArrayRow0["Int8"]);

      var innerArrayRow1 = innerArray[1] as JsonDataMap;
      Aver.IsNotNull(innerArrayRow1);
      Aver.AreObjectsEqual("Anna", innerArrayRow1["Name"]);
      Aver.AreEqual(123.567F, innerArrayRow1["Single"].AsFloat());
    }
    #endregion Amorphous

    #region Targeting

    [Run]
    public void TargetedNames()
    {
      var str =
      @"{
            fn: ""Ivan"",
            ln: ""Kozelov"",
            age: 123
        }";

      var row = new RowWithTargetedNames();
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap, options: JsonReader.DocReadOptions.BindByBackendName(null));
      Aver.AreEqual("Ivan", row.FirstName);
      Aver.AreEqual("Kozelov", row.LastName);
      Aver.AreEqual(123, row.Age);

      str =
      @"{
            fn: ""Ivan"",
            ln: ""Kozelov"",
            a: 123
        }";

      row = new RowWithTargetedNames();
      JsonReader.ToDoc(row, str.JsonToDataObject() as JsonDataMap, options: JsonReader.DocReadOptions.BindByBackendName("MY-TARGET"));
      Aver.AreEqual("Ivan", row.FirstName);
      Aver.AreEqual("Kozelov", row.LastName);
      Aver.AreEqual(123, row.Age);

    }
    #endregion

    #region Types

    public class RowWithArrays : TypedDoc
    {
      [Field]
      public string Name { get; set; }
      [Field]
      public string[] StringArray { get; set; }

      [Field]
      public sbyte Int8 { get; set; }
      [Field]
      public sbyte[] Int8Array { get; set; }
      [Field]
      public sbyte?[] Int8NArray { get; set; }

      [Field]
      public byte UInt8 { get; set; }
      [Field]
      public byte[] UInt8Array { get; set; }
      [Field]
      public byte?[] UInt8NArray { get; set; }

      [Field]
      public short Int16 { get; set; }
      [Field]
      public short[] Int16Array { get; set; }
      [Field]
      public short?[] Int16NArray { get; set; }

      [Field]
      public ushort UInt16 { get; set; }
      [Field]
      public ushort[] UInt16Array { get; set; }
      [Field]
      public ushort?[] UInt16NArray { get; set; }

      [Field]
      public int Int32 { get; set; }
      [Field]
      public int[] Int32Array { get; set; }
      [Field]
      public int?[] Int32NArray { get; set; }

      [Field]
      public uint UInt32 { get; set; }
      [Field]
      public uint[] UInt32Array { get; set; }
      [Field]
      public uint?[] UInt32NArray { get; set; }

      [Field]
      public long Int64 { get; set; }
      [Field]
      public long[] Int64Array { get; set; }
      [Field]
      public long?[] Int64NArray { get; set; }

      [Field]
      public ulong UInt64 { get; set; }
      [Field]
      public ulong[] UInt64Array { get; set; }
      [Field]
      public ulong?[] UInt64NArray { get; set; }

      [Field]
      public float Single { get; set; }
      [Field]
      public float[] SingleArray { get; set; }
      [Field]
      public float?[] SingleNArray { get; set; }

      [Field]
      public double Double { get; set; }
      [Field]
      public double[] DoubleArray { get; set; }
      [Field]
      public double?[] DoubleNArray { get; set; }

      [Field]
      public Guid Guid { get; set; }
      [Field]
      public Guid[] GuidArray { get; set; }
      [Field]
      public Guid?[] GuidNArray { get; set; }

      [Field]
      public GDID GDID { get; set; }
      [Field]
      public GDID[] GDIDArray { get; set; }
      [Field]
      public GDID?[] GDIDNArray { get; set; }

      [Field]
      public RowWithArrays Row { get; set; }
      [Field]
      public RowWithArrays[] RowArray { get; set; }
    }

    public class RowWithLists : TypedDoc
    {
      [Field]
      public string Name { get; set; }
      [Field]
      public List<string> StringList { get; set; }

      [Field]
      public sbyte Int8 { get; set; }
      [Field]
      public List<sbyte> Int8List { get; set; }
      [Field]
      public List<sbyte?> Int8NList { get; set; }

      [Field]
      public byte UInt8 { get; set; }
      [Field]
      public List<byte> UInt8List { get; set; }
      [Field]
      public List<byte?> UInt8NList { get; set; }

      [Field]
      public short Int16 { get; set; }
      [Field]
      public List<short> Int16List { get; set; }
      [Field]
      public List<short?> Int16NList { get; set; }

      [Field]
      public ushort UInt16 { get; set; }
      [Field]
      public List<ushort> UInt16List { get; set; }
      [Field]
      public List<ushort?> UInt16NList { get; set; }

      [Field]
      public int Int32 { get; set; }
      [Field]
      public List<int> Int32List { get; set; }
      [Field]
      public List<int?> Int32NList { get; set; }

      [Field]
      public uint UInt32 { get; set; }
      [Field]
      public List<uint> UInt32List { get; set; }
      [Field]
      public List<uint?> UInt32NList { get; set; }

      [Field]
      public long Int64 { get; set; }
      [Field]
      public List<long> Int64List { get; set; }
      [Field]
      public List<long?> Int64NList { get; set; }

      [Field]
      public ulong UInt64 { get; set; }
      [Field]
      public List<ulong> UInt64List { get; set; }
      [Field]
      public List<ulong?> UInt64NList { get; set; }

      [Field]
      public float Single { get; set; }
      [Field]
      public List<float> SingleList { get; set; }
      [Field]
      public List<float?> SingleNList { get; set; }

      [Field]
      public double Double { get; set; }
      [Field]
      public List<double> DoubleList { get; set; }
      [Field]
      public List<double?> DoubleNList { get; set; }

      [Field]
      public Guid Guid { get; set; }
      [Field]
      public List<Guid> GuidList { get; set; }
      [Field]
      public List<Guid?> GuidNList { get; set; }

      [Field]
      public GDID GDID { get; set; }
      [Field]
      public List<GDID> GDIDList { get; set; }
      [Field]
      public List<GDID?> GDIDNList { get; set; }

      [Field]
      public RowWithLists Row { get; set; }
      [Field]
      public List<RowWithLists> RowList { get; set; }
    }

    public class AmorphousRow : AmorphousDynamicDoc
    {
      public AmorphousRow(Schema schema) : base(schema)
      {
      }
    }


    public class RowWithTargetedNames : TypedDoc
    {
      [Field(backendName: "fn")]
      public string FirstName { get; set; }

      [Field(backendName: "ln")]
      public string LastName { get; set; }

      [Field(backendName: "age")]
      [Field("MY-TARGET", backendName: "a")]
      public int Age { get; set; }

    }


    #endregion Types
  }
}
