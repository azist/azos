using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;

namespace Azos.Sky.Log.Server
{
  /// <summary>
  /// Represents a base for entities that archive log data
  /// </summary>
  public abstract class LogArchiveStore : ApplicationComponent
  {
    protected LogArchiveStore(LogReceiverService director, LogArchiveDimensionsMapper mapper, IConfigSectionNode node) : base(director)
    {
      ConfigAttribute.Apply(this, node);
      m_Mapper = mapper;
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Mapper);
    }

    private LogArchiveDimensionsMapper m_Mapper;

    /// <summary>
    /// Maps archive dimensions to/from model of the particular business system
    /// </summary>
    public LogArchiveDimensionsMapper Mapper { get { return m_Mapper; } }

    /// <summary>
    /// References service that this store is under
    /// </summary>
    public LogReceiverService ArchiveService { get { return (LogReceiverService)ComponentDirector;} }

    /// <summary>
    /// Returns log message by ID
    /// </summary>
    public virtual Message GetByID(Guid id, string channel = null)
    {
      Message result;
      if (!TryGetByID(id, out result, channel))
        throw new LogArchiveException(StringConsts.LOG_ARCHIVE_MESSAGE_NOT_FOUND_ERROR.Args(id));
      return result;
    }

    /// <summary>
    /// Starts transaction represented by return object
    /// </summary>
    public abstract object BeginTransaction();

    /// <summary>
    /// Commits the transaction started with BeginTransaction
    /// </summary>
    public abstract void CommitTransaction(object transaction);

    /// <summary>
    /// Rolls back the transaction started with BeginTransaction
    /// </summary>
    public abstract void RollbackTransaction(object transaction);

    /// <summary>
    /// Writes message to the store within transaction context
    /// </summary>
    public abstract void Put(Message message, object transaction);

    /// <summary>
    /// Tries to fetch message by ID. Returns true if found
    /// </summary>
    public abstract bool TryGetByID(Guid id, out Message message, string channel = null);

    /// <summary>
    /// Returns enumerable of messages according to dimension filter in laconic format
    /// </summary>
    public abstract IEnumerable<Message> List(string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string channel = null, string topic = null,
      Guid? relatedTo = null,
      int skipCount = 0);



    protected Guid Log(MessageType type,
                       string from,
                       string message,
                       Exception error = null,
                       Guid? relatedMessageID = null,
                       string parameters = null)
    {
      return ArchiveService.Log(type, "{0}.{1}".Args(GetType().Name, from), message, error, relatedMessageID, parameters);
    }
  }
}
