/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Serialization.Bix;

namespace Azos.Log
{
  /// <summary>
  /// Represents a Log message
  /// </summary>
  [Serializable]
  [Bix("3AD5E8E1-871C-4B8F-AE16-6D04492B17DF")]
  public sealed class Message : TypedDoc, IArchiveLoggable
  {
    public Message(){ }

    /// <summary>
    /// Initializes log message populating Guid, Host, UTCTimeStamp, App if they are unassigned.
    /// Calling this method multiple time has the same effect
    /// </summary>
    public Message InitDefaultFields(IApplication app = null)
    {
      if (app==null) app = Apps.ExecutionContext.Application;

      if (m_Guid == Guid.Empty)
        m_Guid = Guid.NewGuid();

      if (m_Host.IsNullOrWhiteSpace())
        m_Host = Platform.Computer.HostName;

      if (m_UTCTimeStamp == default(DateTime))
        m_UTCTimeStamp = app.TimeSource.UTCNow;

      if (m_App.IsZero)
        m_App = app.AppId;

      return this;
    }

    #region Private Fields
    private GDID m_Gdid;
    private Guid m_Guid;
    private Guid m_RelatedTo;
    private Atom m_Channel;
    private Atom m_App;
    private MessageType m_Type;
    private int m_Source;
    private DateTime m_UTCTimeStamp;
    private string m_Host;
    private string m_From;
    private string m_Topic;
    private string m_Text;
    private string m_Parameters;
    private WrappedExceptionData m_ExceptionData;
    private Exception m_Exception;
    private string m_ArchiveDimensions;
    private int? m_SrcDataShard;
    #endregion


    #region Properties

    /// <summary>
    /// Global distributed ID used by distributed log warehouses.
    /// The field is assigned by distributed warehouse implementations such as Sky Chronicle Logic.
    /// GDID.ZERO is used for local logging applications which do not use distributed ids
    /// </summary>
    [Field, Field(isArow: true, backendName: "gdid")]
    public GDID Gdid
    {
      get => m_Gdid;
      set => m_Gdid = value;
    }

    /// <summary>
    /// Returns global unique identifier for this particular log message
    /// </summary>
    [Field, Field(isArow: true, backendName: "guid")]
    public Guid Guid
    {
      get => m_Guid;
      set => m_Guid = value;
    }

    /// <summary>
    /// Gets/Sets global unique identifier of a message that this message is related to.
    /// No referential integrity check is performed
    /// </summary>
    [Field, Field(isArow: true, backendName: "rel")]
    public Guid RelatedTo
    {
      get => m_RelatedTo;
      set => m_RelatedTo = value;
    }

    /// <summary>
    /// Identifies the emitting application by including it asset identifier, taken from App.AppId
    /// </summary>
    [Field, Field(isArow: true, backendName: "app")]
    public Atom App
    {
      get => m_App;
      set => m_App = value;
    }

    /// <summary>
    /// Gets/Sets logical partition for messages. This property is usually used in Archive for splitting sinks
    /// </summary>
    [Field, Field(isArow: true, backendName: "chnl")]
    public Atom Channel
    {
      get => m_Channel;
      set => m_Channel = value;
    }
    /// <summary>
    /// Gets/Sets message type, such as: Info/Warning/Error etc...
    /// </summary>
    [Field, Field(isArow: true, backendName: "tp")]
    public MessageType Type
    {
      get => m_Type;
      set => m_Type = value;
    }

    /// <summary>
    /// Gets/Sets message source line number/tracepoint#, this is used in conjunction with From
    /// </summary>
    [Field, Field(isArow: true, backendName: "src")]
    public int Source
    {
      get => m_Source;
      set => m_Source = value;
    }

    /// <summary>
    /// Gets/Sets timestamp when message was generated
    /// </summary>
    [Field, Field(isArow: true, backendName: "utc")]
    public DateTime UTCTimeStamp
    {
      get => m_UTCTimeStamp;
      set => m_UTCTimeStamp = value;
    }

    /// <summary>
    /// Gets/Sets host name that generated the message
    /// </summary>
    [Field, Field(isArow: true, backendName: "hst")]
    public string Host
    {
      get => m_Host ?? string.Empty;
      set => m_Host = value;
    }


    /// <summary>
    /// Gets/Sets logical component ID, such as: class name, method name, process instance, that generated the message.
    /// This field is used in the scope of Topic
    /// </summary>
    [Field, Field(isArow: true, backendName: "frm")]
    public string From
    {
      get => m_From ?? string.Empty;
      set => m_From = value;
    }

    /// <summary>
    /// Gets/Sets a message topic/relation - the name of software concern within a big system, e.g. "Database" or "Security"
    /// </summary>
    [Field, Field(isArow: true, backendName: "top")]
    public string Topic
    {
      get => m_Topic ?? string.Empty;
      set => m_Topic = value;
    }

    /// <summary>
    /// Gets/Sets an unstructured message text, the emitting component name must be in From field, not in text.
    /// Note about logging errors. Use caught exception.ToMessageWithType() method, then attach the caught exception as Exception property
    /// </summary>
    [Field, Field(isArow: true, backendName: "txt")]
    public string Text
    {
      get => m_Text ?? string.Empty;
      set => m_Text = value;
    }

    /// <summary>
    /// Gets/Sets a structured parameter bag, this may be used for additional debug info like source file name, additional context etc.
    /// </summary>
    [Field, Field(isArow: true, backendName: "par")]
    public string Parameters
    {
      get => m_Parameters ?? string.Empty;
      set => m_Parameters = value;
    }

