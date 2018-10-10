

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.IO;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Logs messages in stdio.console
  /// </summary>
  public class ConsoleSink : Sink
  {
    private const string DEF_LOG_TIME_FORMAT = "HH:mm:ss";


    public ConsoleDestination() : this(null) { }
    public ConsoleDestination(string name) : base (name) { }

    [Config]
    public bool Colored { get; set;}

    /// <summary>
    /// Time format for log line entries
    /// </summary>
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

      return string.Format("{0}|{1}|{2}|{3}| {4}",
                    msg.TimeStamp.ToString(tf),
                    msg.Type,
                    msg.Source,
                    msg.From,
                    msg.Text);
    }

  }
}
