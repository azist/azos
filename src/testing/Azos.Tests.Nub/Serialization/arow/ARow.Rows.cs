/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Data;
using Azos.Financial;
using Azos.Pile;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;


namespace Azos.Tests.Nub.Serialization
{
  [Bix("AEAD3688-5660-4555-8379-3B6C222719A5")]
  public class SimplePersonRow : TypedDoc
  {
    [Field(backendName: "id",   isArow: true)]public GDID ID{get; set;}
    [Field(backendName: "name", isArow: true)]public string Name{get; set;}
    [Field(backendName: "age",  isArow: true)]public int Age{ get;set;}
    [Field(backendName: "b1",   isArow: true)]public bool Bool1{ get;set;}
    [Field(backendName: "s1",   isArow: true)]public string Str1{ get;set;}
    [Field(backendName: "s2",   isArow: true)]public string Str2{ get;set;}
    [Field(backendName: "d1",   isArow: true)]public DateTime Date{ get;set;}
    [Field(backendName: "slr",  isArow: true)]public double Salary{ get;set;}
  }

  [Bix("3FA492CC-F5B6-4767-BE38-59482CF5155B")]
  public class SimplePersonWithEnumRow : SimplePersonRow
  {
    public enum MaritalStatus{Single, Married, Divorced, Alien}

    [Field(backendName: "mar",   isArow: true)]public MaritalStatus Married{get; set;}
  }

  [Bix("B5DFC2E6-29AC-470F-A49D-800A297DAC5E")]
  public class SimplePersonWithByteArrayRow : SimplePersonRow
  {
    [Field(backendName: "buf", isArow: true)] public byte[] Buffer { get; set; }
  }


  [Bix("0C0E89B9-3A01-46FD-89B2-9A7A807E0E35")]
  public class FamilyRow : TypedDoc
  {
    [Field(backendName: "id",   isArow: true)] public GDID ID{get; set;}
    [Field(backendName: "fam",  isArow: true)] public string Name{get; set;}
    [Field(backendName: "reg",  isArow: true)] public bool RegisteredToVote{ get;set;}

    [Field(backendName: "fat", isArow: true)]public SimplePersonRow Father{get; set;}
    [Field(backendName: "mot", isArow: true)]public SimplePersonRow Mother{get; set;}

    [Field(backendName: "bro", isArow: true)]public SimplePersonRow[] Brothers{get; set;}
    [Field(backendName: "sis", isArow: true)]public SimplePersonRow[] Sisters{get; set;}
    [Field(backendName: "adv", isArow: true)]public List<SimplePersonRow> Advisers{get; set;}
  }


  [Bix("8BB9F822-D704-4C0A-910C-EC67422D3B03")]
  public class AllArowTypesRow : TypedDoc
  {
    [Field(backendName: "bool1",   isArow: true)]public bool       Bool1{get; set;}
    [Field(backendName: "bool2",   isArow: true)]public bool?      Bool2{get; set;}
    [Field(backendName: "bool3",   isArow: true)]public bool[]     Bool3{get; set;}
    [Field(backendName: "bool4",   isArow: true)]public List<bool> Bool4{get; set;}

    [Field(backendName: "char1",   isArow: true)]public      char     Char1{get; set;}
    [Field(backendName: "char2",   isArow: true)]public      char?    Char2{get; set;}
    [Field(backendName: "char3",   isArow: true)]public      char[]   Char3{get; set;}
    [Field(backendName: "char4",   isArow: true)]public List<char>    Char4{get; set;}
    [Field(backendName: "str1",   isArow: true)]public      string     String1{get; set;}
   // [Field(backendName: "str2",   isArow: true)]public      string?    String2{get; set;}
    [Field(backendName: "str3",   isArow: true)]public      string[]   String3{get; set;}
    [Field(backendName: "str4",   isArow: true)]public List<string>    String4{get; set;}

    [Field(backendName: "sngl1",   isArow: true)]public      float     Float1{get; set;}
    [Field(backendName: "sngl2",   isArow: true)]public      float?    Float2{get; set;}
    [Field(backendName: "sngl3",   isArow: true)]public      float[]   Float3{get; set;}
    [Field(backendName: "sngl4",   isArow: true)]public List<float>    Float4{get; set;}

    [Field(backendName: "dbl1",   isArow: true)]public      double     Double1{get; set;}
    [Field(backendName: "dbl2",   isArow: true)]public      double?    Double2{get; set;}
    [Field(backendName: "dbl3",   isArow: true)]public      double[]   Double3{get; set;}
    [Field(backendName: "dbl4",   isArow: true)]public List<double>    Double4{get; set;}

