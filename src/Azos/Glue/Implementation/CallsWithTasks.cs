/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

namespace Azos.Glue.Implementation
{

    /// <summary>
    /// Provides thread-safe linked list of calls which allocated Tasks and need to track timeout
    /// </summary>
    internal class CallsWithTasks
    {
      const int BUCKETS = 71;//prime

      public CallsWithTasks()
      {
        m_Buckets = new LinkedList<CallSlot>[BUCKETS];
        for(int i=0; i < m_Buckets.Length; i++)
          m_Buckets[i] = new LinkedList<CallSlot>();
      }

      private int m_BucketCount;
      private LinkedList<CallSlot>[] m_Buckets;

      /// <summary>
      /// Tries to put CallSlot instance in the internal list if instance with same ID is not already in the list
      /// </summary>
      public void Put(CallSlot call)
      {
         var requestID = call.RequestID;

         //getBucket() inlined for performance
         var idx = (requestID.GetHashCode() & CoreConsts.ABS_HASH_MASK) % m_BucketCount;
         var bucket = m_Buckets[idx];

         lock(bucket) bucket.AddLast(call);
      }

      /// <summary>
      /// Scans all calls removing the ones that timed-out and returns how many were removed
      /// </summary>
      public int ScanAll()
      {
        var removed = 0;
        foreach(var bucket in m_Buckets)
        {
          lock (bucket)
          {
            var node = bucket.First;
            while (node != null)
            {
              var call = node.Value;
              var status = call.CallStatus; //this will detect timeout
              if (status != CallStatus.Dispatched || call.OneWay)//oneway check for clarity, one way calls do no get registered here anyway
              {
                var toDelete = node;
                node = node.Next;
                bucket.Remove(toDelete);
                removed++;
                continue;
              }
              node = node.Next;
            }
          }
        }

        return removed;
      }
    }
}
