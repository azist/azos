/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Client
{
  /// <summary>
  /// Represents a transport channel which is used to make remote server calls.
  /// For Http this is typically a HttpClient configured with default headers and protocol handlers
  /// </summary>
  public interface ITransport
  {
    /// <summary>
    /// Returns a service endpoint which this transport connects to
    /// </summary>
    EndpointAssignment Assignment { get; }
  }

  /// <summary>
  /// Transport implementation
  /// </summary>
  public interface ITransportImplementation : ITransport, IDisposable
  {
  }

  /// <summary>
  /// Transport implementation for use with TrasportPool
  /// </summary>
  public interface IManagedTrasportImplementation : ITransportImplementation
  {
    /// <summary>
    /// Returns true if this call transition transport state from free to acquired
    /// </summary>
    bool TryAcquire();
    bool Release();

    /// <summary>
    /// True is the transport is acquired for use
    /// </summary>
    bool IsAcquired{ get;}

    /// <summary>
    /// Returns when the transport was used for the last time
    /// </summary>
    DateTime LastUseUtc{ get;}
  }

}