    [Field(backendName: "dcml1",   isArow: true)]public      decimal     Decimal1{get; set;}
    [Field(backendName: "dcml2",   isArow: true)]public      decimal?    Decimal2{get; set;}
    [Field(backendName: "dcml3",   isArow: true)]public      decimal[]   Decimal3{get; set;}
    [Field(backendName: "dcml4",   isArow: true)]public List<decimal>    Decimal4{get; set;}

    [Field(backendName: "amt1",   isArow: true)]public      Amount     Amount1{get; set;}
    [Field(backendName: "amt2",   isArow: true)]public      Amount?    Amount2{get; set;}
    [Field(backendName: "amt3",   isArow: true)]public      Amount[]   Amount3{get; set;}
    [Field(backendName: "amt4",   isArow: true)]public List<Amount>    Amount4{get; set;}

    [Field(backendName: "byte1",   isArow: true)]public      byte     Byte1{get; set;}
    [Field(backendName: "byte2",   isArow: true)]public      byte?    Byte2{get; set;}
    [Field(backendName: "byte3",   isArow: true)]public      byte[]   Byte3{get; set;}
    [Field(backendName: "byte4",   isArow: true)]public List<byte>    Byte4{get; set;}

    [Field(backendName: "sbyte1",   isArow: true)]public      sbyte     SByte1{get; set;}
    [Field(backendName: "sbyte2",   isArow: true)]public      sbyte?    SByte2{get; set;}
    [Field(backendName: "sbyte3",   isArow: true)]public      sbyte[]   SByte3{get; set;}
    [Field(backendName: "sbyte4",   isArow: true)]public List<sbyte>    SByte4{get; set;}

    [Field(backendName: "short1",   isArow: true)]public      short     Short1{get; set;}
    [Field(backendName: "short2",   isArow: true)]public      short?    Short2{get; set;}
    [Field(backendName: "short3",   isArow: true)]public      short[]   Short3{get; set;}
    [Field(backendName: "short4",   isArow: true)]public List<short>    Short4{get; set;}

    [Field(backendName: "ushort1",   isArow: true)]public      ushort     UShort1{get; set;}
    [Field(backendName: "ushort2",   isArow: true)]public      ushort?    UShort2{get; set;}
    [Field(backendName: "ushort3",   isArow: true)]public      ushort[]   UShort3{get; set;}
    [Field(backendName: "ushort4",   isArow: true)]public List<ushort>    UShort4{get; set;}

    [Field(backendName: "int1",   isArow: true)]public int       Int1{get; set;}
    [Field(backendName: "int2",   isArow: true)]public int?      Int2{get; set;}
    [Field(backendName: "int3",   isArow: true)]public int[]     Int3{get; set;}
    [Field(backendName: "int4",   isArow: true)]public List<int> Int4{get; set;}

    [Field(backendName: "uint1",   isArow: true)]public uint       UInt1{get; set;}
    [Field(backendName: "uint2",   isArow: true)]public uint?      UInt2{get; set;}
    [Field(backendName: "uint3",   isArow: true)]public uint[]     UInt3{get; set;}
    [Field(backendName: "uint4",   isArow: true)]public List<uint> UInt4{get; set;}

    [Field(backendName: "long1",   isArow: true)]public      long     Long1{get; set;}
    [Field(backendName: "long2",   isArow: true)]public      long?    Long2{get; set;}
    [Field(backendName: "long3",   isArow: true)]public      long[]   Long3{get; set;}
    [Field(backendName: "long4",   isArow: true)]public List<long>    Long4{get; set;}

    [Field(backendName: "ulong1",   isArow: true)]public      ulong     ULong1{get; set;}
    [Field(backendName: "ulong2",   isArow: true)]public      ulong?    ULong2{get; set;}
    [Field(backendName: "ulong3",   isArow: true)]public      ulong[]   ULong3{get; set;}
    [Field(backendName: "ulong4",   isArow: true)]public List<ulong>    ULong4{get; set;}

    [Field(backendName: "date1",   isArow: true)]public      DateTime     DateTime1{get; set;}
    [Field(backendName: "date2",   isArow: true)]public      DateTime?    DateTime2{get; set;}
    [Field(backendName: "date3",   isArow: true)]public      DateTime[]   DateTime3{get; set;}
    [Field(backendName: "date4",   isArow: true)]public List<DateTime>    DateTime4{get; set;}

    [Field(backendName: "ts1",   isArow: true)]public      TimeSpan     TimeSpan1{get; set;}
    [Field(backendName: "ts2",   isArow: true)]public      TimeSpan?    TimeSpan2{get; set;}
    [Field(backendName: "ts3",   isArow: true)]public      TimeSpan[]   TimeSpan3{get; set;}
    [Field(backendName: "ts4",   isArow: true)]public List<TimeSpan>    TimeSpan4{get; set;}

