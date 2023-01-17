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
    /// <summary>
    /// framework internal method used by processor runtime to initialize fibers.
    /// we don't want to have senseless constructor chaining which just creates meaningless code
    /// </summary>
    internal void __processor__ctor(IFiberRuntime runtime, FiberParameters pars, FiberState state)
    {
      m_Runtime    = runtime.NonNull(nameof(runtime));
      m_Parameters = pars.NonNull(nameof(pars));
      m_State      = state.NonNull(nameof(state));

      //Inject all dependencies
      runtime.App.DependencyInjector.InjectInto(this);
    }

    private IFiberRuntime m_Runtime;
    private FiberParameters m_Parameters;
    private FiberState m_State;

    /// <summary>
    /// References the runtime system such as the hosting process and other services which
    /// make this fiber execute its code
    /// </summary>
    public IFiberRuntime   Runtime =>  m_Runtime;

    /// <summary>
    /// An immutable document of parameters supplied at the fiber start.
    /// This data may not be changed during Fiber lifespan
    /// </summary>
    public FiberParameters Parameters => m_Parameters;

    /// <summary>
    /// A mutable state bag which gets persisted by the runtime.
    /// This is where fiber instances keep their transitive state such as "field values", their collections etc.
    /// The state is persisted after EVERY fiber timeslice execution.
    /// The state object is logically divided into named areas called "slots", each keeping track of what has been changed
    /// </summary>
    public FiberState      State => m_State;


    /// <summary>
    /// Interprets the incoming signal by performing some work and generates <see cref="FiberSignalResult"/>.
    /// Returns null if the signal is unhandled.
    /// The supplied <see cref="FiberSignal"/> instance is already validated by the runtime at this point
    /// </summary>
    public virtual Task<FiberSignalResult> ApplySignalAsync(FiberSignal signal)
    {
      if (signal is PingSignal ping)
      {
        return Task.FromResult<FiberSignalResult>(new PingSignalResult{ Echoed = ping.Echo });
      }
      else
      {
        return Task.FromResult<FiberSignalResult>(null);
      }
    }

    /// <summary>
    /// Executes the most due slice, return the next time the fiber should run.
    /// The default implementation delegates to <see cref="DefaultExecuteSliceStepByConventionAsync"/>
    /// </summary>
    public virtual Task<FiberStep> ExecuteSliceAsync() => DefaultExecuteSliceStepByConventionAsync(this, State.CurrentStep);

    /// <summary>
    /// Performs by-convention invocation of a fiber "step" method for the specified instance,
    /// defaulting the name of the method by convention to "Step_{step: atom}"
    /// </summary>
    protected static Task<FiberStep> DefaultExecuteSliceStepByConventionAsync(Fiber self, Atom step)
    {
      var tself = self.NonNull(nameof(self)).GetType();
      var mi = FiberStep.GetMethodForStepByConvention(tself, step);

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

  /// <summary>
  /// Generic version of <see cref="Fiber"/> with typed `Parameters` and `State` properties.
  /// You should inherit your fibers from this class
  /// </summary>
  public abstract class Fiber<TParameters, TState> : Fiber where TParameters : FiberParameters
                                                           where TState : FiberState
  {
    /// <summary>
    /// An immutable document of parameters supplied at the fiber start.
    /// This data may not be changed during Fiber lifespan
    /// </summary>
    public new TParameters   Parameters => (TParameters)base.Parameters;

    /// <summary>
    /// A mutable state bag which gets persisted by the runtime.
    /// This is where fiber instances keep their transitive state such as "field values", their collections etc.
    /// The state is persisted after EVERY fiber timeslice execution.
    /// The state object is logically divided into named areas called "slots", each keeping track of what has been changed
    /// </summary>
    public new TState        State =>      (TState)base.State;
  }

  #region EXAMPLE ONLY!!!!!!!!!!!!!
  public class BakerFiber : Fiber<FiberParameters, BakerState>
  {
    async Task<FiberStep> Step_Start()
    {
      State.CakeCount++;
      return FiberStep.Continue(Step_Email, TimeSpan.FromHours(0.2));
    }

    async Task<FiberStep> Step_Email()
    {
      return FiberStep.ContinueImmediately(Step_Notify);
    }

    async Task<FiberStep> Step_Notify()
    {
      return FiberStep.Finish(0);
    }
  }
  #endregion

}
