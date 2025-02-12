///////*<FILE_LICENSE>
////// * Azos (A to Z Application Operating System) Framework
////// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
////// * See the LICENSE file in the project root for more information.
//////</FILE_LICENSE>*/

//////using System;
//////using System.Linq;
//////using System.Collections.Generic;

//////using Azos.Financial;
//////using Azos.Data;
//////using Azos.Serialization.Bix;
//////using Azos.Serialization.JSON;
//////using Azos.Collections;
//////using Azos.Pile;

//////namespace Azos.Tests.Nub.Serialization
//////{

//////  public enum CustomEnumType { A=0, B=1, C=100, DElement=200 }

//////  /// <summary>
//////  /// Covers all primitive and intrinsic types
//////  /// </summary>
//////  [Arow]
//////  public class AllTypesDoc : AmorphousTypedDoc
//////  {
//////    [Field, Field(isArow: true, backendName: "enum")] public CustomEnumType Enum { get; set; }
//////    [Field, Field(isArow: true, backendName: "enumn")] public CustomEnumType? EnumN { get; set; }
//////    [Field, Field(isArow: true, backendName: "enuma")] public CustomEnumType[] EnumArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "enuml")] public List<CustomEnumType> EnumList { get; set; }
//////    [Field, Field(isArow: true, backendName: "enumna")] public CustomEnumType?[] EnumNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "enumnl")] public List<CustomEnumType?> EnumNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "doc")]  public AllTypesDoc Doc { get; set; }
//////    [Field, Field(isArow: true, backendName: "doca")] public AllTypesDoc[] DocArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "docl")] public List<AllTypesDoc> DocList { get; set; }

//////    [Field, Field(isArow: true, backendName: "gdid")]   public GDID Gdid {get; set;}
//////    [Field, Field(isArow: true, backendName: "gdidn")]  public GDID? GdidN { get; set; }
//////    [Field, Field(isArow: true, backendName: "gdida")]  public GDID[] GdidArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "gdidl")]  public List<GDID> GdidList { get; set; }
//////    [Field, Field(isArow: true, backendName: "gdidna")] public GDID?[] GdidNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "gdidnl")] public List<GDID?> GdidNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "gsym")]   public GDIDSymbol GdidSymbol { get; set; }
//////    [Field, Field(isArow: true, backendName: "gsymn")]  public GDIDSymbol? GdidSymbolN { get; set; }
//////    [Field, Field(isArow: true, backendName: "gsyma")]  public GDIDSymbol[] GdidSymbolArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "gsyml")]  public List<GDIDSymbol> GdidSymbolList { get; set; }
//////    [Field, Field(isArow: true, backendName: "gsymna")] public GDIDSymbol?[] GdidSymbolNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "gsymnl")] public List<GDIDSymbol?> GdidSymbolNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "guid")]   public Guid Guid { get; set; }
//////    [Field, Field(isArow: true, backendName: "guidn")]  public Guid? GuidN { get; set; }
//////    [Field, Field(isArow: true, backendName: "guida")]  public Guid[] GuidArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "guidl")]  public List<Guid> GuidList { get; set; }
//////    [Field, Field(isArow: true, backendName: "guidna")] public Guid?[] GuidNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "guidnl")] public List<Guid?> GuidNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "atm")]   public Atom Atom { get; set; }
//////    [Field, Field(isArow: true, backendName: "atmn")]  public Atom? AtomN { get; set; }
//////    [Field, Field(isArow: true, backendName: "atma")]  public Atom[] AtomArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "atml")]  public List<Atom> AtomList { get; set; }
//////    [Field, Field(isArow: true, backendName: "atmna")] public Atom?[] AtomNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "atmnl")] public List<Atom?> AtomNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "fid")]   public FID Fid { get; set; }
//////    [Field, Field(isArow: true, backendName: "fidn")]  public FID? FidN { get; set; }
//////    [Field, Field(isArow: true, backendName: "fida")]  public FID[] FidArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "fidl")]  public List<FID> FidList { get; set; }
//////    [Field, Field(isArow: true, backendName: "fidna")] public FID?[] FidNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "fidnl")] public List<FID?> FidNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "ptr")]   public PilePointer Pileptr { get; set; }  //ptr
//////    [Field, Field(isArow: true, backendName: "ptrn")]  public PilePointer? PileptrN { get; set; }
//////    [Field, Field(isArow: true, backendName: "ptra")]  public PilePointer[] PileptrArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "ptrl")]  public List<PilePointer> PileptrList { get; set; }
//////    [Field, Field(isArow: true, backendName: "ptrna")] public PilePointer?[] PileptrNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "ptrnl")] public List<PilePointer?> PileptrNList { get; set; }


