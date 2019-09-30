using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Sky.Identification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Tests.Unit.Sky.Identification
{
  [Runnable]
  public class GdidGenMockAuthorityTests : IRunnableHook
  {
    private MockGdidAuthorityAccessor m_Mock;
    private GdidGenerator m_Gen;

    public void Prologue(Runner runner, FID id)
    {
      m_Mock = new MockGdidAuthorityAccessor();
      m_Gen = new GdidGenerator(NOPApplication.Instance, m_Mock);
    }

    public bool Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_Gen);
      return false;
    }

    [Run("cnt=10")]
    public void OneByOne(int cnt)
    {
      var lst = new List<GDID>();
      for(var i=0; i<cnt; i++)
      {
        var gdid = m_Gen.GenerateOneGdid("scopeA", "seqA");
        lst.Add(gdid);
        gdid.See();
        m_Gen.GetSequenceInfos("scopeA").See();
        Conout.WriteLine("--------------------------------------");
        System.Threading.Thread.Sleep(1000);
      }
      Aver.AreEqual(lst.Count, lst.Distinct().Count());
    }

    [Run("p=false cnt=5 block=105  delay=1")]
    [Run("p=false cnt=5 block=105  delay=25")]
    [Run("p=false cnt=5 block=105  delay=50")]
    [Run("p=false cnt=5 block=105  delay=250")]

    [Run("p=false cnt=8 block=15  delay=1")]
    [Run("p=false cnt=8 block=15  delay=25")]
    [Run("p=false cnt=8 block=15  delay=50")]
    [Run("p=false cnt=8 block=15  delay=250")]

    [Run("p=false cnt=7 block=500  delay=1")]
    [Run("p=false cnt=7 block=500  delay=25")]
    [Run("p=false cnt=7 block=500  delay=50")]
    [Run("p=false cnt=7 block=500  delay=250")]
    [Run("p=false cnt=7 block=500  delay=500")]

    [Run("p=true cnt=5 block=105  delay=1")]
    [Run("p=true cnt=5 block=105  delay=25")]
    [Run("p=true cnt=5 block=105  delay=50")]
    [Run("p=true cnt=5 block=105  delay=250")]

    [Run("p=true cnt=8 block=15  delay=1")]
    [Run("p=true cnt=8 block=15  delay=25")]
    [Run("p=true cnt=8 block=15  delay=50")]
    [Run("p=true cnt=8 block=15  delay=250")]

    [Run("p=true cnt=7 block=500  delay=1")]
    [Run("p=true cnt=7 block=500  delay=25")]
    [Run("p=true cnt=7 block=500  delay=50")]
    [Run("p=true cnt=7 block=500  delay=250")]
    [Run("p=true cnt=7 block=500  delay=500")]

    [Run("p=true cnt=15 block=100  delay=1")]
    [Run("p=true cnt=15 block=100  delay=10")]
    [Run("p=true cnt=15 block=100  delay=25")]
    [Run("p=true cnt=15 block=100  delay=50")]
    [Run("p=true cnt=15 block=100  delay=75")]
    [Run("p=true cnt=15 block=100  delay=100")]
    [Run("p=true cnt=15 block=100  delay=150")]
    [Run("p=true cnt=15 block=100  delay=250")]

    [Run("p=true cnt=8 block=10   delay=1")]
    [Run("p=true cnt=8 block=100  delay=5")]
    [Run("p=true cnt=8 block=500  delay=12")]
    [Run("p=true cnt=8 block=34   delay=1")]
    [Run("p=true cnt=8 block=11   delay=75")]
    [Run("p=true cnt=8 block=230  delay=2")]
    [Run("p=true cnt=8 block=180  delay=1")]
    [Run("p=true cnt=8 block=10   delay=1")]

    [Run("p=true cnt=8 block=10   delay=10")]
    [Run("p=true cnt=8 block=100  delay=50")]
    [Run("p=true cnt=8 block=500  delay=120")]
    [Run("p=true cnt=8 block=34   delay=10")]
    [Run("p=true cnt=8 block=11   delay=300")]
    [Run("p=true cnt=8 block=230  delay=20")]
    [Run("p=true cnt=8 block=180  delay=15")]
    [Run("p=true cnt=8 block=10   delay=15")]
    public void TryMany(bool p, int cnt, int block, int delay)
    {
      var lst = new List<GDID>();

      void body()
      {
        var gdids = m_Gen.TryGenerateManyConsecutiveGdids("scopeA", "seqA", block);
        if (!p) gdids.See($"Size is: {gdids.Length}");
        lock(lst) lst.AddRange(gdids);
        if (!p) m_Gen.GetSequenceInfos("scopeA").See();
        if (!p) Conout.WriteLine("--------------------------------------");
        System.Threading.Thread.Sleep(delay);
        var gdid = m_Gen.GenerateOneGdid("scopeA", "seqA");
        lock(lst) lst.Add(gdid);
        if (!p) gdid.See();
      }

      if (p)
       Parallel.For(0, cnt, i => body());
      else
       for (var i = 0; i < cnt; i++)  body();

      Conout.WriteLine("--------------------------------------");
      Conout.WriteLine("--------------------------------------");
      var gdid2 = m_Gen.GenerateOneGdid("scopeA", "seqA");
      lst.Add(gdid2);
      gdid2.See();
      m_Gen.GetSequenceInfos("scopeA").See();

      "Total: {0}".SeeArgs(lst.Count);
      Aver.AreEqual(lst.Count, lst.Distinct().Count());
    }
  }
}
