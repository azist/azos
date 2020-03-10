using Azos.Scripting;
using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Time;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class TypedDocFieldGetSetTests
  {
    [Run]
    public void String1()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["String1"], null);
      Aver.IsNull(sut.String1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["String1"], "abra");
      Aver.AreEqual("abra", sut.String1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["String1"], null);
      Aver.IsNull(sut.String1);
      sut["String1"] = "yaga";
      Aver.AreEqual("yaga", sut.String1);
    }

    [Run]
    public void Int1()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Int1"], null);
      Aver.AreEqual(0, sut.Int1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Int1"], -1234);
      Aver.AreEqual(-1234, sut.Int1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Int1"], null);
      Aver.AreEqual(0, sut.Int1);
      sut["Int1"] = null;
      Aver.AreEqual(0, sut.Int1);
      sut["Int1"] = 890;
      Aver.AreEqual(890, sut.Int1);
    }

    [Run]
    public void Int2()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Int2"], null);
      Aver.AreEqual(null, sut.Int2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Int2"], -12345);
      Aver.AreEqual(-12345, sut.Int2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Int2"], null);
      Aver.AreEqual(null, sut.Int2);
      sut["Int2"] = null;
      Aver.AreEqual(null, sut.Int2);
      sut["Int2"] = 1890;
      Aver.AreEqual(1890, sut.Int2);
    }

    [Run]
    public void Gdid1()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Gdid1"], null);
      Aver.AreEqual(GDID.ZERO, sut.Gdid1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Gdid1"], new GDID(1,1));
      Aver.AreEqual(new GDID(1,1), sut.Gdid1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Gdid1"], null);
      Aver.AreEqual(GDID.ZERO, sut.Gdid1);
      sut["Gdid1"] = null;
      Aver.AreEqual(GDID.ZERO, sut.Gdid1);
      sut["Gdid1"] = new GDID(1, 234);
      Aver.AreEqual(new GDID(1,234), sut.Gdid1);

      sut["Gdid1"] = 1237890;
      Aver.AreEqual(new GDID(0, 1237890), sut.Gdid1);
    }

    [Run]
    public void Gdid2()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Gdid2"], null);
      Aver.AreEqual(null, sut.Gdid2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Gdid2"], new GDID(1, 1));
      Aver.AreEqual(new GDID(1, 1), sut.Gdid2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Gdid2"], null);
      Aver.AreEqual(null, sut.Gdid2);
      sut["Gdid2"] = null;
      Aver.AreEqual(null, sut.Gdid2);
      sut["Gdid2"] = new GDID(1, 1234);
      Aver.AreEqual(new GDID(1, 1234), sut.Gdid2);

      sut["Gdid2"] = 7890;
      Aver.AreEqual(new GDID(0, 7890), sut.Gdid2);
    }


    [Run]
    public void MyEnum1()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["MyEnum1"], null);
      Aver.IsTrue( MyEnum.A == sut.MyEnum1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["MyEnum1"], MyEnum.C);
      Aver.IsTrue(MyEnum.C == sut.MyEnum1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["MyEnum1"], null);
      Aver.IsTrue(MyEnum.A == sut.MyEnum1);
      sut["MyEnum1"] = null;
      Aver.IsTrue(MyEnum.A == sut.MyEnum1);
      sut["MyEnum1"] = MyEnum.B;
      Aver.IsTrue(MyEnum.B == sut.MyEnum1);
    }

    [Run]
    public void MyEnum2()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["MyEnum2"], null);
      Aver.IsTrue(null == sut.MyEnum2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["MyEnum2"], MyEnum.C);
      Aver.IsTrue(MyEnum.C == sut.MyEnum2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["MyEnum2"], null);
      Aver.IsTrue(null == sut.MyEnum2);
      sut["MyEnum2"] = null;
      Aver.IsTrue(null == sut.MyEnum2);
      sut["MyEnum2"] = MyEnum.B;
      Aver.IsTrue(MyEnum.B == sut.MyEnum2);
    }


    [Run]
    public void IntArray1()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["IntArray1"], null);
      Aver.IsNull(sut.IntArray1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["IntArray1"], new int[]{1,2,3});
      Aver.AreArraysEquivalent(new int[] { 1, 2, 3 }, sut.IntArray1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["IntArray1"], null);
      Aver.IsNull(sut.IntArray1);
      sut["IntArray1"] = null;
      Aver.IsNull(sut.IntArray1);
      sut["IntArray1"] = new int[] { 2, 1, 2, 3 };
      Aver.AreArraysEquivalent(new int[] { 2, 1, 2, 3 }, sut.IntArray1);
    }

    [Run]
    public void IntArray2()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["IntArray2"], null);
      Aver.IsNull(sut.IntArray2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["IntArray2"], new int?[] { 1, null, 7, 1 });
      Aver.AreArraysEquivalent(new int?[] { 1, null, 7, 1 }, sut.IntArray2);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["IntArray2"], null);
      Aver.IsNull(sut.IntArray2);
      sut["IntArray2"] = null;
      Aver.IsNull(sut.IntArray2);
      sut["IntArray2"] = new int?[] { 2, 1, 2, null, null, -5};
      Aver.AreArraysEquivalent(new int?[] { 2, 1, 2, null, null, -5 }, sut.IntArray2);
    }


    [Run]
    public void Typed1()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Typed1"], null);
      Aver.IsNull(sut.Typed1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Typed1"], new Typed{String1="abc"});
      Aver.AreEqual("abc", sut.Typed1.String1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["Typed1"], null);
      Aver.IsNull(sut.Typed1);
      sut["Typed1"] = null;
      Aver.IsNull(sut.Typed1);
      sut["Typed1"] = new Typed { String1 = "def" };
      Aver.AreEqual("def", sut.Typed1.String1);
    }

    [Run]
    public void TypedArray1()
    {
      var sut = new Typed();
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["TypedArray1"], null);
      Aver.IsNull(sut.Typed1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["TypedArray1"], new []{new Typed { String1 = "abc111" }});
      Aver.AreEqual("abc111", sut.TypedArray1[0].String1);
      sut.SetFieldValue(Schema.GetForTypedDoc<Typed>()["TypedArray1"], null);
      Aver.IsNull(sut.TypedArray1);
      sut["TypedArray1"] = null;
      Aver.IsNull(sut.TypedArray1);
      sut["TypedArray1"] = new[]{new Typed { String1 = "def321" }};
      Aver.AreEqual("def321", sut.TypedArray1[0].String1);
    }

    private const int CNT = 1_000_000;

    [Run]
    public void BenchmarkString()
    {
      var sut = new Typed();
      var fd = sut.Schema["String1"];

      var time = Timeter.StartNew();
      for(var i=0; i<CNT; i++)
      {
        sut.SetFieldValue(fd, null);
        sut.SetFieldValue(fd, "abc");
      }
      time.Stop();
      "Speed: {0:n0} ops/sec".SeeArgs(CNT / time.ElapsedSec);
    }

    [Run]
    public void BenchmarkInt()
    {
      var sut = new Typed();
      var fd = sut.Schema["Int1"];

      var time = Timeter.StartNew();
      for (var i = 0; i < CNT; i++)
      {
        sut.SetFieldValue(fd, null);
        sut.SetFieldValue(fd, 123);
      }
      time.Stop();
      "Speed: {0:n0} ops/sec".SeeArgs(CNT / time.ElapsedSec);
    }

    [Run]
    public void BenchmarkIntNullable()
    {
      var sut = new Typed();
      var fd = sut.Schema["Int2"];

      var time = Timeter.StartNew();
      for (var i = 0; i < CNT; i++)
      {
        sut.SetFieldValue(fd, null);
        sut.SetFieldValue(fd, 123);
      }
      time.Stop();
      "Speed: {0:n0} ops/sec".SeeArgs(CNT / time.ElapsedSec);
    }

    public enum MyEnum{ A = 0 ,B,C}

    public class Typed : TypedDoc
    {
      [Field] public string String1{ get; set;}
      [Field] public int Int1 { get; set; }
      [Field] public int? Int2 { get; set; }

      [Field] public MyEnum MyEnum1 { get; set; }
      [Field] public MyEnum? MyEnum2 { get; set; }

      [Field] public GDID Gdid1 { get; set; }
      [Field] public GDID? Gdid2 { get; set; }

      [Field] public int[] IntArray1 { get; set; }
      [Field] public int?[] IntArray2 { get; set; }

      [Field] public Typed Typed1 {get; set; }
      [Field] public Typed[] TypedArray1 {get; set; }
    }

  }
}
