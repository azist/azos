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
  public enum TypeCode : byte
  {
    Null = 0,

    /// <summary>
    /// Array is a prefix code which is followed by array element type. Arrays are length-prefixed
    /// </summary>
    Array,

    /// <summary>
    /// List is a prefix code which is followed by list element type.
    /// Lists are collections of elements just like arrays but are written using IEnumerable so they are more generic.
    /// Lists perform slower than arrays but offer greater flexibility
    /// </summary>
    Collection,

    /// <summary>
    /// Effectively a special optimization for raw byte array
    /// </summary>
    Buffer,

    /// <summary>
    /// Data Document object without type ID
    /// </summary>
    Doc,

    /// <summary>
    /// Data Document object prefixed with document type ID
    /// </summary>
    DocWithType,

    /// <summary>
    /// Json-serialized payloa Payload
    /// </summary>
    Object,

    Atom,
    AtomNull,

    Bool,
    BoolNull,

    Char,
    CharNull,

    String,

    Byte,
    ByteNull,

    Sbyte,
    SbyteNull,

    Int16,
    Int16Null,

    Uint16,
    Uint16Null,

    Int32,
    Int32Null,

    Uint32,
    Uint32Null,

    Int64,
    Int64Null,

    Uint64,
    Uint64Null,

    Double,
    DoubleNull,

    Float,
    FloatNull,

    Decimal,
    DecimalNull,

    Amount,
    AmountNull,

    DateTime,
    DateTimeNull,

    TimeSpan,
    TimeSpanNull,

    Guid,
    GuidNull,

    GDID,
    GDIDNull,

    FID,
    FIDNull,

    PilePointer,
    PilePointerNull,

    NLSMap,
    NLSMapNull,

    StringMap,
    JsonDataMap
  }

}
