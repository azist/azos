/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IO.Archiving
{
  public struct Entry
  {
    public enum Status
    {
      BadHeader = -102,
      InvalidLength = -101,
      BadAddress = -100,

      Unassigned = 0,

      Valid = 1,
      EOF = 2
    }

    public Entry(int address, Status state)
    {
      Address = address;
      State = state;
      Raw = new ArraySegment<byte>();
    }

    public Entry(int address, ArraySegment<byte> raw)
    {
      State = Status.Valid;
      Address = address;
      Raw = raw;
    }


    public readonly Status State;
    public readonly int Address;
    public ArraySegment<byte> Raw;
  }
}
