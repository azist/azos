//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Azos.Client
//{
//  /// <summary>
//  /// Helper class that manages the lifetime of particular type transport instances
//  /// </summary>
//  public abstract class TransportPool<TService, TTransport> : DisposableObject where TService : class, IServiceImplementation
//                                                                               where TTransport : ITransportImplementation
//  {
//    public TransportPool(TService service)
//    {
//      m_Service = service.NonNull(nameof(service));
//    }

//    protected override void Destructor()
//    {
//      base.Destructor();
//    }

//    private TService m_Service;
//    private volatile List<TTransport> m_Transports = new List<TTransport>();
//    private Platform.KeyedMonitor<EndpointAssignment> m_Locks = new Platform.KeyedMonitor<EndpointAssignment>();

//    public TService Service => m_Service;


//    public TTransport Acquire(EndpointAssignment assignment)
//    {
//      EnsureObjectNotDisposed();
//      var transport = TryGetExistingAcquiredTransport(assignment);
//      if (transport != null) return transport;

//      //otherwise we need to create a new transport
//      if (m_MaxCount > 0)
//      {
//        m_Locks.Enter(assignment);
//        try
//        {
//          transport = TryGetExistingAcquiredTransport(assignment);
//          if (transport != null) return transport;
//          return MakeNew(assignment);
//        }
//        finally
//        {
//          m_Locks.Exit(assignment);
//        }
//      }

//      //otherwise make new
//      return MakeNew(assignment);
//    }

//    protected TTransport MakeNew(EndpointAssignment assignment)
//    {
//      var transport = DoMakeNew(assignment);
//      //todo: Add to global transport list
//      return transport;
//    }

//    /// <summary>
//    /// Override to create new instance of transport
//    /// </summary>
//    protected abstract TTransport DoMakeNew(EndpointAssignment assignment);


//    /// <summary>
//    /// Tries to acquire an available transport to make a call.
//    /// This method respects binding/transport settings that impose a limit on the number of
//    ///  open concurrent transports and timeouts for acquisition waiting
//    /// </summary>
//    /// <param name="assignment">endpoint assignment</param>
//    /// <returns>Available acquired existing transport or null</returns>
//    protected virtual TTransport TryGetExistingAcquiredTransport(EndpointAssignment assignment)
//    {
//      int GRANULARITY_MS = 5 + ((System.Threading.Thread.CurrentThread.GetHashCode() & CoreConsts.ABS_HASH_MASK) % 15);

//      var elapsed = 0;
//      do
//      {
//        var lst = m_Transports;//atomic
//        var count = 0;//count transports PER DESTINATION
//        for (var i = 0; i < lst.Count; i++)//the loop is faster than intermediary enumerables produced by LINQ
//        {
//          var tr = lst[i] as ClientTransport;
//          if (tr == null) continue;
//          if (!AreNodesIdentical(tr.Node, remoteNode)) continue;
//          count++;//Per destination
//          if (tr.TryAcquire())
//            return tr;
//        }

//        if (m_ClientTransportMaxCount <= 0 || count < m_ClientTransportMaxCount)
//        {
//          if (count <= m_ClientTransportCountWaitThreshold) return null;//dont wait - create new one
//          if (elapsed >= m_ClientTransportExistingAcquisitionTimeoutMs) return null;
//        }

//        System.Threading.Thread.Sleep(GRANULARITY_MS);
//        elapsed += GRANULARITY_MS;

//        if (m_ClientTransportMaxExistingAcquisitionTimeoutMs > 0 && elapsed > m_ClientTransportMaxExistingAcquisitionTimeoutMs)
//          throw new ClientCallException(CallStatus.Timeout,
//                                        StringConsts.GLUE_CLIENT_CALL_TRANSPORT_ACQUISITION_TIMEOUT_ERROR
//                                        .Args(this.Name, elapsed)
//                                        );

//      } while (App.Active && Running);

//      return null;
//    }

//  }
//}
