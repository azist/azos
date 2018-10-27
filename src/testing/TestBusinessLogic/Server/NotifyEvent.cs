/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


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
