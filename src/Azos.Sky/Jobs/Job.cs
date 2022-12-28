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

    public void ApplySignal(Signal signal)
    {

    }


    /// <summary>
    /// Executes the most due slice, return the next time the job should run
    /// </summary>
    public abstract Task<TimeSpan?> ExecuteSliceAsync();
  }

  public abstract class Job<TState> : Job
  {
    /// <summary>
    /// Jobs current state
    /// </summary>
    public TState State { get; }
  }
}
