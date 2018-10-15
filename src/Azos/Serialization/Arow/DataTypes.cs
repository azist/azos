

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
