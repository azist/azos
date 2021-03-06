/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos;

namespace TestBusinessLogic.Toy
{

    internal static class MachineSpeed
    {
      public static void CRC32()
      {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var buf = new byte[128*1024*1024];

        for(var i=0; i<buf.Length; i++)
        {
          var rnd = Ambient.Random.NextScaledRandomInteger(0, 0xff);
          buf[i] = (byte)rnd;
        }

        var csum = Azos.IO.ErrorHandling.CRC32.ForBytes(buf);

        Console.WriteLine("Checksum {0} in {1:n0} msec".Args(csum, sw.ElapsedMilliseconds));

        Console.WriteLine();
        Console.WriteLine();
     }
    }
}
