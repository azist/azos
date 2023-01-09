/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

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
      /// The response outcome kind was not set
      /// </summary>
      Undefined = 0,

      /// <summary>
      /// Fiber was not found and could not process signal
      /// </summary>
      NotFound = -1,

      /// <summary>
      /// Fiber is suspended and not able to process signals
      /// </summary>
      Suspended = -2,

      /// <summary>
      /// The fiber did not process the signal, e.g. wrong signal type
      /// which fiber does not know how to handle.
      /// Note: if the signal is malformed (e.g. missing required data)
      /// then fiber throws a 400-based exception instead
      /// </summary>
      NotProcessed = -3,

      /// <summary>
      /// Fiber responded with <see cref="FiberSignalResult"/> object
      /// </summary>
      Responded = +1
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
    /// The resulting object of fiber signal processing, null if the signal was not processed see <see cref="Outcome"/>
    /// </summary>
    public readonly FiberSignalResult Result;
  }
}
