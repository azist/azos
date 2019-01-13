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
 }
".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

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
        Aver.IsTrue(new byte[]{0xFA, 0xCA, 0xDA, 0xBA}.SequenceEqual(  root.AttrByName("vBuffer1").ValueAsByteArray() ) );
    }


    [Run]
    public void ByteArray2()
    {
      Aver.IsTrue(new byte[]{0xFA, 0x12, 0b1010}.SequenceEqual(  root.AttrByName("vBuffer2").ValueAsByteArray() ) );
    }

    [Run]
    public void IntArray()
    {
      Aver.IsTrue(new int[]{1,2,3,0b10,0xFAcaca,0xbb, -1_666_123_000}.SequenceEqual(  root.AttrByName("vIntArray").ValueAsIntArray() ) );
    }

    [Run]
    public void LongArray()
    {
      Aver.IsTrue(new long[]{1,2,3,0b10,0xFAcaca,0xbb, -9_666_123_000}.SequenceEqual(  root.AttrByName("vLongArray").ValueAsLongArray() ) );
    }

    [Run]
    public void FloatArray()
    {
      Aver.IsTrue(new float[]{1,2,3,-5.6f, 7e2f}.SequenceEqual(  root.AttrByName("vFloatArray").ValueAsFloatArray() ) );
    }

    [Run]
    public void DoubleArray()
    {
      Aver.IsTrue(new double[]{1,2,3,-5.6d, 7e2d}.SequenceEqual(  root.AttrByName("vDoubleArray").ValueAsDoubleArray() ) );
    }

    [Run]
    public void DecimalArray()
    {
      Aver.IsTrue(new decimal[]{1,2,3,180780.23M, -99.71M}.SequenceEqual(  root.AttrByName("vDecimalArray").ValueAsDecimalArray() ) );
    }

   }//class

}
