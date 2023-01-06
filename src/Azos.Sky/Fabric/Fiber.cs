/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
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
    /// Executes the most due slice, return the next time the job should run
    /// </summary>
    public abstract Task<(Atom nextStep, TimeSpan? nextTime)> ExecuteSliceAsync(Atom step);
  }

  public abstract class Fiber<TParameters, TResult, TState> : Fiber where TParameters : FiberParameters
                                                                    where TResult : FiberResult
                                                                    where TState : FiberState
  {
    protected Fiber(IFiberRuntime runtime, TParameters pars, TState state) : base(runtime, pars, state)
    {
    }

    public new TParameters   Parameters => (TParameters)base.Parameters;
    public new TState        State =>      (TState)base.State;

    public Task<FiberStep> Kaka()
    {
      FiberStep.ContinueAfter(Kaka, TimeSpan.FromHours(0.2));
      return null;
    }

  }
}
