/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Instrumentation;

namespace Azos.Sky.Locking.Instrumentation
{

    [Serializable]
    public abstract class LockingLongGauge : LongGauge, ILockingInstrument
    {
      protected LockingLongGauge(string src, long value) : base(src, value) {}
    }

    [Serializable]
    public abstract class LockingDoubleGauge : DoubleGauge, ILockingInstrument
    {
      protected LockingDoubleGauge(string src, double value) : base(src, value) {}
    }








    [Serializable]
    public abstract class LockingServerGauge : LockingLongGauge
    {
      protected LockingServerGauge(string src, long value) : base(src, value) {}
    }

    [Serializable]
    public abstract class LockingClientGauge : LockingLongGauge
    {
      protected LockingClientGauge(string src, long value) : base(src, value) {}
    }






    [Serializable]
    public abstract class LockingServerDoubleGauge : LockingDoubleGauge
    {
      protected LockingServerDoubleGauge(string src, double value) : base(src, value) {}
    }


    [Serializable]
    public abstract class LockingClientDoubleGauge : LockingDoubleGauge
    {
      protected LockingClientDoubleGauge(string src, double value) : base(src, value) {}
    }

}
