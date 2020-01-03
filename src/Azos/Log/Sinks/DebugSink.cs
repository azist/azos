/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Runtime.Serialization;
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

    /// <summary>
    /// True to include custom exception fields
    /// </summary>
    [Config(Default = true)]
    public bool IncludeFields {  get ; set; } = true;

    /// <summary>
    /// True to include exception "data"
    /// </summary>
    [Config]
    public bool IncludeData { get; set; }

    private IFormatterConverter m_FormatterConverter = new FormatterConverter();

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
      output.Append(sp); output.Append    ("| Target    "); output.AppendLine(error.TargetSite?.Name);
      output.Append(sp); output.Append    ("| Message   "); output.AppendLine(error.Message?.Replace("\n", "\n       .    "+sp));

      //20191023 DKh
      if (IncludeFields)
      {
        try
        {
          var info = new SerializationInfo(error.GetType(), m_FormatterConverter);
          var ctx = new StreamingContext(StreamingContextStates.All);
          error.GetObjectData(info, ctx);
          var e = info.GetEnumerator();
          while(e.MoveNext())
            if (e.Current.Name.IndexOf('-')>0)//we only dump custom exception fields with '-' in between segments
              output.AppendLine(sp + "| `{0}` = \"{1}\"".Args(e.Current.Name, e.Current.Value));
        }
        catch(Exception fldError)
        {
          output.AppendLine(sp + " !!! Error getting custom exception fields: " + fldError.ToMessageWithType());
        }
      }

      //20191023 DKh
      if (IncludeData && error.Data!=null)
      {
        try
        {
          foreach(var okvp in error.Data)
          {
            var de = (DictionaryEntry)okvp;
            output.AppendLine(sp + "| ['{0}'] = \"{1}\"".Args(de.Key, de.Value));
          }
        }
        catch (Exception fldError)
        {
          output.AppendLine(sp + " !!! Error getting custom exception fields: " + fldError.ToMessageWithType());
        }
      }

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
