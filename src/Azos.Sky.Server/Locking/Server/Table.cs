/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Collections;

namespace Azos.Sky.Locking.Server
{
    /// <summary>
    /// 2 VarLists in tentative and committed states
    /// </summary>
    internal sealed class DataSlot
    {

       private bool m_Changing;
       private VarList m_Data;
       private VarList m_RollbackData;

       public VarList Data{ get{ return m_Data;}}

       public bool Changing { get{ return m_Changing;}}

       public void Change(VarList data)
       {
         if (!m_Changing)
         {
           m_RollbackData = m_Data;
           m_Changing = true;
         }
         m_Data = data;
       }

       /// <summary>
       /// Returns how many records have added/removed since change started
       /// </summary>
       public int Commit()
       {
         if (!m_Changing) throw new LockingException(StringConsts.INTERNAL_IMPLEMENTATION_ERROR + "DataSlot.Commit(!changing)");

         var nowCount = m_Data!=null? m_Data.Count : 0;
         var wasCount = m_RollbackData!=null? m_RollbackData.Count : 0;

         m_RollbackData = null;
         m_Changing = false;

         return nowCount - wasCount;
       }

       public void Rollback()
       {
         if (!m_Changing) throw new LockingException(StringConsts.INTERNAL_IMPLEMENTATION_ERROR + "DataSlot.Rollback(!changing)");
         m_Data = m_RollbackData;
         m_RollbackData = null;
         m_Changing = false;
       }

    }

    /// <summary>
    /// Represents an IMMUTABLE variable kept on a lock server
    /// </summary>
    [Serializable]
    public sealed class Variable
    {
      internal Variable(
                         LockSessionID sessionID,
                         Guid tranID,
                         DateTime setTimeUTC,
                         DateTime? expirationTimeUTC,
                         string description,
                         object value
                       )
      {
        SessionID = sessionID;
        TranID = tranID;
        SetTimeUTC = setTimeUTC;
        ExpirationTimeUTC = expirationTimeUTC;
        Description = description;
        Value = value;
      }

      public readonly LockSessionID SessionID;
      public readonly Guid TranID;
      public readonly DateTime SetTimeUTC;
      public readonly DateTime? ExpirationTimeUTC;
      public readonly string Description;
      public readonly object Value;
    }


    internal sealed class VarList : List<Variable>
    {
      public VarList() : base() {}
      public VarList(VarList existing) : base(existing){}
    }




  /// <summary>
  /// Represents a table of locking server namespace.
  /// This class is not thread-safe for tran execution as there is only 1 worker thread that executes the transaction graphs at any given time
  /// </summary>
  internal sealed class Table : INamed
  {

    internal Table(Namespace ns, string name)
    {
      if (ns==null || name.IsNullOrWhiteSpace())
        throw new LockingException(StringConsts.ARGUMENT_ERROR+"Table.ctor(ns==null | name==null|empty)");

      m_Namespace = ns;
      m_Name = name;
    }

    private Namespace m_Namespace;
    private string m_Name;
    internal Dictionary<string, DataSlot> m_Data = new Dictionary<string, DataSlot>(1024, StringComparer.Ordinal);

    private Dictionary<ServerLockSession, HashSet<DataSlot>> m_SessionOwnedData = new Dictionary<ServerLockSession, HashSet<DataSlot>>();


    private List<DataSlot> m_PendingChanges = new List<DataSlot>();

    private int m_TotalRecordCount;


    public Namespace Namespace => m_Namespace;
    public string Name => m_Name;
    public IApplication App => m_Namespace.App;

    /// <summary>
    /// Returns the number of slots in the table
    /// </summary>
    public int Count { get{ return m_Data.Count; }}

    /// <summary>
    /// Returns the total number of committed Variable records in all slots.
    /// This property is THREAD safe (i.e. for instrumentation)
    /// </summary>
    public int TotalRecordCount{ get{ return m_TotalRecordCount;}}


    internal void Commit(ServerLockSession session)
    {
      foreach(var ds in m_PendingChanges)//may have duplicates
        if (ds.Changing)
        {
          var deltaRecords = ds.Commit();

          m_TotalRecordCount += deltaRecords;

          HashSet<DataSlot> set;
          if (!m_SessionOwnedData.TryGetValue(session, out set))
          {
            set = new HashSet<DataSlot>();
            m_SessionOwnedData[session] = set;
          }
          set.Add( ds );

          session.m_MutatedNamespaces.Add( m_Namespace );
        }

      m_PendingChanges.Clear();
    }

    internal void Rollback(ServerLockSession session)
    {
      foreach(var ds in m_PendingChanges)//may have duplicates
        if (ds.Changing) ds.Rollback();
      m_PendingChanges.Clear();
    }

    public void EndSession(ServerLockSession session)
    {
      HashSet<DataSlot> set;
      if (!m_SessionOwnedData.TryGetValue(session, out set)) return;
      foreach(var slot in set)
       m_TotalRecordCount -= slot.Data.RemoveAll( v => v.SessionID.Equals(session.Data.ID) );

      m_SessionOwnedData.Remove( session );
    }


