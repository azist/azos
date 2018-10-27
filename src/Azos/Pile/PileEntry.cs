/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Pile
{
  /// <summary>
  /// Represents an entry stored in a pile - a pointer+size vector.
  /// Used for pile enumeration
  /// </summary>
  public struct PileEntry
  {
    public enum DataType { Object=0, String, Buffer, Link }

    public static PileEntry Invalid{ get{ return new PileEntry(PilePointer.Invalid,DataType.Object, -1); }}

    public PileEntry(PilePointer ptr, DataType tp, int size)
    {
      Pointer = ptr;
      Type = tp;
      Size = size;
    }

    /// <summary>
    /// Points to data
    /// </summary>
    public readonly PilePointer Pointer;

    /// <summary>
    /// Denotes data type, i.e.: Object/String/Buffer/Link
    /// </summary>
    public readonly DataType Type;

    /// <summary>
    /// The byte size of allocation
    /// </summary>
    public readonly int Size;


    public bool Valid
    {
      get{ return Pointer.Valid && Size>=0; }
    }
  }
}
