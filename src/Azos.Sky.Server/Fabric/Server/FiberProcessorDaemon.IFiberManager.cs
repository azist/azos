/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Azos.Sky.Fabric.Server
{
  partial class FiberProcessorDaemon
  {
    public IEnumerable<Atom> GetRunspaces() => m_Runspaces.Select(one => one.Name).ToArray();

    public FiberId AllocateFiberId(Atom runspace)
    {
      CheckDaemonActive();
      runspace.IsValidNonZero(nameof(runspace));
      var rs = m_Runspaces[runspace];
      rs.NonNull($"Runspace `{runspace}`");
      var shard = rs.GetShardForNewAllocation();

      if (shard == null)
      {
        throw new FabricFiberAllocationException(ServerStringConsts.FABRIC_FIBER_ALLOC_NO_SPACE_ERROR.Args());
      }


      //Runspaces must be in separate databases as their ids are used as sequence names
      var gdid = m_Gdid.Provider.GenerateOneGdid(SysConsts.GDID_NS_FABRIC, runspace.Value);

      var result = new FiberId(rs.Name, shard.Name, gdid);
      return result;
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
  }
}
