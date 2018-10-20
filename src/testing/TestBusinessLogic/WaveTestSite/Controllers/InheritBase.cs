
using System;

using Azos.Wave.MVC;

namespace WaveTestSite.Controllers
{
  /// <summary>
  /// Adds numbers
  /// </summary>
  public class InheritBase : Controller
  {

    [Action]
    public string Echo1(string msg)
    {
      return "Base:Echo1 "+msg;
    }

    [Action]
    public string EchoNew(string msg)
    {
      return "Base:EchoNew "+msg;
    }

    [Action]
    public virtual string EchoVirtual(string msg)
    {
      return "Base:EchoVirtual "+msg;
    }
  }


}
