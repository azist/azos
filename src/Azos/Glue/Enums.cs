
using System;

namespace Azos.Glue
{
     /// <summary>
     /// Logging sources - client, server, any
     /// </summary>
     public enum LogSrc
     {
       Any = 0,
       Client = 1,
       Server = 2
     }


     /// <summary>
     /// Stipulates codes returned for contract calls
     /// </summary>
     public enum CallStatus
     {
         /// <summary>
         /// Indicates that call was sucessfully queued/dispatched for processing by remote server
         /// </summary>
         Dispatched = 0,

         /// <summary>
         /// Indicates that operation failed locally because the communication stack is overloaded or some other internal error occured
         /// </summary>
         DispatchError,

         /// <summary>
         /// Indicates that operation timed out before it could be completed
         /// </summary>
         Timeout,


         /// <summary>
         /// Indicates that response came with payload that does not contain error
         /// </summary>
         ResponseOK,

         /// <summary>
         /// Remote server returned response with exception
         /// </summary>
         ResponseError
     }


     /// <summary>
     /// Stipulates operation flow kind - sync/async
     /// </summary>
     public enum OperationFlow
     {
        /// <summary>
        /// Every operation blocks until it is completed or times out
        /// </summary>
        Synchronous = 0,

        /// <summary>
        /// Operations do not block and use reactor to correlate request/responses
        /// </summary>
        Asynchronous
     }

     /// <summary>
     /// Message/data dumping detail
     /// </summary>
     [Flags]
     public enum DumpDetail
     {
         /// <summary>
         /// No dumping
         /// </summary>
         None = 0,

         /// <summary>
         /// Message-level dumping
         /// </summary>
         Message
     }

}
