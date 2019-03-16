/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Financial;
using Azos.Data;
using Azos.Serialization.Arow;
using Azos.Serialization.JSON;
using Azos.Collections;
using Azos.Pile;


namespace Azos.Tests.Nub.Serialization
{

  public enum CustomEnumType { A=0, B=1, C=100, DElement=200 }

  /// <summary>
  /// Covers all primitive and intrinsic types
  /// </summary>
  [Arow]
  public class AllTypesDoc : AmorphousTypedDoc
  {
    [Field, Field(isArow: true, backendName: "enum")]   public CustomEnumType Enum { get; set; }
    [Field, Field(isArow: true, backendName: "enumn")]  public CustomEnumType? EnumN { get; set; }
    [Field, Field(isArow: true, backendName: "enuma")]  public CustomEnumType[] EnumArray { get; set; }
    [Field, Field(isArow: true, backendName: "enuml")]  public List<CustomEnumType> EnumList { get; set; }
    [Field, Field(isArow: true, backendName: "enumna")] public CustomEnumType?[] EnumNArray { get; set; }
    [Field, Field(isArow: true, backendName: "enumnl")] public List<CustomEnumType?> EnumNList { get; set; }

    [Field, Field(isArow: true, backendName: "doc")]  public AllTypesDoc Doc { get; set; }
    [Field, Field(isArow: true, backendName: "doca")] public AllTypesDoc[] DocArray { get; set; }
    [Field, Field(isArow: true, backendName: "docl")] public List<AllTypesDoc> DocList { get; set; }

    [Field, Field(isArow: true, backendName: "gdid")]   public GDID Gdid {get; set;}
    [Field, Field(isArow: true, backendName: "gdidn")]  public GDID? GdidN { get; set; }
    [Field, Field(isArow: true, backendName: "gdida")]  public GDID[] GdidArray { get; set; }
    [Field, Field(isArow: true, backendName: "gdidl")]  public List<GDID> GdidList { get; set; }
    [Field, Field(isArow: true, backendName: "gdidna")] public GDID?[] GdidNArray { get; set; }
    [Field, Field(isArow: true, backendName: "gdidnl")] public List<GDID?> GdidNList { get; set; }

    [Field, Field(isArow: true, backendName: "gsym")]   public GDIDSymbol GdidSymbol { get; set; }
    [Field, Field(isArow: true, backendName: "gsymn")]  public GDIDSymbol? GdidSymbolN { get; set; }
    [Field, Field(isArow: true, backendName: "gsyma")]  public GDIDSymbol[] GdidSymbolArray { get; set; }
    [Field, Field(isArow: true, backendName: "gsyml")]  public List<GDIDSymbol> GdidSymbolList { get; set; }
    [Field, Field(isArow: true, backendName: "gsymna")] public GDIDSymbol?[] GdidSymbolNArray { get; set; }
    [Field, Field(isArow: true, backendName: "gsymnl")] public List<GDIDSymbol?> GdidSymbolNList { get; set; }

    [Field, Field(isArow: true, backendName: "guid")]   public Guid Guid { get; set; }
    [Field, Field(isArow: true, backendName: "guidn")]  public Guid? GuidN { get; set; }
    [Field, Field(isArow: true, backendName: "guida")]  public Guid[] GuidArray { get; set; }
    [Field, Field(isArow: true, backendName: "guidl")]  public List<Guid> GuidList { get; set; }
    [Field, Field(isArow: true, backendName: "guidna")] public Guid?[] GuidNArray { get; set; }
    [Field, Field(isArow: true, backendName: "guidnl")] public List<Guid?> GuidNList { get; set; }

    [Field, Field(isArow: true, backendName: "atm")]   public Atom Atom { get; set; }
    [Field, Field(isArow: true, backendName: "atmn")]  public Atom? AtomN { get; set; }
    [Field, Field(isArow: true, backendName: "atma")]  public Atom[] AtomArray { get; set; }
    [Field, Field(isArow: true, backendName: "atml")]  public List<Atom> AtomList { get; set; }
    [Field, Field(isArow: true, backendName: "atmna")] public Atom?[] AtomNArray { get; set; }
    [Field, Field(isArow: true, backendName: "atmnl")] public List<Atom?> AtomNList { get; set; }

