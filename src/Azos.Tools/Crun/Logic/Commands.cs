using System;

using Azos.Apps;

namespace Azos.Tools.Crun.Logic
{
  public class Commands : CmdBase
  {
    /// <summary>
    /// A CmdAttribute decorated sample method added as a default test placeholder.
    /// </summary>
    [Cmd("echo", "A basic command to echo a message to the console.")]
    private int runEcho(AzosApplication app)
    {
      var args = app.CommandArgs;
      var cfg = args["r"];

      if (cfg.Exists)
      {
        var text = cfg.AttrByName("text")?.Value;
        if (text.IsNotNullOrWhiteSpace()) Console.WriteLine(text);

        var russian = cfg.AttrByName("ru")?.Value;
        if (russian.IsNotNullOrWhiteSpace()) Console.WriteLine(russian);
      }

      Console.WriteLine("ECHO");
      return 0;
    }

    // TODO: add additional commands below

  }
}
