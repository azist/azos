/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Azos.Conf;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Collections.Generic;

namespace Azos.Wave.Kestrel
{
  /// <summary>
  /// Makes and configures IWebHost from captured config options.
  /// Called "Factory" not to clash with Msft's "Builder" names
  /// </summary>
  public class HostFactory
  {
    /// <summary>
    /// Defines binding point which Kestrel listens on
    /// </summary>
    public class Binding
    {
      public Binding(IConfigSectionNode cfg)
      {
        cfg.NonEmpty(nameof(cfg));

        var sip = cfg.Of("ip").Value;

        if (sip.IsNotNullOrWhiteSpace()) //Null = ANY IP
        {
          IPAddress.TryParse(sip, out var ip).IsTrue("Valid ip");
          Ip = ip;
        }

        int.TryParse(cfg.Of("port").Value, out var port).IsTrue("Valid port");
        Port = port;

        using(var scope = new Security.SecurityFlowScope(Security.TheSafe.SAFE_ACCESS_FLAG))
        {
          CertificateFile = cfg.Of("cert-file").Value;
          CertificatePassword = cfg.Of("cert-pwd").Value;
        }
      }


      public Binding(IPAddress ip, int port, string certificateFile = null, string certificatePassword = null)
      {
        Ip = ip;//null = ANY ip
        Port = port.IsTrue(v => v > 0 && v < 0xffff);
        CertificateFile = certificateFile;
        CertificatePassword = certificatePassword;
      }

      public IPAddress Ip { get; private set; }
      public int Port     { get; private set; }
      public string CertificateFile { get; private set; }
      public string CertificatePassword { get; private set; }
    }


    public HostFactory(KestrelServerModule module, IConfigSectionNode cfg)
    {
      Module = module.NonNull(nameof(module));

      if (cfg != null)
      {
        ConfigAttribute.Apply(this, cfg);
        var bindings = new List<Binding>();
        Bindings = bindings;
        cfg.ChildrenNamed("binding").ForEach(one => bindings.Add(new Binding(one)));
      }
    }

    public IApplication App => Module.App;
    public readonly KestrelServerModule Module;

    [Config(Default = LogLevel.Warning)]
    public LogLevel MinAspLogLevel{ get; set; } = LogLevel.Warning;

    public IEnumerable<Binding> Bindings{ get; set; }

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

      var any = false;
      foreach(var binding in Bindings.NonNull(nameof(Bindings)))
      {
        any = true;

        if (binding.Ip == null) //ANY IP
          opt.ListenAnyIP(binding.Port, lopt => DoBindingListenOptions(lopt, binding));
        else
          opt.Listen(binding.Ip, binding.Port, lopt => DoBindingListenOptions(lopt,  binding));
      }

      any.IsTrue("Defined bindings");
    }

    protected virtual void DoBindingListenOptions(ListenOptions opt, Binding binding)
    {
      if (binding.CertificateFile.IsNotNullOrWhiteSpace())
      {
        if (binding.CertificatePassword.IsNotNullOrWhiteSpace())
        {
          opt.UseHttps(binding.CertificateFile, binding.CertificatePassword);
        }
        else
        {
          opt.UseHttps(binding.CertificateFile);
        }
      }
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