//////    [Field, Field(isArow: true, backendName: "nls")]   public NLSMap NLSMap { get; set; }  //nls
//////    [Field, Field(isArow: true, backendName: "nlsn")]  public NLSMap? NLSMapN { get; set; }
//////    [Field, Field(isArow: true, backendName: "nlsa")]  public NLSMap[] NLSMapArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "nlsl")]  public List<NLSMap> NLSMapList { get; set; }
//////    [Field, Field(isArow: true, backendName: "nlsna")] public NLSMap?[] NLSMapNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "nlsnl")] public List<NLSMap?> NLSMapNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "amt")]   public Amount Amount { get; set; }  //amt
//////    [Field, Field(isArow: true, backendName: "amtn")]  public Amount? AmountN { get; set; }
//////    [Field, Field(isArow: true, backendName: "amta")]  public Amount[] AmountArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "amtl")]  public List<Amount> AmountList { get; set; }
//////    [Field, Field(isArow: true, backendName: "amtna")] public Amount?[] AmountNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "amtnl")] public List<Amount?> AmountNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "smap")]  public StringMap StringMap { get; set; }  //smap
//////    [Field, Field(isArow: true, backendName: "amapa")] public StringMap[] StringMapArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "amapl")] public List<StringMap> StringMapList { get; set; }

//////    [Field, Field(isArow: true, backendName: "str")]   public string String { get; set; }  //str
//////    [Field, Field(isArow: true, backendName: "stra")]  public string[] StringArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "strl")]  public List<string> StringList { get; set; }

//////    [Field, Field(isArow: true, backendName: "int")]   public int Int { get; set; }
//////    [Field, Field(isArow: true, backendName: "intn")]  public int? IntN { get; set; }
//////    [Field, Field(isArow: true, backendName: "inta")]  public int[] IntArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "intl")]  public List<int> IntList { get; set; }
//////    [Field, Field(isArow: true, backendName: "intna")] public int?[] IntNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "intnl")] public List<int?> IntNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "uint")]   public uint UInt { get; set; }
//////    [Field, Field(isArow: true, backendName: "uintn")]  public uint? UIntN { get; set; }
//////    [Field, Field(isArow: true, backendName: "uinta")]  public uint[] UIntArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "uintl")]  public List<uint> UIntList { get; set; }
//////    [Field, Field(isArow: true, backendName: "uintna")] public uint?[] UIntNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "uintnl")] public List<uint?> UIntNList { get; set; }

//////    //-------------------------------------------------------

//////    [Field, Field(isArow: true, backendName: "lng")]    public long Long { get; set; }
//////    [Field, Field(isArow: true, backendName: "lngn")]   public long? LongN { get; set; }
//////    [Field, Field(isArow: true, backendName: "lnga")]   public long[] LongArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "lngl")]   public List<long> LongList { get; set; }
//////    [Field, Field(isArow: true, backendName: "lngna")]  public long?[] LongNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "lngnl")]  public List<long?> LongNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "ulng")]   public ulong ULong { get; set; }
//////    [Field, Field(isArow: true, backendName: "ulngn")]  public ulong? ULongN { get; set; }
//////    [Field, Field(isArow: true, backendName: "ulnga")]  public ulong[] ULongArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "ulngl")]  public List<ulong> ULongList { get; set; }
//////    [Field, Field(isArow: true, backendName: "ulngna")] public ulong?[] ULongNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "ulngnl")] public List<ulong?> ULongNList { get; set; }

