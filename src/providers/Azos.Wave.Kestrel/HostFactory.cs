/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Azos.Conf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Azos.Wave.Kestrel
{
  /// <summary>
  /// Makes and configures IWebHost from captured config options
  /// </summary>
  public class HostFactory
  {
    public HostFactory(KestrelServerModule module, IConfigSectionNode cfg)
    {
      Module = module.NonNull(nameof(module));
    }

    public IApplication App => Module.App;
    public readonly KestrelServerModule Module;

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
      opt.ListenAnyIP(8080);
      opt.AllowSynchronousIO = true;
    }

    protected virtual void DoServices(IServiceCollection services)
    {
      services.AddLogging(logging => DoService_Logging(logging));
    }

    protected virtual void DoApp(IApplicationBuilder appb)
    {
      appb.Run((ctx) => Module.WaveAsyncTerminalMiddleware(ctx));
    }

    protected virtual void DoService_Logging(ILoggingBuilder logging)
    {
      logging.AddProvider(new AzosLogProvider(Module.App));
      //Example:
      //logging.AddFilter("Microsoft", LogLevel.Warning)
      //      logging.ClearProviders();
      //         .AddConsole(cfg => {
      //             //  cfg.LogToStandardErrorThreshold = LogLevel.Critical;
      //         });
    }

  }
}
