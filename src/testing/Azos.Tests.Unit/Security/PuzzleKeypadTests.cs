/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Azos.Scripting;

using NFX;
//using Azos.Security.CAPTCHA;

namespace Azos.Tests.Unit.Security
{
    [Runnable]
    public class PuzzleKeypadTests
    {
        [Run]
        public void ParallelRendering_PNG()
        {
            const int CNT = 10000;

            long totalBytes = 0;

            var sw = Stopwatch.StartNew();
            //Parallel.For(0, CNT,
            //   (i) =>
            //   {
            //        var kp = new PuzzleKeypad( (new ELink((ulong)App.Random.NextRandomInteger, null)).Link);
            //        var img = kp.DefaultRender();
            //        var ms = new MemoryStream();
            //        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //        Interlocked.Add(ref totalBytes, ms.Length);

            //   });
            var elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine("Generated {0} in {1} ms at {2} ops./sec. Bytes: {3}".Args(CNT, elapsed, CNT / (elapsed / 1000d), totalBytes));
        }

        [Run]
        public void ParallelRendering_JPEG()
        {
            const int CNT = 10000;

            long totalBytes = 0;

            var sw = Stopwatch.StartNew();
            //Parallel.For(0, CNT,
            //   (i) =>
            //   {
            //        var kp = new PuzzleKeypad( (new ELink((ulong)App.Random.NextRandomInteger, null)).Link);
            //        var img = kp.DefaultRender();
            //        var ms = new MemoryStream();
            //        img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //        Interlocked.Add(ref totalBytes, ms.Length);

            //   });
            var elapsed = sw.ElapsedMilliseconds;
            Console.WriteLine("Generated {0} in {1} ms at {2} ops./sec. Bytes: {3}".Args(CNT, elapsed, CNT / (elapsed / 1000d), totalBytes));
        }
    }
}
