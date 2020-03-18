/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Denotes data stream types that Bix directly supports.
  /// Arrays write .Array prefix first then the element type
  /// Complex object are serialized as JSON
  /// </summary>
  public enum WireType : byte
  {
    Null = 0,
    Doc,
    Array,
    Object,

    Boolean = 32,
    Char,
    String,

    Byte,
    ByteArray,//special optimization case
    SByte,

    Int16,
    Int32,
    Int64,

    UInt16,
    UInt32,
    UInt64,

    Single,
    Double,
    Decimal,
    Amount,

    DateTime,
    TimeSpan,

    Guid,
    GDID,
    FID,
    PilePointer,
    NLSMap,
    Atom
  }

}
