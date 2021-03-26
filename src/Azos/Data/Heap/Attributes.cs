/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Data.Heap
{
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

    public Atom Channel {  get; private set;}

    public string ChannelName
    {
      get => Channel.Value;
      set => Channel = Atom.Encode(value.NonBlank(nameof(ChannelName)));
    }

  }
}
