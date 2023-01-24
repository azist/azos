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
using Azos.Conf;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Log;
using Azos.Serialization.JSON;
using Azos.Sky.Identification;

namespace Azos.Sky.Fabric.Server
{
  /// <summary>
  /// Provides server implementation for IFiberManagerLogic
  /// </summary>
  public sealed class FiberManagerServerLogic : ModuleBase, IFiberManagerLogic
  {
    public const string SEQ_SKY_LOG = "sky_log";
    public const string SEQ_SKY_INSTR = "sky_ins";

    public const string CONFIG_STORE_SECTION = "store";

    public FiberManagerServerLogic(IApplication application) : base(application) { }
    public FiberManagerServerLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
//      DisposeAndNull(ref m_LogArchiveGraph);
      base.Destructor();
    }

    [Inject] IGdidProviderModule m_Gdid;

    //private ILogImplementation m_LogArchiveGraph;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;


    #region Plumbing
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
 //     DisposeAndNull(ref m_LogArchiveGraph);

      if (node == null) return;

      //var nArchive = node[CONFIG_LOG_ARCHIVE_SECTION];
      //if (nArchive.Exists)
      {
      //  m_LogArchiveGraph = FactoryUtils.MakeAndConfigureDirectedComponent<ILogImplementation>(this, nArchive, typeof(LogDaemon));
      }


    }

    protected override bool DoApplicationAfterInit()
    {
  //    m_Log.NonNull($"Configured {nameof(m_Log)}").Start();

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
//      m_Log.WaitForCompleteStop();
      return base.DoApplicationBeforeCleanup();
    }

    #endregion

    #region IFiberManagerLogic
    public IEnumerable<Atom> GetRunspaces()
    {
      //we need  registry of runspaces
      throw new NotImplementedException();
    }

    public FiberId AllocateFiberId(Atom runspace)
    {
      runspace.HasRequiredValue(nameof(runspace))
              .AsValid(nameof(runspace));
      throw new NotImplementedException();
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

    public Task<FiberInfo> RecoverAsync(FiberId idFiber, bool pause, string statusDescription)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
