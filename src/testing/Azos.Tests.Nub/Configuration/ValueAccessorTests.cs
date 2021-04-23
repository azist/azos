/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Conf;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class ValueAccessorTests
  {

    private IConfigSectionNode root = @"
 test
 {
   vEnum1=B
   vInt1=123
   vInt2=-123
   vDouble=-123.8002341
   vDecimal=123000456.1233
   vHex=0xABAB
   vBin=0b1010101001010101 //AA55
   vBool=true
   vStr=$'My
name
 spanning many lines'
   vDate=12/10/2014
   vGuid1='{3A7C4641-B24E-453D-9D28-93D96071B575}'
   vGuid2='3A7C4641-B24E-453D-9D28-93D96071B575'
   vGuid3='3A7C4641B24E453D9D2893D96071B575'

   vBuffer1=fa,CA,dA,Ba
   vBuffer2=0xfa,0x12,0b1010

   vIntArray ='1,2, 3,0b10,0xFACACA,0xBB,-1666123000'
   vLongArray ='1,2, 3,0b10,0xFACACA,0xBB,-9666123000'
   vFloatArray ='1,2, 3, -5.6,7e2'
   vDoubleArray ='1,2, 3, -5.6,7e2'
   vDecimalArray ='1,2, 3, 180780.23, -99.71'

   vGDID1=''
   vGDID2='1:1:123'
   vGDID3='ZAVIFA-YOLO'
   vUri1=''
   vUri2='http://google.com'
   vAtom1=''
   vAtom2='abcd1234'
   vAtom3='#0x3031'//atoms get encoded from ulong if the value is prefixed with #
 }
".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

    private enum TztEnum { A, B, C }

    private class tezt
    {
      [Config] public TztEnum venum1 { get; set; }
      [Config] public int vint1 { get; set; }
      [Config] public int vint2 { get; set; }
      [Config] public double vdouble { get; set; }
      [Config] public decimal vdecimal { get; set; }
      [Config("$vHex")] public uint hex_v1 { get; set; }
      [Config("$vHex")] public ushort hex_v2 { get; set; }
      [Config("$vHex")] public ulong hex_v3 { get; set; }
      [Config("$vHex")] public int hex_v4 { get; set; }
      [Config("$vHex")] public short hex_v5 { get; set; }
      [Config("$vHex")] public long hex_v6 { get; set; }

      [Config("$vBin")] public uint bin_v1 { get; set; }
      [Config("$vBin")] public ushort bin_v2 { get; set; }
      [Config("$vBin")] public ulong bin_v3 { get; set; }
      [Config("$vBin")] public int bin_v4 { get; set; }
      [Config("$vBin")] public short bin_v5 { get; set; }
      [Config("$vBin")] public long bin_v6 { get; set; }
      [Config] public bool vbool { get; set; }

      [Config] public byte[] vbuffer1 { get; set; }
      [Config] public byte[] vbuffer2 { get; set; }


      [Config] public int[] vintarray { get; set; }
      [Config] public long[] vlongarray { get; set; }
      [Config] public float[] vfloatarray { get; set; }
      [Config] public double[] vdoublearray { get; set; }
      [Config] public decimal[] vdecimalarray { get; set; }


      [Config] public GDID vgdid1 { get; set; }
      [Config] public GDID vgdid2 { get; set; }
      [Config] public GDID vgdid3 { get; set; }

      [Config] public Uri vuri1 { get; set; }
      [Config] public Uri vuri2 { get; set; }

      [Config] public Atom vatom1 { get; set; }
      [Config] public Atom vatom2 { get; set; }
      [Config] public Atom vatom3 { get; set; }
    }



    [Run]
    public void AllUsingConfigAttr()
    {
      var obj = new tezt();
      ConfigAttribute.Apply(obj, root);

      Aver.IsTrue(TztEnum.B == obj.venum1);
      Aver.AreEqual(123, obj.vint1);
      Aver.AreEqual(-123, obj.vint2);
      Aver.AreEqual(-123.8002341d, obj.vdouble);
      Aver.AreEqual(123000456.1233m, obj.vdecimal);

      Aver.AreEqual(0xABABu, obj.hex_v1);
      Aver.AreEqual(0xABABu, obj.hex_v2);
      Aver.AreEqual(0xABABu, obj.hex_v3);
      Aver.AreEqual(0xABAB, obj.hex_v4);
      Aver.AreEqual(0xABAB, (int)(ushort)obj.hex_v5);
      Aver.AreEqual(0xABAB, obj.hex_v6);

      Aver.AreEqual(0xaa55u, obj.bin_v1);
      Aver.AreEqual(0xaa55u, obj.bin_v2);
      Aver.AreEqual(0xaa55u, obj.bin_v3);
      Aver.AreEqual(0xaa55, obj.bin_v4);
      Aver.AreEqual(0xaa55, (int)(ushort)obj.bin_v5);
      Aver.AreEqual(0xaa55, obj.bin_v6);

      Aver.AreEqual(4, obj.vbuffer1.Length);
      Aver.AreEqual(0xfa, obj.vbuffer1[0]);
      Aver.AreEqual(0xca, obj.vbuffer1[1]);
      Aver.AreEqual(0xda, obj.vbuffer1[2]);
      Aver.AreEqual(0xba, obj.vbuffer1[3]);

      Aver.AreEqual(3, obj.vbuffer2.Length);
      Aver.AreEqual(0xfa, obj.vbuffer2[0]);
      Aver.AreEqual(0x12, obj.vbuffer2[1]);
      Aver.AreEqual(0b1010, obj.vbuffer2[2]);

      Aver.AreEqual(7, obj.vintarray.Length);
      Aver.AreEqual(1, obj.vintarray[0]);
      Aver.AreEqual(-1666123000, obj.vintarray[6]);

      Aver.AreEqual(7, obj.vlongarray.Length);
      Aver.AreEqual(1L, obj.vlongarray[0]);
      Aver.AreEqual(-9666123000L, obj.vlongarray[6]);

      Aver.AreEqual(5, obj.vfloatarray.Length);
      Aver.AreEqual(-5.6f, obj.vfloatarray[3]);
      Aver.AreEqual(7e2f, obj.vfloatarray[4]);

      Aver.AreEqual(5, obj.vdoublearray.Length);
      Aver.AreEqual(-5.6d, obj.vdoublearray[3]);
      Aver.AreEqual(7e2d, obj.vdoublearray[4]);

      Aver.AreEqual(5, obj.vdecimalarray.Length);
      Aver.AreEqual(180780.23m, obj.vdecimalarray[3]);
      Aver.AreEqual(-99.71m, obj.vdecimalarray[4]);

      Aver.AreEqual(GDID.ZERO, obj.vgdid1);
      Aver.AreEqual(new GDID(1, 1, 123), obj.vgdid2);

      Aver.IsNull(obj.vuri1);
      Aver.AreEqual("http://google.com", obj.vuri2.OriginalString);

      Aver.IsTrue(obj.vatom1.IsZero);
      Aver.AreEqual("abcd1234", obj.vatom2.Value);
      Aver.AreEqual(new Atom(0x3031), obj.vatom3);
    }


    [Run]
    public void Ints()
    {
      Aver.AreEqual(123, root.AttrByName("vInt1").ValueAsInt());
      Aver.AreEqual(-123, root.AttrByName("vInt2").ValueAsInt());
    }

    [Run]
    public void Doubles()
    {
      Aver.AreEqual(-123.8002341d, root.AttrByName("vDouble").ValueAsDouble());
    }

    [Run]
    public void Decimals()
    {
      Aver.AreEqual(123000456.1233M, root.AttrByName("vDecimal").ValueAsDecimal());
    }

    [Run]
    public void HexIntegers()
    {
      Aver.AreEqual((ushort)0xabab, root.AttrByName("vHex").ValueAsUShort());
      Aver.AreEqual((uint)0xabab, root.AttrByName("vHex").ValueAsUInt());
      Aver.AreEqual((ulong)0xabab, root.AttrByName("vHex").ValueAsULong());
    }

    [Run]
    public void BinIntegers()
    {
      Aver.AreEqual((ushort)0xaa55, root.AttrByName("vBin").ValueAsUShort());
      Aver.AreEqual((uint)0xaa55, root.AttrByName("vBin").ValueAsUInt());
      Aver.AreEqual((ulong)0xaa55, root.AttrByName("vBin").ValueAsULong());
    }

    [Run]
    public void Bools()
    {
      Aver.AreEqual(true, root.AttrByName("vBool").ValueAsBool());
    }

    [Run]
    public void Strs()
    {
      Aver.AreEqual(@"My
name
 spanning many lines", root.AttrByName("vStr").ValueAsString());
    }

    [Run]
    public void Dates()
    {
      Aver.AreEqual(2014, root.AttrByName("vDate").ValueAsDateTime(DateTime.Now).Year);
      Aver.AreEqual(12, root.AttrByName("vDate").ValueAsDateTime(DateTime.Now).Month);
    }

    [Run]
    public void Guids()
    {
      Aver.AreEqual(new Guid("3A7C4641B24E453D9D2893D96071B575"), root.AttrByName("vGUID1").ValueAsGUID(Guid.Empty));
      Aver.AreEqual(new Guid("3A7C4641B24E453D9D2893D96071B575"), root.AttrByName("vGUID2").ValueAsGUID(Guid.Empty));
      Aver.AreEqual(new Guid("3A7C4641B24E453D9D2893D96071B575"), root.AttrByName("vGUID3").ValueAsGUID(Guid.Empty));
    }

    [Run]
    public void ByteArray1()
    {
      Aver.IsTrue(new byte[] { 0xFA, 0xCA, 0xDA, 0xBA }.SequenceEqual(root.AttrByName("vBuffer1").ValueAsByteArray()));
    }


    [Run]
    public void ByteArray2()
    {
      Aver.IsTrue(new byte[] { 0xFA, 0x12, 0b1010 }.SequenceEqual(root.AttrByName("vBuffer2").ValueAsByteArray()));
    }

    [Run]
    public void IntArray()
    {
      Aver.IsTrue(new int[] { 1, 2, 3, 0b10, 0xFAcaca, 0xbb, -1_666_123_000 }.SequenceEqual(root.AttrByName("vIntArray").ValueAsIntArray()));
    }

    [Run]
    public void LongArray()
    {
      Aver.IsTrue(new long[] { 1, 2, 3, 0b10, 0xFAcaca, 0xbb, -9_666_123_000 }.SequenceEqual(root.AttrByName("vLongArray").ValueAsLongArray()));
    }

    [Run]
    public void FloatArray()
    {
      Aver.IsTrue(new float[] { 1, 2, 3, -5.6f, 7e2f }.SequenceEqual(root.AttrByName("vFloatArray").ValueAsFloatArray()));
    }

    [Run]
    public void DoubleArray()
    {
      Aver.IsTrue(new double[] { 1, 2, 3, -5.6d, 7e2d }.SequenceEqual(root.AttrByName("vDoubleArray").ValueAsDoubleArray()));
    }

    [Run]
    public void DecimalArray()
    {
      Aver.IsTrue(new decimal[] { 1, 2, 3, 180780.23M, -99.71M }.SequenceEqual(root.AttrByName("vDecimalArray").ValueAsDecimalArray()));
    }


    [Run]
    public void GDIDs()
    {
      Aver.IsTrue(root.AttrByName("vGDID1").ValueAsGDID(GDID.ZERO).IsZero);

      Aver.IsFalse(root.AttrByName("vGDID1").ValueAsNullableGDID().HasValue);

      Aver.AreEqual(new GDID(1, 1, 123), root.AttrByName("vGDID2").ValueAsGDID(GDID.ZERO));
      Aver.AreEqual(new GDID(1, 1, 123), root.AttrByName("vGDID3").ValueAsGDID(GDID.ZERO));
    }

    [Run]
    public void URIs()
    {
      Aver.IsNull(root.AttrByName("vURI1").ValueAsUri(null));

      Aver.AreEqual("http://google.com/", root.AttrByName("vURI2").ValueAsUri(null).AbsoluteUri);
    }

    [Run]
    public void Atoms()
    {
      Aver.IsTrue(root.AttrByName("vAtom1").ValueAsAtom(Atom.ZERO).IsZero);
      Aver.IsFalse(root.AttrByName("vAtom1").ValueAsNullableAtom().HasValue);

      Aver.AreEqual(Atom.Encode("abcd1234"), root.AttrByName("vAtom2").ValueAsAtom(Atom.ZERO));
      Aver.AreEqual(Atom.Encode("10"), root.AttrByName("vAtom3").ValueAsAtom(Atom.ZERO));
    }

  }//class

}
