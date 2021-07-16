/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Embodies data for event with raw byte[] payload
  /// </summary>
  public struct Event
  {
    /// <summary>
    /// Initializes event. This is more of an infrastructure method and most business
    /// applications should use <see cref="IEventProducer.MakeNew(byte[], EventHeader[])"/> instead
    /// </summary>
    public Event(EventId id, EventHeaders headers, byte[] payload)
    {
      Id = id;
      Headers = headers;
      Payload = payload;
    }

    public readonly EventId Id;
    public readonly EventHeaders Headers;
    public readonly byte[] Payload;
  }
}
