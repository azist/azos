/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Linq;


using Azos.Pile;
using Azos.Scripting;

namespace Azos.Tests.Unit.Pile
{
  [Runnable(TRUN.BASE, 8)]
  public class PointerCacheTest
  {
      [Run("count=   100")]
      [Run("count=250111")]
      public void BasicDurable(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeDurableCache())
        {
          var tA = cache.GetOrCreateTable<Guid>("A");

          Aver.AreObjectsEqual( CollisionMode.Durable, tA.CollisionMode );

          var dict = new Dictionary<Guid, PilePointer>();

          for(var i=0; i<COUNT; i++)
          {
            var key = Guid.NewGuid();
            var ptr = cache.Pile.Put(key.ToString(), preallocateBlockSize: 1024);
            dict.Add(key, ptr);

            Aver.IsTrue( PutResult.Inserted == tA.PutPointer(key, ptr) );
          }

          Aver.AreEqual( COUNT, tA.Count );

          foreach(var kvp in dict)
          {
            var gotPointer = tA.GetPointer(kvp.Key);
            Aver.IsTrue( gotPointer.Valid );

            Aver.AreEqual(gotPointer, kvp.Value);

            var gotObject = tA.Get(kvp.Key) as string;
            Aver.IsNotNull( gotObject );

            Aver.AreEqual( kvp.Key.ToString(), gotObject);
          }

          //because there is durable chaining, object count in pile is greater than in table
          Aver.IsTrue(COUNT <= cache.Pile.ObjectCount);

          foreach(var kvp in dict)
          {
            Aver.IsTrue( tA.Remove(kvp.Key) );
          }

          Aver.AreEqual(0, tA.Count );
          Aver.AreEqual(0, cache.Pile.ObjectCount);
        }
      }


      [Run("count=   100")]
      [Run("count=250111")]
      public void BasicSpeculative(int COUNT)
      {
        using (var cache = PileCacheTestCore.MakeCache())
        {
          var tA = cache.GetOrCreateTable<Guid>("A");

          Aver.AreObjectsEqual( CollisionMode.Speculative, tA.CollisionMode );

          var dict = new Dictionary<Guid, PilePointer>();

          for(var i=0; i<COUNT; i++)
          {
            var key = Guid.NewGuid();
            var ptr = cache.Pile.Put(key.ToString(), preallocateBlockSize: 1024);
            dict.Add(key, ptr);

            var pr = tA.PutPointer(key, ptr);
            Aver.IsTrue( PutResult.Inserted == pr || PutResult.Overwritten == pr );
          }

          var ratio = tA.Count / (double)COUNT;
          Console.WriteLine(ratio);
          Aver.IsTrue( ratio > 0.85d );


          foreach(var kvp in dict)
          {
            var gotPointer = tA.GetPointer(kvp.Key);
            if (!gotPointer.Valid) continue;

            Aver.AreEqual(gotPointer, kvp.Value);

            var gotObject = tA.Get(kvp.Key) as string;
            Aver.IsNotNull( gotObject );

            Aver.AreEqual( kvp.Key.ToString(), gotObject);
          }

          foreach(var kvp in dict)
          {
            tA.Remove(kvp.Key);
          }

          Aver.AreEqual(0, tA.Count );
          Aver.AreEqual(0, cache.Pile.ObjectCount);
        }
      }
  }
}
