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
using Azos.Serialization.BSON;
using Azos.Serialization.Arow;

namespace Azos.Log
{
  /// <summary>
  /// Represents a Log message
  /// </summary>
  [Serializable, Arow]
  [BSONSerializable("A05AEE0F-A33C-4B1D-AA45-CDEAF894A095")]
  public sealed class Message : TypedDoc, IArchiveLoggable
  {
    public const string BSON_FLD_APP = "app";
    public const string BSON_FLD_CHANNEL = "chn";
    public const string BSON_FLD_RELATED_TO = "rel";
    public const string BSON_FLD_TYPE = "tp";
    public const string BSON_FLD_SOURCE = "src";
    public const string BSON_FLD_TIMESTAMP = "uts";
    public const string BSON_FLD_HOST = "hst";
    public const string BSON_FLD_FROM = "frm";
    public const string BSON_FLD_TOPIC = "top";
    public const string BSON_FLD_TEXT = "txt";
    public const string BSON_FLD_PARAMETERS = "prm";
    public const string BSON_FLD_EXCEPTION = "ex";
    public const string BSON_FLD_ARCHIVE_DIMENSIONS = "arc";

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
    #endregion

    #region Properties



    /// <summary>
    /// Global distributed ID used by distributed log warehouses. GDID.ZERO for local logging applications
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
      internal set => m_Guid = value;
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
    /// Identifies the emitting application by including it asset identifier, taken from App.AssetId
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

    #endregion

    /// <summary>
    /// Creates log message defaulting from Message.DefaultHostName and UTCTime
    /// </summary>
    [Azos.Serialization.Slim.SlimDeserializationCtorSkip]
    public Message()
    {
      m_Guid = Guid.NewGuid();
      m_Host = Platform.Computer.HostName;
      m_UTCTimeStamp = Ambient.UTCNow;
      m_App = Apps.ExecutionContext.Application.AppId;
    }

    /// <summary>
    /// Creates message with Parameters supplanted with caller file name and line #
    /// </summary>
    public Message(object pars, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0) : this()
    {
      SetParamsAsObject(FormatCallerParams(pars, file, line));
      Source = line;
    }

    public override string ToString()
    {
      return "{0}/{1}, {2}, {3}, {4}, {5}, {6}, {7}".Args(
                           m_Type,
                           m_Source,
                           m_UTCTimeStamp,
                           Host,
                           From,
                           Topic,
                           Text,
                           Exception != null ? Exception.ToString() : string.Empty);
    }

    /// <summary>
    /// Supplants the from string with caller as JSON string
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

    public Message SetParamsAsObject(object p)
    {
      if (p == null)
        m_Parameters = null;
      else
        m_Parameters = p.ToJson(JsonWritingOptions.CompactASCII);

      return this;
    }

    public Message Clone()
    {
      return new Message
      {
        m_Guid = m_Guid,
        m_RelatedTo = m_RelatedTo,
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
        m_Channel = m_Channel
      };
    }


    #region BSON Serialization
    public bool IsKnownTypeForBSONDeserialization(Type type)
    {
      return type == typeof(WrappedException);
    }

    public void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      serializer.AddTypeIDField(doc, parent, this, context);

      var skipNull = (serializer.Flags ^ BSONSerializationFlags.KeepNull) == 0;

      doc.Add(serializer.PKFieldName, m_Guid, required: true)
        .Add(BSON_FLD_RELATED_TO, m_RelatedTo, skipNull)
        .Add(BSON_FLD_CHANNEL, m_Channel.ID, skipNull)
        .Add(BSON_FLD_APP, m_App.ID, skipNull)
        .Add(BSON_FLD_TYPE, m_Type.ToString(), skipNull, required: true)
        .Add(BSON_FLD_SOURCE, m_Source, skipNull)
        .Add(BSON_FLD_TIMESTAMP, m_UTCTimeStamp, skipNull)
        .Add(BSON_FLD_HOST, m_Host, skipNull)
        .Add(BSON_FLD_FROM, m_From, skipNull)
        .Add(BSON_FLD_TOPIC, m_Topic, skipNull)
        .Add(BSON_FLD_TEXT, m_Text, skipNull)
        .Add(BSON_FLD_PARAMETERS, m_Parameters, skipNull)
        .Add(BSON_FLD_ARCHIVE_DIMENSIONS, m_ArchiveDimensions, skipNull);

      if (m_Exception == null) return;

      var we = m_Exception as WrappedException;
      if (we == null)
        we = WrappedException.ForException(m_Exception);

      doc.Add(BSON_FLD_EXCEPTION, serializer.Serialize(we, parent: this), skipNull);
    }

    public void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      m_Guid = doc.TryGetObjectValueOf(serializer.PKFieldName).AsGUID(Guid.Empty);

      m_RelatedTo = doc.TryGetObjectValueOf(BSON_FLD_RELATED_TO).AsGUID(Guid.Empty);

      m_Channel = new Atom( doc.TryGetObjectValueOf(BSON_FLD_CHANNEL).AsULong(0) );
      m_App = new Atom( doc.TryGetObjectValueOf(BSON_FLD_APP).AsULong(0) );

      m_Type = doc.TryGetObjectValueOf(BSON_FLD_TYPE).AsEnum(MessageType.Info);
      m_Source = doc.TryGetObjectValueOf(BSON_FLD_SOURCE).AsInt();
      m_UTCTimeStamp = doc.TryGetObjectValueOf(BSON_FLD_TIMESTAMP).AsDateTime(Ambient.UTCNow);
      m_Host = doc.TryGetObjectValueOf(BSON_FLD_HOST).AsString();
      m_From = doc.TryGetObjectValueOf(BSON_FLD_FROM).AsString();
      m_Topic = doc.TryGetObjectValueOf(BSON_FLD_TOPIC).AsString();
      m_Text = doc.TryGetObjectValueOf(BSON_FLD_TEXT).AsString();
      m_Parameters = doc.TryGetObjectValueOf(BSON_FLD_PARAMETERS).AsString();
      m_ArchiveDimensions = doc.TryGetObjectValueOf(BSON_FLD_ARCHIVE_DIMENSIONS).AsString();

      var ee = doc[BSON_FLD_EXCEPTION] as BSONDocumentElement;
      if (ee == null) return;

      m_Exception = WrappedException.MakeFromBSON(serializer, ee.Value);
    }
    #endregion
  }

  public static class MessageExtensions
  {
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
