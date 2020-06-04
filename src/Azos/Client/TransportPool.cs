//////using System;
//////using System.Collections.Generic;
//////using System.Text;
//////using System.Threading.Tasks;

//////using Azos.Conf;

//////namespace Azos.Client
//////{
//////  /// <summary>
//////  /// Helper class that manages the lifetime of particular type transport instances
//////  /// </summary>
//////  public abstract class TransportPool<TService, TTransport> : DisposableObject where TService : class, IServiceImplementation
//////                                                                               where TTransport : ITransportImplementation
//////  {
//////    public TransportPool(TService service, IConfigSectionNode config)
//////    {
//////      m_Service = service.NonNull(nameof(service));
//////      ConfigAttribute.Apply(this, config);
//////    }

//////    protected override void Destructor()
//////    {
//////      base.Destructor();
//////    }

//////    private TService m_Service;
//////    private Dictionary<EndpointAssignment, List<TTransport>> m_Transports = new Dictionary <EndpointAssignment, List<TTransport>>();
//////    private Platform.KeyedMonitor<EndpointAssignment> m_Locks = new Platform.KeyedMonitor<EndpointAssignment>();

//////    private int m_MaxCount;

//////    /// <summary>
//////    /// When greater than zero, applies a limit on maximum transport count per endpoint. 0 = unlimited (default)
//////    /// </summary>
//////    [Config]
//////    public int MaxCount
//////    {
//////      get => m_MaxCount;
//////      set => m_MaxCount = value.KeepBetween(0, 128_000);
//////    }


//////    public TService Service => m_Service;


//////    public async Task<TTransport> AcquireAsync(EndpointAssignment assignment)
//////    {
//////      EnsureObjectNotDisposed();
//////      var transport = await TryGetExistingAcquiredTransportAsync(assignment);
//////      if (transport != null) return transport;

//////      //otherwise we need to create a new transport
//////      if (m_MaxCount > 0)
//////      {
//////        m_Locks.Enter(assignment);
//////        try
//////        {
//////          transport = await TryGetExistingAcquiredTransportAsync(assignment);
//////          if (transport != null) return transport;
//////          return await MakeNewAsync(assignment);
//////        }
//////        finally
//////        {
//////          m_Locks.Exit(assignment);
//////        }
//////      }

//////      //otherwise make new
//////      return await MakeNewAsync(assignment);
//////    }

//////    protected async Task<TTransport> MakeNewAsync(EndpointAssignment assignment)
//////    {
//////      var transport = await DoMakeNewAsync(assignment);
//////      //todo: Add to global transport list
//////      return transport;
//////    }

//////    /// <summary>
//////    /// Override to create new instance of transport
//////    /// </summary>
//////    protected abstract Task<TTransport> DoMakeNewAsync(EndpointAssignment assignment);


//////    /// <summary>
//////    /// Tries to acquire an available transport to make a call.
//////    /// This method respects binding/transport settings that impose a limit on the number of
//////    ///  open concurrent transports and timeouts for acquisition waiting
//////    /// </summary>
//////    /// <param name="assignment">endpoint assignment</param>
//////    /// <returns>Available acquired existing transport or null</returns>
//////    protected virtual async Task<TTransport> TryGetExistingAcquiredTransportAsync(EndpointAssignment assignment)
//////    {
//////      int GRANULARITY_MS = 5 + ((System.Threading.Thread.CurrentThread.GetHashCode() & CoreConsts.ABS_HASH_MASK) % 15);

//////      var elapsed = 0;
//////      do
//////      {
//////        var lst = m_Transports;//atomic
//////        var count = 0;//count transports PER DESTINATION
//////        for (var i = 0; i < lst.Count; i++)//the loop is faster than intermediary enumerables produced by LINQ
//////        {
//////          var tr = lst[i] as ClientTransport;
//////          if (tr == null) continue;
//////          if (!AreNodesIdentical(tr.Node, remoteNode)) continue;
//////          count++;//Per destination
//////          if (tr.TryAcquire())
//////            return tr;
//////        }

//////        if (m_ClientTransportMaxCount <= 0 || count < m_ClientTransportMaxCount)
//////        {
//////          if (count <= m_ClientTransportCountWaitThreshold) return null;//dont wait - create new one
//////          if (elapsed >= m_ClientTransportExistingAcquisitionTimeoutMs) return null;
//////        }

//////        System.Threading.Thread.Sleep(GRANULARITY_MS);
//////        elapsed += GRANULARITY_MS;

//////        if (m_ClientTransportMaxExistingAcquisitionTimeoutMs > 0 && elapsed > m_ClientTransportMaxExistingAcquisitionTimeoutMs)
//////          throw new ClientCallException(CallStatus.Timeout,
//////                                        StringConsts.GLUE_CLIENT_CALL_TRANSPORT_ACQUISITION_TIMEOUT_ERROR
//////                                        .Args(this.Name, elapsed)
//////                                        );

//////      } while (App.Active && Running);

//////      return null;
//////    }

//////    private List<TTransport> getEndpointList(EndpointAssignment ep, bool create)
//////    {
//////      lock(m_Transports)
//////      {
//////        if (m_Transports.TryGetValue(ep, out var lst)) return lst;
//////        lst = new List<TTransport>();
//////        m_Transports.Add(ep, lst);
//////        return lst;
//////      }
//////    }

//////  }
//////}
