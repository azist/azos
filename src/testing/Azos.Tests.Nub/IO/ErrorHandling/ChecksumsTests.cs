/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Scripting;
using Azos.IO.ErrorHandling;

namespace Azos.Tests.Nub.IO.ErrorHandling
{
    [Runnable]
    public class ChecksumsTests
    {
        [Run]
        public void CRC32_1()
        {
            Aver.AreEqual( UInt32.Parse("D6C12026", System.Globalization.NumberStyles.HexNumber) ,
                              CRC32.ForEncodedString("Hello Dolly!", System.Text.Encoding.ASCII));
        }

        [Run]
        public void CRC32_2()
        {
            Aver.AreEqual( UInt32.Parse("664EF010", System.Globalization.NumberStyles.HexNumber) ,
                              CRC32.ForEncodedString("This is an example of a much longer string of characters", System.Text.Encoding.ASCII));
        }

        [Run]
        public void CRC32_3()
        {
            Aver.AreEqual( UInt32.Parse("D6C12026", System.Globalization.NumberStyles.HexNumber) ,
                              CRC32.ForString("Hello Dolly!"));
        }

        [Run]
        public void CRC32_4()
        {
            Aver.AreEqual( UInt32.Parse("664EF010", System.Globalization.NumberStyles.HexNumber) ,
                              CRC32.ForString("This is an example of a much longer string of characters"));
        }

        [Run]
        public void CRC32_5()
        {
            Aver.AreEqual( UInt32.Parse("CBF43926", System.Globalization.NumberStyles.HexNumber) ,
                              CRC32.ForString("123456789"));
        }


        [Run]
        public void Adler32_1()
        {
            Aver.AreEqual( UInt32.Parse("1BE9043A", System.Globalization.NumberStyles.HexNumber) ,
                              Adler32.ForEncodedString("Hello Dolly!", System.Text.Encoding.ASCII));
        }

        [Run]
        public void Adler32_2()
        {
            Aver.AreEqual( UInt32.Parse("36E81466", System.Globalization.NumberStyles.HexNumber) ,
                              Adler32.ForEncodedString("This is an example of a much longer string of characters", System.Text.Encoding.ASCII));
        }

        [Run]
        public void Adler32_3()
        {
            Aver.AreEqual( UInt32.Parse("1BE9043A", System.Globalization.NumberStyles.HexNumber) ,
                              Adler32.ForString("Hello Dolly!"));
        }

        [Run]
        public void Adler32_4()
        {
            Aver.AreEqual( UInt32.Parse("36E81466", System.Globalization.NumberStyles.HexNumber) ,
                              Adler32.ForString("This is an example of a much longer string of characters"));
        }


        [Run]
        public void Adler32_5()
        {
            Aver.AreEqual( UInt32.Parse("11E60398", System.Globalization.NumberStyles.HexNumber) ,
                              Adler32.ForString("Wikipedia"));
        }
    }
}
