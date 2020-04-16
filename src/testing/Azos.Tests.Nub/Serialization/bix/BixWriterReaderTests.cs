/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Scripting;
using Azos.Serialization.Bix;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class BixWriterReaderTests
  {
    #region test corpus
    private void testScalar<T>(T v, Action<BixWriter> write, Func<BixReader, T> read, int sz = 0) where T : IEquatable<T>
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);

      if (sz>0)
        Aver.AreEqual(sz, ms.Position);

      ms.Position = 0;
      var got = read(reader);

      Aver.AreEqual(v, got);
    }

    private void testScalar<T>(Nullable<T> v, Action<BixWriter> write, Func<BixReader, Nullable<T>> read, int sz = 0) where T : struct, IEquatable<T>
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);

      if (sz > 0)
        Aver.AreEqual(sz, ms.Position);

      ms.Position = 0;
      var got = read(reader);

      Aver.AreEqual(v, got);
    }

    private void testCollection<T>(ICollection<T> v, Action<BixWriter> write, Func<BixReader, ICollection<T>> read, int sz = 0)
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);

      if (sz > 0)
        Aver.AreEqual(sz, ms.Position);

      ms.Position = 0;
      var got = read(reader);

      if (v == null && got == null) return;

      Aver.IsTrue(v.SequenceEqual(got));
    }

    private void testArray<T>(T[] v, Action<BixWriter> write, Func<BixReader, T[]> read, int sz = 0)
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);

      if (sz > 0)
        Aver.AreEqual(sz, ms.Position);

      ms.Position = 0;
      var got = read(reader);

      if (v == null && got == null) return;

      Aver.IsTrue(v.SequenceEqual(got));
    }
    #endregion

    // ---------------------------------------------

    #region BYTE
    [Run] public void Byte_01()
    {
      byte v = byte.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadByte(), 1);

      v = byte.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadByte(), 1);
    }

    [Run] public void Byte_02_Nullable()
    {
      byte? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableByte(), 1);

      v = byte.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableByte(), 2);

      v = byte.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableByte(), 2);
    }

    [Run] public void Byte_03_Collection()
    {
      List<byte> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadByteCollection<List<byte>>());

      v = new List<byte> { 0, 1, 2, 3, 4 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadByteCollection<List<byte>>());
    }

    [Run] public void Byte_04_CollectionNullable()
    {
      List<byte?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableByteCollection<List<byte?>>());

      v = new List<byte?> { 0, null, 2, null, 4 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableByteCollection<List<byte?>>());
    }


    [Run] public void Byte_05_Array()
    {
      byte[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadByteArray());

      v = new byte[] { 1, 2, 5, 6 };
      testArray(v, w => w.Write(v), r => r.ReadByteArray(), 1 + 1 + 4);
    }

    [Run] public void Byte_06_ArrayNullable()
    {
      byte?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableByteArray(), 1);

      v = new byte?[] { 1, null, 25 };
      testArray(v, w => w.Write(v), r => r.ReadNullableByteArray(), 1 + 1 + 5);
    }

    [Run]
    public void Byte_07_Buffer()
    {
      byte[] v = null;
      testArray(v, w => w.WriteBuffer(v), r => r.ReadBuffer(), 1);

      v = new byte[] { 1, 2, 5, 6 };
      testArray(v, w => w.WriteBuffer(v), r => r.ReadBuffer(), 1 + 1 + 4);
    }


    #endregion

    #region BOOL
    [Run]
    public void Bool_01()
    {
      bool v = false;
      testScalar(v, w => w.Write(v), r => r.ReadBool(), 1);
      v = true;
      testScalar(v, w => w.Write(v), r => r.ReadBool(), 1);
    }

    [Run]
    public void Bool_02_Nullable()
    {
      bool? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableBool(), 1);

      v = false;
      testScalar(v, w => w.Write(v), r => r.ReadNullableBool(), 2);
      v = true;
      testScalar(v, w => w.Write(v), r => r.ReadNullableBool(), 2);
    }

    [Run]
    public void Bool_03_Collection()
    {
      List<bool> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadBoolCollection<List<bool>>());

      v = new List<bool> { false, true, true };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadBoolCollection<List<bool>>());
    }

    [Run]
    public void Bool_04_CollectionNullable()
    {
      List<bool?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableBoolCollection<List<bool?>>());

      v = new List<bool?> { true, null, false, false, null, true };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableBoolCollection<List<bool?>>());
    }

    [Run]
    public void Bool_05_Array()
    {
      bool[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadBoolArray());

      v = new bool[] { true, false, true, true };
      testArray(v, w => w.Write(v), r => r.ReadBoolArray());
    }

    [Run]
    public void Bool_06_ArrayNullable()
    {
      bool?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableBoolArray());

      v = new bool?[] { true, null, false, false };
      testArray(v, w => w.Write(v), r => r.ReadNullableBoolArray());
    }
    #endregion

    #region SBYTE
    [Run]
    public void SByte_01()
    {
      sbyte v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadSbyte(), 1);

      v = sbyte.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadSbyte(), 1);

      v = sbyte.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadSbyte(), 1);
    }

    [Run]
    public void SByte_02_Nullable()
    {
      sbyte? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableSbyte(), 1);

      v = sbyte.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableSbyte(), 2);

      v = sbyte.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableSbyte(), 2);
    }

    [Run]
    public void SByte_03_Collection()
    {
      List<sbyte> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadSbyteCollection<List<sbyte>>(), 1);

      v = new List<sbyte> { 0, -1, 127, -128, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadSbyteCollection<List<sbyte>>(), 1+1+5);
    }

    [Run]
    public void SByte_04_CollectionNullable()
    {
      List<sbyte?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableSbyteCollection<List<sbyte?>>(), 1);

      v = new List<sbyte?> { 0, null, -128, null, 127 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableSbyteCollection<List<sbyte?>>(), 1+1+8);
    }

    [Run]
    public void SByte_05_Array()
    {
      sbyte[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadSbyteArray(), 1);

      v = new sbyte[] { 1, -2, 127, -128 };
      testArray(v, w => w.Write(v), r => r.ReadSbyteArray(), 1+1+4);
    }

    [Run]
    public void SByte_06_ArrayNullable()
    {
      sbyte?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableSbyteArray(), 1);

      v = new sbyte?[] { 1, null, -128, 127, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableSbyteArray(), 1+1+11);
    }
    #endregion

    #region SHORT
    [Run]
    public void Short_01()
    {
      short v = short.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadShort(), 3);

      v = short.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadShort(), 3);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadShort(), 1);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadShort(), 2);
    }

    [Run]
    public void Short_02_Nullable()
    {
      short? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableShort(), 1);

      v = short.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableShort(), 4);

      v = short.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableShort(), 4);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableShort(), 2);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableShort(), 3);
    }

    [Run]
    public void Short_03_Collection()
    {
      List<short> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadShortCollection<List<short>>(), 1);

      v = new List<short> { 0, -1, 127, -128, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadShortCollection<List<short>>(), 1 + 1 + 7);
    }


    [Run]
    public void Short_04_CollectionNullable()
    {
      List<short?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableShortCollection<List<short?>>(), 1);

      v = new List<short?> { 0, null, -130, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableShortCollection<List<short?>>(), 1 + 1 + 10);
    }

    [Run]
    public void Short_05_Array()
    {
      short[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadShortArray(), 1);

      v = new short[] { 1, -12, 127, -728 };
      testArray(v, w => w.Write(v), r => r.ReadShortArray(), 1 + 1 + 6);
    }

    [Run]
    public void Short_06_ArrayNullable()
    {
      short?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableShortArray(), 1);

      v = new short?[] { 1, null, -1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableShortArray(), 1 + 1 + 13);
    }

    #endregion

    #region USHORT
    [Run]
    public void Ushort_01()
    {
      ushort v = ushort.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadUshort(), 1);

      v = ushort.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadUshort(), 3);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadUshort(), 1);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadUshort(), 2);
    }

    [Run]
    public void Ushort_02_Nullable()
    {
      ushort? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUshort(), 1);

      v = ushort.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUshort(), 2);

      v = ushort.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUshort(), 4);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUshort(), 2);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUshort(), 3);
    }

    [Run]
    public void Ushort_03_Collection()
    {
      List<ushort> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadUshortCollection<List<ushort>>(), 1);

      v = new List<ushort> { 0, 300, 127, 700, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadUshortCollection<List<ushort>>(), 1 + 1 + 7);
    }


    [Run]
    public void Ushort_04_CollectionNullable()
    {
      List<ushort?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableUshortCollection<List<ushort?>>(), 1);

      v = new List<ushort?> { 0, null, 300, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableUshortCollection<List<ushort?>>(), 1 + 1 + 10);
    }

    [Run]
    public void Ushort_05_Array()
    {
      ushort[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadUshortArray(), 1);

      v = new ushort[] { 1, 300, 127, 728 };
      testArray(v, w => w.Write(v), r => r.ReadUshortArray(), 1 + 1 + 6);
    }

    [Run]
    public void Ushort_06_ArrayNullable()
    {
      ushort?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableUshortArray(), 1);

      v = new ushort?[] { 1, null, 1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableUshortArray(), 1 + 1 + 13);
    }

    #endregion

    #region INT
    [Run]
    public void Int_01()
    {
      int v = int.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadInt(), 5);

      v = int.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadInt(), 5);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadInt(), 1);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadInt(), 2);
    }

    [Run]
    public void Int_02_Nullable()
    {
      int? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableInt(), 1);

      v = int.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableInt(), 6);

      v = int.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableInt(), 6);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableInt(), 2);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableInt(), 3);
    }

    [Run]
    public void Int_03_Collection()
    {
      List<int> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadIntCollection<List<int>>(), 1);

      v = new List<int> { 0, -1, 127, -128, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadIntCollection<List<int>>(), 1 + 1 + 7);
    }


    [Run]
    public void Int_04_CollectionNullable()
    {
      List<int?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableIntCollection<List<int?>>(), 1);

      v = new List<int?> { 0, null, -130, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableIntCollection<List<int?>>(), 1 + 1 + 10);
    }

    [Run]
    public void Int_05_Array()
    {
      int[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadIntArray(), 1);

      v = new int[] { 1, -12, 127, -728 };
      testArray(v, w => w.Write(v), r => r.ReadIntArray(), 1 + 1 + 6);
    }

    [Run]
    public void Int_06_ArrayNullable()
    {
      int?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableIntArray(), 1);

      v = new int?[] { 1, null, -1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableIntArray(), 1 + 1 + 13);
    }

    #endregion

    #region UINT
    [Run]
    public void Uint_01()
    {
      uint v = uint.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadUint(), 1);

      v = uint.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadUint(), 5);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadUint(), 1);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadUint(), 2);
    }

    [Run]
    public void Uint_02_Nullable()
    {
      uint? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUint(), 1);

      v = uint.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUint(), 2);

      v = uint.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUint(), 6);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUint(), 2);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUint(), 3);
    }

    [Run]
    public void Uint_03_Collection()
    {
      List<uint> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadUintCollection<List<uint>>(), 1);

      v = new List<uint> { 0, 300, 127, 700, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadUintCollection<List<uint>>(), 1 + 1 + 7);
    }


    [Run]
    public void Uint_04_CollectionNullable()
    {
      List<uint?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableUintCollection<List<uint?>>(), 1);

      v = new List<uint?> { 0, null, 300, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableUintCollection<List<uint?>>(), 1 + 1 + 10);
    }

    [Run]
    public void Uint_05_Array()
    {
      uint[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadUintArray(), 1);

      v = new uint[] { 1, 300, 127, 728 };
      testArray(v, w => w.Write(v), r => r.ReadUintArray(), 1 + 1 + 6);
    }

    [Run]
    public void Uint_06_ArrayNullable()
    {
      uint?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableUintArray(), 1);

      v = new uint?[] { 1, null, 1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableUintArray(), 1 + 1 + 13);
    }

    #endregion

    #region LONG
    [Run]
    public void Long_01()
    {
      long v = long.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadLong(), 10);

      v = long.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadLong(), 10);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadLong(), 1);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadLong(), 2);
    }

    [Run]
    public void Long_02_Nullable()
    {
      long? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableLong(), 1);

      v = long.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableLong(), 11);

      v = long.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableLong(), 11);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableLong(), 2);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableLong(), 3);
    }

    [Run]
    public void Long_03_Collection()
    {
      List<long> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadLongCollection<List<long>>(), 1);

      v = new List<long> { 0, -1, 127, -128, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadLongCollection<List<long>>(), 1 + 1 + 7);
    }


    [Run]
    public void Long_04_CollectionNullable()
    {
      List<long?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableLongCollection<List<long?>>(), 1);

      v = new List<long?> { 0, null, -130, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableLongCollection<List<long?>>(), 1 + 1 + 10);
    }

    [Run]
    public void Long_05_Array()
    {
      long[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadLongArray(), 1);

      v = new long[] { 1, -12, 127, -728 };
      testArray(v, w => w.Write(v), r => r.ReadLongArray(), 1 + 1 + 6);
    }

    [Run]
    public void Long_06_ArrayNullable()
    {
      long?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableLongArray(), 1);

      v = new long?[] { 1, null, -1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableLongArray(), 1 + 1 + 13);
    }

    #endregion

    #region ULONG
    [Run]
    public void Ulong_01()
    {
      ulong v = ulong.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadUlong(), 1);

      v = ulong.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadUlong(), 10);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadUlong(), 1);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadUlong(), 2);
    }

    [Run]
    public void Ulong_02_Nullable()
    {
      ulong? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUlong(), 1);

      v = ulong.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUlong(), 2);

      v = ulong.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUlong(), 11);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUlong(), 2);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableUlong(), 3);
    }

    [Run]
    public void Ulong_03_Collection()
    {
      List<ulong> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadUlongCollection<List<ulong>>(), 1);

      v = new List<ulong> { 0, 300, 127, 700, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadUlongCollection<List<ulong>>(), 1 + 1 + 7);
    }


    [Run]
    public void Ulong_04_CollectionNullable()
    {
      List<ulong?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableUlongCollection<List<ulong?>>(), 1);

      v = new List<ulong?> { 0, null, 300, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableUlongCollection<List<ulong?>>(), 1 + 1 + 10);
    }

    [Run]
    public void Ulong_05_Array()
    {
      ulong[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadUlongArray(), 1);

      v = new ulong[] { 1, 300, 127, 728 };
      testArray(v, w => w.Write(v), r => r.ReadUlongArray(), 1 + 1 + 6);
    }

    [Run]
    public void Ulong_06_ArrayNullable()
    {
      ulong?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableUlongArray(), 1);

      v = new ulong?[] { 1, null, 1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableUlongArray(), 1 + 1 + 13);
    }

    #endregion

    #region DOUBLE
    [Run]
    public void Double_01()
    {
      double v = double.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadDouble(), 8);

      v = double.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadDouble(), 8);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadDouble(), 8);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadDouble(), 8);
    }

    [Run]
    public void Double_02_Nullable()
    {
      double? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDouble(), 1);

      v = double.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDouble(), 9);

      v = double.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDouble(), 9);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDouble(), 9);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDouble(), 9);
    }

    [Run]
    public void Double_03_Collection()
    {
      List<double> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadDoubleCollection<List<double>>(), 1);

      v = new List<double> { 0, 300.4, 127e4, -700.11, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadDoubleCollection<List<double>>(), 1 + 1 + 40);
    }


    [Run]
    public void Double_04_CollectionNullable()
    {
      List<double?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableDoubleCollection<List<double?>>(), 1);

      v = new List<double?> { 0, null, -300, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableDoubleCollection<List<double?>>(), 1 + 1 + 29);
    }

    [Run]
    public void Double_05_Array()
    {
      double[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadDoubleArray(), 1);

      v = new double[] { 1.02, 300, 127e5, -728e11 };
      testArray(v, w => w.Write(v), r => r.ReadDoubleArray(), 1 + 1 + 32);
    }

    [Run]
    public void Double_06_ArrayNullable()
    {
      double?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableDoubleArray(), 1);

      v = new double?[] { 1, null, 1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableDoubleArray(), 1 + 1 + 46);
    }

    #endregion

    #region FLOAT
    [Run]
    public void Float_01()
    {
      float v = float.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadFloat(), 4);

      v = float.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadFloat(), 4);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadFloat(), 4);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadFloat(), 4);
    }

    [Run]
    public void Float_02_Nullable()
    {
      float? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableFloat(), 1);

      v = float.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableFloat(), 5);

      v = float.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableFloat(), 5);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableFloat(), 5);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableFloat(), 5);
    }

    [Run]
    public void Float_03_Collection()
    {
      List<float> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadFloatCollection<List<float>>(), 1);

      v = new List<float> { 0, 300.4f, 127e4f, -700.11f, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadFloatCollection<List<float>>(), 1 + 1 + 20);
    }


    [Run]
    public void Float_04_CollectionNullable()
    {
      List<float?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableFloatCollection<List<float?>>(), 1);

      v = new List<float?> { 0, null, -300, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableFloatCollection<List<float?>>(), 1 + 1 + 17);
    }

    [Run]
    public void Float_05_Array()
    {
      float[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadFloatArray(), 1);

      v = new float[] { 1.02f, 300, 127e5f, -728e11f };
      testArray(v, w => w.Write(v), r => r.ReadFloatArray(), 1 + 1 + 16);
    }

    [Run]
    public void Float_06_ArrayNullable()
    {
      float?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableFloatArray(), 1);

      v = new float?[] { 1, null, 1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableFloatArray(), 1 + 1 + 26);
    }

    #endregion

    #region DECIMAL
    [Run]
    public void Decimal_01()
    {
      decimal v = decimal.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadDecimal(), 4);

      v = decimal.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadDecimal(), 4);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadDecimal(), 4);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadDecimal(), 5);
    }

    [Run]
    public void Decimal_02_Nullable()
    {
      decimal? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDecimal(), 1);

      v = decimal.MinValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDecimal(), 5);

      v = decimal.MaxValue;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDecimal(), 5);

      v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDecimal(), 5);

      v = 256;
      testScalar(v, w => w.Write(v), r => r.ReadNullableDecimal(), 6);
    }

    [Run]
    public void Decimal_03_Collection()
    {
      List<decimal> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadDecimalCollection<List<decimal>>(), 1);

      v = new List<decimal> { 0, 300.4m, 127e4m, -700.11m, 0 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadDecimalCollection<List<decimal>>(), 1 + 1 + 26);
    }


    [Run]
    public void Decimal_04_CollectionNullable()
    {
      List<decimal?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableDecimalCollection<List<decimal?>>(), 1);

      v = new List<decimal?> { 0, null, -300, null, 180 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableDecimalCollection<List<decimal?>>(), 1 + 1 + 19);
    }

    [Run]
    public void Decimal_05_Array()
    {
      decimal[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadDecimalArray(), 1);

      v = new decimal[] { 1.02m, 300, 127e5m, -728e11m };
      testArray(v, w => w.Write(v), r => r.ReadDecimalArray(), 1 + 1 + 27);
    }

    [Run]
    public void Decimal_06_ArrayNullable()
    {
      decimal?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableDecimalArray(), 1);

      v = new decimal?[] { 1, null, 1282, 1227, 0, 9 };
      testArray(v, w => w.Write(v), r => r.ReadNullableDecimalArray(), 1 + 1 + 28);
    }

    #endregion

  }
}
