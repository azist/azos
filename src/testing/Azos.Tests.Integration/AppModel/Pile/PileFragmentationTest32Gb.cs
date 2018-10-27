/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Scripting;

namespace Azos.Tests.Integration.AppModel.Pile
{
  [Runnable]
  public class PileFragmentationTest32Gb : HighMemoryLoadTest32RAM
  {
    [Run("cnt=100000  durationSec=30  speed=true   payloadSizeMin=2  payloadSizeMax=1000  deleteFreq=3   isParallel=true")]
    [Run("cnt=100000  durationSec=30  speed=false  payloadSizeMin=2  payloadSizeMax=1000  deleteFreq=10  isParallel=true")]
    public void Put_RandomDelete_ByteArray(int cnt, int durationSec, bool speed, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      PileFragmentationTest.Put_RandomDelete_ByteArray(cnt, durationSec, speed, payloadSizeMin, payloadSizeMax, deleteFreq, isParallel);
    }

    [Run("speed=true   durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  deleteFreq=3  isParallel=true")]
    [Run("speed=false  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  deleteFreq=3  isParallel=true")]
    public void DeleteOne_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int deleteFreq, bool isParallel)
    {
      PileFragmentationTest.DeleteOne_ByteArray(speed, durationSec, payloadSizeMin, payloadSizeMax, deleteFreq, isParallel);
    }

    [Run("speed=true   durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    [Run("speed=false  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    public void Chessboard_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      PileFragmentationTest.Chessboard_ByteArray(speed, durationSec, payloadSizeMin, payloadSizeMax, isParallel);
    }

    [Run("speed=true   durationSec=30  putMin=100  putMax=200  delFactor=4  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    [Run("speed=false  durationSec=30  putMin=100  putMax=200  delFactor=4  payloadSizeMin=2  payloadSizeMax=1000  isParallel=true")]
    public void DeleteSeveral_ByteArray(bool speed, int durationSec, int putMin, int putMax, int delFactor, int payloadSizeMin, int payloadSizeMax, bool isParallel)
    {
      PileFragmentationTest.DeleteSeveral_ByteArray(speed, durationSec, putMin, putMax, delFactor, payloadSizeMin, payloadSizeMax, isParallel);
    }

    [Run("speed=true   durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=100   countMax=2000")]
    [Run("speed=false  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=100   countMax=2000")]
    [Run("speed=true   durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=1000  countMax=2000")]
    [Run("speed=false  durationSec=30  payloadSizeMin=2  payloadSizeMax=1000  countMin=1000  countMax=2000")]
    public void NoGrowth_ByteArray(bool speed, int durationSec, int payloadSizeMin, int payloadSizeMax, int countMin, int countMax)
    {
      PileFragmentationTest.NoGrowth_ByteArray(speed, durationSec, payloadSizeMin, payloadSizeMax, countMin, countMax);
    }
  }
}
