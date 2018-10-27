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
    public class BufferSegRStreamTests
    {
        [Run]
        public void T1()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();

            s.BindBuffer(arr, 2, 3);

            Aver.AreEqual(3, s.Length);

              Aver.AreEqual(0, s.Position);
            Aver.AreEqual(3, s.ReadByte());
              Aver.AreEqual(1, s.Position);
            Aver.AreEqual(4, s.ReadByte());
              Aver.AreEqual(2, s.Position);
            Aver.AreEqual(5, s.ReadByte());
              Aver.AreEqual(3, s.Position);
            Aver.AreEqual(-1, s.ReadByte());
              Aver.AreEqual(3, s.Position);
        }

        [Run]
        public void T2()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();

            s.BindBuffer(arr, 0, 3);

            Aver.AreEqual(3, s.Length);

              Aver.AreEqual(0, s.Position);
            Aver.AreEqual(1, s.ReadByte());
              Aver.AreEqual(1, s.Position);
            Aver.AreEqual(2, s.ReadByte());
              Aver.AreEqual(2, s.Position);
            Aver.AreEqual(3, s.ReadByte());
              Aver.AreEqual(3, s.Position);
            Aver.AreEqual(-1, s.ReadByte());
              Aver.AreEqual(3, s.Position);
        }


        [Run]
        public void T3_1()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();

            s.BindBuffer(arr, 2, 3);

            Aver.AreEqual(0, s.Position);
            Aver.AreEqual(3, s.Length);

            var a2 = new byte[10];

            Aver.AreEqual(3,  s.Read(a2, 0, 10));
            Aver.AreEqual(3, a2[0]);
            Aver.AreEqual(4, a2[1]);
            Aver.AreEqual(5, a2[2]);
            Aver.AreEqual(0, a2[3]);
            Aver.AreEqual(3, s.Position);
        }

        [Run]
        public void T3_2()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();

            s.BindBuffer(arr, 2, 3);

            Aver.AreEqual(0, s.Position);
            Aver.AreEqual(3, s.Length);

            var a2 = new byte[10];

            Aver.AreEqual(2,  s.Read(a2, 1, 2));
            Aver.AreEqual(0, a2[0]);
            Aver.AreEqual(3, a2[1]);
            Aver.AreEqual(4, a2[2]);
            Aver.AreEqual(0, a2[3]);
            Aver.AreEqual(2, s.Position);
        }


        [Run]
        public void T4()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();
            var ms = new MemoryStream(arr, 2, 3);

            s.BindBuffer(arr, 2, 3);

            Aver.AreEqual(3, ms.Length);
            Aver.AreEqual(3, s.Length);

            s.Seek(-1, SeekOrigin.End);
            ms.Seek(-1, SeekOrigin.End);
            Aver.AreEqual(2, ms.Position);
            Aver.AreEqual(5, ms.ReadByte());
            Aver.AreEqual(2, s.Position);
            Aver.AreEqual(5, s.ReadByte());

            s.Seek(0, SeekOrigin.End);
            ms.Seek(0, SeekOrigin.End);
            Aver.AreEqual(3, ms.Position);
            Aver.AreEqual(-1, ms.ReadByte());
            Aver.AreEqual(3, s.Position);
            Aver.AreEqual(-1, s.ReadByte());
        }

        [Run]
        public void T5()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();
            var ms = new MemoryStream(arr, 2, 3);

            s.BindBuffer(arr, 2, 3);

            Aver.AreEqual(3, ms.Length);
            Aver.AreEqual(3, s.Length);

            s.Seek(1, SeekOrigin.Begin);
            ms.Seek(1, SeekOrigin.Begin);
            Aver.AreEqual(1, ms.Position);
            Aver.AreEqual(4, ms.ReadByte());
            Aver.AreEqual(1, s.Position);
            Aver.AreEqual(4, s.ReadByte());

            s.Seek(0, SeekOrigin.Begin);
            ms.Seek(0, SeekOrigin.Begin);
            Aver.AreEqual(0, ms.Position);
            Aver.AreEqual(3, ms.ReadByte());
            Aver.AreEqual(0, s.Position);
            Aver.AreEqual(3, s.ReadByte());
        }


        [Run]
        public void T6()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();
            var ms = new MemoryStream(arr, 2, 3);

            s.BindBuffer(arr, 2, 3);

            Aver.AreEqual(3, ms.Length);
            Aver.AreEqual(3, s.Length);

            s.Seek(1, SeekOrigin.Current);
            ms.Seek(1, SeekOrigin.Current);
            Aver.AreEqual(1, ms.Position);
            Aver.AreEqual(4, ms.ReadByte());
            Aver.AreEqual(1, s.Position);
            Aver.AreEqual(4, s.ReadByte());

            s.Seek(-1, SeekOrigin.Current);
            ms.Seek(-1, SeekOrigin.Current);
            Aver.AreEqual(1, ms.Position);
            Aver.AreEqual(4, ms.ReadByte());
            Aver.AreEqual(1, s.Position);
            Aver.AreEqual(4, s.ReadByte());
        }

       
        [Run]
        public void T7()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();

            Aver.Throws<NFXIOException>( () =>  s.BindBuffer(arr, 22, 3) );
            Aver.Throws<NFXIOException>( () =>  s.BindBuffer(arr, 0, 23) );
            Aver.Throws<NFXIOException>( () =>  s.BindBuffer(arr, -1, 1) );
            Aver.Throws<NFXIOException>( () =>  s.BindBuffer(arr, 0, -1) );
        }

        [Run]
        public void T8()
        {
            var arr = new byte[]{1,2,3,4,5};
            var s = new BufferSegmentReadingStream();

            s.BindBuffer(arr, 2, 3);

            Aver.AreEqual(3, s.Length);

              Aver.AreEqual(0, s.Position);
            Aver.AreEqual(3, s.ReadByte());
              Aver.AreEqual(1, s.Position);
            Aver.AreEqual(4, s.ReadByte());
              Aver.AreEqual(2, s.Position);
            Aver.AreEqual(5, s.ReadByte());
              Aver.AreEqual(3, s.Position);
            Aver.AreEqual(-1, s.ReadByte());
              Aver.AreEqual(3, s.Position);

            var arr2 = new byte[]{101,102,103,104,105,106,107,108,109,110};
            
            s.BindBuffer(arr2, 2, 3);

            Aver.AreEqual(3, s.Length);

              Aver.AreEqual(0, s.Position);
            Aver.AreEqual(103, s.ReadByte());
              Aver.AreEqual(1, s.Position);
            Aver.AreEqual(104, s.ReadByte());
              Aver.AreEqual(2, s.Position);
            Aver.AreEqual(105, s.ReadByte());
              Aver.AreEqual(3, s.Position);
            Aver.AreEqual(-1, s.ReadByte());
              Aver.AreEqual(3, s.Position);

        }


    }
}
