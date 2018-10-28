/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Tests.Unit.Pile;
using Azos.Scripting;

namespace Azos.Tests.Integration.Pile
{
    [Runnable]
    public class CacheTest64gb : HighMemoryLoadTest64RAM
    {
        [Run("cnt=10000000  tbls=1")]
        [Run("cnt=10000000  tbls=16")]
        [Run("cnt=10000000  tbls=512")]
        public void T190_FID_PutGetCorrectness(int cnt, int tbls)
        {
            PileCacheTestCore.FID_PutGetCorrectness(cnt, tbls);
        }

        [Run("workers=16  tables=7   putCount=25000   durationSec=40")]
        [Run("workers=5   tables=20  putCount=50000   durationSec=20")]
        [Run("workers=16  tables=20  putCount=150000  durationSec=40")]
        public void T9000000_ParalellGetPutRemove(int workers, int tables, int putCount, int durationSec)
        {
            PileCacheTestCore.ParalellGetPutRemove(workers, tables, putCount, durationSec);
        }
    }
}
