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


      foreach(var nep in conf.Children.Where( c => c.IsSameName(CONFIG_ENDPOINT_SECTION)))
      {
        var ep = FactoryUtils.Make<TEndpoint>(nep, args: new object[]{this, nep});
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
    public override string ComponentLogTopic => CoreConsts.WEB_TOPIC;


    public IEnumerable<IEndpoint> Endpoints => m_Endpoints.Cast<IEndpoint>();

    public virtual string DefaultNetwork   => m_DefaultNetwork;
    public virtual string DefaultBinding   => m_DefaultBinding;
    public virtual int    DefaultTimeoutMs => m_DefaultTimeoutMs;

    public ITransportImplementation AcquireTransport(IEndpoint endpoint)
    {
      EnsureObjectNotDisposed();

      if (endpoint is TEndpoint ep)
      {
        if (ep.Service != this) throw new NotImplementedException("todo");
        return DoAcquireTransport(ep);
      }

      throw new NotImplementedException("todo");
    }

    public void ReleaseTransport(ITransportImplementation transport)
    {
      EnsureObjectNotDisposed();

      if (transport is TTransport tran)
      {
        if (tran.Endpoint.Service != this) throw new NotImplementedException("todo");
        DoReleaseTransport(tran);
        return;
      }

      throw new NotImplementedException("todo");
    }

    public IEnumerable<IEndpoint> GetEndpointsForCall(string contract, object shardKey = null, string network = null, string binding = null)
    {
      if (Disposed) return Enumerable.Empty<IEndpoint>();
      contract.NonBlank(nameof(contract));
      return DoGetEndpointsForCall(contract, shardKey, network.Default(DefaultNetwork), binding.Default(DefaultBinding));
    }

    protected abstract TTransport DoAcquireTransport(TEndpoint endpoint);
    protected abstract TTransport DoReleaseTransport(TTransport endpoint);
    protected abstract IEnumerable<IEndpoint> DoGetEndpointsForCall(string contract, object shardKey, string network, string binding);


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