//////    //----

//////    [Field, Field(isArow: true, backendName: "shrt")]   public short Short { get; set; }
//////    [Field, Field(isArow: true, backendName: "shrtn")]  public short? ShortN { get; set; }
//////    [Field, Field(isArow: true, backendName: "shrta")]  public short[] ShortArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "shrtl")]  public List<short> ShortList { get; set; }
//////    [Field, Field(isArow: true, backendName: "shrtna")] public short?[] ShortNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "shrtnl")] public List<short?> ShortNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "ushrt")]   public ushort UShort { get; set; }
//////    [Field, Field(isArow: true, backendName: "ushrtn")]  public ushort? UShortN { get; set; }
//////    [Field, Field(isArow: true, backendName: "ushrta")]  public ushort[] UShortArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "ushrtl")]  public List<ushort> UShortList { get; set; }
//////    [Field, Field(isArow: true, backendName: "ushrtna")] public ushort?[] UShortNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "ushrtnl")] public List<ushort?> UShortNList { get; set; }


//////    [Field, Field(isArow: true, backendName: "byte")]   public byte Byte { get; set; }
//////    [Field, Field(isArow: true, backendName: "byten")]  public byte? ByteN { get; set; }
//////    [Field, Field(isArow: true, backendName: "bytea")]  public byte[] ByteArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "bytel")]  public List<byte> ByteList { get; set; }
//////    [Field, Field(isArow: true, backendName: "bytena")] public byte?[] ByteNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "bytenl")] public List<byte?> ByteNList { get; set; }


//////    [Field, Field(isArow: true, backendName: "sbyte")]   public sbyte Sbyte { get; set; }
//////    [Field, Field(isArow: true, backendName: "sbyten")]  public sbyte? SbyteN { get; set; }
//////    [Field, Field(isArow: true, backendName: "sbytea")]  public sbyte[] SbyteArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "sbytel")]  public List<sbyte> SbyteList { get; set; }
//////    [Field, Field(isArow: true, backendName: "sbytena")] public sbyte?[] SbyteNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "sbytenl")] public List<sbyte?> SbyteNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "char")]    public char Char { get; set; }
//////    [Field, Field(isArow: true, backendName: "charn")]   public char? CharN { get; set; }
//////    [Field, Field(isArow: true, backendName: "chara")]   public char[] CharArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "charl")]   public List<char> CharList { get; set; }
//////    [Field, Field(isArow: true, backendName: "charna")]  public char?[] CharNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "charnl")]  public List<char?> CharNList { get; set; }


//////    [Field, Field(isArow: true, backendName: "bl")]     public bool Bool { get; set; }
//////    [Field, Field(isArow: true, backendName: "bln")]    public bool? BoolN { get; set; }
//////    [Field, Field(isArow: true, backendName: "bla")]    public bool[] BoolArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "bll")]    public List<bool> BoolList { get; set; }
//////    [Field, Field(isArow: true, backendName: "blna")]   public bool?[] BoolNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "blnl")]   public List<bool?> BoolNList { get; set; }


