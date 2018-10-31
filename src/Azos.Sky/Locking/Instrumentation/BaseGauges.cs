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
