/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;

using Azos.Apps;
using Azos.Collections;
using Azos.Instrumentation;

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
  public interface ITransportImplementation : ITransport, IDisposable{ }

}
