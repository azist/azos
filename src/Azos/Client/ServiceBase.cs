using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }

    /// <summary>
    /// Override to build endpoint list. The default implementation reads them from config
    /// </summary>
    protected virtual void BuildEndpoints(IConfigSectionNode conf)
    {
      foreach (var nep in conf.Children.Where(c => c.IsSameName(CONFIG_ENDPOINT_SECTION)))
      {
        var ep = FactoryUtils.Make<TEndpoint>(nep, args: new object[] { this, nep });
        m_Endpoints.Add(ep);
      }
    }


    protected override void Destructor()
    {
      m_Endpoints.ForEach(ep => ep.Dispose());
      base.Destructor();
    }

    [Config]
    private string m_Name;
    protected List<TEndpoint> m_Endpoints = new List<TEndpoint>();
    protected bool m_InstrumentationEnabled;

    [Config] protected string m_DefaultNetwork;
    [Config] protected string m_DefaultBinding;
    [Config] protected int m_DefaultTimeoutMs;


    public string Name => m_Name;
    public override string ComponentLogTopic => CoreConsts.CLIENT_TOPIC;


    public IEnumerable<IEndpoint> Endpoints => m_Endpoints.Cast<IEndpoint>();

    public virtual string DefaultNetwork   => m_DefaultNetwork;
    public virtual string DefaultBinding   => m_DefaultBinding;
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

    public IEnumerable<EndpointAssignment> GetEndpointsForCall(string remoteAddress, string contract, object shardKey = null, string network = null, string binding = null)
    {
      if (Disposed) return Enumerable.Empty<EndpointAssignment>();
      return DoGetEndpointsForCall(remoteAddress.NonBlank(nameof(remoteAddress)),
                                   contract.NonBlank(nameof(contract)),
                                   shardKey,
                                   network.Default(DefaultNetwork),
                                   binding.Default(DefaultBinding));
    }

    protected abstract TTransport DoAcquireTransport(EndpointAssignment assignment, bool reserve);
    protected abstract void DoReleaseTransport(TTransport endpoint);
    protected abstract IEnumerable<EndpointAssignment> DoGetEndpointsForCall(string remoteAddress, string contract, object shardKey, string network, string binding);




    #region IInstrumentation


    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public virtual bool InstrumentationEnabled { get { return m_InstrumentationEnabled; } set { m_InstrumentationEnabled = value; } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters { get { return ExternalParameterAttribute.GetParameters(this); } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }

    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      return ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);
    }

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(App, this, name, value, groups);
    }

    #endregion
  }
}