    [Field(backendName: "guid1",   isArow: true)]public Guid Guid1{get; set;}
    [Field(backendName: "guid2",   isArow: true)]public Guid? Guid2{get; set;}
    [Field(backendName: "guid3",   isArow: true)]public Guid[] Guid3{get; set;}
    [Field(backendName: "guid4",   isArow: true)]public List<Guid> Guid4{get; set;}

    [Field(backendName: "gdid1",   isArow: true)]public GDID GDID1{get; set;}
    [Field(backendName: "gdid2",   isArow: true)]public GDID? GDID2{get; set;}
    [Field(backendName: "gdid3",   isArow: true)]public GDID[] GDID3{get; set;}
    [Field(backendName: "gdid4",   isArow: true)]public List<GDID> GDID4{get; set;}

    [Field(backendName: "fid1",   isArow: true)]public FID Fid1{get; set;}
    [Field(backendName: "fid2",   isArow: true)]public FID? Fid2{get; set;}
    [Field(backendName: "fid3",   isArow: true)]public FID[] Fid3{get; set;}
    [Field(backendName: "fid4",   isArow: true)]public List<FID> Fid4{get; set;}

    [Field(backendName: "pp1",   isArow: true)]public      PilePointer     PilePointer1{get; set;}
    [Field(backendName: "pp2",   isArow: true)]public      PilePointer?    PilePointer2{get; set;}
    [Field(backendName: "pp3",   isArow: true)]public      PilePointer[]   PilePointer3{get; set;}
    [Field(backendName: "pp4",   isArow: true)]public List<PilePointer>    PilePointer4{get; set;}

    [Field(backendName: "nls1",   isArow: true)]public      NLSMap     NLS1{get; set;}
    [Field(backendName: "nls2",   isArow: true)]public      NLSMap?    NLS2{get; set;}
    [Field(backendName: "nls3",   isArow: true)]public      NLSMap[]   NLS3{get; set;}
    [Field(backendName: "nls4",   isArow: true)]public List<NLSMap>    NLS4{get; set;}

    [Field(backendName: "rarr",   isArow: true)]public    AllArowTypesRow[]       RowArray{get; set;}
    [Field(backendName: "rlst",   isArow: true)]public    List<AllArowTypesRow>   RowList{get; set;}

    public bool AllFieldsEqual(AllArowTypesRow other)
    {
      foreach(var def in this.Schema)
      {
        var v1 = this.GetFieldValue(def);
        var v2 = other.GetFieldValue(def);
        if (v1==null)
        {
          if (v2==null) continue;
          else return false;
        }

        if (v1 is IEnumerable)
        {
          var ev1 = (IEnumerable)v1;
          var ev2 = (IEnumerable)v2;

          var e1 = ev1.GetEnumerator();
          var e2 = ev2.GetEnumerator();

          while(true)
          {
            var has1 = e1.MoveNext();
            var has2 = e2.MoveNext();
            if (has1!=has2) return false;
            if (!has1) break;

            var cv1 = e1.Current;
            var cv2 = e2.Current;
            if (cv1==null)
            {
              if (cv2==null) continue;
              else return false;
            }

            if (!cv1.Equals(cv2)) return false;
          }
          continue;
        }

        if (!v1.Equals(v2)) return false;
      }

      return true;
    }

  }


  [Bix("B44FADC1-CB11-44B7-9AA0-C90BA9643783")]
  public class Ver1Row : AmorphousTypedDoc
  {
    [Field(backendName: "a", isArow: true)]public string A {get; set;}
    [Field(backendName: "b", isArow: true)]public int    B {get; set;}
    [Field(backendName: "c", isArow: true)]public int?   C { get;set;}
    [Field(backendName: "d", isArow: true)]public bool?  D { get;set;}
    [Field(backendName: "e", isArow: true)]public byte[] E{ get;set;}
    [Field(backendName: "f", isArow: true)]public DateTime F{ get;set;}
    [Field(backendName: "g", isArow: true)]public List<Ver1Row> G{ get;set;}
  }

  [Bix("14573619-2F21-4027-B479-2BB8C9A770FB")]
  public class Ver2Row : AmorphousTypedDoc
  {
    [Field(backendName: "a", isArow: true)]public string  A {get; set;}
    [Field(backendName: "be", isArow: true)]public int    B {get; set;}
    [Field(backendName: "ce", isArow: true)]public int?   C { get;set;}
    [Field(backendName: "d", isArow: true)]public bool    D { get;set;}
    [Field(backendName: "e", isArow: true)]public char[] E{ get;set;}
    [Field(backendName: "f", isArow: true)]public DateTime? F{ get;set;}
    [Field(backendName: "ge", isArow: true)]public List<Ver2Row> G{ get;set;}

    public override bool AmorphousDataEnabled { get { return true; } }
  }

}