    [Field, Field(isArow: true, backendName: "fid")]   public FID Fid { get; set; }
    [Field, Field(isArow: true, backendName: "fidn")]  public FID? FidN { get; set; }
    [Field, Field(isArow: true, backendName: "fida")]  public FID[] FidArray { get; set; }
    [Field, Field(isArow: true, backendName: "fidl")]  public List<FID> FidList { get; set; }
    [Field, Field(isArow: true, backendName: "fidna")] public FID?[] FidNArray { get; set; }
    [Field, Field(isArow: true, backendName: "fidnl")] public List<FID?> FidNList { get; set; }

    [Field, Field(isArow: true, backendName: "ptr")]   public PilePointer Pileptr { get; set; }  //ptr
    [Field, Field(isArow: true, backendName: "ptrn")]  public PilePointer? PileptrN { get; set; }
    [Field, Field(isArow: true, backendName: "ptra")]  public PilePointer[] PileptrArray { get; set; }
    [Field, Field(isArow: true, backendName: "ptrl")]  public List<PilePointer> PileptrList { get; set; }
    [Field, Field(isArow: true, backendName: "ptrna")] public PilePointer?[] PileptrNArray { get; set; }
    [Field, Field(isArow: true, backendName: "ptrnl")] public List<PilePointer?> PileptrNList { get; set; }


    [Field, Field(isArow: true, backendName: "nls")]   public NLSMap NLSMap { get; set; }  //nls
    [Field, Field(isArow: true, backendName: "nlsn")]  public NLSMap? NLSMapN { get; set; }
    [Field, Field(isArow: true, backendName: "nlsa")]  public NLSMap[] NLSMapArray { get; set; }
    [Field, Field(isArow: true, backendName: "nlsl")]  public List<NLSMap> NLSMapList { get; set; }
    [Field, Field(isArow: true, backendName: "nlsna")] public NLSMap?[] NLSMapNArray { get; set; }
    [Field, Field(isArow: true, backendName: "nlsnl")] public List<NLSMap?> NLSMapNList { get; set; }

    [Field, Field(isArow: true, backendName: "amt")]   public Amount Amount { get; set; }  //amt
    [Field, Field(isArow: true, backendName: "amtn")]  public Amount? AmountN { get; set; }
    [Field, Field(isArow: true, backendName: "amta")]  public Amount[] AmountArray { get; set; }
    [Field, Field(isArow: true, backendName: "amtl")]  public List<Amount> AmountList { get; set; }
    [Field, Field(isArow: true, backendName: "amtna")] public Amount?[] AmountNArray { get; set; }
    [Field, Field(isArow: true, backendName: "amtnl")] public List<Amount?> AmountNList { get; set; }

    [Field, Field(isArow: true, backendName: "smap")] public StringMap StringMap { get; set; }  //smap
    [Field, Field(isArow: true, backendName: "amapa")] public StringMap[] StringMapArray { get; set; }
    [Field, Field(isArow: true, backendName: "amapl")] public List<StringMap> StringMapList { get; set; }

    [Field, Field(isArow: true, backendName: "str")] public string String { get; set; }  //str
    [Field, Field(isArow: true, backendName: "stra")] public string[] StringArray { get; set; }
    [Field, Field(isArow: true, backendName: "strl")] public List<string> StringList { get; set; }

    [Field, Field(isArow: true, backendName: "int")] public int Int { get; set; }
    [Field, Field(isArow: true, backendName: "intn")] public int? IntN { get; set; }
    [Field, Field(isArow: true, backendName: "inta")] public int[] IntArray { get; set; }
    [Field, Field(isArow: true, backendName: "intl")] public List<int> IntList { get; set; }
    [Field, Field(isArow: true, backendName: "intna")] public int?[] IntNArray { get; set; }
    [Field, Field(isArow: true, backendName: "intnl")] public List<int?> IntNList { get; set; }

    [Field, Field(isArow: true, backendName: "uint")] public uint UInt { get; set; }
    [Field, Field(isArow: true, backendName: "uintn")] public uint? UIntN { get; set; }
    [Field, Field(isArow: true, backendName: "uinta")] public uint[] UIntArray { get; set; }
    [Field, Field(isArow: true, backendName: "uintl")] public List<uint> UIntList { get; set; }
    [Field, Field(isArow: true, backendName: "uintna")] public uint?[] UIntNArray { get; set; }
    [Field, Field(isArow: true, backendName: "uintnl")] public List<uint?> UIntNList { get; set; }

    //-------------------------------------------------------