//////    [Field, Field(isArow: true, backendName: "flt")]   public float Float { get; set; } //flt
//////    [Field, Field(isArow: true, backendName: "fltn")]  public float? FloatN { get; set; }
//////    [Field, Field(isArow: true, backendName: "flta")]  public float[] FloatArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "fltl")]  public List<float> FloatList { get; set; }
//////    [Field, Field(isArow: true, backendName: "fltna")] public float?[] FloatNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "fltnl")] public List<float?> FloatNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "dbl")]   public double Double { get; set; } //dbl
//////    [Field, Field(isArow: true, backendName: "dbln")]  public double? DoubleN { get; set; }
//////    [Field, Field(isArow: true, backendName: "dbla")]  public double[] DoubleArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "dbll")]  public List<double> DoubleList { get; set; }
//////    [Field, Field(isArow: true, backendName: "dblna")] public double?[] DoubleNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "dblnl")] public List<double?> DoubleNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "dec")]   public decimal Decimal { get; set; } //dec
//////    [Field, Field(isArow: true, backendName: "decn")]  public decimal? DecimalN { get; set; }
//////    [Field, Field(isArow: true, backendName: "deca")]  public decimal[] DecimalArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "decl")]  public List<decimal> DecimalList { get; set; }
//////    [Field, Field(isArow: true, backendName: "decna")] public decimal?[] DecimalNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "decnl")] public List<decimal?> DecimalNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "ts")]   public TimeSpan Timespan { get; set; } //ts
//////    [Field, Field(isArow: true, backendName: "tsn")]  public TimeSpan? TimespanN { get; set; }
//////    [Field, Field(isArow: true, backendName: "tsa")]  public TimeSpan[] TimespanArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "tsl")]  public List<TimeSpan> TimespanList { get; set; }
//////    [Field, Field(isArow: true, backendName: "tsna")] public TimeSpan?[] TimespanNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "tsnl")] public List<TimeSpan?> TimespanNList { get; set; }

//////    [Field, Field(isArow: true, backendName: "dt")]   public DateTime Datetime { get; set; } //dt
//////    [Field, Field(isArow: true, backendName: "dtn")]  public DateTime? DatetimeN { get; set; }
//////    [Field, Field(isArow: true, backendName: "dta")]  public DateTime[] DatetimeArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "dtl")]  public List<DateTime> DatetimeList { get; set; }
//////    [Field, Field(isArow: true, backendName: "dtna")] public DateTime?[] DatetimeNArray { get; set; }
//////    [Field, Field(isArow: true, backendName: "dtnl")] public List<DateTime?> DatetimeNList { get; set; }


//////    public void AverEquality(AllTypesDoc another)
//////    {
//////      foreach(var fdef in Schema)
//////      {
//////        var v1 = this[fdef.Order];
//////        var v2 = another[fdef.Order];

//////        if (v1 is Array a)
//////         Aver.AreArraysEquivalent(a, v2 as Array);
//////        if (v1 is IEnumerable<object> eo)
//////         Aver.IsTrue( eo.SequenceEqual(v2 as IEnumerable<object>));
//////        else
//////         Aver.AreObjectsEqual(v1, v2);
//////      }
//////    }

//////    public AllTypesDoc Populate(bool withDoc = true)
//////    {
//////      Enum = CustomEnumType.DElement;
//////      EnumN = CustomEnumType.C;
//////      EnumArray = new CustomEnumType[] { CustomEnumType.A, CustomEnumType.B };
//////      EnumList = new List<CustomEnumType> { CustomEnumType.C, CustomEnumType.DElement };
//////      EnumNArray = new CustomEnumType?[] { null, null, CustomEnumType.B };
//////      EnumNList = new List<CustomEnumType?> { CustomEnumType.C, null, null, CustomEnumType.DElement };

//////      if (withDoc)
//////      {
//////        Doc = new AllTypesDoc().Populate(false);
//////        DocArray = new AllTypesDoc[]{ null, null, new AllTypesDoc().Populate(false) };
//////        DocList  = new List<AllTypesDoc> { new AllTypesDoc().Populate(false) };
//////      }

//////      Gdid = new GDID(12, 23423);
//////      GdidN = new GDID(24,243534);
//////      GdidArray = new GDID[]{ new GDID(122, 2342543) , new GDID(122, 233423) , new GDID(12, 232423) };
//////      GdidList = new List<GDID> { new GDID(122, 2342543), new GDID(122, 233423), new GDID(12, 232423) };
//////      GdidNArray = new GDID?[] { new GDID(122, 2342543), null, new GDID(12, 232423) };
//////      GdidNList = new List<GDID?> { null, new GDID(122, 233423), new GDID(12, 232423) };


