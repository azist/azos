using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Glue;
using Azos.Conf;
using Azos.Log;
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
  public partial class ServiceClientHub : ApplicationComponent
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

    /// <summary>
    /// Override to configure custom members.
    /// The default implementation populates the CacheMap registry
    /// </summary>
    protected ServiceClientHub(IApplication app, IConfigSectionNode config) : base(app)
    {
      ConfigAttribute.Apply(this, config);
      foreach (var nmapping in config.Children.Where(c => c.IsSameName(CONFIG_MAP_SECTION)))
      {
        var mapping = new ContractMapping(nmapping);
        m_CachedMap.Register(mapping);
      }
    }

    private Registry<ContractMapping> m_CachedMap = new Registry<ContractMapping>();


    #region Public


    public new ISkyApplication App => base.App.AsSky();

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_SVC;

    /// <summary>
    /// Returns the registry of cached ContractMapping instances
    /// </summary>
    public IRegistry<ContractMapping> CachedMap { get { return m_CachedMap; } }


    /// <summary>
    /// Makes an appropriate implementor for requested service client contract type.
    /// Pass svcName parameter in cases when the requested contract may get implemented by more than one network service.
    /// The call is thread-safe. The caller should Dispose() the returned instance after it has been used
    /// </summary>
    public TServiceClient MakeNew<TServiceClient>(Metabase.Metabank.SectionHost toHost, Metabase.Metabank.SectionHost fromHost = null, string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      return MakeNew<TServiceClient>(toHost.RegionPath, fromHost != null ? fromHost.RegionPath : null, svcName);
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
    public TResult CallWithRetry<TServiceClient, TResult>(Func<TServiceClient, TResult> callBody,
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
        using (var client = MakeNew<TServiceClient>(host, fromHost, svcName))
          try
          {
            return callBody(client);
          }
          catch (Exception error)
          {
            WriteLog(MessageType.Error,
                    nameof(CallWithRetry),
                    StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host, error.ToMessageWithType()),
                    error);

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
    public void CallWithRetry<TServiceClient>(Action<TServiceClient> callBody,
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
        using (var client = MakeNew<TServiceClient>(host, fromHost, svcName))
          try
          {
            callBody(client);
            return;
          }
          catch (Exception error)
          {
            WriteLog(MessageType.Error,
                   nameof(CallWithRetry),
                   StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host, error.ToMessageWithType()),
                   error);

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
    public Task<TResult> CallWithRetryAsync<TServiceClient, TResult>(Func<TServiceClient, Task<TResult>> callBody,
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

    private void callWithRetryAsync<TServiceClient, TResult>(
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
          client = MakeNew<TServiceClient>(host, fromHost, svcName);
          t = callBody(client);
          break;
        }
        catch (Exception ex)
        {
          WriteLog(MessageType.Error,
                   nameof(CallWithRetryAsync),
                   StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host, ex.ToMessageWithType()),
                   ex);

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

              WriteLog(MessageType.Error,
                   nameof(CallWithRetryAsync),
                   StringConsts.SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR.Args(typeof(TServiceClient).FullName, host, innerException.ToMessageWithType()),
                   innerException);

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
    public Task CallWithRetryAsync<TServiceClient>(Func<TServiceClient, Task> callBody,
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
    /// Makes a client for the call to the specified contract on the specified host
    /// </summary>
    public TServiceClient MakeNew<TServiceClient>(string toHost, string fromHost = null, string svcName = null) where TServiceClient : class, ISkyServiceClient
    {
      Type tcontract = typeof(TServiceClient);

      if (toHost.IsNullOrWhiteSpace())
        throw new Clients.SkyClientException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetByContract<{0}>(host==null|empty)".Args(tcontract.Name));

      if (fromHost.IsNullOrWhiteSpace()) fromHost = App.HostName;

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

      if (fromHost.IsNullOrWhiteSpace()) fromHost = App.HostName;

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
      isGlobal = !App.Metabase.CatalogReg.ArePathsInSameNOC(fromHost, toHost);

      var mappingData = isGlobal ? mapping.Global : mapping.Local;

      if (svcName.IsNullOrWhiteSpace()) svcName = mappingData.Service;

      return App.Metabase.ResolveNetworkService(toHost, mappingData.Net, svcName, mappingData.Binding, fromHost);
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
    /// The default implementation injects headers, timeouts, and inspectors for Glue.ClientEndPoints
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
          MsgInspectorConfigurator.ConfigureClientInspectors(App, gep.MsgInspectors, icfg);
          HeaderConfigurator.ConfigureHeaders(gep.Headers, icfg);
        }
      }
    }
    #endregion
  }
}