    /// <summary>
    /// Gets/Sets exception data associated with this message.
    /// If this is set then Exception property is made from this value.
    /// This is what gets serialized by DataDoc
    /// </summary>
    [Field, Field(isArow: true, backendName: "exc")]
    public WrappedExceptionData ExceptionData
    {
      get
      {
        if (m_ExceptionData==null)
        {
          if (m_Exception!=null)
            m_ExceptionData = new WrappedExceptionData(m_Exception);
        }
        return m_ExceptionData;
      }
      set
      {
        m_ExceptionData = value;
        m_Exception = null;
      }
    }


    /// <summary>
    /// Gets/Sets exception associated with message.
    /// Set this property EVEN IF the name/text of exception is already included in Text as log sinks may elect to dump the whole stack trace.
    /// When this is set the ExceptionData property is made from this value.
    /// Note: This is not a field of DataDoc, hence it is not written/read, instead the ExecptionData is written/read
    /// </summary>
    public Exception Exception
    {
      get
      {
        if (m_Exception==null)
        {
          if (m_ExceptionData!=null)
            m_Exception = new WrappedException(m_ExceptionData);
        }
        return m_Exception;
      }
      set
      {
        m_Exception = value;
        m_ExceptionData = null;
      }
    }

    /// <summary>
    /// Gets/Sets archive dimension content for later retrieval of messages by key, i.e. a user ID may be used.
    /// In most cases JSON or Laconic content is stored, the format depends on a concrete system
    /// </summary>
    [Field, Field(isArow: true, backendName: "adims")]
    public string ArchiveDimensions
    {
      get => m_ArchiveDimensions ?? string.Empty;
      set => m_ArchiveDimensions = value;
    }

    /// <summary>
    /// Used by multiplexed servers: Gets/Sets shard number where data came from.
    /// This is non-stored lookup property which is set by server when sourcing data from multiple shards
    /// </summary>
    [Field, Field(isArow: true, backendName: "dshard")]
    public int? SrcDataShard
    {
      get => m_SrcDataShard;
      set => m_SrcDataShard = value;
    }

    #endregion


    public override string ToString()
      => "{0:yyyyMMdd-HHmmss.fff}, {1}, {2}, {3}, {4}, {5}, {6}".Args(m_UTCTimeStamp, m_Guid, m_Host, m_Type, Topic, From, Text);

    /// <summary>
    /// Supplants the from string with caller info encoded as JSON string
    /// </summary>
    public static object FormatCallerParams(object pars,
                                            [CallerFilePath]  string file = null,
                                            [CallerLineNumber]int line = 0)
    {
      if (pars == null)
        return new
        {
          f = file.IsNotNullOrWhiteSpace() ? Path.GetFileName(file) : null,
          L = line,
        };
      else
        return new
        {
          f = file.IsNotNullOrWhiteSpace() ? Path.GetFileName(file) : null,
          L = line,
          p = pars
        };
    }

    /// <summary>
    /// Sets parameter content as JSON representation of the supplied object
    /// </summary>
    public Message SetParamsAsObject(object p)
    {
      if (p == null)
        m_Parameters = null;
      else
        m_Parameters = p.ToJson(JsonWritingOptions.CompactRowsAsMap);

      return this;
    }

    /// <summary>
    /// Deep clones all fields into a new instance
    /// </summary>
    public Message Clone()
    => new Message
      {
        m_Gdid = m_Gdid,
        m_Guid = m_Guid,
        m_RelatedTo = m_RelatedTo,
        m_Channel = m_Channel,
        m_App = m_App,
        m_Type = m_Type,
        m_Source = m_Source,
        m_UTCTimeStamp = m_UTCTimeStamp,
        m_Host = m_Host,
        m_From = m_From,
        m_Topic = m_Topic,
        m_Text = m_Text,
        m_Parameters = m_Parameters,
        m_Exception = m_Exception,
        m_ArchiveDimensions = m_ArchiveDimensions,
        m_SrcDataShard = m_SrcDataShard,
      };


    protected override object FilterJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, out string name)
    {
      if (
          (def.Name == nameof(RelatedTo)  && RelatedTo == Guid.Empty) ||
          (def.Name == nameof(Channel)    && Channel.IsZero) ||
          (def.Name == nameof(Parameters) && Parameters.IsNullOrWhiteSpace()) ||
          (def.Name == nameof(ExceptionData)     && ExceptionData==null)  ||
          (def.Name == nameof(ArchiveDimensions) && ArchiveDimensions.IsNullOrWhiteSpace()) ||
          (def.Name == nameof(SrcDataShard)      && !SrcDataShard.HasValue)
         )
      {
        name = null;
        return null;
      }
      return base.FilterJsonSerializerField(def, options, out name);
    }
  }

  public static class MessageExtensions
  {
    /// <summary>
    /// Passes through an existing log message if it is null, does not have exception, or has exception set to WrappedException instance already,
    /// otherwise clones the message into a new instance wrapping the Exception as WrappedException
    /// </summary>
    public static Message ThisOrNewSafeWrappedException(this Message msg, bool captureStack = true)
    {
      if (msg == null || msg.Exception == null || msg.Exception is WrappedException) return msg;

      var clone = msg.Clone();
      clone.Exception = WrappedException.ForException(msg.Exception, captureStack);
      return clone;
    }
  }

  internal class MessageList : List<Message>
  {
    public MessageList() { }
    public MessageList(IEnumerable<Message> other) : base(other) { }
  }
}
