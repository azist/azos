using System;
using System.Collections.Generic;
using System.Text;

using Azos.IO.Console;
using Azos.Wave.Mvc;

namespace Azos.Wave.Tv
{
  /// <summary>
  /// Implements MVC controller that acts as a server for Tele-services (Tele-Vision)
  /// </summary>
  [NoCache]
  public class ConPort : Controller
  {
    [ActionOnPost, AcceptsJson]
    public object Push(TeleConsoleMsgBatch batch)
    {
      return new {OK = true, got = batch};
    }
  }
}
