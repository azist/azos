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
  /// Provides a uniform abstraction base for Fibers.
  /// Fibers are units of logical cooperative multi-task execution.
  /// Unlike threads, fibers are not preempted but rather "yield" control back to their runtime using "steps"
  /// in a coroutine-like fashion. The runtime system then persist the <see cref="FiberState"/> transitive fiber data
  /// which gets created during fiber step execution, in a safe storage, this way fiber execution survives system restarts and crashes.
  /// All fibers inherit from this class indirectly.
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


    /// <summary>
    /// Interprets the incoming signal by performing some work and generates <see cref="FiberSignalResult"/>.
    /// Returns null if the signal is unhandled.
    /// The supplied <see cref="FiberSignal"/> instance is already validated by the runtime at this point
    /// </summary>
    public virtual Task<FiberSignalResult> ApplySignalAsync(FiberSignal signal)
    {
      return Task.FromResult<FiberSignalResult>(null);
    }

    /// <summary>
    /// Executes the most due slice, return the next time the fiber should run.
    /// The default implementation delegates to <see cref="DefaultExecuteSliceByConventionAsync"/>
    /// </summary>
    public virtual Task<FiberStep> ExecuteSliceAsync() => DefaultExecuteSliceByConventionAsync(this, State.CurrentStep);

    /// <summary>
    /// Performs by-convention invocation of a fiber "step" method for the specified instance,
    /// defaulting the name of the method by convention to "Step_{step: atom}"
    /// </summary>
    protected static Task<FiberStep> DefaultExecuteSliceByConventionAsync(Fiber self, Atom step)
    {
      var mn = FiberStep.CONVENTION_STEP_METHOD_NAME_PREFIX + step.Value;

      var mi = self.GetType().GetMethod(mn);
      mi.NonNull("Existing method: " + mn);

      Task<FiberStep> resultTask;
      try
      {
        resultTask = mi.Invoke(self, null) as Task<FiberStep>;
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


# region EXAMPLE ONLY!!!!!!!!!!!!!
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
      return FiberStep.Finish(0);
    }
#endregion

  }
}
