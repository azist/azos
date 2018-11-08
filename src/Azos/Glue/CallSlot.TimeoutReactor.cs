/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Glue.Protocol;

namespace Azos.Glue{  public sealed partial class CallSlot {

  /// <summary>
  /// Internal class that polls call slots for timeout
  /// </summary>
  internal static class TimeoutReactor
  {
    private const int BUCKETS = 127;//prime
    private const int GRANULARITY_MS = 500;

    private static LinkedList<CallSlot>[] s_Calls = new LinkedList<CallSlot>[BUCKETS];
    private static volatile Thread s_Thread;

    static TimeoutReactor()
    {
      for(var i=0; i<BUCKETS; i++) s_Calls[i] = new LinkedList<CallSlot>();
    }

    public static void Subscribe(CallSlot call)
    {
      var bucket = s_Calls[(call.GetHashCode() & CoreConsts.ABS_HASH_MASK) % BUCKETS];

      lock(bucket) bucket.AddLast(call);
      if (s_Thread==null)
      {
        lock(typeof(TimeoutReactor))
          if (s_Thread==null)
          {
            s_Thread = new Thread(threadSpin);
            s_Thread.IsBackground = true;
            s_Thread.Name = typeof(TimeoutReactor).FullName;
            s_Thread.Start();
          }
      }
    }

      private static void threadSpin()
      {
        while(App.Active)
        {
          try
          {
            scanOnce();
          }
          catch(Exception err)
          {
            App.Log.Write(new Log.Message {
              Type = Log.MessageType.Critical,
              Topic = CoreConsts.GLUE_TOPIC,
              From = typeof(TimeoutReactor).FullName + ".scanOnce()",
              Text = "Exception leaked: " + err.ToMessageWithType(),
              Exception = err
            });
          }

          Thread.Sleep(GRANULARITY_MS);
        }
        s_Thread = null;
      }

      private static void scanOnce()
      {
          for(var i=0;i<BUCKETS; i++)
          {
            var  bucket = s_Calls[i];
            lock(bucket)
            {
              var node = bucket.First;
              while(node!=null)
              {
                var call = node.Value;
                var status = call.CallStatus; //this will detect timeout
                if (status!=CallStatus.Dispatched || call.OneWay)//oneway check for clarity, one way calls do no get registered here anyway
                {
                  var toDelete = node;
                  node = node.Next;
                  bucket.Remove( toDelete );
                  continue;
                }
                node = node.Next;
              }
            }
          }
      }

  }//TimeoutReactor












    /// <summary>
    /// Provides a higher-level wrapper around CallSlot returned value by Glue.
    /// All property accessors evaluate synchronously on the calling thread.
    /// This struct should not be used with One-Way calls or calls that return void
    /// </summary>
    public struct Future<T>
    {
        public Future(CallSlot call)
        {
          Call = call;
        }

        /// <summary>
        /// Returns the underlying CallSlot object
        /// </summary>
        public readonly CallSlot Call;

        /// <summary>
        /// Non-blocking call that returns true if result arrived, false otherwise
        /// </summary>
        public bool Available { get{ return Call.Available;} }

        /// <summary>
        /// Blocking call that waits for return value. Use non-blocking Available to see if result arrived
        /// </summary>
        public T Value { get{ return Call.GetValue<T>(); } }
    }

    /// <summary>
    /// Provides a higher-level wrapper around CallSlot returned by Glue.
    /// All property accessors evaluate synchronously on the calling thread.
    /// This struct should not be used with One-Way calls
    /// </summary>
    public struct FutureVoid
    {
        public FutureVoid(CallSlot call)
        {
          Call = call;
        }

        /// <summary>
        /// Returns the underlying CallSlot object
        /// </summary>
        public readonly CallSlot Call;

        /// <summary>
        /// Non-blocking call that returns true if result arrived, false otherwise
        /// </summary>
        public bool Available { get{ return Call.Available;} }

        /// <summary>
        /// Blocking call that waits for call completion. Use non-blocking Available to see if call has completed
        /// </summary>
        public void Wait() { Call.CheckVoidValue(); }
    }

  }}
