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
  /// Provides a CSV file storage log destination implementation
  /// </summary>
  public class CSVFileSink : TextFileSink
  {
    public  const string DEFAULT_LOG_TIME_FORMAT  = "yyyyMMdd-HHmmss";
    private const string DEFAULT_FILENAME         = "{0:yyyyMMdd}.log.csv";

    public CSVFileSink(ISinkOwner owner) : base(owner)
    {
    }

    public CSVFileSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }


    /// <summary>
    /// Sets time formatting for CSV log line
    /// </summary>
    [Config]
    public string LogTimeFormat { get; set;}


    public static string MessageToCSVLine(Sink sink, Message msg, string logTimeFormat = null)
    {
        if (logTimeFormat.IsNullOrWhiteSpace())
         logTimeFormat = DEFAULT_LOG_TIME_FORMAT;

        StringBuilder line = new StringBuilder();

        line.Append(escape(msg.Channel.Value)); line.Append(',');
        line.Append(msg.Guid); line.Append(',');
        line.Append(msg.RelatedTo == Guid.Empty ? string.Empty : msg.RelatedTo.ToString()); line.Append(',');
        line.Append(msg.Type.ToString()); line.Append(',');
        line.Append(sink.UniversalTimeToLocalizedTime(msg.UTCTimeStamp).ToString(logTimeFormat)); line.Append(',');
        line.Append(escape(msg.Host));    line.Append(',');
        line.Append(escape(msg.Topic));   line.Append(',');
        line.Append(escape(msg.From));    line.Append(',');
        line.Append(escape(msg.Text));    line.Append(',');
        line.Append(msg.Source); line.Append(',');
        line.Append(escape(msg.ArchiveDimensions)); line.Append(',');
        line.Append(escape(msg.Parameters)); line.Append(',');
        if (msg.Exception != null)
          line.Append(escape(msg.Exception.GetType().FullName + "::" + msg.Exception.Message));

        return line.ToString();
    }

    protected override string DefaultFileName =>  DEFAULT_FILENAME;

    /// <summary>
    /// Spools instance data in CSV format for storage in a file destination
    /// </summary>
    protected override string DoFormatMessage(Message msg) => MessageToCSVLine(this, msg, LogTimeFormat);


    private static string escape(string str)
    {
      if (str==null) return string.Empty;

      bool needsQuotes = str.IndexOfAny(new char[] { ' ', ',', '\n', '\r', '"' }) >= 0;

      str = str.Replace("\n", @"\n");
      str = str.Replace("\r", @"\r");
      str = str.Replace("\"", "\"\"");

      return needsQuotes ? "\"" + str + "\"" : str;
    }
  }
}