    [Field, Field(isArow: true, backendName: "int")] public long Long { get; set; }
    [Field, Field(isArow: true, backendName: "int")] public long? LongN { get; set; }
    [Field, Field(isArow: true, backendName: "int")] public long[] LongArray { get; set; }
    [Field, Field(isArow: true, backendName: "int")] public List<long> LongList { get; set; }
    [Field] public long?[] LongNArray { get; set; }
    [Field] public List<long?> LongNList { get; set; }

    [Field] public ulong ULong { get; set; }
    [Field] public ulong? ULongN { get; set; }
    [Field] public ulong[] ULongArray { get; set; }
    [Field] public List<ulong> ULongList { get; set; }
    [Field] public ulong?[] ULongNArray { get; set; }
    [Field] public List<ulong?> ULongNList { get; set; }

    //----

    [Field] public short Short { get; set; }
    [Field] public short? ShortN { get; set; }
    [Field] public short[] ShortArray { get; set; }
    [Field] public List<short> ShortList { get; set; }
    [Field] public short?[] ShortNArray { get; set; }
    [Field] public List<short?> ShortNList { get; set; }

    [Field] public ushort UShort { get; set; }
    [Field] public ushort? UShortN { get; set; }
    [Field] public ushort[] UShortArray { get; set; }
    [Field] public List<ushort> UShortList { get; set; }
    [Field] public ushort?[] UShortNArray { get; set; }
    [Field] public List<ushort?> UShortNList { get; set; }


    [Field] public byte Byte { get; set; }
    [Field] public byte? ByteN { get; set; }
    [Field] public byte[] ByteArray { get; set; }
    [Field] public List<byte> ByteList { get; set; }
    [Field] public byte?[] ByteNArray { get; set; }
    [Field] public List<byte?> ByteNList { get; set; }


    [Field] public sbyte Sbyte { get; set; }
    [Field] public sbyte? SbyteN { get; set; }
    [Field] public sbyte[] SbyteArray { get; set; }
    [Field] public List<sbyte> SbyteList { get; set; }
    [Field] public sbyte?[] SbyteNArray { get; set; }
    [Field] public List<sbyte?> SbyteNList { get; set; }

    [Field] public char Char { get; set; }
    [Field] public char? CharN { get; set; }
    [Field] public char[] CharArray { get; set; }
    [Field] public List<char> CharList { get; set; }
    [Field] public char?[] CharNArray { get; set; }
    [Field] public List<char?> CharNList { get; set; }


    [Field] public bool Bool { get; set; }
    [Field] public bool? BoolN { get; set; }
    [Field] public bool[] BoolArray { get; set; }
    [Field] public List<bool> BoolList { get; set; }
    [Field] public bool?[] BoolNArray { get; set; }
    [Field] public List<bool?> BoolNList { get; set; }


    [Field] public float Float { get; set; } //flt
    [Field] public float? FloatN { get; set; }
    [Field] public float[] FloatArray { get; set; }
    [Field] public List<float> FloatList { get; set; }
    [Field] public float?[] FloatNArray { get; set; }
    [Field] public List<float?> FloatNList { get; set; }

    [Field] public double Double { get; set; } //dbl
    [Field] public double? DoubleN { get; set; }
    [Field] public double[] DoubleArray { get; set; }
    [Field] public List<double> DoubleList { get; set; }
    [Field] public double?[] DoubleNArray { get; set; }
    [Field] public List<double?> DoubleNList { get; set; }

    [Field] public decimal Decimal { get; set; } //dec
    [Field] public decimal? DecimalN { get; set; }
    [Field] public decimal[] DecimalArray { get; set; }
    [Field] public List<decimal> DecimalList { get; set; }
    [Field] public decimal?[] DecimalNArray { get; set; }
    [Field] public List<decimal?> DecimalNList { get; set; }

    [Field] public TimeSpan Timespan { get; set; } //ts
    [Field] public TimeSpan? TimespanN { get; set; }
    [Field] public TimeSpan[] TimespanArray { get; set; }
    [Field] public List<TimeSpan> TimespanList { get; set; }
    [Field] public TimeSpan?[] TimespanNArray { get; set; }
    [Field] public List<TimeSpan?> TimespanNList { get; set; }

    [Field] public DateTime Datetime { get; set; } //dt
    [Field] public DateTime? DatetimeN { get; set; }
    [Field] public DateTime[] DatetimeArray { get; set; }
    [Field] public List<DateTime> DatetimeList { get; set; }
    [Field] public DateTime?[] DatetimeNArray { get; set; }
    [Field] public List<DateTime?> DatetimeNList { get; set; }

  }
}
