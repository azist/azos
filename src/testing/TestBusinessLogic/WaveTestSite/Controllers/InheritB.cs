
using System;

using Azos.Wave.MVC;

namespace WaveTestSite.Controllers
{

  public class InheritB : InheritBase
  {

    [Action("exo", 0)]
    public new string EchoNew(string msg)
    {
      return "B:EchoNew "+msg;
    }

    [Action("exov", 0)]
    public override string EchoVirtual(string msg)
    {
      return "B:EchoVirtual "+msg;
    }
  }


}