//////      GdidSymbol = new GDIDSymbol(new GDID(12, 23423), "fsdfsd");
//////      GdidSymbolN = new GDIDSymbol(new GDID(312, 323423), "jezl kulaz");
//////      GdidSymbolArray = new GDIDSymbol[]{ new GDIDSymbol(new GDID(1, 53), "fs44dfsd"), new GDIDSymbol(new GDID(2, 923), "liopa") };
//////      GdidSymbolList  = new List<GDIDSymbol> { new GDIDSymbol(new GDID(1, 53), "fs44dfsd"), new GDIDSymbol(new GDID(2, 923), "liopa") };
//////      GdidSymbolNArray = new GDIDSymbol?[] { null, new GDIDSymbol(new GDID(2, 923), "liopa") };
//////      GdidSymbolNList = new List<GDIDSymbol?> { new GDIDSymbol(new GDID(1, 53), "fs44dfsd"), null };

//////      Guid = System.Guid.NewGuid();
//////      GuidN = System.Guid.NewGuid();
//////      GuidArray = new Guid[] { System.Guid.NewGuid(), System.Guid.NewGuid() };
//////      GuidList = new List<Guid> { System.Guid.NewGuid(), System.Guid.NewGuid(), System.Guid.NewGuid() };
//////      GuidNArray = new Guid?[] { null, System.Guid.NewGuid() };
//////      GuidNList = new List<Guid?> { System.Guid.NewGuid(), null };


//////      Atom = Atom.Encode("a");
//////      AtomN = Atom.Encode("ae3");
//////      AtomArray = new Atom[] { Atom.Encode("a2e3"), Atom.Encode("a1e3") };
//////      AtomList = new List<Atom> { Atom.Encode("a3"), Atom.Encode("23"), Atom.Encode("r4") };
//////      AtomNArray = new Atom?[] { null, Atom.Encode("a3") };
//////      AtomNList = new List<Atom?> { Atom.Encode("2a3"), null };


//////      Fid = new FID(23);
//////      FidN = new FID(323);
//////      FidArray = new FID[] { new FID(23), new FID(123) };
//////      FidList = new List<FID> { new FID(233), new FID(23), new FID(23) };
//////      FidNArray = new FID?[] { null, new FID(223) };
//////      FidNList = new List<FID?> { new FID(423), null };

//////      Pileptr = new PilePointer(1, 23);
//////      PileptrN = new PilePointer(1, 23);
//////      PileptrArray = new PilePointer[] { new PilePointer(1, 243), new PilePointer(1, 223) };
//////      PileptrList = new List<PilePointer> { new PilePointer(21, 23), new PilePointer(1, 23), new PilePointer(1, 263) };
//////      PileptrNArray = new PilePointer?[] { null, new PilePointer(1, 293) };
//////      PileptrNList = new List<PilePointer?> { new PilePointer(1, 223), null };


//////      NLSMap = new NLSMap("{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke',d: 'Es ist grün'}}");
//////      NLSMapN = new NLSMap("{eng: {n: 'Cur',d: 'Is'}}");
//////      NLSMapArray = new NLSMap[] { new NLSMap("{eng: {n: 'Cewqeur',d: 'Is'}}"), new NLSMap("{eng: {n: 'Cur23',d: 'Isee'}}") };
//////      NLSMapList = new List<NLSMap> { new NLSMap("{eng: {n: 'Cr',d: 'Is'}}"), new NLSMap("{eng: {n: 'Currr',d: 'Iws'}}"), new NLSMap("{eng: {n: 'ertCur',d: 'rtIs'}}") };
//////      NLSMapNArray = new NLSMap?[] { null, new NLSMap("{eng: {n: 'Cweur',d: 'Is'}}") };
//////      NLSMapNList = new List<NLSMap?> { new NLSMap("{eng: {n: 'Cuer',d: 'Is'}}"), null };

