/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using Azos.Conf;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Provides a file storage destination implementation for debug and trace logging
  /// </summary>
  public sealed class DebugSink : TextFileSink
  {
    private const char   SEPARATOR = '|';
    private const string LOG_TIME_FORMAT_DFLT = "yyyyMMdd-HHmmss";

    public DebugSink(ISinkOwner owner) : base(owner)
    {
    }

    public DebugSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }

    /// <summary>
    /// Specifies time format to be used for message logging
    /// </summary>
    [Config]
    public string LogTimeFormat { get; set; }

    protected override string DoFormatMessage(Message msg)
    {
      var output = new StringBuilder();
      var fmt  = LogTimeFormat;

      if (fmt.IsNullOrWhiteSpace())
        fmt = LOG_TIME_FORMAT_DFLT;

      string time;

      var now = UniversalTimeToLocalizedTime(msg.UTCTimeStamp);
      try { time = now.ToString(fmt); }
      catch (Exception e)
      {
        time = now.ToString(LOG_TIME_FORMAT_DFLT) + " " + e.ToMessageWithType();
      }

      output.Append('@');
      output.Append(time);        output.Append(SEPARATOR);
      output.Append(msg.Guid);    output.Append(SEPARATOR);
      output.Append(msg.Host);    output.Append(SEPARATOR);
      output.Append(msg.Channel); output.Append(SEPARATOR);
      output.AppendLine(msg.RelatedTo == Guid.Empty ? string.Empty : msg.RelatedTo.ToString());

      output.Append(msg.Type); output.Append(SEPARATOR); output.Append(msg.Topic); output.Append(SEPARATOR); output.Append(msg.From); output.Append(SEPARATOR); output.AppendLine(msg.Source.ToString());
      output.Append(msg.Text); output.AppendLine();

      if (msg.Parameters.IsNotNullOrWhiteSpace()) output.AppendLine(msg.Parameters);

      dumpException(output, msg.Exception, 1);

      output.AppendLine();
      output.AppendLine();

      return output.ToString();
    }

    private void dumpException(StringBuilder output, Exception error, int level)
    {
      if (error==null) return;
      output.AppendLine();

      var sp = new string(' ', level * 2);
      output.Append(sp); output.AppendLine("+-Exception ");
      output.Append(sp); output.Append    ("| Type      "); output.AppendLine(error.GetType().FullName);
      output.Append(sp); output.Append    ("| Source    "); output.AppendLine(error.Source);
      output.Append(sp); output.Append    ("| Target    "); output.AppendLine(error.TargetSite.Name);
      output.Append(sp); output.Append    ("| Message   "); output.AppendLine(error.Message.Replace("\n", "\n       .    "+sp));
      output.Append(sp); output.AppendLine("| Stack     ");

      var stackTrace = error.StackTrace;

      if (stackTrace.IsNotNullOrWhiteSpace())
                         output.AppendLine(sp + stackTrace.Replace("\n", "\n"+sp) );
      else
                         output.AppendLine(sp + " <no stack trace> ");

      dumpException(output, error.InnerException, level+1);
    }
  }
}
