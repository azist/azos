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


  }
}
