/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Client
{
  /// <summary>
  /// Provides base for endpoint implementations
  /// </summary>
  public abstract class EndpointBase<TService> : ApplicationComponent<TService>, IEndpointImplementation where TService : IServiceImplementation
  {
    protected EndpointBase(TService service, IConfigSectionNode conf) : base(service)
    {
      ConfigAttribute.Apply(this, conf.NonEmpty(nameof(conf)));
    }

    [Config] protected Atom m_Network;
    [Config] protected Atom m_Binding;
    [Config] protected string m_RemoteAddress;
    [Config] protected string m_Contract;
    [Config] protected int m_Shard;
    [Config] protected int m_ShardOrder;
    [Config] protected int m_TimeoutMs;

    protected DateTime? m_CircuitBreakerTimeStampUtc;
    protected DateTime? m_OfflineTimeStampUtc;
    protected string m_StatusMsg;
    private OrderedRegistry<IAspect> m_Aspects = new OrderedRegistry<IAspect>(caseSensitive: false);

    public override string ComponentLogTopic => CoreConsts.CLIENT_TOPIC;

    IService IEndpoint.Service => ComponentDirector;
    public TService    Service => ComponentDirector;

    IOrderedRegistry<IAspect> IEndpoint.Aspects => m_Aspects;
    public OrderedRegistry<IAspect> Aspects => m_Aspects;

    public virtual Atom Network => m_Network;

    public virtual Atom Binding => m_Binding;

    public virtual string RemoteAddress => m_RemoteAddress;

    public virtual string Contract => m_Contract;

    public int Shard => m_Shard;

    public int ShardOrder => m_ShardOrder;

    public int TimeoutMs => m_TimeoutMs;

    public DateTime? CircuitBreakerTimeStampUtc => m_CircuitBreakerTimeStampUtc;

    public DateTime? OfflineTimeStampUtc => m_OfflineTimeStampUtc;

    public bool IsAvailable => !m_CircuitBreakerTimeStampUtc.HasValue && !m_OfflineTimeStampUtc.HasValue;

    public virtual string StatusMsg => m_StatusMsg;

    public virtual string ServiceDescription => "{0}:{1}:{2}://{3}::{4}.{5}".Args(Network, Binding, Contract, RemoteAddress, Shard, ShardOrder);

    public virtual string StatusDescription => "{0} / {1}".Args(IsAvailable ? "Available" : "Unavailable Circuit: {0}; Offline: {1}".Args(CircuitBreakerTimeStampUtc, OfflineTimeStampUtc),  StatusMsg);

    public override string ToString() => GetType().DisplayNameWithExpandedGenericArgs() + " " + ServiceDescription.TakeFirstChars(128);

    public abstract CallErrorClass NotifyCallError(ITransport transport, Exception error);
    public abstract void NotifyCallSuccess(ITransport transport);

    public virtual void PutOffline(string statusMsg)
    {
      m_StatusMsg = statusMsg.Default("Offline");
      m_OfflineTimeStampUtc = App.TimeSource.UTCNow;
    }

    public virtual void PutOnline(string statusMsg)
    {
      m_StatusMsg = statusMsg.Default("Online");
      m_OfflineTimeStampUtc = null;
    }

    public virtual bool TryResetCircuitBreaker(string statusMessage)
    {
      m_CircuitBreakerTimeStampUtc = null;
      return true;
    }

  }
}
