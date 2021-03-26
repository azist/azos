/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azos.Data.Heap
{

  public class ObjectVersion : IEquatable<ObjectVersion>, IComparable<ObjectVersion>, IJsonWritable, IJsonReadable
  {
    public enum State : byte
    {
      Undefined = 0,
      Created = 1,
      Modified = 2,
      Deleted = 3
    }

    [Field]
    public State Status { get; set; }

    [Field]
    public ulong Utc { get; set; }


    public bool Equals(ObjectVersion other)
    {
      throw new NotImplementedException();
    }

    public int CompareTo(ObjectVersion other)
    {
      throw new NotImplementedException();
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      throw new NotImplementedException();
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      throw new NotImplementedException();
    }
  }
}
