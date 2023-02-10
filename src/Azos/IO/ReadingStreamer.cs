/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using Azos.Serialization.JSON;

namespace Azos.IO
{
  /// <summary>
  /// Reads primitives from stream
  /// </summary>
  public abstract class ReadingStreamer : Streamer
  {
    #region .ctor
    protected ReadingStreamer(Encoding encoding=null) : base(encoding)
    {
    }

    #endregion

    #region Protected

    protected int ReadFromStream(byte[] buffer, int count)
    {
      if (count<=0) return 0;
      var total = 0;
      do
      {
        var got = m_Stream.Read(buffer, total, count-total);
        if (got==0) //EOF
        throw new AzosIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadFromStream(Need: {0}; Got: {1})".Args(count, total));
        total += got;
      }while(total<count);

      return total;
    }


    #endregion

    #region Public

    public abstract bool ReadBool();
    public abstract bool? ReadNullableBool();

    public byte ReadByte()
    {
      var b = m_Stream.ReadByte();
      if (b<0) throw new AzosIOException(StringConsts.SLIM_STREAM_CORRUPTED_ERROR + "ReadByte(): eof");

      return (byte)b;
    }

    public abstract byte? ReadNullableByte();



    public abstract byte[] ReadByteArray();
    public abstract int[] ReadIntArray();
    public abstract long[] ReadLongArray();
    public abstract double[] ReadDoubleArray();
    public abstract float[] ReadFloatArray();
    public abstract decimal[] ReadDecimalArray();


    public abstract char ReadChar();
    public abstract char? ReadNullableChar();

    public abstract char[] ReadCharArray();
    public abstract string[] ReadStringArray();

    public abstract decimal ReadDecimal();
    public abstract decimal? ReadNullableDecimal();

    public abstract double ReadDouble();
    public abstract double? ReadNullableDouble();

    public abstract float ReadFloat();
    public abstract float? ReadNullableFloat();

    public abstract int ReadInt();
    public abstract int? ReadNullableInt();

    public abstract long ReadLong();
    public abstract long? ReadNullableLong();

    public abstract sbyte ReadSByte();
    public abstract sbyte? ReadNullableSByte();


    public abstract short ReadShort();
    public abstract short? ReadNullableShort();

    public abstract string ReadString();

    public abstract uint ReadUInt();
    public abstract uint? ReadNullableUInt();

    public abstract ulong ReadULong();
    public abstract ulong? ReadNullableULong();

    public abstract ushort ReadUShort();
    public abstract ushort? ReadNullableUShort();

    public abstract MetaHandle ReadMetaHandle();
    public abstract MetaHandle? ReadNullableMetaHandle();

    public abstract DateTime ReadDateTime();
    public abstract DateTime? ReadNullableDateTime();

    public abstract DateTimeOffset ReadDateTimeOffset();
    public abstract DateTimeOffset? ReadNullableDateTimeOffset();

    public abstract TimeSpan ReadTimeSpan();
    public abstract TimeSpan? ReadNullableTimeSpan();

    public abstract Guid ReadGuid();
    public abstract Guid? ReadNullableGuid();

    public abstract Data.GDID ReadGDID();
    public abstract Data.GDID? ReadNullableGDID();

    public abstract Data.RGDID ReadRGDID();
    public abstract Data.RGDID? ReadNullableRGDID();

    public abstract Glue.Protocol.TypeSpec ReadTypeSpec();
    public abstract Glue.Protocol.MethodSpec ReadMethodSpec();

    public abstract FID ReadFID();
    public abstract FID? ReadNullableFID();

    public abstract Pile.PilePointer ReadPilePointer();
    public abstract Pile.PilePointer? ReadNullablePilePointer();

    public abstract VarIntStr ReadVarIntStr();
    public abstract VarIntStr? ReadNullableVarIntStr();

    public abstract NLSMap ReadNLSMap();
    public abstract NLSMap? ReadNullableNLSMap();

    public abstract Financial.Amount ReadAmount();
    public abstract Financial.Amount? ReadNullableAmount();

    public abstract Collections.StringMap  ReadStringMap();

    public abstract Atom ReadAtom();
    public abstract Atom? ReadNullableAtom();
    #endregion
  }
}
