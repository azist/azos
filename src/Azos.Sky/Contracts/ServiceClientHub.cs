using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Glue;
using Azos.Conf;
using Azos.Collections;
using Azos.Glue.Protocol;
using Azos.Serialization.BSON;

namespace Azos.Sky.Contracts
{
  /// <summary>
  /// Represents a centralized manager and client factory for obtaining ISkyServiceClient-implementing instances
  ///  that are most likely Glue-based but do not have to be.
  ///  Custom solutions may derive from this class and override specifics (i.e. inject some policies in all service call clients)
  /// </summary>
  public class ServiceClientHub : ApplicationComponent
  {
    #region CONSTS
    public const string CONFIG_SERVICE_CLIENT_HUB_SECTION = "service-client-hub";
    public const string CONFIG_MAP_SECTION = "map";

    public const string CONFIG_LOCAL_SECTION = "local";
    public const string CONFIG_GLOBAL_SECTION = "global";
    public const string CONFIG_OPTIONS_SECTION = "options";

    public const string CONFIG_MAP_CLIENT_CONTRACT_ATTR = "client-contract";
    public const string CONFIG_MAP_IMPLEMENTOR_ATTR = "implementor";
    #endregion

    #region Inner Types
    /// <summary>
    /// Provides mapping information for service contract
    /// </summary>
    [Serializable]
    public sealed class ContractMapping : INamed
    {
      public ContractMapping(IConfigSectionNode config)
      {
        try
        {
          var cname = config.AttrByName(CONFIG_MAP_CLIENT_CONTRACT_ATTR).Value;
          if (cname.IsNullOrWhiteSpace()) throw new Clients.SkyClientException(CONFIG_MAP_CLIENT_CONTRACT_ATTR + " is unspecified");
          m_Contract = Type.GetType(cname, true);

          m_Local = new Data(config, config[CONFIG_LOCAL_SECTION], false);
          m_Global = new Data(config, config[CONFIG_GLOBAL_SECTION], true);
        }
        catch (Exception error)
        {
          throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_MAPPING_CTOR_ERROR.Args(
                                              config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact),
                                              error.ToMessageWithType()), error);
        }
      }


      private Type m_Contract;
      //------------------------------
      private Data m_Local;
      private Data m_Global;


      public sealed class Data
      {

        internal Data(IConfigSectionNode baseConfig, IConfigSectionNode config, bool global)
        {
          var cname = config.AttrByName(CONFIG_MAP_IMPLEMENTOR_ATTR).ValueAsString(baseConfig.AttrByName(CONFIG_MAP_IMPLEMENTOR_ATTR).Value);
          if (cname.IsNullOrWhiteSpace()) throw new Clients.SkyClientException(CONFIG_MAP_IMPLEMENTOR_ATTR + " is unspecified");
          m_Implementor = Type.GetType(cname, true);

          if (!m_Implementor.GetInterfaces().Any(i => i == typeof(ISkyServiceClient)))
            throw new Clients.SkyClientException("Implementor {0} is not ISkyServiceClient".Args(m_Implementor.FullName));

          ConfigAttribute.Apply(this, baseConfig);
          ConfigAttribute.Apply(this, config);

          //service may be null

          if (m_Net.IsNullOrWhiteSpace())
            m_Net = global ? SysConsts.NETWORK_INTERNOC : SysConsts.NETWORK_NOCGOV;

          if (m_Binding.IsNullOrWhiteSpace())
            m_Binding = SysConsts.DEFAULT_BINDING;

          if (m_Options == null)
            m_Options = "options{ }".AsLaconicConfig();
        }

        private Type m_Implementor;
        [Config] private string m_Service = string.Empty;
        [Config] private string m_Net;
        [Config] private string m_Binding;
        [Config] private int m_CallTimeoutMs = 0;
        [Config] private bool m_ReserveTransport = false;
        [Config(CONFIG_OPTIONS_SECTION)] private IConfigSectionNode m_Options;

