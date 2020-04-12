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
    private void testScalar<T>(T v, Action<BixWriter> write, Func<BixReader, T> read) where T : IEquatable<T>
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);
      ms.Position = 0;
      var got = read(reader);

      Aver.AreEqual(v, got);
    }

    private void testScalar<T>(Nullable<T> v, Action<BixWriter> write, Func<BixReader, Nullable<T>> read) where T : struct, IEquatable<T>
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);
      ms.Position = 0;
      var got = read(reader);

      Aver.AreEqual(v, got);
    }

    private void testCollection<T>(ICollection<T> v, Action<BixWriter> write, Func<BixReader, ICollection<T>> read)
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);
      ms.Position = 0;
      var got = read(reader);

      if (v == null && got == null) return;

      Aver.IsTrue(v.SequenceEqual(got));
    }

    private void testArray<T>(T[] v, Action<BixWriter> write, Func<BixReader, T[]> read)
    {
      var ms = new MemoryStream();
      var reader = new BixReader(ms);
      var writer = new BixWriter(ms);

      ms.Position = 0;
      write(writer);
      ms.Position = 0;
      var got = read(reader);

      if (v == null && got == null) return;

      Aver.IsTrue(v.SequenceEqual(got));
    }
    #endregion



    #region BYTE
    [Run] public void Byte_01()
    {
      byte v = 0;
      testScalar(v, w => w.Write(v), r => r.ReadByte());
    }

    [Run] public void Byte_02()
    {
      byte v = 255;
      testScalar(v, w => w.Write(v), r => r.ReadByte());
    }

    [Run] public void Byte_03_Nullable()
    {
      byte? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableByte());
    }

    [Run] public void Byte_04_Nullable()
    {
      byte? v = 255;
      testScalar(v, w => w.Write(v), r => r.ReadNullableByte());
    }

    [Run] public void Byte_05_Collection()
    {
      List<byte> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadByteCollection<List<byte>>());
    }

    [Run] public void Byte_06_Collection()
    {
      List<byte> v = new List<byte>{0,1, 2, 3, 4};
      testCollection(v, w => w.WriteCollection(v), r => r.ReadByteCollection<List<byte>>());
    }

    [Run] public void Byte_07_CollectionNullable()
    {
      List<byte?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableByteCollection<List<byte?>>());
    }

    [Run] public void Byte_08_CollectionNullable()
    {
      List<byte?> v = new List<byte?> { 0, null, 2, null, 4 };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableByteCollection<List<byte?>>());
    }

    [Run] public void Byte_09_Array()
    {
      byte[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadByteArray());
    }

    [Run] public void Byte_10_Array()
    {
      byte[] v = new byte[]{1,2,5,6};
      testArray(v, w => w.Write(v), r => r.ReadByteArray());
    }

    [Run] public void Byte_11_ArrayNullable()
    {
      byte?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableByteArray());
    }

    [Run] public void Byte_12_ArrayNullable()
    {
      byte?[] v = new byte?[]{1, null, 25};
      testArray(v, w => w.Write(v), r => r.ReadNullableByteArray());
    }
    #endregion

    #region BOOL
    [Run]
    public void Bool_01()
    {
      bool v = false;
      testScalar(v, w => w.Write(v), r => r.ReadBool());
    }

    [Run]
    public void Bool_02()
    {
      bool v = true;
      testScalar(v, w => w.Write(v), r => r.ReadBool());
    }

    [Run]
    public void Bool_03_Nullable()
    {
      bool? v = null;
      testScalar(v, w => w.Write(v), r => r.ReadNullableBool());
    }

    [Run]
    public void Bool_04_Nullable()
    {
      bool? v = true;
      testScalar(v, w => w.Write(v), r => r.ReadNullableBool());
    }

    [Run]
    public void Bool_05_Collection()
    {
      List<bool> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadBoolCollection<List<bool>>());
    }

    [Run]
    public void Bool_06_Collection()
    {
      List<bool> v = new List<bool> { false, true, true };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadBoolCollection<List<bool>>());
    }

    [Run]
    public void Bool_07_CollectionNullable()
    {
      List<bool?> v = null;
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableBoolCollection<List<bool?>>());
    }

    [Run]
    public void Bool_08_CollectionNullable()
    {
      List<bool?> v = new List<bool?> { true, null, false, false, null, true };
      testCollection(v, w => w.WriteCollection(v), r => r.ReadNullableBoolCollection<List<bool?>>());
    }

    [Run]
    public void Bool_09_Array()
    {
      bool[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadBoolArray());
    }

    [Run]
    public void Bool_10_Array()
    {
      bool[] v = new bool[] { true, false, true, true };
      testArray(v, w => w.Write(v), r => r.ReadBoolArray());
    }

    [Run]
    public void Bool_11_ArrayNullable()
    {
      bool?[] v = null;
      testArray(v, w => w.Write(v), r => r.ReadNullableBoolArray());
    }

    [Run]
    public void Bool_12_ArrayNullable()
    {
      bool?[] v = new bool?[] { true, null, false, false };
      testArray(v, w => w.Write(v), r => r.ReadNullableBoolArray());
    }
    #endregion




  }
}
