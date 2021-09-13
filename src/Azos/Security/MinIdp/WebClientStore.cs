/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Azos.Apps;
using Azos.Client;
using Azos.Conf;
using Azos.Web;
using Azos.Instrumentation;
using Azos.Serialization.JSON;
using Azos.Log;
using Azos.Data;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Implements IMinIdpStore by calling a remote MinIdpServer using Web
  /// </summary>
  public sealed class WebClientStore : DaemonWithInstrumentation<IApplicationComponent>, IMinIdpStoreImplementation, IExternallyCallable
  {
    public const string CONFIG_SERVER_SECTION = "server";

    [SystemAdministratorPermission(AccessLevel.ADVANCED)]
    public sealed class Exec : ExternalCallRequest<WebClientStore>
    {
      public Exec(WebClientStore ctx) : base(ctx){ }

      public IConfigSectionNode Script { get; set;}

      public override void Configure(IConfigSectionNode node)
      {
        Script = node;
      }

      [Config] public Atom Realm {  get ; set;}

      public override ExternalCallResponse Describe()
        => new ExternalCallResponse(ContentType.TEXT,
@"#Executes an MinIdp management script
```
manc
{
  name=minidp
  call
  {
    exec
    {
      DropRole //step 1
      {
        realm='realm' //atom
        id='roleId' //string
      }

      SetRole //step 2 ...
      {
        realm='realm' //atom
        id='roleId' //string
        rights{ .... }
      }
    }
  }
}
```
");
      private class step
      {
        public string src     {get; set;}
        public bool   ok      { get; set; }
        public object result  { get; set; }
      }


      public override ExternalCallResponse Execute()
      {
        Script.NonEmpty(nameof(Script));

        var results = new List<step>();
        foreach(var step in Script.Children)
        {
          try
          {
            var result = Context.ExecCommandAsync(step).GetAwaiter().GetResult();
            results.Add(new step{src = step.RootPath, ok = true, result = result});
          }
          catch(Exception error)
          {
            var e = new WrappedExceptionData(error, false, true);
            results.Add(new step{src = step.RootPath, ok = false, result = e});
          }
        }

        return new ExternalCallResponse(ContentType.JSON, new {OK = true, results = results}.ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII));
      }
    }


    public WebClientStore(IApplicationComponent dir) : base(dir)
    {
      m_Handler = new ExternalCallHandler<WebClientStore>(App, this, null, typeof(Exec));
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private ExternalCallHandler<WebClientStore> m_Handler;
    private HttpService m_Server;

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;
    public override string ComponentCommonName => "minidp";

    public IExternalCallHandler GetExternalCallHandler() => m_Handler;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }


    [Config("$msg-algo;$msg-algorithm")]
    public string MessageProtectionAlgorithmName { get; set; }

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => MessageProtectionAlgorithmName.IsNullOrWhiteSpace() ? null :
                                                                      App.SecurityManager
                                                                         .Cryptography
                                                                         .MessageProtectionAlgorithms[MessageProtectionAlgorithmName]
                                                                         .NonNull("Algo `{0}`".Args(MessageProtectionAlgorithmName))
                                                                         .IsTrue(a => a.Audience == CryptoMessageAlgorithmAudience.Internal &&
                                                                                      a.Flags.HasFlag(CryptoMessageAlgorithmFlags.Cipher) &&
                                                                                      a.Flags.HasFlag(CryptoMessageAlgorithmFlags.CanUnprotect),
                                                                                      "Algo `{0}` !internal !cipher".Args(MessageProtectionAlgorithmName));

    /// <summary>
    /// Logical service address of IDP server
    /// </summary>
    [Config, ExternalParameter("IdpServerAddress", ExternalParameterSecurityCheck.OnSet, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public string IdpServerAddress { get; set; }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Server);
      if (node==null) return;

      var nServer = node[CONFIG_SERVER_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override void DoStart()
    {
      m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVER_SECTION));
      IdpServerAddress.NonBlank(nameof(IdpServerAddress));
      var algo = this.MessageProtectionAlgorithm;//this throws if not configured properly
      base.DoStart();
    }


    private async Task<MinIdpUserData> guardedIdpAccess(Func<Guid, Task<MinIdpUserData>> body)
    {
      var rel = Guid.NewGuid();
      try
      {
        if (ComponentEffectiveLogLevel < MessageType.Trace)
          WriteLog(MessageType.DebugA, nameof(guardedIdpAccess), "#001 IDP.Call", related: rel);

        var result = await body(rel).ConfigureAwait(false);

        if (ComponentEffectiveLogLevel < MessageType.Trace)
          WriteLog(MessageType.DebugA, nameof(guardedIdpAccess), "#002 IDP.Call result. User name: `{0}`".Args(result?.Name), related: rel);

        return result;
      }
      catch(Exception cause)
      {
        if (ComponentEffectiveLogLevel < MessageType.Trace)
          WriteLog(MessageType.DebugError, nameof(guardedIdpAccess), "#003 IDP.Call result exception: {0}".Args(cause.ToMessageWithType()), related: rel);

        if (cause is WebCallException wce && wce.HttpStatusCode == 404)
        {
          if (ComponentEffectiveLogLevel < MessageType.Trace)
            WriteLog(MessageType.DebugA, nameof(guardedIdpAccess), "#004 Got WCE-404", related: rel);

          return null;//404 is treated as "user not found"
        }

        var error = new SecurityException(StringConsts.SECURITY_IDP_UPSTREAM_CALL_ERROR.Args(cause.ToMessageWithType()), cause);
        WriteLog(MessageType.CriticalAlert, nameof(guardedIdpAccess), error.Message, error: error, related: rel);
        throw error;
      }
    }


    private MinIdpUserData processResponse(Guid rel, JsonDataMap response)
    {
      if (ComponentEffectiveLogLevel < MessageType.Trace)
        WriteLog(MessageType.DebugA, nameof(processResponse), "#100 Got response", related: rel, pars: response.ToJson(JsonWritingOptions.CompactASCII));

      var got = response.UnwrapPayloadObject();
      if (got == null) return null;

      var dataMap = got as JsonDataMap;

      if (dataMap == null)
      {
        if (got is string ciphered && ciphered.IsNotNullOrWhiteSpace())
        {
          if (ComponentEffectiveLogLevel < MessageType.Trace)
            WriteLog(MessageType.DebugA, nameof(processResponse), "#150 Deciphering", related: rel, pars: ciphered);

          var deciphered = MessageProtectionAlgorithm.NonNull(nameof(MessageProtectionAlgorithm))
                                                     .UnprotectObject(ciphered);

          if (deciphered == null)//returns null if message could not be deciphered
            WriteLog(MessageType.Critical, nameof(processResponse), StringConsts.SECURITY_IDP_RESPONSE_DECIPHER_ERROR, related: rel);

          dataMap =  deciphered as JsonDataMap;
        }
        else
        {
          var etext = StringConsts.SECURITY_IDP_PROTOCOL_ERROR.Args("unsupported `data` of type `{0}`".Args(got.GetType().Name));

          if (ComponentEffectiveLogLevel < MessageType.Trace)
            WriteLog(MessageType.DebugError, nameof(processResponse), "#155 "+ etext, related: rel);

          throw new SecurityException(etext);
        }
      }

      if (dataMap == null)
      {
        if (ComponentEffectiveLogLevel < MessageType.Trace)
          WriteLog(MessageType.DebugA, nameof(processResponse), "#160 datamap is Null", related: rel);
        return null;
      }

      if (ComponentEffectiveLogLevel < MessageType.Trace)
        WriteLog(MessageType.DebugA, nameof(processResponse), "#170 ToDoc", related: rel, pars: dataMap.ToJson(JsonWritingOptions.CompactASCII));

      return JsonReader.ToDoc<MinIdpUserData>(dataMap);
    }

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id, AuthenticationRequestContext ctx = null)
      => await guardedIdpAccess(async (rel) =>
          {
              var response = await m_Server.Call(IdpServerAddress,
                                            nameof(IMinIdpStore),
                                            new ShardKey(id),
                                            (tx, c) => tx.Client
                                                         .PostAndGetJsonMapAsync("byid", new { realm = realm, id = id, ctx = ctx }))//do NOT del prop names
                                           .ConfigureAwait(false);

              return processResponse(rel, response);
          }).ConfigureAwait(false);

    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx = null)
      => await guardedIdpAccess(async (rel) =>
          {
            var response = await m_Server.Call(IdpServerAddress,
                                          nameof(IMinIdpStore),
                                          new ShardKey(sysToken),
                                          (tx, c) => tx.Client
                                                       .PostAndGetJsonMapAsync("bysys", new { realm = realm, sysToken = sysToken, ctx = ctx }))//do NOT del prop names
                                         .ConfigureAwait(false);

            return processResponse(rel, response);
          }).ConfigureAwait(false);

    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx = null)
      => await guardedIdpAccess(async (rel) =>
          {
            var response = await m_Server.Call(IdpServerAddress,
                                          nameof(IMinIdpStore),
                                          new ShardKey(uri),
                                          (tx, c) => tx.Client
                                                       .PostAndGetJsonMapAsync("byuri", new { realm = realm, uri =  uri, ctx = ctx }))//do NOT del prop names
                                         .ConfigureAwait(false);

            return processResponse(rel, response);
          }).ConfigureAwait(false);


    public async Task<IEnumerable<object>> ExecCommandAsync(IConfigSectionNode step)
    {
      var stepSrc = step.NonEmpty(nameof(step)).ToLaconicString();

      //take all servers in all shards
      var shards = m_Server.GetEndpointsForAllShards(IdpServerAddress, nameof(IMinIdpStore));

      //and select physically distinct servers on their Uris
      //regardless of shard ordering, because we need to apply this command on ALL PHYSICALL boxes
      var allServers = shards.SelectMany(s => s)
                             .DistinctBy(ep => ((IHttpEndpoint)ep.Endpoint).Uri);

      //broadcast command to all physical servers
      //20210206 Impersonation aspect is not used because it is only needed for this ONE call to exec
      var impersonationAuthHeader = Ambient.CurrentCallUser.MakeSysTokenAuthHeader();//get current call flow identity
      var results = new List<object>();
      foreach(var server in allServers)
      {
        var result = new JsonDataMap();
        result["host"] = ((IHttpEndpoint)server.Endpoint).Uri.ToString();
        result["shard"] = "{0}[{1}]".Args(server.Endpoint.Shard, server.Endpoint.ShardOrder);
        results.Add(result);
        try
        {
          var cohortOfOne = server.ToEnumerable();

          var got = await cohortOfOne.Call
          (
            (http, ct) => http.Client
                              .PostAndGetJsonMapAsync
                               (
                                 "exec",
                                 new { source = stepSrc },
                                 requestHeaders: impersonationAuthHeader.ToEnumerable()//Auth on remote server using this call flow identity
                               )
          ).ConfigureAwait(false);

          //try to convert JSON from string representation to object, to avoid extra padding
          if (got["data"] is string sdata && got["ctype"].AsString().IndexOf("json", StringComparison.InvariantCultureIgnoreCase)>=0)
          {
            try
            {
              var d = sdata.JsonToDataObject();//if conversion succeeds
              got["data"] = d;//re-assigned parsed json as object
            }catch{ /* otherwise, keep payload as is*/ }
          }

          result["response"] = got;
        }
        catch(Exception error)
        {
          var e = new WrappedExceptionData(error, false, true);
          result["error"] = e;
          //keep on executing script
        }
      }

      return results;
    }

  }
}
