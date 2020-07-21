using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Pile;
using Azos.Scripting;

namespace Azos.Tests.Nub.Pile
{
  [Runnable]
  public class BasicCacheTests
  {
    [Run]
    public void PutGetSamePointer()
    {
      using (var m_Sut = new LocalCache(NOPApplication.Instance))
      {
        m_Sut.Pile = new DefaultPile(m_Sut);
        m_Sut.Start();

        m_Sut.DefaultTableOptions = new TableOptions("*") { CollisionMode = CollisionMode.Durable};

        var tbl = m_Sut.GetOrCreateTable<int>("a");

        Aver.IsTrue(PutResult.Inserted == tbl.Put(1, "my string 1234567", out var ptr));
        Aver.IsTrue(PutResult.Inserted == tbl.PutPointer(123_000_000, ptr));

        Aver.IsTrue(tbl.ContainsKey(1));
        var got = tbl.Get(1);
        Aver.AreObjectsEqual("my string 1234567", got);

        Aver.IsFalse(tbl.ContainsKey(2));
        got = tbl.Get(2);
        Aver.AreObjectsEqual(null, got);

        Aver.IsTrue(tbl.ContainsKey(123_000_000));
        got = tbl.Get(123_000_000);
        Aver.AreObjectsEqual("my string 1234567", got);

        Aver.AreEqual(2, tbl.Count);
        Aver.AreEqual(1, m_Sut.Pile.ObjectCount);

        Aver.IsTrue(tbl.Remove(123_000_000));// <---------- explicitly DELETE one key

        Aver.AreEqual(1, tbl.Count);
        Aver.AreEqual(0, m_Sut.Pile.ObjectCount);

        Aver.IsTrue(tbl.ContainsKey(1));//it still contains key "1"
        got = tbl.Get(1);// but it points to non - existing value
        Aver.IsNull(got);//and returns NULL

        Aver.IsFalse(tbl.ContainsKey(2));
        got = tbl.Get(2);
        Aver.IsNull(got);

        Aver.IsFalse(tbl.ContainsKey(123_000_000)); //this key was explicitly deleted above
        got = tbl.Get(123_000_000);
        Aver.IsNull(got);
      }
    }


    [Run("cnt = 100")]
    [Run("cnt = 10000")]
    [Run("cnt = 150000")]
    public void PutSpeculative(int cnt)
    {
      using (var m_Sut = new LocalCache(NOPApplication.Instance))
      {
        m_Sut.Pile = new DefaultPile(m_Sut);
        m_Sut.Start();

        m_Sut.DefaultTableOptions = new TableOptions("*") { CollisionMode = CollisionMode.Speculative, MaximumCapacity = cnt / 2};

        var tbl = m_Sut.GetOrCreateTable<int>("a");

        var inserted = 0;
        var over = 0;

        for (var i = 0; i < cnt; i++)
        {
          var result = tbl.Put(i, "value#" + i, 10000);
          if (PutResult.Inserted == result) inserted++;
          else if (PutResult.Overwritten == result) over++;
          else Aver.Fail("Unexpected result: "+result);
        }

        "Inserted: {0}  Overwritten: {1}".SeeArgs(inserted, over);

        var inserted2 = 0;
        var over2 = 0;

        for (var i = 0; i < cnt; i++)
        {
          var got = tbl.Get(i) as string;

          if (got==null)
          {
            over2++;
            continue;
          }

          inserted2++;
          Aver.AreEqual("value#" + i, got);
        }
        "Inserted2: {0}  Overwritten2: {1}".SeeArgs(inserted2, over2);

        Aver.AreEqual(inserted, inserted2);
        Aver.AreEqual(over, over2);

        tbl.Purge();

        Aver.AreEqual(0, tbl.Count);
      }
    }


    [Run("cnt = 100")]
    [Run("cnt = 10000")]
    [Run("cnt = 150000")]
    public void PutDurable(int cnt)
    {
      using(var m_Sut = new LocalCache(NOPApplication.Instance))
      {
        m_Sut.Pile = new DefaultPile(m_Sut);
        m_Sut.Start();

        m_Sut.DefaultTableOptions = new TableOptions("*"){ CollisionMode = CollisionMode.Durable, MaximumCapacity = cnt / 2 };

        var tbl = m_Sut.GetOrCreateTable<int>("a");

        for(var i=0; i<cnt; i++)
        {
          var result = tbl.Put(i, "value#"+i, 10000);
          Aver.IsTrue(PutResult.Inserted == result);
        }

        Aver.AreEqual(cnt, tbl.Count);

        for (var i = 0; i < cnt; i++)
        {
          var got = tbl.Get(i) as string;
          Aver.AreEqual("value#" + i, got);
        }

        tbl.Purge();

        Aver.AreEqual(0, tbl.Count);
      }
    }

    [Run("cnt = 1000")]
    [Run("cnt = 50000")]
    [Run("cnt = 375000")]
    public void PutDurableParallel(int cnt)
    {
      using (var m_Sut = new LocalCache(NOPApplication.Instance))
      {
        m_Sut.Pile = new DefaultPile(m_Sut);
        m_Sut.Start();

        m_Sut.DefaultTableOptions = new TableOptions("*") { CollisionMode = CollisionMode.Durable, MaximumCapacity = cnt / 2 };

        var tbl = m_Sut.GetOrCreateTable<int>("a");

        Parallel.For(0, cnt, i =>
        {
          var result = tbl.Put(i, "value#" + i, 10000);
          Aver.IsTrue(PutResult.Inserted == result);
        });

        Aver.AreEqual(cnt, tbl.Count);

        Parallel.For(0, cnt, i =>
        {
          var got = tbl.Get(i) as string;
          Aver.AreEqual("value#" + i, got);
        });

        tbl.Purge();

        Aver.AreEqual(0, tbl.Count);
      }
    }
  }
}
