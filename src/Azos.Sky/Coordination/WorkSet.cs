using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Sky.Locking;
using SrvVar=Azos.Sky.Locking.Server.Variable;

namespace Azos.Sky.Coordination
{

  /// <summary>
  /// Facilitates distributed processing coordination of the set of work which consists of TItems,
  /// by using lock manager to exchange the status with other workers working on the same set.
  /// Sets are identified by Name within the cluster Path.
  /// This class is abstract and must be inherited-from to specify the actual work partitioning -
  /// as required by the particular task (i.e. Process all users that...) by overriding AssignWorkSegment().
  /// The system retains the lock session for the duration of this instance existence, please dispose
  /// both the instance and the enumerable returned by GetEnumerator().
  /// This class IS NOT thread safe.
  /// </summary>
  public abstract class WorkSet<TItem> : ApplicationComponent, INamed, IEnumerable<TItem>
  {
    public const string WORKSET_NS = "~WORKSET~";

    protected WorkSet(string path, string name)
    {
      if (name.IsNullOrWhiteSpace())
        throw new CoordinationException(StringConsts.ARGUMENT_ERROR+"WorkSet.ctor(name=null|empty)");

      if (path.IsNullOrWhiteSpace())
        path = SkySystem.HostMetabaseSection.ParentZone.RegionPath;

      m_Path = path;
      m_Name = name;
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Session);
      base.Destructor();
    }



    private string m_Path;
    private string m_Name;
    private LockSession m_Session;



    private string m_Round;

    private int m_WorkerCount;
    private int m_MyIndex;

    protected int m_TotalWorkCount;
    protected int m_MyFirstWorkIndex = -1;
    protected int m_MyWorkCount;

    /// <summary>
    /// The cluster path/level where the workset is going to execute.
    /// The workset.Name is unique within this path
    /// </summary>
    public string Path { get { return m_Path;}}

    /// <summary>
    /// Returns the workset name which uniquely identifies the set at the cluster level
    /// </summary>
    public string Name { get { return m_Name;}}


    /// <summary>
    /// Override to specify the average anticipated time of one item processing
    /// </summary>
    public virtual int OneItemProcessingTimeSec { get { return 5;} }


    /// <summary>
    /// Specifies the duration of the work round - this is when a worker gets assigned a position in the set.
    /// The periodic repositioning is needed to ensure the 100% eventual coverage of the whole set as some workers may fail.
    /// It also controls how often the whole work set is Assigned into portions between workers
    /// </summary>
    public virtual int RoundDurationSec { get { return 15 * 60; }}


    /// <summary>
    /// Returns the total number of workers in the set including this one
    /// </summary>
    public int WorkerCount{ get{ return m_WorkerCount; }}

    /// <summary>
    /// Returns the index of this worker in the set
    /// </summary>
    public int MyIndex{ get{ return m_MyIndex; }}

    /// <summary>
    /// Returns the total number of work units int the set
    /// </summary>
    public int TotalWorkCount { get { return m_TotalWorkCount; }}


    /// <summary>
    /// Returns the index of the first work item that this worker is assigned to
    /// </summary>
    public int MyFirstWorkIndex { get { return m_MyFirstWorkIndex; }}

    /// <summary>
    /// Returns the number of work items to be processed by this worker starting at MyFirstWorkIndex
    /// </summary>
    public int MyWorkCount { get { return m_MyWorkCount; }}





    IEnumerator IEnumerable.GetEnumerator() {  return this.GetEnumerator();  }
    /// <summary>
    /// Enumerates the work items which are assigned just to this worker.
    /// The WorkSet class tries to coordinate the work of multiple workers so that they
    /// do not intersect
    /// </summary>
    public IEnumerator<TItem> GetEnumerator()
    {
      refresh();

      var hasWork = m_MyFirstWorkIndex >= 0 && m_MyWorkCount > 0;

      var source = hasWork ? GetSegmentEnumerator() : Enumerable.Empty<TItem>().GetEnumerator();
      return new enumerator(this, source);
    }


    /// <summary>
    /// Call this method when the instance is not enumerated for a long time.
    /// Enumeration refreshes the worker registration automatically.
    /// </summary>
    public void Touch()
    {
      try
      {
        refresh();
      }
      catch (Exception error)
      {
        throw new CoordinationException(error.ToMessageWithType(), error);
      }
    }

    /// <summary>
    /// Override to estimate the whole work by setting TotalWorkUnits,
    /// and depending on TotalWorkUnits, set ThisFirstUnitIndex, and ThisLastUnitIndex
    /// </summary>
    protected abstract void AssignWorkSegment();

