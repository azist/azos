
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Conf;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Provides a CSV file storage log destination implementation
  /// </summary>
  public class CSVFileSink : TextFileSink
  {
    public const string DEFAULT_LOG_TIME_FORMAT       = "yyyyMMdd-HHmmss";
    private  const string DEF_FILENAME = "{0:yyyyMMdd}.log.csv";

    public CSVFileSink() : this(null) { }
    public CSVFileSink(string name) : base(name) { }


    /// <summary>
    /// Sets time formatting for CSV log line
    /// </summary>
    [Config]
    public string LogTimeFormat { get; set;}


    public static string MessageToCSVLine(Message msg, string logTimeFormat = null)
    {
        if (logTimeFormat.IsNullOrWhiteSpace())
         logTimeFormat = DEFAULT_LOG_TIME_FORMAT;

        StringBuilder line = new StringBuilder();

        line.Append(msg.Guid); line.Append(',');
        line.Append(msg.RelatedTo == Guid.Empty ? string.Empty : msg.RelatedTo.ToString()); line.Append(',');
        line.Append(msg.Type.ToString()); line.Append(',');
        line.Append(msg.Source.ToString()); line.Append(',');
        line.Append(msg.TimeStamp.ToString(logTimeFormat)); line.Append(',');

        line.Append(escape(msg.Host));    line.Append(',');
        line.Append(escape(msg.Channel)); line.Append(',');
        line.Append(escape(msg.From));    line.Append(',');
        line.Append(escape(msg.Topic));   line.Append(',');
        line.Append(escape(msg.Text));    line.Append(',');
        line.Append(escape(msg.ArchiveDimensions)); line.Append(',');
        line.Append(escape(msg.Parameters)); line.Append(',');
        if (msg.Exception != null)
            line.Append(escape(msg.Exception.GetType().FullName + "::" + msg.Exception.Message));

        return line.ToString();
    }

    protected override string DefaultFileName  { get { return DEF_FILENAME;  } }

    /// <summary>
    /// Spools instance data in CSV format for storage in a file destination
    /// </summary>
    protected override string DoFormatMessage(Message msg)
    {
      return MessageToCSVLine(msg, LogTimeFormat);
    }


    private static string escape(string str)
    {
      bool needsQuotes = str.IndexOfAny(new char[] { ' ', ',', '\n', '\r', '"' }) >= 0;

      str = str.Replace("\n", @"\n");
      str = str.Replace("\r", @"\r");
      str = str.Replace("\"", "\"\"");

      return needsQuotes ? "\"" + str + "\"" : str;
    }
  }
}
