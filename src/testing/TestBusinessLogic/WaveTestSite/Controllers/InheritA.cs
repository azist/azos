
using System;

using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{

  public class InheritA : InheritBase
  {

    [Action]
    public new string EchoNew(string msg)
    {
      return "A:EchoNew "+msg;
    }

    [Action]
    public override string EchoVirtual(string msg)
    {
      return "A:EchoVirtual "+msg;
    }
  }


}
