using System;
using System.Linq;
using System.Threading;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Instrumentation;
using Azos.Log;

namespace Azos.Sky.Locking.Server
{
  /// <summary>
  /// Implements lock server. Usually this service is activated by Zone gov process
  /// </summary>
  public sealed class LockServerService : ServiceWithInstrumentationBase<object>, Contracts.ILocker
  {
    #region CONSTS
       public const string THREAD_NAME = "LockServerService";

       public const string CONFIG_LOCK_SERVER_SECTION = "lock-server";

       public const int DEFAULT_SESSION_MAX_AGE_SEC = 60;
       public const int MIN_SESSION_MAX_AGE_SEC = 1;

    #endregion

    #region .ctor
       public LockServerService(object director) : base(director)
       {

       }
    #endregion

    #region Fields
      private Thread m_Thread;
      private AutoResetEvent m_WaitEvent;

      private DateTime m_StartTimeUTC;
      private double m_TrustLevel;


      private double m_ServerCallsNorm;
      private int m_CurrentServerCalls;

      private int m_DefaultSessionMaxAgeSec = DEFAULT_SESSION_MAX_AGE_SEC;

      private Registry<ServerLockSession> m_Sessions = new Registry<ServerLockSession>();
      private Registry<Namespace> m_Namespaces = new Registry<Namespace>();

      private bool m_InstrumentationEnabled;
      private bool m_DetailedTableInstrumentation;

      private NamedInterlocked m_stat_ExecuteTranCalls = new NamedInterlocked();
      private int m_stat_EndLockSessionCalls;
      private long m_stat_ExpiredRecords;
      private int m_stat_ExpiredSessions;
      private int m_stat_RemovedEmptyTables;

    #endregion

    #region Properties