    /// <summary>
    /// Override to provide source enumeration of work items which depends on  ThisFirstUnitIndex, ThisLastUnitIndex and TotalWorkUnits
    /// </summary>
    protected abstract IEnumerator<TItem> GetSegmentEnumerator();


            private class enumerator : DisposableObject, IEnumerator<TItem>
            {
              public enumerator(WorkSet<TItem> wset, IEnumerator<TItem> source)
              {
                m_WorkSet = wset;
                m_Source = source;
              }

              protected override void Destructor()
              {
                m_Source.Dispose();
              }

              private WorkSet<TItem> m_WorkSet;
              private IEnumerator<TItem> m_Source;
              private TItem m_Current;

              object IEnumerator.Current{ get{ return this.Current;} }

              public TItem Current{ get{ return m_Current;} }

              public bool MoveNext()
              {
                m_WorkSet.refresh();

                if (m_Source.MoveNext())
                {
                  m_Current = m_Source.Current;
                  return true;
                }

                return false;
              }

              public void Reset()
              {
                throw new NotSupportedException("WorkSet.enumerator.Reset()");
              }
            }

    private DateTime m_LastWorkAssign;
    private DateTime m_LastRegistration;

    private void refresh()
    {
      var now = App.TimeSource.UTCNow;


      var sinceReg = now - m_LastRegistration;
      var sinceAssign = now - m_LastWorkAssign;
      var needReg = sinceReg.TotalSeconds > 10 + App.Random.NextScaledRandomInteger(0, 20);
      var needAssign = sinceAssign.TotalSeconds > RoundDurationSec;

      if (needAssign)
       m_Round = ExternalRandomGenerator.Instance.NextRandomWebSafeString();

      //in case of lock server failure, we need to update who we are, where, and what we do
      if (needReg || needAssign)
      {
        m_LastRegistration = now;
        registerThisWorker();
      }
      else
        ensureSession();


      if (needAssign)
      {
        m_LastWorkAssign = now;
        AssignWorkSegment();
      }
    }

    private DateTime m_LastSession;

    private void ensureSession()
    {
      const int SESSION_DURATION_ITEMS = 10;

      var locker = SkySystem.LockManager;

      if (m_Session!=null)
      {
        var passed = App.TimeSource.UTCNow - m_LastRegistration;
        if (passed.TotalSeconds < ((SESSION_DURATION_ITEMS * 0.25) * OneItemProcessingTimeSec)) return;
        //just ping;
        var result = locker.ExecuteLockTransaction(m_Session, LockTransaction.PingAnyReliability);
        if (result.ErrorCause==LockErrorCause.Unspecified)
        {
          m_LastSession = App.TimeSource.UTCNow;
          return;
        }
        DisposeAndNull(ref m_Session);
      }

      var timeoutSec = SESSION_DURATION_ITEMS * OneItemProcessingTimeSec;
      var descr = "'{0}'@'{1}'".Args(Name, Path);
      m_Session = locker.MakeSession(Path, Name, descr, timeoutSec);
      m_LastSession = App.TimeSource.UTCNow;
    }

    private void registerThisWorker()
    {
      ensureSession();

      var table = this.GetType().AssemblyQualifiedName;

      var value = "{0}::{1}".Args(m_Round, SkySystem.HostName);//round must be the first - it ensures different sorting every time when round changes
      var descr = "{0} {1}".Args(GetType().Name, value);
      var script = new LockTransaction(descr, WORKSET_NS, 0, 0.0d,
              LockOp.SelectVarValue("Workers", table, Name, ignoreThisSession: true, abortIfNotFound: false, selectMany: true),
              LockOp.AnywayContinueAfter( LockOp.DeleteVar(table, Name), resetAbort: true ),
              LockOp.Assert( LockOp.SetVar(table, Name, value, allowDuplicates: true) )
      );
      var result = SkySystem.LockManager.ExecuteLockTransaction(m_Session, script);
      if (result.ErrorCause!=LockErrorCause.Unspecified)
      {
        //todo che delat?
        //Interumentatiopn + logging
        //Log()
      }

      var vars = result["Workers"] as SrvVar[];
      if (vars != null)
      {
        var values = new List<string>(vars.Select(v => v.Value.AsString()));
        values.Add(value);//add myself
        values.Sort(StringComparer.InvariantCulture);//must have pre-determined order

        m_WorkerCount = values.Count;
        m_MyIndex = values.IndexOf(value);//my index in the set
      }
      else
      {
        m_WorkerCount = 1;
        m_MyIndex = 0;
      }
    }
  }

}
