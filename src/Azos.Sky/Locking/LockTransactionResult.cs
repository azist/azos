using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Denotes statuses of transaction execution
  /// </summary>
  public enum LockStatus
  {
    /// <summary>
    /// The call could not be placed/did not return success
    /// </summary>
    CallFailed=0,

    /// <summary>
    /// The requested transaction could not be executed as there are locking conflicts
    /// </summary>
    TransactionError,

    /// <summary>
    /// The requested transaction was executed OK
    /// </summary>
    TransactionOK
  }


  /// <summary>
  /// Provides transaction execution error causes
  /// </summary>
  public enum LockErrorCause
  {
    /// <summary>
    /// Unknown/unspecified cause
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// The part of transaction statement graph failed, see FailedStatement for details
    /// </summary>
    Statement,

    /// <summary>
    /// The requirements for transaction execution are not met
    /// </summary>
    MinimumRequirements,

    /// <summary>
    /// The requested transaction was executed OK
    /// </summary>
    SessionExpired
  }



  /// <summary>
  /// Represents a result of locking operation performed by ILockManager.
  /// If transaction selects some results then this class enumerates all key value pairs that may contain duplicate names
  /// </summary>
  [Serializable]
  public sealed class LockTransactionResult : IEnumerable<KeyValuePair<string, object>>
  {

    public static LockTransactionResult CallFailed
    {
       get { return new LockTransactionResult(Guid.Empty, null, LockStatus.CallFailed, LockErrorCause.Unspecified, null, 0, 0d, null); }
    }


    public LockTransactionResult(Guid tranID, string server, LockStatus status, LockErrorCause errorCause,  string failedStatement, uint runtimeSec, double trustLevel,  KeyValuePair<string, object>[] data)
    {
      TransactionID = tranID;
      ServerHost = server;
      Status = status;
      ErrorCause = errorCause;
      FailedStatement = failedStatement;
      ServerRuntimeSec = runtimeSec;
      ServerTrustLevel = trustLevel;
      m_Data = data;
    }

    //the array is used instead of dictionary for serialization speed
    private readonly KeyValuePair<string, object>[] m_Data;


    /// <summary>
    /// Returns the original transaction ID
    /// </summary>
    public readonly Guid TransactionID;

    /// <summary>
    /// Returns the host name of the host that serviced the call
    /// </summary>
    public readonly string ServerHost;

    /// <summary>
    /// Returns the status of the call: whether call failed or transaction could not be executed
    /// </summary>
    public readonly LockStatus Status;

    /// <summary>
    /// In case of error specifies the cause
    /// </summary>
    public readonly LockErrorCause ErrorCause;

    /// <summary>
    /// Returns the description of failed statement or null if transactions succeeded
    /// </summary>
    public readonly string FailedStatement;

    /// <summary>
    /// Returns for how many seconds the server has been running
    /// </summary>
    public readonly uint ServerRuntimeSec;

    /// <summary>
    /// Returns the coefficient of trust 0..1(maximum) of the server state.
    /// The value is computed based on the length of server uninterrupted runtime.
    /// This value plays an important role in speculative locking when lock server crashes, the caller may examine
    /// this returned value and based on it reject the fact that lock was taken, or the caller may specify
    /// the LockTransaction.MinimumRequiredTrustLevel for the transaction to succeed
    /// </summary>
    public readonly double ServerTrustLevel;

    /// <summary>
    /// Returns the count of returned variables
    /// </summary>
    public int Count {get{ return m_Data==null? 0 : m_Data.Length;}}

    /// <summary>
    /// Returns the first variable value by case-sensitive name. The var names may not be unique,
    /// as they are added during tran execution. Use instance of this class to enumerate all key value pairs
    /// </summary>
    public object this[string var]
    {
      get
      {
        if (m_Data==null) return null;
        for (var i=0; i<m_Data.Length; i++)
        {
          var kvp = m_Data[i];
          if (string.Equals(kvp.Key, var, StringComparison.Ordinal)) return kvp.Value;
        }
        return null;
      }
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      return m_Data==null ? Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator() : ((IEnumerable<KeyValuePair<string, object>>)m_Data).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return m_Data==null ? Enumerable.Empty<KeyValuePair<string, object>>().GetEnumerator() : m_Data.GetEnumerator();
    }

    public override string ToString()
    {
      return "{0}({1},'{2}','{3}')".Args(TransactionID, Status, ErrorCause, FailedStatement);
    }
  }




}
