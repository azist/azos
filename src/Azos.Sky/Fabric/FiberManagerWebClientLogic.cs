/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Client;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Web;


namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Provides client for consuming IFiberManagerLogic remote services
  /// </summary>
  public sealed class FiberManagerWebClientLogic : ModuleBase, IFiberManagerLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public FiberManagerWebClientLogic(IApplication application) : base(application) { }
    public FiberManagerWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.FABRIC_TOPIC;


    /// <summary>
    /// Logical service address of logger
    /// </summary>
    [Config]
    public string ServiceAddress{  get; set; }


    #region Lifetime
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Server);
      if (node == null) return;

      var nServer = node[CONFIG_SERVICE_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVICE_SECTION));
      ServiceAddress.NonBlank(nameof(ServiceAddress));

      return base.DoApplicationAfterInit();
    }

    #endregion

    #region IFiberManagerLogic .impl

#warning THIS NEEDS to be completed after controller is done on the server side #813
    public IEnumerable<Atom> GetRunspaces()
    {
      throw new NotImplementedException();
    }

    public FiberId AllocateFiberId(Atom runspace)
    {
      runspace.HasRequiredValue(nameof(runspace))
              .AsValid(nameof(runspace));

      //////GDID gFiber;
      //////if (m_Gdid != null)
      //////{
      //////  //use local Gdid generator
      //////  //gFiber = ......
      //////}

      //otherwise delagte Gdid generation to server
      throw new NotImplementedException();

      //return new FiberId(runspace, gFiber);
    }

    public Task<FiberInfo> StartFiberAsync(FiberStartArgs args)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<FiberInfo>> GetFiberListAsync(FiberFilter args)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> GetFiberInfoAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberParameters> GetFiberParametersAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberResult> GetFiberResultAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<FiberState> GetFiberStateAsync(FiberId idFiber)
    {
      throw new NotImplementedException();
    }

    public Task<bool> LoadFiberStateSlotAsync(FiberId idFiber, FiberState.Slot slot)
    {
      throw new NotImplementedException();
    }

    public Task<FiberSignalResponse> SendSignalAsync(FiberSignal signal)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> SetPriorityAsync(FiberId idFiber, float priority, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> PauseAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> SuspendAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> ResumeAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    public Task<FiberInfo> AbortAsync(FiberId idFiber, string statusDescription)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
