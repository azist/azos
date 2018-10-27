/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Scripting;

using Azos.IO;

namespace Azos.Tests.Unit.IO
{
    [Runnable(TRUN.BASE)]
    public class LEB128Tests
    {
        [Run]
        public void Basic_ULEBEncoding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteULEB128(0, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(0, buf[0]);
          Aver.IsTrue(0 == buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(1, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(1, buf[0]);
          Aver.IsTrue(1 == buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0x7f, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(0x7f, buf[0]);
          Aver.IsTrue(0x7f == buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0x80, out cnt);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0x80, buf[0]);
          Aver.AreEqual(0x01, buf[1]);
          Aver.IsTrue(0x80 == buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0x81, out cnt);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0x81, buf[0]);
          Aver.AreEqual(0x01, buf[1]);
          Aver.IsTrue(0x81 == buf.ReadULEB128());

          buf = new byte[16];
          buf.WriteULEB128(0xff, out cnt);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0xff, buf[0]);
          Aver.AreEqual(0x01, buf[1]);
          Aver.IsTrue(0xff == buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0x101, out cnt);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0x81, buf[0]);
          Aver.AreEqual(0x02, buf[1]);
          Aver.IsTrue(0x101 == buf.ReadULEB128());

        }

        [Run]
        public void Basic_ULEBEncoding_Padding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteULEB128(0, out cnt, padding: 1);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0x80, buf[0]);
          Aver.AreEqual(0x00, buf[1]);
          Aver.IsTrue(0 == buf.ReadULEB128());


          buf = new byte[16];
          buf.WriteULEB128(0, out cnt, padding: 2);
          Aver.AreEqual(3, cnt);
          Aver.AreEqual(0x80, buf[0]);
          Aver.AreEqual(0x80, buf[1]);
          Aver.AreEqual(0x00, buf[2]);
          Aver.IsTrue(0 == buf.ReadULEB128());

          buf = new byte[16];
          buf.WriteULEB128(0x80, out cnt, padding: 2);
          Aver.AreEqual(4, cnt);
          Aver.AreEqual(0x80, buf[0]);
          Aver.AreEqual(0x81, buf[1]);
          Aver.AreEqual(0x80, buf[2]);
          Aver.AreEqual(0x00, buf[3]);
          Aver.IsTrue(0x80 == buf.ReadULEB128());
        }



        [Run]
        public void Basic_SLEBEncoding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteSLEB128(0, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(0, buf[0]);
          Aver.AreEqual(0, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(1, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(1, buf[0]);
          Aver.AreEqual(1, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(-1, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(0x7f, buf[0]);
          Aver.AreEqual(-1, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(+63, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(0x3f, buf[0]);
          Aver.AreEqual(63, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(-63, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(0x41, buf[0]);
          Aver.AreEqual(-63, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-64, out cnt);
          Aver.AreEqual(1, cnt);
          Aver.AreEqual(0x40, buf[0]);
          Aver.AreEqual(-64, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-65, out cnt);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0xbf, buf[0]);
          Aver.AreEqual(0x7f, buf[1]);
          Aver.AreEqual(-65, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(64, out cnt);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0xc0, buf[0]);
          Aver.AreEqual(0x00, buf[1]);
          Aver.AreEqual(64, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-12345L, out cnt);
          Aver.AreEqual(3, cnt);
          Aver.AreEqual(0xc7, buf[0]);
          Aver.AreEqual(0x9f, buf[1]);
          Aver.AreEqual(0x7f, buf[2]);
          Aver.AreEqual(-12345L, buf.ReadSLEB128());
        }



        [Run]
        public void Basic_SLEBEncoding_Padding()
        {
          int cnt;

          var buf = new byte[16];
          buf.WriteSLEB128(-1, out cnt, padding: 1);
          Aver.AreEqual(2, cnt);
          Aver.AreEqual(0x7f, buf[0]);
          Aver.AreEqual(0x00, buf[1]);
          Aver.AreEqual(-1, buf.ReadSLEB128());


          buf = new byte[16];
          buf.WriteSLEB128(-129, out cnt, padding: 1);
          Aver.AreEqual(3, cnt);
          Aver.AreEqual(0xff, buf[0]);
          Aver.AreEqual(0x7e, buf[1]);
          Aver.AreEqual(0x00, buf[2]);
          Aver.AreEqual(-129, buf.ReadSLEB128());

          buf = new byte[16];
          buf.WriteSLEB128(-12345L, out cnt, padding: 2);
          Aver.AreEqual(5, cnt);
          Aver.AreEqual(0xc7, buf[0]);
          Aver.AreEqual(0x9f, buf[1]);
          Aver.AreEqual(0x7f, buf[2]);
          Aver.AreEqual(0x80, buf[3]);
          Aver.AreEqual(0x00, buf[4]);
          Aver.AreEqual(-12345L, buf.ReadSLEB128());
        }




        [Run]
        public void Basic_ULEBUsingStream()
        {
          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(1);
             ms.Position = 0;
             Aver.IsTrue(1 == ms.ReadULEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(65);
             ms.Position = 0;
             Aver.IsTrue(65 == ms.ReadULEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(0x45789878L);
             ms.Position = 0;
             Aver.IsTrue(0x45789878L == ms.ReadULEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteULEB128(0xFA76f812638796f1UL);
             ms.Position = 0;
             Aver.IsTrue(0xFA76f812638796f1L == ms.ReadULEB128());
          }
        }


        [Run]
        public void Basic_SLEBUsingStream()
        {
          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(-1);
             ms.Position = 0;
             Aver.AreEqual(-1, ms.ReadSLEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(-65);
             ms.Position = 0;
             Aver.AreEqual(-65, ms.ReadSLEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(0x45789878L);
             ms.Position = 0;
             Aver.AreEqual(0x45789878L, ms.ReadSLEB128());
          }

          using(var ms = new MemoryStream())
          {
             ms.WriteSLEB128(0x7A76f812638796f1L);
             ms.Position = 0;
             Aver.AreEqual(0x7A76f812638796f1L, ms.ReadSLEB128());
          }
        }



    }
}
