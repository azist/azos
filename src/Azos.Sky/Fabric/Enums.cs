using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Fiber execution statuses
  /// </summary>
  public enum FiberStatus
  {
    /// <summary>
    /// The fiber was created but ts status has not been determined yet
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// The fiber has been created but has not started (yet)
    /// </summary>
    Created,

    /// <summary>
    /// The fiber is running
    /// </summary>
    Started,

    /// <summary>
    /// Paused fiber skip their time slices but still react to signals (e.g. termination), contrast with Suspended mode
    /// </summary>
    Paused,

    /// <summary>
    /// Suspended fiber do not react to signals and do not execute their time slices
    /// </summary>
    Suspended,

    /// <summary>
    /// The fiber has finished normally
    /// </summary>
    Finished = 0x1FFFF,

    /// <summary>
    /// The fiber has finished abnormally with unhandled exception
    /// </summary>
    Crashed = -1,

    /// <summary>
    /// The fiber has finished abnormally due to signal intervention, e.g. manual termination or call to Abort("reason");
    /// </summary>
    Aborted = -2
  }



  /// <summary>
  /// A tuple of (Kind[NotFound|Suspended|Responded], FiberSignalResult)
  /// </summary>
  public struct FiberSignalResponse
  {
    /// <summary>
    /// Defines the kinds of fiber signal sending outcomes
    /// </summary>
    public enum Kind
    {
      /// <summary>
      /// Fiber was not found and could not process signal
      /// </summary>
      NotFound,

      /// <summary>
      /// Fiber is suspended and not able to process signals
      /// </summary>
      Suspended,

      /// <summary>
      /// Fiber responded with <see cref="FiberSignalResult"/> object
      /// </summary>
      Responded
    }

    public FiberSignalResponse(Kind outcome, FiberSignalResult result)
    {
      Outcome = outcome;
      Result = result;
    }

    /// <summary>
    /// Defines the outcome type of signal response
    /// </summary>
    public readonly Kind Outcome;

    /// <summary>
    /// The resulting object of fiber signal processing
    /// </summary>
    public readonly FiberSignalResult Result;
  }
}
