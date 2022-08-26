/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Wave.Kestrel
{
  /// <summary>
  /// Hosts Kestrel server which routes incoming traffic into <see cref="WaveServer.Pool"/> for processing
  /// </summary>
  public sealed class KestrelServerModule : ModuleBase
  {
    public const int DEFAULT_SHUTDOWN_TIMEOUT_MS = 3580;
    public const int MIN_SHUTDOWN_TIMEOUT_MS = 100;
    public const int MAX_SHUTDOWN_TIMEOUT_MS = 20000;

    public const string CONFIG_HOST_SECTION = "host";

    public KestrelServerModule(IApplication application) : base(application){ }

    public KestrelServerModule(IModule parent) : base(parent) { }

    private HostFactory m_Factory;
    private IWebHost m_WebHost;

    [Config(Default = DEFAULT_SHUTDOWN_TIMEOUT_MS)]
    public int ShutdownTimeoutMs { get; set; } = DEFAULT_SHUTDOWN_TIMEOUT_MS;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.WEB_TOPIC;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      node.NonEmpty(nameof(node));

      base.DoConfigure(node);

      //this is were you configure your Kestrel options etc..
      var nHost = node[CONFIG_HOST_SECTION].NonEmpty("section `{0}`".Args(CONFIG_HOST_SECTION));
      m_Factory = FactoryUtils.Make<HostFactory>(nHost, typeof(HostFactory), new object[]{this, nHost});
    }

    protected override bool DoApplicationAfterInit()
    {
      //This is where you start the Kestrel application host
      m_WebHost = m_Factory.Make();
      m_WebHost.Start();
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      //This is where you stop the Kestrel application host
      if (m_WebHost != null)
      {
        var timeout = TimeSpan.FromMilliseconds(ShutdownTimeoutMs.KeepBetween(MIN_SHUTDOWN_TIMEOUT_MS, MAX_SHUTDOWN_TIMEOUT_MS));
        m_WebHost.StopAsync(timeout).Await();
        DisposeAndNull(ref m_WebHost);
      }
      return base.DoApplicationBeforeCleanup();
    }

    /// <summary>
    /// Implements functional-style terminal middleware which always handles the request
    /// </summary>
    public async Task WaveAsyncTerminalMiddleware(HttpContext context)
    {
      try
      {
        var pool = WaveServer.Pool.Get(App);
        var server = await pool.DispatchAsync(context).ConfigureAwait(false);
        if (server == null)
        {
          //503 Service Unavailable
          //The server cannot handle the request(because it is overloaded or down for maintenance).Generally, this is a temporary state.
          // See: https://en.wikipedia.org/wiki/List_of_HTTP_status_codes
          context.Response.StatusCode = 503;
          await context.Response.WriteAsJsonAsync(new
          {
            OK = false,
            status = 503,
            description = "Request was not handled by any Wave server instance"
          }).ConfigureAwait(false);
        }
      }
      catch(Exception leak)
      {
        WriteLogFromHere(Log.MessageType.Critical, $"{nameof(WaveAsyncTerminalMiddleware)} leaked: {leak.ToMessageWithType()}", leak);
        context.Response.StatusCode = 500;
      }
    }

  }
}