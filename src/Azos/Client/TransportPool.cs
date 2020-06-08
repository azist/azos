using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Conf;

namespace Azos.Client
{
  /// <summary>
  /// Helper class that manages the lifetime of particular type transport instances
  /// </summary>
  ///

  #warning wip  not done
  public abstract class TransportPool<TService, TTransport> : DisposableObject where TService : class, IServiceImplementation
                                                                               where TTransport : class, IPooledTrasportImplementation
  {
    public TransportPool(TService service, IConfigSectionNode config)
    {
      m_Service = service.NonNull(nameof(service));
      ConfigAttribute.Apply(this, config);
    }

    protected override void Destructor()
    {
      base.Destructor();
    }

    private TService m_Service;
    private Dictionary<EndpointAssignment, List<TTransport>> m_Transports = new Dictionary<EndpointAssignment, List<TTransport>>();
    private Platform.KeyedMonitor<EndpointAssignment> m_Locks = new Platform.KeyedMonitor<EndpointAssignment>();

    private int m_MaxCount;
    private int m_WaitStartCount = 4;
    private int m_ExistingTimeoutMs = 2000;
    private int m_TimeoutMs = 30_000;

    /// <summary>
    /// When greater than zero, applies an absolute limit on maximum transport count per endpoint. 0 = unlimited (default).
    /// The system tries to acquire and existing transport for call first, before allocating new transports
    /// </summary>
    [Config]
    public int MaxCount
    {
      get => m_MaxCount;
      set => m_MaxCount = value.KeepBetween(0, 128_000);
    }

    /// <summary>
    /// Defines a count of active transports per endpoint beyond which the system would start waiting for
    /// existing transport availability before allocating a new instance for the requested endpoint
    /// </summary>
    [Config(Default = 4)]
    public int WaitStartCount
    {
      get => m_WaitStartCount;
      set => m_WaitStartCount = value.KeepBetween(0, 1024);
    }

    /// <summary>
    /// Defines a timeout in ms for the system operation trying to get an existing transport after WaitStartCount was reached.
    /// If the system can not get the existing transport after the specified timeout, it will try to allocate a new one
    /// if MaxCount has not been exceeded yet, otherwise throw to indicate an inability to service the call for that endpoint
    /// </summary>
    [Config(Default = 2000)]
    public int ExistingTimeoutMs
    {
      get => m_ExistingTimeoutMs;
      set => m_ExistingTimeoutMs = value.KeepBetween(100, 60_000);
    }

    /// <summary>
    /// Defines a timeout for the complete operation of getting a transport - waiting for existing instance/and/or waiting for
    /// max transport limit availability. If this value is exceeded then exception is thrown indicating inability to get a transport
    /// </summary>
    [Config(Default = 30_000)]
    public int TimeoutMs
    {
      get => m_TimeoutMs;
      set => m_TimeoutMs = value.KeepBetween(0, 900_000);
    }


    public TService Service => m_Service;

    /// <summary>
    /// Tries to get a pooled instance first per endpoint assignment, the makes a new instance
    /// </summary>
    public async Task<TTransport> AcquireAsync(EndpointAssignment assignment)
    {
      EnsureObjectNotDisposed();
      var transport = await TryGetExistingAcquiredTransportAsync(assignment);
      if (transport != null) return transport;

      if (m_MaxCount>0)
      {
        m_Locks.Enter(assignment);
        try
        {
          transport = await TryGetExistingAcquiredTransportAsync(assignment);
          if (transport != null) return transport;
          transport = await MakeNewAsync(assignment);//under logical lock
        }
        finally
        {
          m_Locks.Exit(assignment);
        }
      }

      //otherwise we need to create a new transport
      if (transport==null)
        transport = await MakeNewAsync(assignment);

      return transport;
    }

    public bool Release(TTransport transport)
    {
      if (transport==null) return false;
      return DoRelease(transport);
    }

    protected async Task<TTransport> MakeNewAsync(EndpointAssignment assignment)
    {
      var lst = GetEndpointList(assignment);

      var transport = await DoMakeNewAcquiredAsync(assignment);

      lock(lst) lst.Add(transport);

      return transport;
    }

    /// <summary>
    /// Override to create new instance of transport which must be in acquired state
    /// </summary>
    protected abstract Task<TTransport> DoMakeNewAcquiredAsync(EndpointAssignment assignment);

    /// <summary>
    /// Override to release, the default implementation delegate to tranport.Release
    /// </summary>
    protected virtual bool DoRelease(TTransport transport)
    {
      return transport.Release();
    }


    /// <summary>
    /// Tries to acquire an available transport to make a call.
    /// This method respects binding/transport settings that impose a limit on the number of
    ///  open concurrent transports and timeouts for acquisition waiting
    /// </summary>
    /// <param name="assignment">endpoint assignment</param>
    /// <returns>Available acquired existing transport or null</returns>
    protected virtual async Task<TTransport> TryGetExistingAcquiredTransportAsync(EndpointAssignment assignment)
    {
      List<TTransport> lst;
      lock(m_Transports)
      {
        if (!m_Transports.TryGetValue(assignment, out lst)) return null;//nothing exists yet
      }

      var time = Time.Timeter.StartNew();
      while(!Disposed)
      {
        var count = 0;
        lock(lst)
        {
          count = lst.Count;
          var acquired = lst.FirstOrDefault(t => t.TryAcquire());
          if (acquired != null) return acquired;
        }

        if (m_MaxCount <= 0 || count < m_MaxCount)
        {
          if (count <= m_WaitStartCount) return null;//dont wait - create a new one right away
          if (time.ElapsedMs >= m_ExistingTimeoutMs) return null;//enough time elapsed so create a new one
        }

        if (m_TimeoutMs > 0 && time.ElapsedMs > m_TimeoutMs)
         throw new ClientException("Timeout of {0} ms exceeded while getting transport for `{1}`. Revise transport pool limits".Args(m_TimeoutMs, assignment));

        await Task.Delay(Platform.RandomGenerator.Instance.NextScaledRandomInteger(10, 37));
      }

      return null;
    }

    protected List<TTransport> GetEndpointList(EndpointAssignment ep)
    {
      lock (m_Transports)
      {
        if (m_Transports.TryGetValue(ep, out var lst)) return lst;
        lst = new List<TTransport>();
        m_Transports.Add(ep, lst);
        return lst;
      }
    }

  }
}
