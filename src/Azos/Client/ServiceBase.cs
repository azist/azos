/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Client
{
  /// <summary>
  /// Provides base for providing various IService implementations
  /// </summary>
  /// <typeparam name="TEndpoint">Typed endpoint</typeparam>
  /// <typeparam name="TTransport">Typed transport</typeparam>
  public abstract class ServiceBase<TEndpoint, TTransport> : ApplicationComponent, IServiceImplementation
                          where TEndpoint : IEndpointImplementation
                          where TTransport : ITransportImplementation
  {
    public const string CONFIG_ENDPOINT_SECTION = "endpoint";

    protected ServiceBase(IApplicationComponent director, IConfigSectionNode conf) : base(director)
      => ctor(conf.NonEmpty(nameof(conf)));

    private void ctor(IConfigSectionNode conf)
    {
      ConfigAttribute.Apply(this, conf);
      if (m_Name.IsNullOrWhiteSpace()) m_Name = GetType().Name;
      BuildEndpoints(conf);
      BuildAspects(conf);
    }

    /// <summary>
    /// Override to build endpoint list. The default implementation reads them from config
    /// </summary>
    protected virtual void BuildEndpoints(IConfigSectionNode conf)
    {
      CleanupEndpoints();
      m_Endpoints = new List<TEndpoint>();
      foreach (var nep in conf.ChildrenNamed(CONFIG_ENDPOINT_SECTION))
      {
        var ep = FactoryUtils.Make<TEndpoint>(nep, typeof(TEndpoint), args: new object[] { this, nep });
        m_Endpoints.Add(ep);
      }
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

    protected void CleanupEndpoints()
    {
      var was = System.Threading.Interlocked.Exchange(ref m_Endpoints, null);
      if (was == null) return;
      was.ForEach(ep => ep.Dispose());
    }

    protected void CleanupAspects()
    {
      var was = System.Threading.Interlocked.Exchange(ref m_Aspects, null);
      if (was == null) return;
      was.ForEach(asp => DisposeIfDisposableAndNull(ref asp));
    }

    protected override void Destructor()
    {
      CleanupEndpoints();
      CleanupAspects();
      base.Destructor();
    }

    [Config]
    private string m_Name;

    protected List<TEndpoint> m_Endpoints;

    protected bool m_InstrumentationEnabled;

    private OrderedRegistry<IAspect> m_Aspects;

    [Config] protected Atom m_DefaultNetwork;

    [Config] protected Atom m_DefaultBinding;

    [Config] protected int m_DefaultTimeoutMs;

    public string Name => m_Name;

    public override string ComponentLogTopic => CoreConsts.CLIENT_TOPIC;

    public IEnumerable<IEndpoint> Endpoints => m_Endpoints.Cast<IEndpoint>();

    public IOrderedRegistry<IAspect> Aspects => m_Aspects;

    public virtual Atom DefaultNetwork   => m_DefaultNetwork;

    public virtual Atom DefaultBinding   => m_DefaultBinding;

    public virtual int    DefaultTimeoutMs => m_DefaultTimeoutMs;

    public ITransportImplementation AcquireTransport(EndpointAssignment assignment, bool reserve)
    {
      EnsureObjectNotDisposed();

      if (assignment.Endpoint.NonNull("assignment/endpoint").Service != this)
        throw new ClientException(StringConsts.CLIENT_WRONG_ENDPOINT_SERVICE_ERROR.Args(Name));

      return DoAcquireTransport(assignment, reserve);
    }

    public void ReleaseTransport(ITransportImplementation transport)
    {
      transport.NonNull(nameof(transport));

      //Attention: this may be called for already DISPOSED service on cleanup
      if (transport is TTransport tran)
      {
        if (tran.Assignment.Endpoint.Service != this)
          throw new ClientException(StringConsts.CLIENT_WRONG_ENDPOINT_SERVICE_ERROR.Args(Name));

        DoReleaseTransport(tran);
        return;
      }

      throw new ClientException(StringConsts.CLIENT_WRONG_TRANSPORT_TYPE_ERROR.Args(Name, transport.GetType().Name)); ;
    }

    /// <summary>
    /// Returns endpoints which should be re-tried subsequently on failure.
    /// The endpoints are returned in the sequence which depend on implementation.
    /// Typically the sequence is based on network routing efficiency and least/loaded resources.
    /// The optional shardingKey parameter may be passed for multi-sharding scenarios.
    /// </summary>
    /// <param name="remoteAddress">
    ///   The remote service logical address, such as the regional host name for Sky applications.
    ///   The system resolves this address to physical address depending on binding and contract on the remote host
    /// </param>
    /// <param name="contract">Service contract name</param>
    /// <param name="shardKey">
    ///  Optional sharding parameter. The system will direct the call to the appropriate shard in the service partition if it is used.
    ///  You can use primitive values (such as integers/longs etc.) for sharding, as long as you do not change what value is used for
    ///  `shardKey` parameter, the call routing will remain deterministic
    /// </param>
    /// <param name="network">A name of the logical network to use for a call, or null to use the default network</param>
    /// <param name="binding">
    ///   The service binding to use, or null for default.
    ///   Bindings are connection technology/protocols (such as Http(s)/Glue/GRPC etc..) used to make the call
    /// </param>
    /// <returns>Endpoint(s) which should be (re)tried in the order of enumeration</returns>
    public IEnumerable<EndpointAssignment> GetEndpointsForCall(string remoteAddress, string contract, ShardKey shardKey = default(ShardKey), Atom? network = null, Atom? binding = null)
    {
      if (Disposed) return Enumerable.Empty<EndpointAssignment>();
      return DoGetEndpointsForCall(remoteAddress.NonBlank(nameof(remoteAddress)),
                                   contract.NonBlank(nameof(contract)),
                                   shardKey,
                                   network ?? DefaultNetwork,
                                   binding ?? DefaultBinding);
    }

    /// <summary>
    /// Returns endpoint set for all shards for a specific `remoteAddress/contract/network/binding`
    /// </summary>
    /// <param name="remoteAddress">
    ///   The remote service logical address, such as the regional host name for Sky applications.
    ///   The system resolves this address to physical address depending on binding and contract on the remote host
    /// </param>
    /// <param name="contract">Service contract name</param>
    /// <param name="network">A name of the logical network to use for a call, or null to use the default network</param>
    /// <param name="binding">
    ///   The service binding to use, or null for default.
    ///   Bindings are connection technology/protocols (such as Http(s)/Glue/GRPC etc..) used to make the call
    /// </param>
    /// <returns>
    /// An enumerable of enumerable of EndpointAssigments.
    /// A top level enumerable represents shards. Each shard is further represented by an enumerable of endpoint assignments which should be re-tried
    /// in case of failure in the order of their enumeration.
    /// Endpoint(s) which should be (re)tried in the order of enumeration
    /// </returns>
    public IEnumerable<IEnumerable<EndpointAssignment>> GetEndpointsForAllShards(string remoteAddress, string contract, Atom? network = null, Atom? binding = null)
    {
      if (Disposed) return Enumerable.Empty<IEnumerable<EndpointAssignment>>();
      return DoGetEndpointsForAllShards(remoteAddress.NonBlank(nameof(remoteAddress)),
                                   contract.NonBlank(nameof(contract)),
                                   network ?? DefaultNetwork,
                                   binding ?? DefaultBinding);
    }

    /// <summary>
    /// Called by system after endpoints have changed, for example when more endpoints have been added.
    /// The overrides are typically used to clear cached values
    /// </summary>
    protected abstract void EndpointsHaveChanged();

    protected abstract TTransport DoAcquireTransport(EndpointAssignment assignment, bool reserve);

    protected abstract void DoReleaseTransport(TTransport transport);

    protected abstract IEnumerable<EndpointAssignment> DoGetEndpointsForCall(string remoteAddress, string contract, ShardKey shardKey, Atom network, Atom binding);

    protected abstract IEnumerable<IEnumerable<EndpointAssignment>> DoGetEndpointsForAllShards(string remoteAddress, string contract, Atom network, Atom binding);

    #region IInstrumentation

    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public virtual bool InstrumentationEnabled { get { return m_InstrumentationEnabled; } set { m_InstrumentationEnabled = value; } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters => ExternalParameterAttribute.GetParameters(this);

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      => ExternalParameterAttribute.GetParameters(this, groups);

    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public bool ExternalGetParameter(string name, out object value, params string[] groups)
      => ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public bool ExternalSetParameter(string name, object value, params string[] groups)
      => ExternalParameterAttribute.SetParameter(App, this, name, value, groups);

    #endregion
  }
}
