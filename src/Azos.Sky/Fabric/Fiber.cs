/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Uniform abstraction of a fiber.
  /// All fibers inherit from this class
  /// </summary>
  public abstract class Fiber
  {
    protected Fiber(IFiberRuntime runtime, FiberParameters pars, FiberState state)
    {

    }

    private readonly IFiberRuntime m_Runtime;
    private readonly FiberParameters m_Parameters;
    private readonly FiberState m_State;

    public IFiberRuntime   Runtime =>  m_Runtime;
    public FiberParameters Parameters => m_Parameters;
    public FiberState      State => m_State;



    public void ApplySignal(/*IJobRuntime,*/ FiberSignal signal)
    {

    }


    /// <summary>
    /// Executes the most due slice, return the next time the fiber should run.
    /// The default implementation delegates to <see cref="DefaultExecuteSliceByConventionAsync"/>
    /// </summary>
    public virtual Task<FiberStep> ExecuteSliceAsync() => DefaultExecuteSliceByConventionAsync();

    public Task<FiberStep> DefaultExecuteSliceByConventionAsync()
    {
      var mn = FiberStep.CONVENTION_STEP_METHOD_NAME_PREFIX + State.CurrentStep.Value;

      var mi = this.GetType().GetMethod(mn);
      mi.NonNull("Existing method: " + mn);

      Task<FiberStep> resultTask;
      try
      {
        resultTask = mi.Invoke(this, null) as Task<FiberStep>;
      }
      catch(TargetInvocationException tie)
      {
        throw tie.InnerException;
      }

      return resultTask.NonNull(nameof(resultTask));
    }
  }

  public abstract class Fiber<TParameters, TState> : Fiber where TParameters : FiberParameters
                                                           where TState : FiberState
  {
    protected Fiber(IFiberRuntime runtime, TParameters pars, TState state) : base(runtime, pars, state)
    {
    }

    public new TParameters   Parameters => (TParameters)base.Parameters;
    public new TState        State =>      (TState)base.State;

    public async Task<FiberStep> Step_Start()
    {
      return FiberStep.Continue(Step_Email, TimeSpan.FromHours(0.2));
    }

    public async Task<FiberStep> Step_Email()
    {
      return FiberStep.Continue(Step_Notify, TimeSpan.FromHours(0.2));
    }

    public async Task<FiberStep> Step_Notify()
    {
      return FiberStep.Finish();
    }

  }
}
