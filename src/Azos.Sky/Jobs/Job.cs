/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Jobs
{
  /// <summary>
  /// Uniform abstraction of a job.
  /// All jobs inherit from this class
  /// </summary>
  public abstract class Job
  {

    public void ApplySignal(/*IJobRuntime,*/ Signal signal)
    {

    }


    /// <summary>
    /// Executes the most due slice, return the next time the job should run
    /// </summary>
    public abstract Task<TimeSpan?> ExecuteSliceAsync(/*IJobRuntime*/);
  }

  public abstract class Job<TParameters, TState> : Job where TState : State
  {

    /// <summary>
    /// Job creation parameters
    /// </summary>
    public TParameters Parameters { get; }

    /// <summary>
    /// Jobs current state
    /// </summary>
    public TState State { get; }
  }
}
