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
      base.DoStart();
    }


    private async Task<MinIdpUserData> guardedIdpAccess(Func<Task<MinIdpUserData>> body)
    {
      try
      {
        var result = await body();
        return result;
      }
      catch(Exception cause)
      {
        if (cause is WebCallException wce && wce.HttpStatusCode == 404) return null;//404 is treated as "user not found"

        var error = new SecurityException(StringConsts.SECURITY_IDP_UPSTREAM_CALL_ERROR.Args(cause.ToMessageWithType()), cause);
        WriteLog(MessageType.CriticalAlert, nameof(guardedIdpAccess), error.Message, error);
        throw error;
      }
    }

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
      => await guardedIdpAccess(async () =>
          {
              var map = await m_Server.Call(IdpServerAddress,
                                            nameof(IMinIdpStore),
                                            id,
                                            (tx, c) => tx.Client
                                                         .PostAndGetJsonMapAsync("byid", new { realm = realm, id = id}));//do NOT del prop names

              return JsonReader.ToDoc<MinIdpUserData>(map.UnwrapPayloadMap());
          });

    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
      => await guardedIdpAccess(async () =>
          {
            var map = await m_Server.Call(IdpServerAddress,
                                          nameof(IMinIdpStore),
                                          sysToken,
                                          (tx, c) => tx.Client
                                                       .PostAndGetJsonMapAsync("bysys", new { realm = realm, sysToken = sysToken }));//do NOT del prop names

            return JsonReader.ToDoc<MinIdpUserData>(map.UnwrapPayloadMap());
          });

    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
      => await guardedIdpAccess(async () =>
          {
            var map = await m_Server.Call(IdpServerAddress,
                                          nameof(IMinIdpStore),
                                          uri,
                                          (tx, c) => tx.Client
                                                       .PostAndGetJsonMapAsync("byuri", new { realm = realm, uri =  uri }));//do NOT del prop names

            return JsonReader.ToDoc<MinIdpUserData>(map.UnwrapPayloadMap());
          });


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
          );

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
