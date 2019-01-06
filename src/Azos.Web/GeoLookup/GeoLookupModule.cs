/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.GeoLookup
{
  /// <summary>
  /// Provides module facade for GeoLookupService
  /// </summary>
  public sealed class GeoLookupModule : ModuleBase, IGeoLookup
  {
    /// <summary>
    /// Creates a root module without a parent
    /// </summary>
    public GeoLookupModule(IApplication application) : base(application) => ctor();

    /// <summary>
    /// Creates a module under a parent module, such as HubModule
    /// </summary>
    public GeoLookupModule(IModule parent) : base(parent) => ctor();


    private void ctor()
    {
      m_Service = new GeoLookupService(this);
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Service);
      base.Destructor();
    }

    private GeoLookupService m_Service;


    public bool Available => m_Service.Available;
    public override string ComponentLogTopic => m_Service.ComponentLogTopic;
    public override bool IsHardcodedModule => false;
    public LookupResolution Resolution => m_Service.Resolution;

    public GeoEntity Lookup(IPAddress address) => m_Service.Lookup(address);


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      m_Service.Configure(node);
    }

    protected override bool DoApplicationAfterInit()
    {
      Task.Run( () =>
      {
        try {m_Service.Start(); }
        catch(Exception error)
        {
          WriteLog(Log.MessageType.CatastrophicError, "m_GeoService.Start()", "Leaked: "+error.ToMessageWithType(), error);
        }
      });
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      m_Service.SignalStop();
      return base.DoApplicationBeforeCleanup();
    }

  }
}
