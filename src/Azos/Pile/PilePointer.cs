/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.IO;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Pile
{
  /// <summary>
  /// Represents a pointer to the pile object (object stored in a pile).
  /// The reference may be local or distributed in which case the NodeID is>=0.
  /// Distributed pointers are very useful for organizing piles of objects distributed among many servers, for example
  ///  for "Big Memory" implementations or large neural networks where nodes may inter-connect between servers.
  /// The CLR reference to the IPile is not a part of this struct for performance and practicality reasons, as
  ///  it is highly unlikely that there are going to be more than one instance of a pile in a process, however
  ///  should more than 1 pile be allocated than this pointer would need to be wrapped in some other structure along with source IPile reference
  /// </summary>
  [Serializable]
  public struct PilePointer : IEquatable<PilePointer>, IJsonWritable, IJsonReadable, IValidatable
  {
    /// <summary>
    /// Returns a -1:-1 non-valid pointer (either local or distributed)
    /// </summary>
    public static PilePointer Invalid => new PilePointer(-1,-1);

    /// <summary>
    /// Creates distributed pointer
    /// </summary>
    public PilePointer(int nodeId, int seg, int addr)
    {
      NodeID = nodeId;
      Segment = seg;
      Address = addr;
    }

    /// <summary>
    /// Create local pointer
    /// </summary>
    public PilePointer(int seg, int addr)
    {
      NodeID = -1;
      Segment = seg;
      Address = addr;
    }

    /// <summary>
    /// Distributed Node ID. The local pile sets this to -1 rendering this pointer as !DistributedValid
    /// </summary>
    public readonly int NodeID;

    /// <summary>
    /// Segment # within pile
    /// </summary>
    public readonly int Segment;

    /// <summary>
    /// Address within the segment
    /// </summary>
    public readonly int Address;

    /// <summary>
    /// Returns true if the pointer has positive segment and address, however this does not mean that pointed-to data exists.
    /// Even if this is a valid local pointer it may be an invalid distributed pointer
    /// </summary>
    public bool Valid => Segment >= 0 && Address >= 0;

    /// <summary>
    /// Returns true if the pointer has positive distributed NodeID and has a valid local pointer
    /// </summary>
    public bool DistributedValid{ get{ return NodeID >= 0 && Valid;}}

    public override int GetHashCode() => Address;

    public override bool Equals(object obj) => obj is PilePointer pp ? this.Equals(pp) : false;

    public bool Equals(PilePointer other) => (this.NodeID == other.NodeID) &&
                                             (this.Segment == other.Segment) &&
                                             (this.Address == other.Address);

    public override string ToString()
    {
      if (NodeID<0)
       return "L:"+Segment.ToString("X4")+":"+Address.ToString("X8");
      else
       return NodeID.ToString("X4")+":"+Segment.ToString("X4")+":"+Address.ToString("X8");
    }

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("n", NodeID),
                                                      new DictionaryEntry("s", Segment),
                                                      new DictionaryEntry("a", Address));
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        var node = map["n"].AsInt(-1);
        var seg  = map["s"].AsInt(-1);
        var addr = map["a"].AsInt(-1);

        return (true, new PilePointer(node, seg, addr));
      }

      return (false, null);
    }

    public ValidState Validate(ValidState state, string scope = null)
    {
      if (!Valid) state = new ValidState(state, new ValidationException(StringConsts.PILE_PTR_VALIDATION_ERROR.Args(this, scope)));
      return state;
    }

    public static bool operator ==(PilePointer l, PilePointer r) => l.Equals(r);

    public static bool operator !=(PilePointer l, PilePointer r) => !l.Equals(r);

    /// <summary>
    /// Dereference operator: var x = pile ^ ptr;
    /// </summary>
    public static object operator ^(IPile pile, PilePointer ptr) => pile.NonNull(nameof(pile)).Get(ptr);//->     var x = pile ^ ptr;
  }
}
