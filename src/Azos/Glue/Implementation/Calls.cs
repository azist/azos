
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

using Azos.Glue.Protocol;

namespace Azos.Glue.Implementation
{

    /// <summary>
    /// Provides thread-safe CallSlot registry where items can be gotten and removed by request ID guid
    /// </summary>
    internal class Calls
    {
          private class _dict : ConcurrentDictionary<Apps.FID, CallSlot>{}//ConcurrentDictionary Sets levelof parallelism to CPU count * 4

      public Calls(int bucketCount)
      {
        if (bucketCount<1) bucketCount = 97;

        m_BucketCount = bucketCount;

        m_Buckets = new _dict[bucketCount];
        for(int i=0; i < m_Buckets.Length; i++)
         m_Buckets[i] = new _dict();
      }

      private int m_BucketCount;
      private _dict[] m_Buckets;



      /// <summary>
      /// Tries to pur CallSlot instance in the internal list if instance with same ID is not already in the list
      /// </summary>
      public void Put(CallSlot call)
      {
         var requestID = call.RequestID;

         //getBucket() inlined for performance
         var idx = (requestID.GetHashCode() & CoreConsts.ABS_HASH_MASK) % m_BucketCount;
         var bucket = m_Buckets[idx];

         bucket.TryAdd(requestID, call);
      }


      /// <summary>
      /// Tries to get, return and remove CallSlot instance by its RequestID from the list.
      /// Returns null if CallSlot with such an id does not exist and nothing was removed
      /// </summary>
      public CallSlot TryGetAndRemove(FID requestID)
      {
          //getBucket() inlined for performance
          var idx = (requestID.GetHashCode() & CoreConsts.ABS_HASH_MASK) % m_BucketCount;
          var bucket = m_Buckets[idx];

          CallSlot result;
          if (bucket.TryRemove(requestID, out result)) return result;

          return null;
      }

      /// <summary>
      /// Deletes CallSlot entries that have already timed-out. Returns count of slots that got removed.
      /// This method is thread-safe but may take some time as it has to visit all buckets.
      /// Should be called by Glue manager not frequently (i.e. every few minutes)
      /// </summary>
      public int PurgeTimedOutSlots()
      {
        var total = 0;
        foreach(var dict in m_Buckets)
        {
          List<CallSlot> timedOut = null;
          foreach(var kvp in dict)
          {
            if (kvp.Value.CallStatus==CallStatus.Timeout)
            {
              if (timedOut==null) timedOut = new List<CallSlot>();
              timedOut.Add(kvp.Value);
            }
          }
          if (timedOut!=null)
          {
           CallSlot dummy;
           foreach(var cs in timedOut)
            if (dict.TryRemove(cs.RequestID, out dummy)) total++;
          }
        }

        return total;
      }


    }
}
