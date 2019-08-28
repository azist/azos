/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub
{

    [Runnable]
    public class StringValueConversionTests
    {
        [Run]
        public void GDID()
        {
           var link1 = new ELink(new GDID(5,1,2));
           var link2 = new ELink(new GDID(0,1,2));


           Aver.AreEqual( new GDID(5, 1, 2),  "5:1:2".AsGDID());
           Aver.AreObjectsEqual( new GDID(5, 1, 2),  "5:1:2".AsType(typeof(GDID)));
           Aver.AreObjectsEqual( new GDID(5, 1, 2),  link1.Link.AsType(typeof(GDID), false));


           Aver.AreEqual( new GDID(0, 1, 2),  "0:1:2".AsGDID());
           Aver.AreObjectsEqual( new GDID(0, 1, 2),  "0:1:2".AsType(typeof(GDID)));
           Aver.AreObjectsEqual( new GDID(0, 1, 2),  link2.Link.AsType(typeof(GDID), false));

           string ns = null;
           Aver.IsNull( ns.AsNullableGDID());
           Aver.IsNull( ns.AsNullableGDID(new GDID(7, 8, 9)));
           Aver.AreEqual( new GDID(7,8,9), "dewsdfwefwerf".AsNullableGDID(new GDID(7, 8, 9)));
        }

        [Run]
        public void GDIDSymbol()
        {
           var link1 = new ELink(new GDID(5,1,2));
           var link2 = new ELink(new GDID(0,1,2));

           Aver.AreEqual( new GDIDSymbol(new GDID(5, 1, 2), "5:1:2"),  "5:1:2".AsGDIDSymbol());
           Aver.AreObjectsEqual( new GDIDSymbol(new GDID(5, 1, 2), "5:1:2"),  "5:1:2".AsType(typeof(GDIDSymbol)));
           Aver.AreObjectsEqual( link1.AsGDIDSymbol, link1.Link.AsType(typeof(GDIDSymbol), false));


           Aver.AreEqual( new GDIDSymbol(new GDID(0, 1, 2), "0:1:2"),  "0:1:2".AsGDIDSymbol());
           Aver.AreObjectsEqual( new GDIDSymbol(new GDID(0, 1, 2), "0:1:2"),  "0:1:2".AsType(typeof(GDIDSymbol)));
           Aver.AreObjectsEqual( link2.AsGDIDSymbol, link2.Link.AsType(typeof(GDIDSymbol), false));

           string ns = null;
           Aver.IsNull( ns.AsNullableGDIDSymbol());
           Aver.IsNull( ns.AsNullableGDIDSymbol(new GDIDSymbol(new GDID(7, 8, 9), "AAA")));
           Aver.AreEqual( new GDIDSymbol(new GDID(7,8,9), "AAA"), "wdef8we9f9u8".AsNullableGDIDSymbol(new GDIDSymbol(new GDID(7, 8, 9), "AAA")));
        }

        [Run]
        public void GDIDSymbol_from_string()
        {
           //1280:2:8902382::'AUCKIR-ABASVITILT'
           var slink = "AUCKIR-ABASVITILT";
           var gs = slink.AsGDIDSymbol();
           Aver.AreEqual(new GDID(1280, 2, 8902382), gs.GDID);
           Aver.AreEqual(slink, gs.Symbol);

           var gs2 = slink.AsType(typeof(GDIDSymbol?), false);
           Aver.AreEqual(new GDID(1280, 2, 8902382), ((GDIDSymbol)gs2).GDID);
           Aver.AreEqual(slink, ((GDIDSymbol)gs2).Symbol);
        }

         [Run]
        public void GUID()
        {
           Aver.AreEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "CF04F818-6194-48C3-B618-8965ACA4D229".AsGUID(Guid.Empty));
           Aver.AreEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "{CF04F818-6194-48C3-B618-8965ACA4D229}".AsGUID(Guid.Empty));
           Aver.AreEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "CF04F818619448C3B6188965ACA4D229".AsGUID(Guid.Empty));
           Aver.AreObjectsEqual( new Guid("CF04F818619448C3B6188965ACA4D229"),  "CF04F818-6194-48C3-B618-8965ACA4D229".AsType(typeof(Guid)));
        }

        [Run]
        public void ByteArray()
        {
           Aver.IsTrue( new byte[]{0x2f, 0x3d, 0xea, 0x22}.SequenceEqual("2f,3d,ea,22".AsByteArray()));
           Aver.IsTrue( new byte[]{1,2,3,4,5,6,7,8,9,0}.SequenceEqual("1,2,3,4, 5,    6,7,8,9, 0".AsByteArray()));
           Aver.IsTrue( new byte[]{0xFA, 0x12, 0b1010}.SequenceEqual("0xfa,0x12,0b1010".AsByteArray() ) );
           Aver.IsTrue( new byte[]{0xFA, 0x12, 0b1010}.SequenceEqual((byte[])"0xfa,0x12,0b1010".AsType(typeof(byte[])) ));

           //base 64 encoding
           Aver.AreArraysEquivalent(new byte[]{0x19, 0xff, 0x10, 0x50, 0xa0, 0xbb}, "base64:Gf8QUKC7".AsByteArray());
        }

        [Run]
        public void IntArray()
        {
          Aver.IsTrue(new int[]{1,2,3,0b10,0xFAcaca,0xbb, -1_666_123_000}.SequenceEqual(  "1,2, 3,0b10,0xFACACA,0xBB,-1666123000".AsIntArray() ) );
          Aver.IsTrue(new int[]{1,2,3,0b10,0xFAcaca,0xbb, -1_666_123_000}.SequenceEqual(  (int[])"1,2, 3,0b10,0xFACACA,0xBB,-1666123000".AsType(typeof(int[])) ) );
        }

        [Run]
        public void LongArray()
        {
          Aver.IsTrue(new long[]{1,2,3,0b10,0xFAcaca,0xbb, -9_666_123_000}.SequenceEqual( "1,2, 3,0b10,0xFACACA,0xBB,-9666123000".AsLongArray() ) );
          Aver.IsTrue(new long[]{1,2,3,0b10,0xFAcaca,0xbb, -9_666_123_000}.SequenceEqual( (long[])"1,2, 3,0b10,0xFACACA,0xBB,-9666123000".AsType(typeof(long[])) ) );
        }

        [Run]
        public void FloatArray()
        {
          Aver.IsTrue(new float[]{1,2,3,-5.6f, 7e2f}.SequenceEqual( "1,2, 3, -5.6,7e2".AsFloatArray() ) );
          Aver.IsTrue(new float[]{1,2,3,-5.6f, 7e2f}.SequenceEqual( (float[])"1,2, 3, -5.6,7e2".AsType(typeof(float[])) ) );
        }

        [Run]
        public void DoubleArray()
        {
          Aver.IsTrue(new double[]{1,2,3,-5.6d, 7e2d}.SequenceEqual(  "1,2, 3, -5.6,7e2".AsDoubleArray() ) );
          Aver.IsTrue(new double[]{1,2,3,-5.6d, 7e2d}.SequenceEqual(  (double[])"1,2, 3, -5.6,7e2".AsType(typeof(double[])) ) );
        }

        [Run]
        public void DecimalArray()
        {
         Aver.IsTrue(new decimal[]{1,2,3,180780.23M, -99.71M}.SequenceEqual( "1,2, 3, 180780.23, -99.71".AsDecimalArray() ) );
         Aver.IsTrue(new decimal[]{1,2,3,180780.23M, -99.71M}.SequenceEqual( (decimal[])"1,2, 3, 180780.23, -99.71".AsType(typeof(decimal[])) ) );
        }


        [Run]
        public void Int()
        {
           Aver.AreEqual( 10,  "10".AsShort());
           Aver.AreEqual( 10,  "10".AsUShort());
           Aver.AreEqual( 10,  "10".AsInt());
           Aver.AreEqual( 10ul,  "10".AsUInt());
           Aver.AreEqual( 10,  "10".AsLong());
           Aver.AreEqual( 10ul,  "10".AsULong());
           Aver.AreEqual( 10f, "10".AsFloat());
           Aver.AreEqual( 10d, "10".AsDouble());
           Aver.AreEqual( 10m, "10".AsDecimal());

           Aver.AreObjectsEqual( (short)10,  "10".AsType(typeof(short)));
           Aver.AreObjectsEqual( (ushort)10,  "10".AsType(typeof(ushort)));
           Aver.AreObjectsEqual( (int)10,  "10".AsType(typeof(int)));
           Aver.AreObjectsEqual( (uint)10,  "10".AsType(typeof(uint)));
           Aver.AreObjectsEqual( (long)10,  "10".AsType(typeof(long)));
           Aver.AreObjectsEqual( (ulong)10,  "10".AsType(typeof(ulong)));
           Aver.AreObjectsEqual( 10f, "10".AsType(typeof(float)));
           Aver.AreObjectsEqual( 10d, "10".AsType(typeof(double)));
           Aver.AreObjectsEqual( 10m, "10".AsType(typeof(decimal)));
        }

        [Run]
        public void Int_Negative()
        {
           Aver.AreEqual( -10,  "-10".AsShort());
           Aver.AreEqual( 0,  "-10".AsUShort());
           Aver.AreEqual( -10,  "-10".AsInt());
           Aver.AreEqual( 0ul,  "-10".AsUInt());
           Aver.AreEqual( -10,  "-10".AsLong());
           Aver.AreEqual( 0ul,  "-10".AsULong());
           Aver.AreEqual( -10f, "-10".AsFloat());
           Aver.AreEqual( -10d, "-10".AsDouble());
           Aver.AreEqual( -10m, "-10".AsDecimal());

           Aver.AreObjectsEqual( (short)-10,  "-10".AsType(typeof(short)));
           Aver.AreObjectsEqual( (int)-10,  "-10".AsType(typeof(int)));
           Aver.AreObjectsEqual( -10f, "-10".AsType(typeof(float)));
           Aver.AreObjectsEqual( -10d, "-10".AsType(typeof(double)));
           Aver.AreObjectsEqual( -10m, "-10".AsType(typeof(decimal)));
        }

        [Run]
        public void Bool()
        {
           Aver.AreEqual( true, "1".AsBool());
           Aver.AreEqual( true, "yes".AsBool());
           Aver.AreEqual( true, "YES".AsBool());
           Aver.AreEqual( true, "true".AsBool());
           Aver.AreEqual( true, "True".AsBool());
           Aver.AreEqual( true, "TRUE".AsBool());
           Aver.AreEqual( true, "on".AsBool());
           Aver.AreEqual( true, "ON".AsBool());
        }

        [Run]
        public void Double()
        {
           Aver.AreEqual( 10f, "10.0".AsFloat());
           Aver.AreEqual( 10d, "10.0".AsDouble());
           Aver.AreEqual( 10m, "10.0".AsDecimal());

           Aver.AreObjectsEqual( 10f, "10.0".AsType(typeof(float)));
           Aver.AreObjectsEqual( 10d, "10.0".AsType(typeof(double)));
           Aver.AreObjectsEqual( 10m, "10.0".AsType(typeof(decimal)));
        }

        [Run]
        public void Double_Negative()
        {
           Aver.AreEqual( -10f, "-10.0".AsFloat());
           Aver.AreEqual( -10d, "-10.0".AsDouble());
           Aver.AreEqual( -10m, "-10.0".AsDecimal());

           Aver.AreObjectsEqual( -10f, "-10.0".AsType(typeof(float)));
           Aver.AreObjectsEqual( -10d, "-10.0".AsType(typeof(double)));
           Aver.AreObjectsEqual( -10m, "-10.0".AsType(typeof(decimal)));
        }


        [Run]
        public void DateTime()
        {
           Aver.AreEqual( new DateTime(1953, 12, 10, 14, 0, 0, DateTimeKind.Local), "12/10/1953 14:00:00".AsDateTime());
           Aver.AreEqual( new DateTime(1953, 12, 10, 14, 0, 0, DateTimeKind.Local), "Dec 10 1953 14:00:00".AsDateTime());
        }

        [Run]
        public void ISODateTime()
        {
           Aver.AreEqual( new DateTime(1953, 12, 10, 1,  0, 0, DateTimeKind.Utc), "1953-12-10T03:00:00+02:00".AsDateTime().ToUniversalTime());
        }


        [Run]
        public void Defaults_BadData()
        {
           string data = "dsifj f80q9w8";

           Aver.AreEqual( 0,  data.AsShort());
           Aver.AreEqual( 0,  data.AsInt());
           Aver.AreEqual( 0f, data.AsFloat());
           Aver.AreEqual( 0d, data.AsDouble());
           Aver.AreEqual( 0m, data.AsDecimal());

           Aver.AreObjectsEqual( (short)0,  data.AsType(typeof(short),   false));
           Aver.AreObjectsEqual( (int)0,  data.AsType(typeof(int),     false));
           Aver.AreObjectsEqual( 0f, data.AsType(typeof(float),   false));
           Aver.AreObjectsEqual( 0d, data.AsType(typeof(double),  false));
           Aver.AreObjectsEqual( 0m, data.AsType(typeof(decimal), false));

           Aver.AreEqual( 0,  data.AsNullableShort());
           Aver.AreEqual( 0,  data.AsNullableInt());
           Aver.AreEqual( 0f, data.AsNullableFloat());
           Aver.AreEqual( 0d, data.AsNullableDouble());
           Aver.AreEqual( 0m, data.AsNullableDecimal());

           Aver.AreObjectsEqual( (short)0,  data.AsType(typeof(short?),   false));
           Aver.AreObjectsEqual( (int)0,  data.AsType(typeof(int?),     false));
           Aver.AreObjectsEqual( 0f, data.AsType(typeof(float?),   false));
           Aver.AreObjectsEqual( 0d, data.AsType(typeof(double?),  false));
           Aver.AreObjectsEqual( 0m, data.AsType(typeof(decimal?), false));
        }

        [Run]
        public void Defaults_StringNull()
        {
           string data = null;
           Aver.AreEqual( 0,  data.AsShort());
           Aver.AreEqual( 0,  data.AsInt());
           Aver.AreEqual( 0f, data.AsFloat());
           Aver.AreEqual( 0d, data.AsDouble());
           Aver.AreEqual( 0m, data.AsDecimal());

           Aver.AreObjectsEqual( (short)0,  data.AsType(typeof(short),   false));
           Aver.AreObjectsEqual( (int)0,  data.AsType(typeof(int),     false));
           Aver.AreObjectsEqual( 0f, data.AsType(typeof(float),   false));
           Aver.AreObjectsEqual( 0d, data.AsType(typeof(double),  false));
           Aver.AreObjectsEqual( 0m, data.AsType(typeof(decimal), false));

           Aver.AreEqual( null, data.AsNullableShort());
           Aver.AreEqual( null, data.AsNullableInt());
           Aver.AreEqual( null, data.AsNullableFloat());
           Aver.AreEqual( null, data.AsNullableDouble());
           Aver.AreEqual( null, data.AsNullableDecimal());

           Aver.AreObjectsEqual( null, data.AsType(typeof(short?),   false));
           Aver.AreObjectsEqual( null, data.AsType(typeof(int?),     false));
           Aver.AreObjectsEqual( null, data.AsType(typeof(float?),   false));
           Aver.AreObjectsEqual( null, data.AsType(typeof(double?),  false));
           Aver.AreObjectsEqual( null, data.AsType(typeof(decimal?), false));

        }


        [Run]
        public void AsType_Speed()
        {
           var CNT = 100_000;

           var sw = System.Diagnostics.Stopwatch.StartNew();

           for(var i=0; i<CNT; i++)
           {
              "123".AsType(typeof(int), false);
              "123".AsType(typeof(int), true);
              "123".AsType(typeof(double), false);
              "123".AsType(typeof(double), true);
              "123".AsType(typeof(string), false);
              "123".AsType(typeof(bool), false);
              "123".AsType(typeof(decimal), false);
              "123".AsType(typeof(decimal), true);

              "123".AsType(typeof(int?), false);
              "123".AsType(typeof(int?), true);
              "123".AsType(typeof(double?), false);
              "123".AsType(typeof(double?), true);
              "123".AsType(typeof(bool?), false);
              "123".AsType(typeof(decimal?), false);
              "123".AsType(typeof(decimal?), true);
           }


           sw.Stop();
           Console.WriteLine("Did {0:n0} in {1}ms at {2:n0}/sec".Args(CNT, sw.ElapsedMilliseconds, (int)(CNT / (sw.ElapsedMilliseconds/1000d))));
           Aver.IsTrue( sw.ElapsedMilliseconds < 3000); //400ms on core i7 4 Ghz; this test ensures no major stall is introduced with new code
        }


        [Run]
        public void HexNumbers()
        {
            Aver.AreEqual(0xed, "0xed".AsByte());
            Aver.AreEqual(0xed, "0xEd".AsByte());
            Aver.AreEqual(0xed, "0xED".AsByte());

            Aver.AreEqual(0x10ba, "0x10BA".AsShort());
            Aver.AreEqual(0x10ba, "0x10BA".AsUShort());

            Aver.AreEqual(0xdd10BA, "0xdd10BA".AsInt());
            Aver.AreEqual(0xdd10BAul, "0xdd10BA".AsUInt());

            Aver.AreEqual(0x40001020f0a0f0ba, "0x40001020f0a0f0ba".AsLong());
            Aver.AreEqual(0x40001020f0a0f0baul, "0x40001020f0a0f0ba".AsULong());

            Aver.AreEqual(0xA0001020f0a0f0ba, "0xA0001020f0a0f0ba".AsULong());
        }

        [Run]
        public void BinNumbers()
        {
            Aver.AreEqual(0xA2, "0b10100010".AsByte());

            Aver.AreEqual(0xA2A2, "0b1010001010100010".AsUShort());

            Aver.AreEqual(0xaa55aa55, "0b10101010010101011010101001010101".AsUInt());

            Aver.AreEqual(0xaa55aa55aa55aa55, "0b1010101001010101101010100101010110101010010101011010101001010101".AsULong());

        }


        [Run]
        public void Uri()
        {
          Aver.AreObjectsEqual(new Uri("https://example.org/absolute/URI/resource.txt"), "https://example.org/absolute/URI/resource.txt".AsUri());
          Aver.AreObjectsEqual(new Uri("ftp://example.org/resource.txt"), "ftp://example.org/resource.txt".AsUri());
          Aver.AreObjectsEqual(new Uri("urn:ISSN:1535�3613"), "urn:ISSN:1535�3613".AsType(typeof(Uri)));
          Aver.IsNull("   ".AsType(typeof(Uri)));

          Aver.Throws<UriFormatException>(() => "resource.txt".AsUri(handling: ConvertErrorHandling.Throw));

          var uri = "schema://username:password@example.com:123/path/data?key=value#fragid".AsUri();
          Aver.AreEqual(uri.Scheme, "schema");
          Aver.AreEqual(uri.Authority, "example.com:123");
          Aver.AreEqual(uri.UserInfo, "username:password");
          Aver.AreEqual(uri.Host, "example.com");
          Aver.AreEqual(uri.Port, 123);
          Aver.AreEqual(uri.PathAndQuery, "/path/data?key=value");
          Aver.AreEqual(uri.AbsolutePath, "/path/data");
          Aver.AreEqual(uri.Query, "?key=value");
          Aver.AreEqual(uri.Fragment, "#fragid");
        }
    }
}
