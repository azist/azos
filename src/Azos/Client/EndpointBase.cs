/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
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
      => ctor(conf.NonEmpty(nameof(conf)));

    private void ctor(IConfigSectionNode conf)
    {
      ConfigAttribute.Apply(this, conf);
      BuildAspects(conf);
    }

    protected override void Destructor()
    {
      CleanupAspects();
      base.Destructor();
    }

    protected void CleanupAspects()
    {
      var was = System.Threading.Interlocked.Exchange(ref m_Aspects, null);
      if (was == null) return;
      was.ForEach(asp => DisposeIfDisposableAndNull(ref asp));
    }

    /// <summary>
    /// Override to build aspects list. The default implementation reads them from config
    /// </summary>
    protected virtual void BuildAspects(IConfigSectionNode conf)
    {
      CleanupAspects();
      m_Aspects = new OrderedRegistry<IAspect>(caseSensitive: false);
      foreach (var nas in conf.ChildrenNamed(AspectBase.CONFIG_ASPECT_SECTION))
      {
        var aspect = FactoryUtils.Make<IAspect>(nas, args: new object[] { this, nas });
        if (!m_Aspects.Register(aspect))
        {
          throw new ClientException(StringConsts.HTTP_CLIENT_DUPLICATE_ASPECT_CONFIG_ERROR
                                                .Args(aspect.Name, GetType().DisplayNameWithExpandedGenericArgs()));
        }
      }
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

    public IOrderedRegistry<IAspect> Aspects => m_Aspects;

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

    public virtual bool TryResetCircuitBreaker(string statusMsg)
    {
      m_StatusMsg = statusMsg.Default("Reset brkr");
      m_CircuitBreakerTimeStampUtc = null;
      return true;
    }

    public TAspect TryGetAspect<TAspect>(string name = null, bool notInherited = false) where TAspect : class, IAspect
    {
      var result = findAspect<TAspect>(m_Aspects, name);
      if (result != null) return result;
      if (notInherited) return null;
      result = findAspect<TAspect>(Service.Aspects, name);
      return result;
    }

    private static TAspect findAspect<TAspect>(IOrderedRegistry<IAspect> where, string name) where TAspect : class, IAspect
    {
      TAspect result;

      if (name.IsNotNullOrWhiteSpace())
        result = where[name] as TAspect;
      else
        result = where.OrderedValues.OfType<TAspect>().FirstOrDefault();

      return result;
    }

  }
}
