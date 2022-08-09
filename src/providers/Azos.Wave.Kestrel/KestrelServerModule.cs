/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Wave.Kestrel
{
  /// <summary>
  /// Hosts Kestrel server which routes incoming traffic into <see cref="WaveServer.Pool"/> for processing
  /// </summary>
  public sealed class KestrelServerModule : ModuleBase
  {
    public KestrelServerModule(IApplication application) : base(application){ }

    public KestrelServerModule(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.WEB_TOPIC;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      //this is were you configure your Kestrel options etc..
    }

    protected override bool DoApplicationAfterInit()
    {
      //This is where you start the Kestrel application host
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      //THis is where you stop the Kestrel application host
      return base.DoApplicationBeforeCleanup();
    }

    /// <summary>
    /// Implements functional-style terminal middleware which always handles the request
    /// </summary>
    public async Task WaveAsyncTerminalMiddleware(HttpContext context, RequestDelegate next)
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
            description = "Request was not handled by any Wave server at this time"
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