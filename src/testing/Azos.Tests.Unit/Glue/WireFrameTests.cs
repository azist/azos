/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Scripting;
using Azos.Glue.Native;
using System.IO;
using Azos.Glue;
using Azos.IO;

namespace Azos.Tests.Unit.Glue
{
    [Runnable(TRUN.BASE)]
    public class WireFrameTests
    {
        [Run]
        public void Glue_SerializeDeserialize()
        {
            var frm1 = new WireFrame(123, false, FID.Generate());

            Aver.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();

            Aver.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);

            Aver.IsTrue( frm1.Type == frm2.Type );
            Aver.AreEqual( frm1.RequestID, frm2.RequestID );
            Aver.AreEqual( frm1.OneWay, frm2.OneWay );

            Aver.AreEqual( frm1.Length, frm2.Length );
            Aver.AreEqual( frm1.Format, frm2.Format );
            Aver.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Aver.IsFalse( frm2.OneWay );

        }

        [Run]
        public void Glue_SerializeDeserialize_WithHeadersWithLatinText()
        {
            var hdr = "<a><remote name='zzz'/></a>";//Latin only chars

            var frm1 = new WireFrame(123, false, FID.Generate(), hdr);


            var utfLen = WireFrame.HEADERS_ENCODING.GetByteCount( hdr );

            Aver.IsTrue( utfLen == hdr.Length);

            Aver.AreEqual( WireFrame.FRAME_LENGTH + hdr.Length, frm1.Length );

            var ms = new MemoryStream();

            Aver.AreEqual(WireFrame.FRAME_LENGTH + hdr.Length, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);

            Aver.IsTrue( frm1.Type == frm2.Type );
            Aver.AreEqual( frm1.RequestID, frm2.RequestID );
            Aver.AreEqual( frm1.OneWay, frm2.OneWay );

            Aver.AreEqual( frm1.Length, frm2.Length );
            Aver.AreEqual( frm1.Format, frm2.Format );
            Aver.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Aver.IsFalse( frm2.OneWay );

            Aver.AreEqual( "zzz", frm2.Headers["remote"].AttrByName("name").Value);
        }

        [Run]
        public void Glue_SerializeDeserialize_WithHeadersWithChineseText()
        {
            var hdr = "<a><remote name='久有归天愿'/></a>";

            var frm1 = new WireFrame(123, false, FID.Generate(), hdr);


            var utfLen = WireFrame.HEADERS_ENCODING.GetByteCount( hdr );

            Aver.IsTrue( utfLen > hdr.Length);
            Console.WriteLine("{0} has {1} byte len and {2} char len".Args(hdr, utfLen, hdr.Length) );

            Aver.AreEqual( WireFrame.FRAME_LENGTH + utfLen, frm1.Length );

            var ms = new MemoryStream();

            Aver.AreEqual(WireFrame.FRAME_LENGTH + utfLen, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);

            Aver.IsTrue( frm1.Type == frm2.Type );
            Aver.AreEqual( frm1.RequestID, frm2.RequestID );
            Aver.AreEqual( frm1.OneWay, frm2.OneWay );

            Aver.AreEqual( frm1.Length, frm2.Length );
            Aver.AreEqual( frm1.Format, frm2.Format );
            Aver.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Aver.IsFalse( frm2.OneWay );

            Aver.AreEqual( "久有归天愿", frm2.Headers["remote"].AttrByName("name").Value);
        }



        [Run]
        public void Echo_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.Echo, 123, true, FID.Generate());
            Aver.IsFalse( frm1.OneWay );
            Aver.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();

            Aver.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);

            Aver.IsTrue( frm1.Type == frm2.Type );
            Aver.AreEqual( frm1.RequestID, frm2.RequestID );
            Aver.AreEqual( frm1.OneWay, frm2.OneWay );

            Aver.AreEqual( frm1.Length, frm2.Length );
            Aver.AreEqual( frm1.Format, frm2.Format );
            Aver.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Aver.IsFalse( frm2.OneWay );
        }

        [Run]
        public void Dummy_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.Dummy, 123, false, FID.Generate());
            Aver.IsTrue( frm1.OneWay );
            Aver.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();

            Aver.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);

            Aver.IsTrue( frm1.Type == frm2.Type );
            Aver.AreEqual( frm1.RequestID, frm2.RequestID );
            Aver.AreEqual( frm1.OneWay, frm2.OneWay );

            Aver.AreEqual( frm1.Length, frm2.Length );
            Aver.AreEqual( frm1.Format, frm2.Format );
            Aver.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Aver.IsTrue( frm2.OneWay );
        }

        [Run]
        public void EchoResponse_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.EchoResponse, 123, false, FID.Generate());
            Aver.IsTrue( frm1.OneWay );
            Aver.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();

            Aver.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);

            Aver.IsTrue( frm1.Type == frm2.Type );
            Aver.AreEqual( frm1.RequestID, frm2.RequestID );
            Aver.AreEqual( frm1.OneWay, frm2.OneWay );

            Aver.AreEqual( frm1.Length, frm2.Length );
            Aver.AreEqual( frm1.Format, frm2.Format );
            Aver.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Aver.IsTrue( frm2.OneWay );
        }

        [Run]
        public void HeartBeat_SerializeDeserialize()
        {
            var frm1 = new WireFrame(FrameType.Heartbeat, 123, false, FID.Generate());
            Aver.IsTrue( frm1.OneWay );
            Aver.AreEqual( WireFrame.FRAME_LENGTH, frm1.Length );

            var ms = new MemoryStream();

            Aver.AreEqual(WireFrame.FRAME_LENGTH, frm1.Serialize(ms));

            ms.Position = 0;

            var frm2 = new WireFrame(ms);

            Aver.IsTrue( frm1.Type == frm2.Type );
            Aver.AreEqual( frm1.RequestID, frm2.RequestID );
            Aver.AreEqual( frm1.OneWay, frm2.OneWay );

            Aver.AreEqual( frm1.Length, frm2.Length );
            Aver.AreEqual( frm1.Format, frm2.Format );
            Aver.AreEqual( frm1.HeadersContent, frm2.HeadersContent );

            Aver.IsTrue( frm2.OneWay );
        }


    }
}
