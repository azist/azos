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
      Unassigned = 0,
      Valid = 1,
      EOF = 2,
      BadAddress = -100,
      InvalidLength = -101,
      BadHeader = -102
    }

    public Entry(Status state)
    {
      State = state;
      Address = 0;
      Raw = new ArraySegment<byte>();
    }

    public Entry(Status state, int address, ArraySegment<byte> raw)
    {
      State = state;
      Address = address;
      Raw = raw;
    }


    public readonly Status State;
    public readonly int Address;
    public ArraySegment<byte> Raw;
  }
}