    /// <summary>
    /// Returns true if named variable exists created by this or another session
    /// </summary>
    public bool Exists(EvalContext ctx, string name, object value, bool ignoreThisSession)
    {
      DataSlot slot;
      if (!m_Data.TryGetValue(name, out slot)) return false;

      var list = slot.Data;
      if (list==null) return false;

      if (!ignoreThisSession)
      {
        if (value==null) return list.Count>0;
        return list.Any(e => e.Value!=null && value.Equals(e.Value));
      }

      for(var i=0; i<list.Count; i++)
      {
        var var = list[i];
        if (!var.SessionID.Equals(ctx.SessionID))
        {
          if (value==null) return true;
          if (var.Value!=null && var.Value.Equals(value)) return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns named variable value or value array created by this or another session.
    /// If not found then null is returned
    /// </summary>
    public object GetVariable(bool many, EvalContext ctx, string name, bool ignoreThisSession)
    {
      DataSlot slot;
      if (!m_Data.TryGetValue(name, out slot)) return null;

      var list = slot.Data;
      if (list==null) return null;

      if (many)
      {
        Variable[] result;

        if (!ignoreThisSession)
          result = list.ToArray();
        else
          result = list.Where(i => !i.SessionID.Equals(ctx.SessionID)).ToArray();

        return result.Length>0 ? result : null;
      }
      //else Single
      if (list.Count==0) return null;
      if (!ignoreThisSession) return list[0];

      return list.FirstOrDefault(i => !i.SessionID.Equals(ctx.SessionID));
    }



    /// <summary>
    /// Sets named var
    /// </summary>
    public bool SetVariable(EvalContext ctx, string name, object value, string description, DateTime? expirationTimeUTC, bool allowDuplicates)
    {
      DataSlot slot;
      VarList list;
      if (!m_Data.TryGetValue(name, out slot))
      {
        slot = new DataSlot();
        list = new VarList();

        var var = new Variable(ctx.SessionID,
                               ctx.Transaction.ID,
                               App.TimeSource.UTCNow,
                               expirationTimeUTC,
                               getDescr(description, ctx),
                               value);
        list.Add( var );
        slot.Change( list );

        m_Data.Add(name, slot);
        m_PendingChanges.Add( slot );
        m_Namespace.m_MutatedTables.Add( this );
        return true;
      }

      list = slot.Data;

      if (list==null)
      {
        list = new VarList();

        var var = new Variable(ctx.SessionID,
                               ctx.Transaction.ID,
                               App.TimeSource.UTCNow,
                               expirationTimeUTC,
                               getDescr(description, ctx),
                               value);
        list.Add( var );
        slot.Change( list );

        //20161102
        //m_Data.Add(name, slot);
        m_PendingChanges.Add( slot );
        m_Namespace.m_MutatedTables.Add( this );
        return true;
      }

      if (list.Count>0 && !allowDuplicates) return false;

      var newList = new VarList(list);

      var replaced = false;
      for(var i=0; i<newList.Count; i++)
      {
        var var = newList[i];
        if (var.SessionID.Equals(ctx.SessionID))
        {
          var nvar = new Variable(ctx.SessionID,
                                  ctx.Transaction.ID,
                                  App.TimeSource.UTCNow,
                                  expirationTimeUTC,
                                  getDescr(description, ctx),
                                  value);
          newList[i] = nvar;
          replaced = true;
          break;
        }
      }

      if (!replaced)
      {
       var nvar = new Variable(ctx.SessionID,
                               ctx.Transaction.ID,
                               App.TimeSource.UTCNow,
                               expirationTimeUTC,
                               getDescr(description, ctx),
                               value);
       newList.Add( nvar );
      }
      slot.Change( newList );
      m_PendingChanges.Add( slot );
      m_Namespace.m_MutatedTables.Add( this );
      return true;
    }


     /// <summary>
    /// Deletes named var, true if found and removed
    /// </summary>
    public bool DeleteVariable(EvalContext ctx, string name, object value)
    {
      DataSlot slot;
      if (!m_Data.TryGetValue(name, out slot))
        return false;

      var list = slot.Data;
      if (list==null) return false;

      var anymatch = list.Any( v => v.SessionID.Equals(ctx.SessionID) && (value==null || value.Equals(v.Value)));
      if (!anymatch) return false;

      VarList newList = new VarList();
      for(var i=0; i<list.Count; i++)
      {
        var var = list[i];
        if (var.SessionID.Equals(ctx.SessionID) && (value==null || value.Equals(var.Value)))
          continue;//skip what gets deleted

        newList.Add( var );
      }
      slot.Change( newList );
      m_PendingChanges.Add( slot );
      m_Namespace.m_MutatedTables.Add( this );
      return true;
    }

    /// <summary>
    /// Called on a server thread UNDER LOCK to purge outdated data
    /// </summary>
    public int RemoveExpired(DateTime now)
    {
      var removed = 0;
      foreach(var kvp in m_Data)
      {
        var lst = kvp.Value.Data;
        if (lst==null) continue;
        for(var i=0; i<lst.Count; )
        {
          var var = lst[i];
          var exp = var.ExpirationTimeUTC;
          if (exp.HasValue && exp.Value < now)
          {
            lst.RemoveAt(i);
            removed++;
          }
          else
            i++;
        }
      }
      return removed;
    }


    private string getDescr(string descr, EvalContext ctx)
    {
      if (descr.IsNullOrWhiteSpace())
        descr = ctx.Transaction.Description;

      if (descr.IsNullOrWhiteSpace())
        descr = ctx.SessionDescription;

      return descr;
    }

  }

}
