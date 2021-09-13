/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;

using Azos.Scripting;
using Azos.Serialization.Slim;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class Slim2Tests
  {
    [Run]
    public void T01_NestedClass()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C1() { FInt = 7, FC11 = new C1.C11() { FInt = -19 } };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C1)s.Deserialize(ms);

        Aver.AreEqual(7, dOut.FInt);
        Aver.AreEqual(-19, dOut.FC11.FInt);
      }
    }


    internal class C1
    {
      internal class C11
      {
        public int FInt { get; set; }
      }

      public int FInt { get; set; }

      public C11 FC11 { get; set; }
    }


    [Run]
    public void T02_FieldsWithSameInstance()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var c3 = new C3() { Fint1 = 137 };

        var dIn = new C2 { FObj1 = c3, FI11 = c3, FC31 = c3 };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C2)s.Deserialize(ms);

        Aver.AreSameRef(dOut.FObj1, dOut.FI11);
        Aver.AreSameRef(dOut.FObj1, dOut.FC31);
      }
    }


    internal class C2
    {
      public object FObj1 { get; set; }
      public I1 FI11 { get; set; }
      public C3 FC31 { get; set; }
    }


    internal class C3 : I1
    {
      public int Fint1 { get; set; }
    }


    internal interface I1
    {
      int Fint1 { get; set; }
    }


    [Run]
    public void T03_CyclicSelfReferences()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C4();
        dIn.FC4 = dIn;

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C4)s.Deserialize(ms);

        Aver.IsTrue(object.ReferenceEquals(dOut, dOut.FC4));
        Aver.IsTrue(object.ReferenceEquals(dOut.FC4, dOut.FC4.FC4));
      }
    }


    internal class C4
    {
      public C4 FC4 { get; set; }
    }


    [Run]
    public void T04_CyclicReference()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var c6 = new C6();
        var dIn = new C5() { FC6 = c6 };
        c6.FC5 = dIn;

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C5)s.Deserialize(ms);

        Aver.AreSameRef(dOut, dOut.FC6.FC5);
      }
    }


    internal class C5
    {
      public C6 FC6 { get; set; }
    }


    internal class C6
    {
      public C5 FC5 { get; set; }
    }


    [Run]
    public void T05_CyclicArraySelfReference()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C7();
        dIn.FObjs = new Dictionary<int, object> { { 17, dIn }, { 0, dIn } };
        dIn.FIC7s = new IC7[] { null, dIn };
        dIn.FC7s = (new C7[] { dIn, null }).ToList();

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C7)s.Deserialize(ms);

        Aver.IsTrue(object.ReferenceEquals(dOut, dOut.FObjs[0]));
        Aver.IsTrue(object.ReferenceEquals(dOut, dOut.FObjs[17]));
        Aver.IsTrue(object.ReferenceEquals(dOut, dOut.FIC7s[1]));
        Aver.IsTrue(object.ReferenceEquals(dOut, dOut.FC7s[0]));
      }
    }


    internal class C7 : IC7
    {
      public Dictionary<int, object> FObjs { get; set; }
      public IC7[] FIC7s { get; set; }
      public List<C7> FC7s { get; set; }
    }


    internal interface IC7
    {
      IC7[] FIC7s { get; set; }
    }


    [Run]
    public void T06_CovariantCyclicArrays()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C8();
        dIn.FObjs = new object[] { null, (object)dIn, (IC8)dIn, (C8_1)dIn, dIn };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C8)s.Deserialize(ms);

        Aver.IsNull(dOut.FObjs[0]);
        Aver.AreSameRef(dOut, dOut.FObjs[1]);
        Aver.AreSameRef(dOut, dOut.FObjs[2]);
        Aver.AreSameRef(dOut, dOut.FObjs[3]);
      }
    }


    internal class C8 : C8_1
    {
      public object[] FObjs { get; set; }
    }


    internal class C8_1 : IC8 { }


    internal interface IC8 { }


    [Run]
    public void T07_CovariantArrays()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C10() { FIC9s = new IC9[] { new C9_1(), new C9() } };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C10)s.Deserialize(ms);

        Aver.IsTrue(dOut.FIC9s[0] is C9_1);
        Aver.IsTrue(dOut.FIC9s[1] is C9);
      }
    }


    internal class C10
    {
      public IC9[] FIC9s { get; set; }
    }


    internal class C9 : C9_1 { }


    internal class C9_1 : IC9 { }


    internal interface IC9 { }


    [Run]
    public void T08_PreserveReferenceToTheSameArray()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var bytes = new byte[] { 0x07, 0x12 };
        var strings = new string[] { "Veingarten", "Vecherovskiy", "Zahar Gubar'" };
        var ints = new int[] { -100, 2345, 19044, 888889 };
        var dIn = new C11()
        {
          FBytes = bytes,
          FC11 = new C11_1() { FBytes_1 = bytes, FStrings_1 = strings, FInts_1 = ints },
          FC12 = new C11_2() { FBytes_2 = bytes, FStrings_2 = strings, FInts_2 = ints }
        };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C11)s.Deserialize(ms);

        Aver.AreSameRef(dOut.FBytes, dOut.FC11.FBytes_1);
        Aver.AreSameRef(dOut.FBytes, dOut.FC12.FBytes_2);

        Aver.AreSameRef(dOut.FC11.FStrings_1, dOut.FC12.FStrings_2);
        Aver.AreSameRef(dOut.FC11.FInts_1, dOut.FC12.FInts_2);
      }
    }

    internal class C11
    {
      public C11_1 FC11 { get; set; }
      public C11_2 FC12 { get; set; }

      public byte[] FBytes { get; set; }
    }


    internal class C11_1
    {
      public byte[] FBytes_1 { get; set; }
      public string[] FStrings_1 { get; set; }
      public int[] FInts_1 { get; set; }
    }


    internal class C11_2
    {
      public int[] FInts_2 { get; set; }
      public byte[] FBytes_2 { get; set; }
      public string[] FStrings_2 { get; set; }
    }


    [Run]
    public void T10_CovariantStructInterfaceArrays()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new C12() { FIC12s = new IC12[] { new C12_2(), new C12_1() } };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (C12)s.Deserialize(ms);

        Aver.IsTrue(dOut.FIC12s[0] is C12_2);
        Aver.IsTrue(dOut.FIC12s[1] is C12_1);
      }
    }


    internal struct C12
    {
      public IC12[] FIC12s { get; set; }
    }


    internal struct C12_1 : IC12 { }


    internal struct C12_2 : IC12 { }


    internal interface IC12 { }


    [Run]
    public void T11_DictComparer()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new Dictionary<int, int>(new Comparer1());
        dIn[1] = 100;
        dIn[11] = 200;
        dIn[2] = 300;

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (Dictionary<int, int>)s.Deserialize(ms);

        Aver.AreEqual(2, dOut.Count);

        Aver.AreEqual(200, dOut[1]);
        Aver.AreEqual(300, dOut[2]);

        dOut[1] = 100;

        Aver.AreEqual(2, dOut.Count);
        Aver.AreEqual(100, dOut[1]);
      }
    }


    internal class Comparer1 : IEqualityComparer<int>
    {
      public bool Equals(int x, int y)
      {
        return (x % 10) == (y % 10);
      }

      public int GetHashCode(int obj)
      {
        return (obj % 10).GetHashCode();
      }
    }


    [Run]
    public void T12_DictComparerComplex()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var cmpr = new Comparer1();

        var c1 = new C13();
        var c2 = new C13();

        var dict1 = new Dictionary<int, C13>(cmpr);
        dict1[1] = c1;
        dict1[2] = c1;
        dict1[3] = c2;
        var dict2 = new Dictionary<int, C13>(cmpr);
        dict2[1] = c2;
        dict2[2] = c1;

        var dIn = new List<S3>();
        dIn.Add(new S3() { FDict = dict1 });
        dIn.Add(new S3() { FDict = dict1 });
        dIn.Add(new S3() { FDict = dict2 });

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (List<S3>)s.Deserialize(ms);

        Aver.AreSameRef(dOut[0].FDict, dOut[1].FDict);
        Aver.AreNotSameRef(dOut[0].FDict, dOut[2].FDict);

        Aver.AreSameRef(dOut[0].FDict[1], dOut[0].FDict[2]);
        Aver.AreNotSameRef(dOut[0].FDict[1], dOut[0].FDict[3]);

        Aver.AreSameRef(dOut[0].FDict[1], dOut[2].FDict[2]);
        Aver.AreSameRef(dOut[0].FDict[3], dOut[2].FDict[1]);
      }
    }

    [Run]
    public void T13_Struct1_PilePointer()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new Azos.Pile.PilePointer(10, 231, 223421);

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (Azos.Pile.PilePointer)s.Deserialize(ms);

        Azos.Serialization.JSON.JsonWriter.Write(s1).See();
        Azos.Serialization.JSON.JsonWriter.Write(s2).See();

        Aver.IsTrue(s1 == s2);
      }
    }


    private struct MyStructFields
    {
      public int X;
      public int Y;
      public bool F;
    }


    private struct MyStructWithReadonlyField
    {
      public MyStructWithReadonlyField(int x, int y, bool f)
      {
        A = 890;
        X = x;
        Y = y;
        F = f;
      }

      public readonly int A;
      public int X;
      public int Y;
      public readonly bool F;
    }


    [Run]
    public void T14_Struct2()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new MyStructFields { X = 10, Y = 15, F = true };

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (MyStructFields)s.Deserialize(ms);

        Aver.AreEqual(s1.X, s2.X);
        Aver.AreEqual(s1.Y, s2.Y);
        Aver.AreEqual(s1.F, s2.F);
      }
    }

    [Run]
    public void T15_Struct3()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new MyStructWithReadonlyField(10, 15, true);

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (MyStructWithReadonlyField)s.Deserialize(ms);

        Azos.Serialization.JSON.JsonWriter.Write(s1).See();
        Azos.Serialization.JSON.JsonWriter.Write(s2).See();

        Aver.AreEqual(s1.X, s2.X);
        Aver.AreEqual(s1.Y, s2.Y);
        Aver.AreEqual(s1.F, s2.F);
      }
    }


    private class sw { public MyStructWithReadonlyField s; }


    [Run]
    public void T16_Struct4()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var s1 = new sw { s = new MyStructWithReadonlyField(10, 15, true) };

        s.Serialize(ms, s1);
        ms.Seek(0, SeekOrigin.Begin);

        var s2 = (sw)s.Deserialize(ms);

        Azos.Serialization.JSON.JsonWriter.Write(s1.s).See();
        Azos.Serialization.JSON.JsonWriter.Write(s2.s).See();

        Aver.AreEqual(s1.s.X, s2.s.X);
        Aver.AreEqual(s1.s.Y, s2.s.Y);
        Aver.AreEqual(s1.s.F, s2.s.F);
      }
    }

    [Run]
    public void T1000_CustomSerializableWithNesting()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        var o1 = new CustomSer { Data = new object[] { 1, 2, true, "ok" } };
        s.Serialize(ms, o1);

        ms.Position = 0;

        var o2 = s.Deserialize(ms) as CustomSer;

        Aver.IsNotNull(o2);

        Aver.AreEqual(4, ((object[])o2.Data).Length);
        Aver.AreObjectsEqual("ok", ((object[])o2.Data)[3]);
      }
    }

    [Run]
    public void T2000_TypeRegistry()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        var tr1 = new TypeRegistry();

        Aver.IsTrue(tr1.Add(typeof(List<S3>)));
        Aver.IsTrue(tr1.Add(typeof(S3[])));
        Aver.IsTrue(tr1.Add(typeof(Tuple<string, S3>)));

        s.Serialize(ms, tr1);

        ms.Position = 0;

        var tr2 = s.Deserialize(ms) as TypeRegistry;

        Aver.IsNotNull(tr2);

        Aver.AreEqual(7, tr2.Count);//including system types
        Aver.AreObjectsEqual(typeof(Tuple<string, S3>), tr2["$6"]);
        Aver.AreObjectsEqual(typeof(S3[]), tr2["$5"]);
        Aver.AreObjectsEqual(typeof(List<S3>), tr2["$4"]);
      }
    }

    [Run]
    public void T3000_Dictionary_TYPE_Int_Root()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        var d1 = new Dictionary<Type, int>();

        d1.Add(typeof(List<S3>), 1);
        d1.Add(typeof(S3[]), 20);
        d1.Add(typeof(Tuple<string, S3>), 90);

        s.Serialize(ms, d1);

        ms.Position = 0;

        var d2 = s.Deserialize(ms) as Dictionary<Type, int>;

        Aver.IsNotNull(d2);

        Aver.AreEqual(3, d2.Count);
        Aver.AreEqual(1, d2[typeof(List<S3>)]);
        Aver.AreEqual(20, d2[typeof(S3[])]);
        Aver.AreEqual(90, d2[typeof(Tuple<string, S3>)]);
      }
    }


    private class dictTypeClass
    {
      public List<Type> ADummy;
      public Dictionary<Type, int> Data;
    }


    [Run]
    public void T3100_Dictionary_TYPE_Int_Field()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();
        var d1 = new dictTypeClass
        {
          Data = new Dictionary<Type, int>(0xff),
          ADummy = new List<Type>(0xff)
        };

        d1.Data.Add(typeof(List<S3>), 1);
        d1.Data.Add(typeof(S3[]), 20);
        d1.Data.Add(typeof(Tuple<string, S3>), 90);

        d1.ADummy.Add(typeof(List<S3>));
        d1.ADummy.Add(typeof(S3[]));
        d1.ADummy.Add(typeof(Tuple<string, S3>));

        s.Serialize(ms, d1);

        ms.Position = 0;

        var d2 = s.Deserialize(ms) as dictTypeClass;

        Aver.IsNotNull(d2);
        Aver.IsNotNull(d2.Data);
        Aver.IsNotNull(d2.ADummy);

        Aver.AreEqual(3, d2.Data.Count);
        Aver.AreEqual(1, d2.Data[typeof(List<S3>)]);
        Aver.AreEqual(20, d2.Data[typeof(S3[])]);
        Aver.AreEqual(90, d2.Data[typeof(Tuple<string, S3>)]);

        Aver.AreEqual(3, d2.ADummy.Count);
        Aver.AreObjectsEqual(typeof(List<S3>), d2.ADummy[0]);
        Aver.AreObjectsEqual(typeof(S3[]), d2.ADummy[1]);
        Aver.AreObjectsEqual(typeof(Tuple<string, S3>), d2.ADummy[2]);
      }
    }


    internal struct S3
    {
      public Dictionary<int, C13> FDict { get; set; }
    }


    internal class C13 { }


    internal class CustomSer : ISerializable
    {
      public object Data;

      public CustomSer() { }

      public CustomSer(SerializationInfo info, StreamingContext context)
      {
        var buf = (byte[])info.GetValue("d", typeof(byte[]));
        var ms = new MemoryStream(buf);
        var s = new SlimSerializer();
        Data = s.Deserialize(ms);
      }

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        var ms = new MemoryStream();
        var s = new SlimSerializer();
        s.Serialize(ms, Data);
        info.AddValue("d", ms.GetBuffer(), typeof(byte[]));
      }
    }


  }
}