        /// <summary>
        /// Implements IInstrumentable
        /// </summary>
        [Config(Default=false)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOCKING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public override bool InstrumentationEnabled
        {
          get { return m_InstrumentationEnabled;}
          set
          {
             if (m_InstrumentationEnabled!=value)
             {
               m_InstrumentationEnabled = value;

               m_stat_ExecuteTranCalls.Clear();
               m_stat_EndLockSessionCalls = 0;
               m_stat_ExpiredRecords = 0;
               m_stat_ExpiredSessions = 0;
               m_stat_RemovedEmptyTables = 0;
             }
          }
        }


        /// <summary>
        /// When true, instruments every table
        /// </summary>
        [Config(Default=false)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOCKING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public bool DetailedTableInstrumentation
        {
          get { return m_DetailedTableInstrumentation;}
          set { m_DetailedTableInstrumentation = value; }
        }


      /// <summary>
      /// Returns when server started
      /// </summary>
      public DateTime StartTimeUTC{ get{ return m_StartTimeUTC;}}


      /// <summary>
      /// Returns the current norm for the number of calls - the trust level goes down when
      ///  server experiences a sharp call drop
      /// </summary>
      public int CurrentServerCallsNorm{ get{ return (int)m_ServerCallsNorm;}}

      /// <summary>
      /// Returns the current trust level 0.0 .. 1.0 of this server
      /// </summary>
      public double CurrentTrustLevel{ get{ return m_TrustLevel; }}

      /// <summary>
      /// Default session maximum age in seconds
      /// </summary>
      [Config(Default=DEFAULT_SESSION_MAX_AGE_SEC)]
      public int DefaultSessionMaxAgeSec
      {
        get{ return m_DefaultSessionMaxAgeSec;}
        set{ m_DefaultSessionMaxAgeSec = value < MIN_SESSION_MAX_AGE_SEC ? MIN_SESSION_MAX_AGE_SEC : value; }
      }

    #endregion

    #region Public

        public LockTransactionResult ExecuteLockTransaction(LockSessionData session, LockTransaction transaction)
        {
          var result = executeLockTransaction(session, transaction);

          if (m_InstrumentationEnabled)
          {
           var key = result.Status==LockStatus.TransactionOK ? "OK" //for speed not to concat strings
                                                             : result.Status.ToString() + ':' + result.ErrorCause.ToString();
           m_stat_ExecuteTranCalls.IncrementLong(key);
          }

          return result;
        }

        private LockTransactionResult executeLockTransaction(LockSessionData session, LockTransaction transaction)
        {
          if (!Running) return LockTransactionResult.CallFailed;

          Interlocked.Increment(ref m_CurrentServerCalls);

          if (session==null || transaction==null)
           throw new LockingException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ExecuteLockTransaction(session|transaction==null)");

          var isPing = transaction.Statements == null;

          if (!isPing && transaction.Namespace.IsNullOrWhiteSpace())
           throw new LockingException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ExecuteLockTransaction(transaction.Namespace==null|empty)");

          var currentTrustLevel = CurrentTrustLevel;
          var appRunTimeSec = (uint)(DateTime.UtcNow - m_StartTimeUTC).TotalSeconds;

          //insufficient runtime period length or trust level
          if (transaction.MinimumRequiredRuntimeSec>appRunTimeSec ||
              transaction.MinimumRequiredTrustLevel>currentTrustLevel)
             return new LockTransactionResult(transaction.ID,
                                            SkySystem.HostName,
                                            LockStatus.TransactionError,
                                            LockErrorCause.MinimumRequirements,
                                            null,
                                            appRunTimeSec,
                                            currentTrustLevel,
                                            null);


          var sid = session.ID.ToString();
          var ss = m_Sessions.GetOrRegister(sid, (sd) => new ServerLockSession(sd), session);
          lock(ss)
          {
            if (ss.Disposed)
             return new LockTransactionResult(transaction.ID,
                                            SkySystem.HostName,
                                            LockStatus.TransactionError,
                                            LockErrorCause.SessionExpired,
                                            null,
                                            appRunTimeSec,
                                            currentTrustLevel,
                                            null);

            ss.m_LastInteractionUTC = App.TimeSource.UTCNow;

            if (isPing)//ping just touches session above
             return new LockTransactionResult(transaction.ID,
                                            SkySystem.HostName,
                                            LockStatus.TransactionOK,
                                            LockErrorCause.Unspecified,
                                            null,
                                            appRunTimeSec,
                                            currentTrustLevel,
                                            null);

            var ns = m_Namespaces.GetOrRegister<object>(transaction.Namespace, (_) => new Namespace(transaction.Namespace), null);

            var ectx = new EvalContext(ss, ns, transaction);

            prepareTransaction( ectx ); //prepare is not under the lock

            LockTransactionResult result;

            lock(ns)              //execute is UNDER THE LOCK
              result = executeTransaction( ectx, appRunTimeSec, currentTrustLevel );
            return result;
          }
        }

        public bool EndLockSession(LockSessionID sessionID)
        {
          if (!Running) return false;

          Interlocked.Increment(ref m_CurrentServerCalls);


          var sid = sessionID.ToString();
          var ss = m_Sessions[sid];
          if (ss==null) return false;


          if (m_InstrumentationEnabled)
             Interlocked.Increment( ref m_stat_EndLockSessionCalls);


          lock(ss)
          {
            //note: no need to update last interaction time
            m_Sessions.Unregister(ss);
            if (ss.Disposed) return false;

            ss.Dispose();
            return true;
          }
        }

    #endregion

    #region Protected

        protected override void DoConfigure(IConfigSectionNode node)
        {
          base.DoConfigure(node);
        }

        protected override void DoStart()
        {
            m_StartTimeUTC = DateTime.UtcNow;
            m_CurrentServerCalls = 0;
            m_ServerCallsNorm = 0.0d;

            m_WaitEvent = new AutoResetEvent(false);

            m_Thread = new Thread(threadSpin);
            m_Thread.Name = THREAD_NAME;
            m_Thread.Start();
        }

        protected override void DoWaitForCompleteStop()
        {
            m_TrustLevel = 0d;

            m_WaitEvent.Set();

            m_Thread.Join();
            m_Thread = null;

            m_WaitEvent.Close();
            m_WaitEvent = null;

            m_Sessions.Clear();
        }

    #endregion

    #region .pvt

      private void prepareTransaction(EvalContext ctx)
      {
         var statements = ctx.Transaction.Statements;
         for(var i=0; i< statements.Length; i++)
         {
           var statement = statements[i];
           statement.Prepare( ctx, i.ToString() + ":/");
         }
      }

      //warning: this is called under the global lock on NS, do not block
      private LockTransactionResult executeTransaction(EvalContext ctx, uint appRunTimeSec, double trustLevel)
      {
         var status = LockStatus.TransactionOK;
         var errorCause = LockErrorCause.Unspecified;

         var statements = ctx.Transaction.Statements;

         for(var i=0; i<statements.Length; i++)
         {
           var statement = statements[i];
           statement.Execute( ctx );
           if (ctx.Aborted)
           {
             status = LockStatus.TransactionError;
             errorCause = LockErrorCause.Statement;
             break;
           }
         }

         //now Commit/Rollback all changes
         if (status==LockStatus.TransactionOK)
         {//commit
           foreach(var tbl in ctx.Namespace.m_MutatedTables)
            tbl.Commit(ctx.Session);
         }
         else
         {//rollback
           foreach(var tbl in ctx.Namespace.m_MutatedTables)
            tbl.Rollback(ctx.Session);
         }

         ctx.Namespace.m_MutatedTables.Clear();


         return new LockTransactionResult(ctx.Transaction.ID,
                                          SkySystem.HostName,
                                          status,
                                          errorCause,
                                          ctx.FailedStatement,
                                          appRunTimeSec,//passed-in not to cacluate under lock
                                          trustLevel,
                                          ctx.Data);
      }


        private void threadSpin()
        {
            const string FROM = "threadSpin()";
            const int THREAD_GRANULARITY_MS = 3570;//this deals with trust level


            while (Running)
            {
                try
                {
                  var now = App.TimeSource.UTCNow;

                  //todo handle individual errors
                  updateTrustLevel();

                  removeExpiredTableData(now);

                  removeExpiredSessions(now);

                  dumpStats();

                  m_WaitEvent.WaitOne(THREAD_GRANULARITY_MS);
                }
                catch(Exception error)
                {
                  log(MessageType.CatastrophicError, FROM, error.ToMessageWithType(), error);
                }
            }

        }

        private void updateTrustLevel()
        {
           const double NORM_DECAY_FACTOR = 0.8321d;

            var currentCalls  = (double)Interlocked.Exchange(ref m_CurrentServerCalls, 0);
            if (currentCalls > m_ServerCallsNorm)
            {
              m_ServerCallsNorm = currentCalls;//norm is INSTANTLY adjusted to the number of calls made
            }
            else
            {
              m_ServerCallsNorm *= NORM_DECAY_FACTOR;
            }

            if (m_ServerCallsNorm>1.0d)
            {
              var drop =  m_ServerCallsNorm - currentCalls;
              var ratio = drop / m_ServerCallsNorm;

              m_TrustLevel = 1.0d - ratio;

              if (m_TrustLevel>1.0d) m_TrustLevel = 1.0d;
            }
            else
            {
              m_ServerCallsNorm = 0.0d;
              m_TrustLevel = 1.0d;
            }

            if (m_TrustLevel < 0.9d)
              App.Random.FeedExternalEntropySample( (int)( m_TrustLevel * 1378459007 )  ); //0.0 .. 0.99  *  some odd number 1b+
        }

        private void removeExpiredTableData(DateTime now)
        {
          long totalRemoved = 0;
          foreach( var ns in m_Namespaces)//thread-safe snapshot
          {
            if (!Running) return;

            lock(ns)
            {
              foreach(var tbl in ns.Tables)
              {
                if (!Running) return;

                totalRemoved += tbl.RemoveExpired( now );

                if (ns.RemoveTableIfEmpty( tbl ))//does not affect the foreach as it is snapshot based
                 if (m_InstrumentationEnabled)
                  Interlocked.Increment(ref m_stat_RemovedEmptyTables);
              }
            }
          }


          if (m_InstrumentationEnabled)
           Interlocked.Add(ref m_stat_ExpiredRecords, totalRemoved);
        }


        private void removeExpiredSessions(DateTime now)
        {
          int totalTimedOut = 0;
          foreach( var session in m_Sessions)//thread-safe snapshot
          {
            if (!Running) return;

            if (Monitor.TryEnter(session))
              try
              {
                var sessionMaxAge = session.Data.MaxAgeSec.HasValue ? session.Data.MaxAgeSec.Value : this.m_DefaultSessionMaxAgeSec;
                if ((now - session.m_LastInteractionUTC).TotalSeconds > sessionMaxAge)//session times out
                {
                  m_Sessions.Unregister( session );
                  session.Dispose();
                  totalTimedOut++;
                }
              }
              finally
              {
                Monitor.Exit(session);
              }
          }

          if (m_InstrumentationEnabled)
           Interlocked.Increment(ref m_stat_ExpiredSessions);

        }


        private void dumpStats()
        {
          if (!App.Instrumentation.Enabled || !m_InstrumentationEnabled) return;

          foreach(var src in m_stat_ExecuteTranCalls.AllNames)
            App.Instrumentation.Record( new Instrumentation.ServerLockTransactions(src, m_stat_ExecuteTranCalls.ExchangeLong(src, 0)) );

          App.Instrumentation.Record( new Instrumentation.ServerEndLockSessionCalls( Interlocked.Exchange(ref m_stat_EndLockSessionCalls, 0)) );

          App.Instrumentation.Record( new Instrumentation.ServerExpiredRecords( Interlocked.Exchange(ref m_stat_ExpiredRecords, 0)) );
          App.Instrumentation.Record( new Instrumentation.ServerExpiredSessions( Interlocked.Exchange(ref m_stat_ExpiredSessions, 0)) );
          App.Instrumentation.Record( new Instrumentation.ServerRemovedEmptyTables( Interlocked.Exchange(ref m_stat_RemovedEmptyTables, 0)) );

          App.Instrumentation.Record( new Instrumentation.ServerTrustLevel( m_TrustLevel )  );
          App.Instrumentation.Record( new Instrumentation.ServerCallsNorm( m_ServerCallsNorm ) );

          if (m_DetailedTableInstrumentation)
            foreach(var ns in m_Namespaces)
            {
               App.Instrumentation.Record( new Instrumentation.ServerNamespaceTables(ns.Name, ns.Tables.Count ) );
               foreach(var tbl in ns.Tables)
                App.Instrumentation.Record( new Instrumentation.ServerNamespaceTableRecordCount( ns.Name+"::"+tbl.Name, tbl.TotalRecordCount ) );
            }
        }


        internal void log(MessageType type, string from, string text, Exception error = null, Guid? related = null)
        {
           var msg = new Log.Message
              {
                 Type = type,
                 Topic = SysConsts.LOG_TOPIC_LOCKING,
                 From = "{0}.{1}".Args(GetType().FullName, from),
                 Text = text,
                 Exception = error
              };

              if (related.HasValue) msg.RelatedTo = related.Value;

           App.Log.Write( msg );
        }
    #endregion

  }

}