//////      Amount = new Amount("usd", 123.11m);
//////      AmountN = new Amount("usd", 223.11m);
//////      AmountArray = new Amount[] { new Amount("usd", 123.11m), new Amount("usd", 223.11m) };
//////      AmountList = new List<Amount> { new Amount("usd", 123.11m), new Amount("usd", 253.11m), new Amount("usd", 243.11m) };
//////      AmountNArray = new Amount?[] { null, new Amount("usd", 323.11m) };
//////      AmountNList = new List<Amount?> { new Amount("usd", 523.11m), null };

//////      StringMap = new StringMap{ {"a", "aaa" }, { "b", "bbb" } };
//////      StringMapArray = new StringMap[]{ new StringMap { { "a", "aaa" }, { "b", "bbb" } }, null, new StringMap { { "23a", "23423weaaa" } } };
//////      StringMapList = new List<StringMap> { new StringMap { { "a", "aaa" }, { "b", "bbb" } }, null, new StringMap { { "23a", "23423weaaa" } } };

//////      this.String = "kazapzon";
//////      this.StringArray = new string[]{"mox", null, null, "zaporojets", "kefal"};
//////      this.StringList = new List<string> { "mox", null, null, "zaporojets", "kefal" };


//////      Int = -7;
//////      IntN = -789;
//////      IntArray = new int[]{3,8,9,-23, 234,0};
//////      IntList = new List<int>(){0,1,4,8};
//////      IntNArray = new int?[] { 3, null, 9, -23, 234, 0 };
//////      IntNList = new List<int?>() { 0, 1, null, 8 };

//////      UInt = 7;
//////      UIntN = 789;
//////      UIntArray = new uint[] { 3, 8, 9, 23, 234, 0 };
//////      UIntList = new List<uint>() { 0, 1, 4, 8 };
//////      UIntNArray = new uint?[] { 3, null, 9, 23, 234, 0 };
//////      UIntNList = new List<uint?>() { 0, 1, null, 8 };


//////      //-------------------------------------------------------

//////      Long = -74;
//////      LongN = -7893;
//////      LongArray = new long[] { 43, 8, 59, -243, 234, 0 };
//////      LongList = new List<long>() { 03, 1, 43, 8 };
//////      LongNArray = new long?[] { 3, null, 9, -23, 2344, 0 };
//////      LongNList = new List<long?>() { 0, 1, null, 58 };

//////      ULong = 78;
//////      ULongN = 159;
//////      ULongArray = new ulong[] { 3, 5, 19, 23, 24, 0 };
//////      ULongList = new List<ulong>() { 0, 1, 7, 9 };
//////      ULongNArray = new ulong?[] { 3, null, 9, 5, 24, 0 };
//////      ULongNList = new List<ulong?>() { 0, 7, null, 8 };

//////      Short = -4;
//////      ShortN = -73;
//////      ShortArray = new short[] { 16, 8, 59, -23, 34, 0 };
//////      ShortList = new List<short>() { 0, 3, 1, 4, 8 };
//////      ShortNArray = new short?[] { 3, null, 9, -23, 24, 0 };
//////      ShortNList = new List<short?>() { 0, 4, null, 5 };

//////      UShort = 5;
//////      UShortN = 59;
//////      UShortArray = new ushort[] { 3, 5, 67, 23, 38, 0 };
//////      UShortList = new List<ushort>() { 0, 1, 5, 9 };
//////      UShortNArray = new ushort?[] { 3, null, 9, 0, 4, 0 };
//////      UShortNList = new List<ushort?>() { 0, 7, null, 8 , 9};


//////      //----
//////      Byte  = 15;
//////      ByteN = 22;
//////      ByteArray = new byte[] { 16, 22, 59, 28, 34, 0 };
//////      ByteList = new List<byte>() { 0, 8, 1, 6, 3 };
//////      ByteNArray = new byte?[] { 3, null, 9, 15, 33, 0 };
//////      ByteNList = new List<byte?>() { 0, 4, null, 5 };

