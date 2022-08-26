/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Azos.Conf;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Azos.Wave.Kestrel
{
  /// <summary>
  /// Makes and configures IWebHost from captured config options.
  /// Called "Factory" not to clash with Msft's "Builder" names
  /// </summary>
  public class HostFactory
  {
    public HostFactory(KestrelServerModule module, IConfigSectionNode cfg)
    {
      Module = module.NonNull(nameof(module));
      ConfigAttribute.Apply(this, cfg);
    }

    public IApplication App => Module.App;
    public readonly KestrelServerModule Module;

    [Config] public int ListenAnyIpPort{ get; set;}

    [Config(Default = LogLevel.Warning)]
    public LogLevel MinAspLogLevel{ get; set; } = LogLevel.Warning;


    public virtual IWebHost Make()
    {
      var kestrelCommandArgs = new string[0];
      var builder = WebHost.CreateDefaultBuilder(kestrelCommandArgs);

      builder.UseKestrel(opt => DoKestrel(opt));
      builder.ConfigureServices(services => DoServices(services));
      builder.Configure(appb => DoApp(appb)  );

      var host = builder.Build();
      return host;
    }

    protected virtual void DoKestrel(KestrelServerOptions opt)
    {
      opt.AddServerHeader = false;
      opt.AllowSynchronousIO = true;//used by Wave for now in some legacy code path (e.g. StockHandler)
      //opt.Limits....
      if (ListenAnyIpPort>0) opt.ListenAnyIP(ListenAnyIpPort);
    }

    protected virtual void DoServices(IServiceCollection services)
    {
      services.AddSingleton<Azos.IApplication>(App);
      // Net 6 does not add CTRL+C handler?
      // services.AddSingleton<IHostLifetime, AzosHostLifetime>();
      services.AddLogging(logging => DoService_Logging(logging));
    }

    protected virtual void DoApp(IApplicationBuilder app)
    {
    ///////  app.Run((ctx) => ctx.Response.WriteAsync("aaaaaa"));
      app.Run((ctx) => Module.WaveAsyncTerminalMiddleware(ctx));
    }

    protected virtual void DoService_Logging(ILoggingBuilder logging)
    {
      logging.ClearProviders();//clear in-built console logger
      logging.SetMinimumLevel(MinAspLogLevel);
      logging.AddProvider(new AzosLogProvider(Module.App));

      ////////////Example:
      ////////////logging.AddFilter("Microsoft", LogLevel.Warning)
      ////////////      logging.ClearProviders();
      ////////////         .AddConsole(cfg => {
      ////////////             //  cfg.LogToStandardErrorThreshold = LogLevel.Critical;
      ////////////         });
    }

  }
}