        public Type Implementor { get { return m_Implementor; } }
        public string Service { get { return m_Service; } }
        public string Net { get { return m_Net; } }
        public string Binding { get { return m_Binding; } }
        public int CallTimeoutMs { get { return m_CallTimeoutMs; } }
        public bool ReserveTransport { get { return m_ReserveTransport; } }
        public IConfigSectionNode Options { get { return m_Options; } }
      }

      public string Name { get { return m_Contract.AssemblyQualifiedName; } }
      public Type Contract { get { return m_Contract; } }
      public Data Local { get { return m_Local; } }
      public Data Global { get { return m_Global; } }

      public override string ToString()
      {
        return "Mapping[{0} -> Local: {1}; Global: {2}]".Args(m_Contract.FullName, Local.Implementor.FullName, Global.Implementor.FullName);
      }

      public override bool Equals(object obj)
      {
        var cm = obj as ContractMapping;
        if (cm == null) return false;
        return this.m_Contract == cm.m_Contract;
      }

      public override int GetHashCode()
      {
        return m_Contract.GetHashCode();
      }
    }
    #endregion

    #region .static/.ctor
    private static object s_Lock = new object();
    private static volatile ServiceClientHub s_Instance;

    /// <summary>
    /// Returns the singleton instance of the ServiceClient or its derivative as configured
    /// </summary>
    public static ServiceClientHub Instance
    {
      get
      {
        var instance = s_Instance;
        if (instance != null) return instance;

        lock (s_Lock)
        {
          if (s_Instance == null)
            s_Instance = makeInstance();
        }

        return s_Instance;
      }
    }

    private static ServiceClientHub makeInstance()
    {
      string tpn = typeof(ServiceClientHub).FullName;
      try
      {
        var mbNode = SkySystem.Metabase.ServiceClientHubConfNode as ConfigSectionNode;
        var appNode = App.ConfigRoot[SysConsts.APPLICATION_CONFIG_ROOT_SECTION][CONFIG_SERVICE_CLIENT_HUB_SECTION] as ConfigSectionNode;

        var effectiveConf = new MemoryConfiguration();
        effectiveConf.CreateFromMerge(mbNode, appNode);
        var effective = effectiveConf.Root;

        tpn = effective.AttrByName(FactoryUtils.CONFIG_TYPE_ATTR).ValueAsString(typeof(ServiceClientHub).FullName);

        return FactoryUtils.Make<ServiceClientHub>(effective, typeof(ServiceClientHub), new object[] { effective });
      }
      catch (Exception error)
      {
        throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_SINGLETON_CTOR_ERROR.Args(tpn, error.ToMessageWithType()), error);
      }
    }


    /// <summary>
    /// Makes an appropriate implementor for requested service client contract type.
    /// Pass svcName parameter in cases when the requested contract may get implemented by more than one network service.
    /// The call is thread-safe. The caller should Dispose() the returned instance after it has been used
    /// </summary>
    public static TServiceClient New<TServiceClient>(string toHost, string fromHost = null, string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      return Instance.GetNew<TServiceClient>(toHost, fromHost, svcName);
    }

    /// <summary>
    /// Makes an appropriate implementor for requested service client contract type.
    /// Pass svcName parameter in cases when the requested contract may get implemented by more than one network service.
    /// The call is thread-safe. The caller should Dispose() the returned instance after it has been used
    /// </summary>
    public static TServiceClient New<TServiceClient>(Metabase.Metabank.SectionHost toHost, Metabase.Metabank.SectionHost fromHost = null, string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      return Instance.GetNew<TServiceClient>(toHost.RegionPath, fromHost != null ? fromHost.RegionPath : null, svcName);
    }

    /// <summary>
    /// Tries to resolve contract type to implementor and tests network service resolvability. Throws in case of error
    /// </summary>
    public static void TestSetupOf<TServiceClient>(string toHost, string fromHost = null, string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      Instance.RunTestSetupOf<TServiceClient>(toHost, fromHost, svcName);
    }


    /// <summary>
    /// Performs a call by making the appropriate client and retries if the ClientCallException arises,
    /// otherwise fails if no backup hosts are left
    /// </summary>
    /// <param name="callBody">The body of the client call, must not be null</param>
    /// <param name="hosts">The array of hosts to retry against, left to right</param>
    /// <param name="abortFilter">Optional functor that returns true for call exceptions that should abort the retry process</param>
    /// <param name="fromHost">Optional</param>
    /// <param name="svcName">Optional</param>
    public static TResult CallWithRetry<TServiceClient, TResult>(Func<TServiceClient, TResult> callBody,
                                                                 IEnumerable<string> hosts,
                                                                 Func<TServiceClient, Exception, bool> abortFilter = null,
                                                                 string fromHost = null,
                                                                 string svcName = null
                                                                ) where TServiceClient : class, ISkyServiceClient
    {
      if (callBody == null)
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + typeof(ServiceClientHub).Name + ".CallWithRetry<{0}>(callBody==null)".Args(typeof(TServiceClient).Name));

      if (hosts == null || !hosts.Any())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + typeof(ServiceClientHub).Name + ".CallWithRetry<{0}>(hosts==null|[0])".Args(typeof(TServiceClient).Name));

      foreach (var host in hosts)
      {
        using (var client = New<TServiceClient>(host, fromHost, svcName))
          try
          {
            return callBody(client);
          }
          catch (Exception error)
          {
            App.Log.Write(
              new Azos.Log.Message
              {
                Type = Azos.Log.MessageType.Error,
                Topic = SysConsts.LOG_TOPIC_SVC,
                From = "{0}.CallWithRetry()".Args(Instance.GetType().Name),
                Source = 248,
                Text = StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host),
                Exception = error
              }
            );

            Instrumentation.ServiceClientHubRetriableCallError.Happened(typeof(TServiceClient), host);

            var shouldAbort = !(error is ClientCallException);
            if (abortFilter != null) shouldAbort = abortFilter(client, error);
            if (shouldAbort) throw;
            //otherwise eat the exception and retry
          }
      }

      throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_CALL_RETRY_FAILED_ERROR.Args(typeof(TServiceClient).Name, hosts.Count()));
    }


    /// <summary>
    /// Performs a call by making the appropriate client and retries if the ClientCallException arises,
    /// otherwise fails if no backup hosts are left
    /// </summary>
    /// <param name="callBody">The body of the client call, must not be null</param>
    /// <param name="hosts">The array of hosts to retry against, left to right</param>
    /// <param name="abortFilter">Optional functor that returns true for call exceptions that should abort the retry process</param>
    /// <param name="fromHost">Optional</param>
    /// <param name="svcName">Optional</param>
    public static void CallWithRetry<TServiceClient>(Action<TServiceClient> callBody,
                                                     IEnumerable<string> hosts,
                                                     Func<TServiceClient, Exception, bool> abortFilter = null,
                                                     string fromHost = null,
                                                     string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      if (callBody == null)
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + typeof(ServiceClientHub).Name + ".CallWithRetry<{0}>(callBody==null)".Args(typeof(TServiceClient).Name));

      if (hosts == null || !hosts.Any())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + typeof(ServiceClientHub).Name + ".CallWithRetry<{0}>(hosts==null|[0])".Args(typeof(TServiceClient).Name));

      foreach (var host in hosts)
      {
        using (var client = New<TServiceClient>(host, fromHost, svcName))
          try
          {
            callBody(client);
            return;
          }
          catch (Exception error)
          {
            App.Log.Write(
              new Azos.Log.Message
              {
                Type = Azos.Log.MessageType.Error,
                Topic = SysConsts.LOG_TOPIC_SVC,
                From = "{0}.CallWithRetry()".Args(Instance.GetType().Name),
                Source = 248,
                Text = StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host),
                Exception = error
              }
            );

            Instrumentation.ServiceClientHubRetriableCallError.Happened(typeof(TServiceClient), host);

            var shouldAbort = !(error is ClientCallException);
            if (abortFilter != null) shouldAbort = abortFilter(client, error);
            if (shouldAbort) throw;
            //otherwise eat the exception and retry
          }
      }

      throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_CALL_RETRY_FAILED_ERROR.Args(typeof(TServiceClient).Name, hosts.Count()));
    }


    /// <summary>
    /// Performs a call by making the appropriate client and retries if the ClientCallException arises,
    /// otherwise fails if no backup hosts are left
    /// </summary>
    /// <param name="callBody">The body of the client call, must not be null</param>
    /// <param name="hosts">The array of hosts to retry against, left to right</param>
    /// <param name="abortFilter">Optional functor that returns true for call exceptions that should abort the retry process</param>
    /// <param name="fromHost">Optional</param>
    /// <param name="svcName">Optional</param>
    public static Task<TResult> CallWithRetryAsync<TServiceClient, TResult>(Func<TServiceClient, Task<TResult>> callBody,
                                                                 IEnumerable<string> hosts,
                                                                 Func<TServiceClient, Exception, bool> abortFilter = null,
                                                                 string fromHost = null,
                                                                 string svcName = null
                                                                ) where TServiceClient : class, ISkyServiceClient
    {
      if (callBody == null)
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + typeof(ServiceClientHub).Name + ".CallWithRetry<{0}>(callBody==null)".Args(typeof(TServiceClient).Name));

      if (hosts == null || !hosts.Any())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + typeof(ServiceClientHub).Name + ".CallWithRetry<{0}>(hosts==null|[0])".Args(typeof(TServiceClient).Name));

      var hostsCopy = hosts.ToArray();
      var hostsEnum = ((IEnumerable<string>)hostsCopy).GetEnumerator();

      var tcs = new TaskCompletionSource<TResult>();

      callWithRetryAsync(tcs, callBody, hostsEnum, hosts.Count(), abortFilter, fromHost, svcName);

      return tcs.Task;
    }

    private static void callWithRetryAsync<TServiceClient, TResult>(
                                                                  TaskCompletionSource<TResult> tcs,
                                                                  Func<TServiceClient, Task<TResult>> callBody,
                                                                  IEnumerator<string> hosts,
                                                                  int hostsCount,
                                                                  Func<TServiceClient, Exception, bool> abortFilter = null,
                                                                  string fromHost = null,
                                                                  string svcName = null
                                                                ) where TServiceClient : class, ISkyServiceClient
    {
      Task<TResult> t = null;
      TServiceClient client = null;
      string host = null;

      while (hosts.MoveNext())
      {
        try
        {
          host = hosts.Current;
          client = New<TServiceClient>(host, fromHost, svcName);
          t = callBody(client);
          break;
        }
        catch (Exception ex)
        {
          App.Log.Write(new Azos.Log.Message
          {
            Type = Azos.Log.MessageType.Error,
            Topic = SysConsts.LOG_TOPIC_SVC,
            From = "{0}.CallWithRetryAsync()".Args(Instance.GetType().Name),
            Source = 304,
            Text = StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host),
            Exception = ex
          });

          Instrumentation.ServiceClientHubRetriableCallError.Happened(typeof(TServiceClient), host);

          var shouldAbort = !(ex is ClientCallException);
          if (abortFilter != null) shouldAbort = abortFilter(client, ex);
          if (shouldAbort)
          {
            tcs.TrySetException(ex);
            return;
          }
        }
        finally
        {
          if (client != null && t == null)
            DisposableObject.DisposeAndNull(ref client);
        }
      }

      if (t != null)
      {
        t.ContinueWith(antecedent =>
        {
          try
          {
            var err = antecedent.Exception;
            if (err == null)
            {
              tcs.SetResult(antecedent.Result);
            }
            else
            {
              var innerException = err.GetBaseException();

              App.Log.Write(new Azos.Log.Message
              {
                Type = Azos.Log.MessageType.Error,
                Topic = SysConsts.LOG_TOPIC_SVC,
                From = "{0}.CallWithRetryAsync()".Args(Instance.GetType().Name),
                Source = 304,
                Text = StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host),
                Exception = innerException
              });

              Instrumentation.ServiceClientHubRetriableCallError.Happened(typeof(TServiceClient), host);

              var shouldAbort = !(innerException is ClientCallException);
              if (abortFilter != null) shouldAbort = abortFilter(client, innerException);
              if (shouldAbort)
              {
                tcs.TrySetException(innerException);
                return;
              }
              else
              {
                callWithRetryAsync(tcs, callBody, hosts, hostsCount, abortFilter, fromHost, svcName);
              }
            }
          }
          finally
          {
            DisposableObject.DisposeAndNull(ref client);
          }
        }, TaskContinuationOptions.ExecuteSynchronously);

        return;
      }

      tcs.TrySetException(new Clients.SkyClientException(
            StringConsts.SKY_SVC_CLIENT_HUB_CALL_RETRY_FAILED_ERROR.Args(typeof(TServiceClient).Name, hostsCount)));
    }


    /// <summary>
    /// Performs a call by making the appropriate client and retries if the ClientCallException arises,
    /// otherwise fails if no backup hosts are left
    /// </summary>
    /// <param name="callBody">The body of the client call, must not be null</param>
    /// <param name="hosts">The array of hosts to retry against, left to right</param>
    /// <param name="abortFilter">Optional functor that returns true for call exceptions that should abort the retry process</param>
    /// <param name="fromHost">Optional</param>
    /// <param name="svcName">Optional</param>
    public static Task CallWithRetryAsync<TServiceClient>(Func<TServiceClient, Task> callBody,
                                                                 IEnumerable<string> hosts,
                                                                 Func<TServiceClient, Exception, bool> abortFilter = null,
                                                                 string fromHost = null,
                                                                 string svcName = null
                                                                ) where TServiceClient : class, ISkyServiceClient
    {
      return CallWithRetryAsync<TServiceClient, bool>(cl =>
                                                          callBody(cl)
                                                            .ContinueWith<bool>(antecedent =>
                                                              {
                                                                var err = antecedent.Exception;
                                                                if (err != null) throw err;

                                                                return true;
                                                              }
                                                            ),
                                                        hosts, abortFilter, fromHost, svcName);
    }

    /// <summary>
    /// Override to configure custom memebers.
    /// The default implementation populates the CacheMap registry
    /// </summary>
    protected ServiceClientHub(IConfigSectionNode config)
    {
      ConfigAttribute.Apply(this, config);
      foreach (var nmapping in config.Children.Where(c => c.IsSameName(CONFIG_MAP_SECTION)))
      {
        var mapping = new ContractMapping(nmapping);
        m_CachedMap.Register(mapping);
      }
    }
    #endregion

    #region Fields
    private Registry<ContractMapping> m_CachedMap = new Registry<ContractMapping>();
    #endregion


    #region Public
    /// <summary>
    /// Returns the registry of cached ContractMapping instances
    /// </summary>
    public IRegistry<ContractMapping> CachedMap { get { return m_CachedMap; } }


    /// <summary>
    /// Instance version of static ByContract(). See ByContract{TServiceClient}()
    /// </summary>
    public TServiceClient GetNew<TServiceClient>(string toHost, string fromHost = null, string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      Type tcontract = typeof(TServiceClient);

      if (toHost.IsNullOrWhiteSpace())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetByContract<{0}>(host==null|empty)".Args(tcontract.Name));

      if (fromHost.IsNullOrWhiteSpace()) fromHost = SkySystem.HostName;

      if (fromHost.IsNullOrWhiteSpace())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetByContract<{0}>(fromHost==null|empty & AgnySystem is not avail)".Args(tcontract.Name));


      ContractMapping mapping = MapContractToImplementation(tcontract);

      bool isGlobal;
      Node node = ResolveNetworkService(mapping, toHost, fromHost, svcName, out isGlobal);

      var result = MakeClientInstance<TServiceClient>(mapping, isGlobal, node);

      SetupClientInstance(mapping, isGlobal, result, toHost, fromHost);

      return result;
    }


    /// <summary>
    /// Tries to resolve contract type to implementor and tests network service resolvability. Throws in case of error
    /// </summary>
    public void RunTestSetupOf<TServiceClient>(string toHost, string fromHost = null, string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      Type tcontract = typeof(TServiceClient);

      if (toHost.IsNullOrWhiteSpace())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".TestByContract<{0}>(host==null|empty)".Args(tcontract.Name));

      if (fromHost.IsNullOrWhiteSpace()) fromHost = SkySystem.HostName;

      if (fromHost.IsNullOrWhiteSpace())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".TestByContract<{0}>(fromHost==null|empty & SkySystem is not avail)".Args(tcontract.Name));


      ContractMapping mapping = MapContractToImplementation(tcontract);

      bool isGlobal;
      Node node = ResolveNetworkService(mapping, toHost, fromHost, svcName, out isGlobal);
    }
    #endregion

    #region Protected
    protected ContractMapping MapContractToImplementation(Type tContract)
    {
      try
      {
        var mapped = DoMapContractToImplementation(tContract);
        if (mapped == null) throw new Clients.SkyClientException("Map['{0}'] -> <null>".Args(tContract.FullName));
        return mapped;
      }
      catch (Exception error)
      {
        throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_MAPPING_ERROR.Args(tContract.FullName, error.ToMessageWithType()), error);
      }
    }

    protected Node ResolveNetworkService(ContractMapping mapping, string toHost, string fromHost, string svcName, out bool isGlobal)
    {
      try { return DoResolveNetworkService(mapping, toHost, fromHost, svcName, out isGlobal); }
      catch (Exception error)
      {
        throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_NET_RESOLVE_ERROR.Args(mapping, error.ToMessageWithType()), error);
      }
    }

    protected TServiceClient MakeClientInstance<TServiceClient>(ContractMapping mapping, bool isGlobal, Node node) where TServiceClient : ISkyServiceClient
    {
      try { return DoMakeClientInstance<TServiceClient>(mapping, isGlobal, node); }
      catch (Exception error)
      {
        throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_MAKE_INSTANCE_ERROR.Args(mapping, error.ToMessageWithType()), error);
      }
    }

    protected void SetupClientInstance(ContractMapping mapping, bool isGlobal, ISkyServiceClient instance, string toHost, string fromHost)
    {
      try { DoSetupClientInstance(mapping, isGlobal, instance, toHost, fromHost); }
      catch (Exception error)
      {
        throw new Clients.SkyClientException(StringConsts.SKY_SVC_CLIENT_HUB_SETUP_INSTANCE_ERROR.Args(mapping, error.ToMessageWithType()), error);
      }
    }

    /// <summary>
    /// Override to map tContract into ContractMapping object. The default implementation uses cached registry of mappings
    /// </summary>
    protected virtual ContractMapping DoMapContractToImplementation(Type tContract)
    {
      return m_CachedMap[tContract.AssemblyQualifiedName];
    }

    /// <summary>
    /// Override to resolve ContractMapping into physical connection parameters.
    /// The default implementation uses metabase's ResolveNetworkService
    /// </summary>
    protected virtual Node DoResolveNetworkService(ContractMapping mapping, string toHost, string fromHost, string svcName, out bool isGlobal)
    {
      isGlobal = !SkySystem.Metabase.CatalogReg.ArePathsInSameNOC(fromHost, toHost);

      var mappingData = isGlobal ? mapping.Global : mapping.Local;

      if (svcName.IsNullOrWhiteSpace()) svcName = mappingData.Service;

      return SkySystem.Metabase.ResolveNetworkService(toHost, mappingData.Net, svcName, mappingData.Binding, fromHost);
    }

    protected static readonly Type[] CTOR_SIG_GLUE_ENDPOINT = new Type[] { typeof(Azos.Glue.Node), typeof(Azos.Glue.Binding) };

    /// <summary>
    /// Override to make an instance of client per ContractMapping.
    /// The default implementation uses activator
    /// </summary>
    protected virtual TServiceClient DoMakeClientInstance<TServiceClient>(ContractMapping mapping, bool isGlobal, Node node) where TServiceClient : ISkyServiceClient
    {
      var mappingData = isGlobal ? mapping.Global : mapping.Local;

      TServiceClient result;

      try
      {
        if (mappingData.Implementor.GetConstructor(CTOR_SIG_GLUE_ENDPOINT) != null)
          result = (TServiceClient)Activator.CreateInstance(mappingData.Implementor, node, null); //Glue endpoint signature: node, binding
        else
          result = (TServiceClient)Activator.CreateInstance(mappingData.Implementor, node);//otherwise must have NODE as a sole parameter
      }
      catch (Exception error)
      {
        if (error is System.Reflection.TargetInvocationException) throw error.InnerException;
        throw new Clients.SkyClientException("Implementor '{0}' must have .ctor(Glue.Node) or .ctor(GlueNode,Glue.Binding). Error: {1}".Args(mappingData.Implementor.FullName, error.ToMessageWithType()));
      }

      return result;
    }

    /// <summary>
    /// Override to setup the instance of client after it has been allocated.
    /// The default implementation injects headers, timeoutes, and inspectors for Glue.ClientEndPoints
    /// </summary>
    protected virtual void DoSetupClientInstance(ContractMapping mapping, bool isGlobal, ISkyServiceClient instance, string toHost, string fromHost)
    {
      var mappingData = isGlobal ? mapping.Global : mapping.Local;

      var tms = mappingData.CallTimeoutMs;
      if (tms > 0) instance.TimeoutMs = tms;
      instance.ReserveTransport = mappingData.ReserveTransport;


      var gep = instance as ClientEndPoint;
      if (gep != null)
      {
        var icfg = mappingData.Options;
        if (icfg != null)
        {
          MsgInspectorConfigurator.ConfigureClientInspectors(gep.MsgInspectors, icfg);
          HeaderConfigurator.ConfigureHeaders(gep.Headers, icfg);
        }
      }
    }
    #endregion
  }


  namespace Instrumentation
  {

    [Serializable]
    public abstract class ServiceClientHubEvent : Azos.Instrumentation.Event, Azos.Instrumentation.INetInstrument
    {
      protected ServiceClientHubEvent(string src) : base(src) { }
    }

    [Serializable]
    public abstract class ServiceClientHubErrorEvent : ServiceClientHubEvent, Azos.Instrumentation.IErrorInstrument
    {
      protected ServiceClientHubErrorEvent(string src) : base(src) { }
    }


    //todo Need to add the counter for successful calls as well, however be carefull,
    //as to many details may create much instrumentation data (dont include contract+toHost)?
    //or have level of detalization setting


    [Serializable]
    [BSONSerializable("D2BDA600-7B13-4701-BC8F-C9E72A26CED8")]
    public class ServiceClientHubRetriableCallError : ServiceClientHubErrorEvent
    {
      protected ServiceClientHubRetriableCallError(string src) : base(src) { }

      public static void Happened(Type tContract, string toName)
      {
        var inst = ExecutionContext.Application.Instrumentation;
        if (inst.Enabled)
          inst.Record(new ServiceClientHubRetriableCallError("{0}::{1}".Args(tContract.FullName, toName)));
      }

      public override string Description { get { return "Service client hub retriable call failed"; } }


      protected override Azos.Instrumentation.Datum MakeAggregateInstance()
      {
        return new ServiceClientHubRetriableCallError(this.Source);
      }
    }

  }
}
