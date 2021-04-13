/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Assigns (AREA, COLLECTION) tuple to HeapObjects. The attribute must be set to bind
  /// a CLI type to collection within data heap area.
  /// You can only bind no more than one CLR object type to a named collection at a time.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class HeapAttribute : Attribute
  {
    public HeapAttribute(string area, string collection)
    {
      Area = Atom.Encode(area.NonBlank(nameof(area)));
      Collection = Atom.Encode(area.NonBlank(nameof(collection)));
    }

    public Atom Area{ get; private set;}
    public Atom Collection { get; private set; }

    public Atom Channel { get; private set; }

    public string ChannelName
    {
      get => Channel.Value;
      set => Channel = Atom.Encode(value.NonBlank(nameof(ChannelName)));
    }

  }
}
