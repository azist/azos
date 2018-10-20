

using Azos.Apps;
using Azos.Instrumentation;

namespace TestBusinessLogic.Server
{
    /// <summary>
    /// Instrumentation event for testing
    /// </summary>
    public class NotifyEvent : Event, IOperationClass
    {
        protected NotifyEvent() : base("JokeServer.Notify") {}

        public static void Happened()
        {
          var inst = ExecutionContext.Application.Instrumentation;
          if (inst.Enabled)
           inst.Record(new NotifyEvent());
        }


        protected override Datum MakeAggregateInstance()
        {
            return new NotifyEvent();
        }
    }
}
