/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Serialization.Arow
{
  /// <summary>
  /// Denotes types that Arow directly supports. Complex object are serialized as POD
  /// </summary>
  public enum DataType
  {
    Null = 0,
    Doc,
    Array,
    POD,

    Boolean = 100,

    Char,
    String,

    Single,
    Double,
    Decimal,
    Amount,

    Byte,
    ByteArray,
    SByte,

    Int16,
    Int32,
    Int64,

    UInt16,
    UInt32,
    UInt64,

    DateTime,
    TimeSpan,

    Guid,
    GDID,
    FID,
    PilePointer,
    NLSMap
  }

}
