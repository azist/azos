/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.IO.Console;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Logs messages in stdio.console
  /// </summary>
  public sealed class ConsoleSink : Sink
  {
    private const string DEF_LOG_TIME_FORMAT = "HH:mm:ss";


    public ConsoleSink(ISinkOwner owner) : base(owner)
    {
    }

    public ConsoleSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }

    /// <summary>Set to true to colorize the console background/foreground per message type </summary>
    [Config]
    public bool Colored { get; set;}

    /// <summary>Time format for log line entries </summary>
    [Config]
    public string LogTimeFormat{ get; set;}

    protected internal override void DoSend(Message msg)
    {
      var txt = fmt(msg);

      if (Colored)
      {
        if (msg.Type<MessageType.Warning) ConsoleUtils.Info(txt);
        else
        if (msg.Type<MessageType.Error) ConsoleUtils.Warning(txt);
        else
          ConsoleUtils.Error(txt);
      }
      else
        Console.WriteLine(txt);
    }

    private string fmt(Message msg)
    {
      var tf = LogTimeFormat;
      if (tf.IsNullOrWhiteSpace()) tf = DEF_LOG_TIME_FORMAT;

      return "{0}|{1}|{2}|{3}| {4}".Args(
                    UniversalTimeToLocalizedTime(msg.UTCTimeStamp).ToString(tf),
                    msg.Type,
                    msg.Source,
                    msg.From,
                    msg.Text);
    }

  }
}