//////      Sbyte = 6;
//////      SbyteN = 97;
//////      SbyteArray = new sbyte[] { 3, 38, 45, 2, 38, 0 };
//////      SbyteList = new List<sbyte>() { 0, 1, 7, 4 };
//////      SbyteNArray = new sbyte?[] { 3, null, 2, 6, 4, 0 };
//////      SbyteNList = new List<sbyte?>() { 0, 7, null, 8, 9 };

//////      Char  = 'a';
//////      CharN = 'c';
//////      CharArray = new char[] {'b', 'c', 'f'};
//////      CharList = new List<char>() { 'a', 'd', 'e', 'h'};
//////      CharNArray = new char?[] {'v', 'r', 'u',};
//////      CharNList = new List<char?>() { 'w', 'e', 'r', 't'};

//////      Bool = true;
//////      BoolN = false;
//////      BoolArray = new bool[] { true,false,false,true };
//////      BoolList = new List<bool>() { true, true, false };
//////      BoolNArray = new bool?[] { false, false, true};
//////      BoolNList = new List<bool?>() { true, false,false};

//////      Float = -8;
//////      FloatN = -79;
//////      FloatArray = new float[] { 3.89f, 8, 9, -2332.5f, 34, 0 };
//////      FloatList = new List<float>() { 0, 1.3f, 4, 8 };
//////      FloatNArray = new float?[] { 3, null, 9, -23, 234, 0 };
//////      FloatNList = new List<float?>() { 0, 1, null, 8 };

//////      this.Double = -7;
//////      DoubleN = -89;
//////      DoubleArray = new double[] { 3.2, 8, 9, -3.93, 23, 0 };
//////      DoubleList = new List<double>() { 0.1, 1.7, 4, 8 };
//////      DoubleNArray = new double?[] { 3, null, 9, -23, 234, 0 };
//////      DoubleNList = new List<double?>() { 0, 1, null, 8 };

//////      this.Decimal = 2;
//////      DecimalN = 89;
//////      DecimalArray = new decimal[] { 7, 8, 9, 3, 234, 0 };
//////      DecimalList = new List<decimal>() { 0, 1, 4, 8 };
//////      DecimalNArray = new decimal?[] { 3, null, 9, -23, 234, 0 };
//////      DecimalNList = new List<decimal?>() { 0, 1, null, 8 };

//////      Timespan = TimeSpan.FromHours(2.45);
//////      TimespanN = TimeSpan.FromHours(8.19);
//////      TimespanArray = new TimeSpan[] { TimeSpan.FromHours(2.001), TimeSpan.FromHours(4.72) };
//////      TimespanList = new List<TimeSpan>() { TimeSpan.FromHours(12.45), TimeSpan.FromHours(11.09) };
//////      TimespanNArray = new TimeSpan?[] { TimeSpan.FromHours(2.435), null, TimeSpan.FromHours(32.45) };
//////      TimespanNList = new List<TimeSpan?>() { TimeSpan.FromHours(2.45), null, TimeSpan.FromHours(7.401) };

//////      Datetime = new DateTime(1980, 2, 3);
//////      DatetimeN = new DateTime(1980, 2, 3);
//////      DatetimeArray = new DateTime[] { new DateTime(1980, 2, 3), new DateTime(1980, 2, 3) };
//////      DatetimeList = new List<DateTime>() { new DateTime(1980, 2, 3), new DateTime(1980, 2, 3) };
//////      DatetimeNArray = new DateTime?[] { new DateTime(1980, 2, 3), null, new DateTime(1980, 2, 3) };
//////      DatetimeNList = new List<DateTime?>() { new DateTime(1980, 2, 3), null, new DateTime(1980, 2, 3) };

//////      return this;
//////    }
//////  }
//////}
