using Azos.Apps;
using Azos.Scripting;
using Azos.Sky.Identification;
using System;
using System.Collections.Generic;
using System.Text;

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
      for(var i=0; i<cnt; i++)
      {
        var gdid = m_Gen.GenerateOneGdid("scopeA", "seqA");
        gdid.See();
        m_Gen.GetSequenceInfos("scopeA").See();
        Conout.WriteLine("--------------------------------------");
        System.Threading.Thread.Sleep(1000);
      }
    }

    [Run("cnt=5")]
    public void TryMany(int cnt)
    {
      for (var i = 0; i < cnt; i++)
      {
        var gdids = m_Gen.TryGenerateManyConsecutiveGdids("scopeA", "seqA", 105);
        gdids.See($"Size is: {gdids.Length}");
        m_Gen.GetSequenceInfos("scopeA").See();
        Conout.WriteLine("--------------------------------------");
        System.Threading.Thread.Sleep(25);
      }

      Conout.WriteLine("--------------------------------------");
      Conout.WriteLine("--------------------------------------");
      var gdid2 = m_Gen.GenerateOneGdid("scopeA", "seqA");
      gdid2.See();
      m_Gen.GetSequenceInfos("scopeA").See();
    }
  }
}
