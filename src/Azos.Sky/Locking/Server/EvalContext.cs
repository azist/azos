using System.Collections.Generic;


namespace Azos.Sky.Locking.Server
{
  /// <summary>
  /// Holds context information during execution of LockTransaction graph
  /// </summary>
  public sealed class EvalContext
  {
     internal EvalContext(ServerLockSession session, Namespace ns, LockTransaction tran)
     {
       Session = session;
       Namespace = ns;
       Transaction = tran;
     }

     private bool m_Aborted;
     private string m_FailedStatement;
     private List<KeyValuePair<string, object>> m_Data;


     /// <summary>
     /// Lock Session that this request is under
     /// </summary>
     internal readonly ServerLockSession Session;

     /// <summary>
     /// ID of a session that this request is under
     /// </summary>
     public LockSessionID SessionID { get{ return Session.ID;}}

     /// <summary>
     /// Description of the session that this request is under
     /// </summary>
     public string SessionDescription { get { return Session.Data.Description; } }

     /// <summary>
     /// Returns the transaction being evaluated
     /// </summary>
     public readonly LockTransaction Transaction;


     internal readonly Namespace Namespace;


     /// <summary>
     /// Returns true when graph evaluation was aborted.
     /// This flag is checked by all operations instead of throwing exception which is much slower
     /// </summary>
     public bool Aborted { get { return m_Aborted;}    }

     /// <summary>
     /// If aborted, returns the description of failed statement
     /// </summary>
     public string FailedStatement { get{ return m_FailedStatement;}}

     /// <summary>
     /// Aborts the evaluation of lock tran graph. Conceptually similar to throw, however is 8-10x faster
     /// </summary>
     public void Abort(string failedStatement)
     {
        m_FailedStatement = failedStatement;
        m_Aborted = true;
     }

     /// <summary>
     /// Resets abort condition
     /// </summary>
     public void ResetAbort()
     {
       m_FailedStatement = null;
       m_Aborted = false;
     }


     public KeyValuePair<string, object>[] Data {get{ return m_Data==null ? null : m_Data.ToArray();}}

     /// <summary>
     /// Adds named key with value to result list
     /// </summary>
     public void AddData(string key, object value)
     {
         if (m_Data==null) m_Data = new List<KeyValuePair<string,object>>(32);
         m_Data.Add( new KeyValuePair<string,object>(key, value) );
     }


     internal Table GetExistingOrMakeTableByName(string name)
     {
       return Namespace.GetExistingOrMakeTableByName( name );
     }


  }
}
