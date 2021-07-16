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
  /// Name:Value tuple used as Event header.
  /// For best performance, event headers should limit their use and should
  /// contain as little as possible data in their name or value segments.
  /// </summary>
  public struct EventHeader
  {
    public const int MAX_COUNT = 255;
    public const int MAX_VALUE_LEN = 1024;

    public static readonly Atom HDR_CONTENT_TYPE = Atom.Encode("-ctp");

    public static readonly EventHeader ZERO = new EventHeader();

    public EventHeader(Atom id, string value)
    {
      Id = id.IsTrue(v => !v.IsZero && v.IsValid, "id.valid");
      Value = value.IsTrue(v => v == null || v.Length < MAX_VALUE_LEN, nameof(value));
    }

    /// <summary> Header id </summary>
    public readonly Atom Id;

    /// <summary> Header value; up to <see cref="MAX_VALUE_LEN"/> characters but may be blank </summary>
    public readonly string Value;

    public bool Assigned => !Id.IsZero;
  }

  /// <summary>
  /// EventHeader collection interface
  /// </summary>
  public struct EventHeaders : IEnumerable<EventHeader>
  {
    public EventHeaders(EventHeader[] data) => m_Data = data;

    private EventHeader[] m_Data;

    public bool Assigned => m_Data != null;
    public EventHeader this[int i] => Assigned ? m_Data[i] : EventHeader.ZERO;
    public EventHeader this[Atom id] => Assigned ? m_Data.FirstOrDefault(v => v.Id == id) : EventHeader.ZERO;

    public IEnumerator<EventHeader> GetEnumerator() => ((IEnumerable<EventHeader>)m_Data).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Data.GetEnumerator();
  }
}
