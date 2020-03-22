/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using Azos.Serialization.JSON;

Why not use UINT for array lengths, int is more complex and slower???????????????????????????????

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Facilitates reading primitives from stream in Bix Format
  /// </summary>
  public struct BixReader
  {
    public BixReader(Stream stream) => m_Stream = stream;

    private readonly Stream m_Stream;

    #region BYTE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadFromStream(byte[] buffer, int count)
    {
      if (count <= 0) return;
      var total = 0;
      do
      {
        var got = m_Stream.Read(buffer, total, count - total);

        if (got < 1) //EOF
          throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadFromStream(Need: {0}; Got: {1})".Args(count, total));

        total += got;
      } while (total < count);
    }

    public TCollection ReadByteCollection<TCollection>() where TCollection : class, ICollection<byte>, new()
      => ReadCollection<TCollection, byte>( bix => bix.ReadByte());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
      var b = m_Stream.ReadByte();
      if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadByte(): eof");
      return (byte)b;
    }

    public byte? ReadNullableByte()
    {
      if (ReadBool()) return ReadByte();
      return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadByteArray() => ReadBuffer();

    public byte[] ReadBuffer()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "byte", Format.MAX_BYTE_ARRAY_LEN));

      var buf = new byte[len];

      ReadFromStream(buf, len);

      return buf;
    }

    public TCollection ReadNullableByteCollection<TCollection>() where TCollection : class, ICollection<byte?>, new()
     => ReadCollection<TCollection, byte?>(bix => bix.ReadNullableByte());

    public byte?[] ReadNullableByteArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bytes?", Format.MAX_BYTE_ARRAY_LEN));

      var result = new byte?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableByte();

      return result;
    }

    #endregion

    #region BOOL
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool()//important to inline as this is used by all Nullables* and ref types
    {
      var b = m_Stream.ReadByte();
      if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadBool(): eof");
      return b != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool? ReadNullableBool()
    {
      if (ReadBool()) return ReadBool();
      return null;
    }

    public TCollection ReadBoolCollection<TCollection>() where TCollection : class, ICollection<bool>, new()
      => ReadCollection<TCollection, bool>(bix => bix.ReadBool());

    public bool[] ReadBoolArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bool", Format.MAX_BYTE_ARRAY_LEN));

      var result = new bool[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadBool();

      return result;
    }


    public TCollection ReadNullableBoolCollection<TCollection>() where TCollection : class, ICollection<bool?>, new()
      => ReadCollection<TCollection, bool?>(bix => bix.ReadNullableBool());

    public bool?[] ReadNullableBoolArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bool?", Format.MAX_BYTE_ARRAY_LEN));

      var result = new bool?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableBool();

      return result;
    }

    #endregion

    #region SBYTE

    public sbyte ReadSbyte()
    {
      var b = m_Stream.ReadByte();
      if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadSbyte(): eof");
      return (sbyte)b;
    }

    public sbyte? ReadNullableSbyte()
    {
      if (ReadBool()) return ReadSbyte();
      return null;
    }

    public TCollection ReadSbyteCollection<TCollection>() where TCollection : class, ICollection<sbyte>, new()
      => ReadCollection<TCollection, sbyte>(bix => bix.ReadSbyte());

    public sbyte[] ReadSbyteArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "sbyte", Format.MAX_BYTE_ARRAY_LEN));

      var result = new sbyte[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadSbyte();

      return result;
    }

    public TCollection ReadNullableSbyteCollection<TCollection>() where TCollection : class, ICollection<sbyte?>, new()
      => ReadCollection<TCollection, sbyte?>(bix => bix.ReadNullableSbyte());

    public sbyte?[] ReadNullableSbyteArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "sbytes?", Format.MAX_BYTE_ARRAY_LEN));

      var result = new sbyte?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableSbyte();

      return result;
    }

    #endregion

    #region SHORT
    public short ReadShort()
    {
      short result = 0;
      var b = m_Stream.ReadByte();
      if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadShort(): eof");

      var neg = ((b & 1) != 0);


      var has = (b & 0x80) > 0;
      result |= (short)((b & 0x7f) >> 1);
      var bitcnt = 6;

      while (has)
      {
        if (bitcnt > 15)
          throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadShort()");

        b = m_Stream.ReadByte();
        if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadShort(): eof");
        has = (b & 0x80) > 0;
        result |= (short)((b & 0x7f) << bitcnt);
        bitcnt += 7;
      }

      return (short)(neg ? ~result : result);
    }

    public short? ReadNullableShort()
    {
      if (ReadBool()) return ReadShort();
      return null;
    }


    public TCollection ReadShortCollection<TCollection>() where TCollection : class, ICollection<short>, new()
     => ReadCollection<TCollection, short>(bix => bix.ReadShort());

    public short[] ReadShortArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "shorts", Format.MAX_SHORT_ARRAY_LEN));

      var result = new short[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadShort();

      return result;
    }

    public TCollection ReadNullableShortCollection<TCollection>() where TCollection : class, ICollection<short?>, new()
     => ReadCollection<TCollection, short?>(bix => bix.ReadNullableShort());

    public short?[] ReadNullableShortArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "shorts?", Format.MAX_SHORT_ARRAY_LEN));

      var result = new short?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableShort();

      return result;
    }

    #endregion

    #region USHORT

    public ushort ReadUshort()
    {
      ushort result = 0;
      var bitcnt = 0;
      var has = true;

      while (has)
      {
        if (bitcnt > 31)
          throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadUshort()");

        var b = m_Stream.ReadByte();
        if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadUshort(): eof");
        has = (b & 0x80) > 0;
        result |= (ushort)((b & 0x7f) << bitcnt);
        bitcnt += 7;
      }

      return result;
    }

    public ushort? ReadNullableUshort()
    {
      if (ReadBool()) return ReadUshort();
      return null;
    }

    public TCollection ReadUshortCollection<TCollection>() where TCollection : class, ICollection<ushort>, new()
     => ReadCollection<TCollection, ushort>(bix => bix.ReadUshort());

    public ushort[] ReadUshortArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ushort", Format.MAX_SHORT_ARRAY_LEN));

      var result = new ushort[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadUshort();

      return result;
    }

    public TCollection ReadNullabeUshortCollection<TCollection>() where TCollection : class, ICollection<ushort?>, new()
     => ReadCollection<TCollection, ushort?>(bix => bix.ReadNullableUshort());

    public ushort?[] ReadNullableUshortArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ushort?", Format.MAX_SHORT_ARRAY_LEN));

      var result = new ushort?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableUshort();

      return result;
    }
    #endregion

    #region INT
    public int ReadInt()
    {
      int result = 0;
      var b = m_Stream.ReadByte();
      if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadInt(): eof");

      var neg = ((b & 1) != 0);


      var has = (b & 0x80) > 0;
      result |= ((b & 0x7f) >> 1);
      var bitcnt = 6;

      while (has)
      {
        if (bitcnt > 31)
          throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadInt()");

        b = m_Stream.ReadByte();
        if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadInt(): eof");
        has = (b & 0x80) > 0;
        result |= (b & 0x7f) << bitcnt;
        bitcnt += 7;
      }

      return neg ? ~result : result;
    }

    public int? ReadNullableInt()
    {
      if (ReadBool()) return ReadInt();
      return null;
    }

    public TCollection ReadIntCollection<TCollection>() where TCollection : class, ICollection<int>, new()
     => ReadCollection<TCollection, int>(bix => bix.ReadInt());

    public int[] ReadIntArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ints", Format.MAX_INT_ARRAY_LEN));

      var result = new int[len];

      for (int i = 0; i < len; i++)
        result[i] = this.ReadInt();

      return result;
    }

    public TCollection ReadNullableIntCollection<TCollection>() where TCollection : class, ICollection<int?>, new()
     => ReadCollection<TCollection, int?>(bix => bix.ReadNullableInt());

    public int?[] ReadNullableIntArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ints?", Format.MAX_INT_ARRAY_LEN));

      var result = new int?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableInt();

      return result;
    }
    #endregion

    public uint[] ReadUintArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "uints", Format.MAX_INT_ARRAY_LEN));

      var result = new uint[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadUint();

      return result;
    }

    public uint?[] ReadNullableUintArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "uints?", Format.MAX_INT_ARRAY_LEN));

      var result = new uint?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadUint();

      return result;
    }

    public long[] ReadLongArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "longs", Format.MAX_LONG_ARRAY_LEN));

      var result = new long[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadLong();

      return result;
    }

    public long?[] ReadNullableLongArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "longs?", Format.MAX_LONG_ARRAY_LEN));

      var result = new long?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableLong();

      return result;
    }

    public ulong[] ReadUlongArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ulongs", Format.MAX_LONG_ARRAY_LEN));

      var result = new ulong[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadUlong();

      return result;
    }

    public ulong?[] ReadNullableUlongArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ulongs?", Format.MAX_LONG_ARRAY_LEN));

      var result = new ulong?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableUlong();

      return result;
    }

    public double[] ReadDoubleArray()
    {
      if (!ReadBool()) return null;

      var len = this.ReadInt();
      if (len > Format.MAX_DOUBLE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "doubles", Format.MAX_DOUBLE_ARRAY_LEN));

      var result = new double[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadDouble();

      return result;
    }

    public double?[] ReadNullableDoubleArray()
    {
      if (!ReadBool()) return null;

      var len = this.ReadInt();
      if (len > Format.MAX_DOUBLE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "doubles?", Format.MAX_DOUBLE_ARRAY_LEN));

      var result = new double?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableDouble();

      return result;
    }

    public float[] ReadFloatArray()
    {
      if (!ReadBool()) return null;

      var len = this.ReadInt();
      if (len > Format.MAX_FLOAT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "floats", Format.MAX_FLOAT_ARRAY_LEN));

      var result = new float[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadFloat();

      return result;
    }

    public float?[] ReadNullableFloatArray()
    {
      if (!ReadBool()) return null;

      var len = this.ReadInt();
      if (len > Format.MAX_FLOAT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "floats?", Format.MAX_FLOAT_ARRAY_LEN));

      var result = new float?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableFloat();

      return result;
    }

    public decimal[] ReadDecimalArray()
    {
      if (!ReadBool()) return null;

      var len = this.ReadInt();
      if (len > Format.MAX_DECIMAL_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "decimals", Format.MAX_DECIMAL_ARRAY_LEN));

      var result = new decimal[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadDecimal();

      return result;
    }

    public decimal?[] ReadNullableDecimalArray()
    {
      if (!ReadBool()) return null;

      var len = this.ReadInt();
      if (len > Format.MAX_DECIMAL_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "decimals?", Format.MAX_DECIMAL_ARRAY_LEN));

      var result = new decimal?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableDecimal();

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char ReadChar() => (char)ReadUshort();

    public char? ReadNullableChar()
    {
      if (!ReadBool()) return null;
      return ReadChar();
    }

    public char[] ReadCharArray()
    {
      if (!ReadBool()) return null;
      return ReadString().ToCharArray();
    }

    public char?[] ReadNullableCharArray()
    {
      if (!ReadBool()) return null;

      var len = this.ReadInt();
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "char?", Format.MAX_SHORT_ARRAY_LEN));

      var result = new char?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableChar();

      return result;
    }

    public string[] ReadStringArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();

      if (len > Format.MAX_STRING_ARRAY_CNT)
        throw new BixException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "strings", Format.MAX_STRING_ARRAY_CNT));

      var result = new string[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadString();

      return result;
    }

    public decimal ReadDecimal()
    {
      var bits_0 = ReadInt();
      var bits_1 = ReadInt();
      var bits_2 = ReadInt();
      var bits_3 = ReadByte();
      return new Decimal(bits_0,
                          bits_1,
                          bits_2,
                          (bits_3 & 0x80) != 0,
                          (byte)(bits_3 & 0x7F));
    }

    public decimal? ReadNullableDecimal()
    {
      if (ReadBool()) return ReadDecimal();
      return null;
    }

    public unsafe double ReadDouble()
    {
      var buf = Format.GetBuff32();
      ReadFromStream(buf, 8);

      uint seg1 = (uint)((int)buf[0] |
                          (int)buf[1] << 8 |
                          (int)buf[2] << 16 |
                          (int)buf[3] << 24);

      uint seg2 = (uint)((int)buf[4] |
                          (int)buf[5] << 8 |
                          (int)buf[6] << 16 |
                          (int)buf[7] << 24);

      ulong core = (ulong)seg2 << 32 | (ulong)seg1;

      return *(double*)(&core);
    }

    public double? ReadNullableDouble()
    {
      if (ReadBool()) return ReadDouble();
      return null;
    }

    public unsafe float ReadFloat()
    {
      var buf = Format.GetBuff32();
      ReadFromStream(buf, 4);

      uint core = (uint)((int)buf[0] |
                          (int)buf[1] << 8 |
                          (int)buf[2] << 16 |
                          (int)buf[3] << 24);
      return *(float*)(&core);
    }

    public float? ReadNullableFloat()
    {
      if (ReadBool()) return ReadFloat();
      return null;
    }



    public long ReadLong()
    {
      long result = 0;
      var b = m_Stream.ReadByte();
      if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadLong(): eof");

      var neg = ((b & 1) != 0);


      var has = (b & 0x80) > 0;
      result |= ((long)(b & 0x7f) >> 1);
      var bitcnt = 6;

      while (has)
      {
        if (bitcnt > 63)
          throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadLong()");

        b = m_Stream.ReadByte();
        if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadLong(): eof");
        has = (b & 0x80) > 0;
        result |= (long)(b & 0x7f) << bitcnt;
        bitcnt += 7;
      }

      return neg ? ~result : result;
    }

    public long? ReadNullableLong()
    {
      if (ReadBool()) return ReadLong();
      return null;
    }





    public string ReadString()
    {
      if (!ReadBool()) return null;

      var bsz = this.ReadInt();
      if (bsz==0) return string.Empty;

      if (bsz < Format.STR_BUF_SZ)
      {
        var reused = Format.GetStrBuff();
        ReadFromStream(reused, bsz);
        return Format.ENCODING.GetString(reused, 0, bsz);
      }


      if (bsz > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.SLIM_READ_X_ARRAY_MAX_SIZE_ERROR.Args(bsz, "string bytes", Format.MAX_BYTE_ARRAY_LEN));

      var buf = new byte[bsz];

      ReadFromStream(buf, bsz);

      return Format.ENCODING.GetString(buf);
    }

    public uint ReadUint()
    {
      uint result = 0;
      var bitcnt = 0;
      var has = true;

      while (has)
      {
        if (bitcnt > 31)
          throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadUint()");

        var b = m_Stream.ReadByte();
        if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadUint(): eof");
        has = (b & 0x80) != 0;
        result |= (uint)(b & 0x7f) << bitcnt;
        bitcnt += 7;
      }

      return result;
    }

    public uint? ReadNullableUint()
    {
      if (ReadBool()) return ReadUint();
      return null;
    }

    public ulong ReadUlong()
    {
      ulong result = 0;
      var bitcnt = 0;
      var has = true;

      while (has)
      {
        if (bitcnt > 63)
          throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadUlong()");

        var b = m_Stream.ReadByte();
        if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadUlong(): eof");
        has = (b & 0x80) > 0;
        result |= (ulong)(b & 0x7f) << bitcnt;
        bitcnt += 7;
      }

      return result;
    }

    public ulong? ReadNullableUlong()
    {
      if (ReadBool()) return ReadUlong();
      return null;
    }




    public DateTime ReadDateTime()
    {
      var ticks = (long)m_Stream.ReadBEUInt64();
      var kind = (DateTimeKind)m_Stream.ReadByte();
      return new DateTime(ticks, kind);
    }

    public DateTime? ReadNullableDateTime()
    {
      if (ReadBool()) return ReadDateTime();
      return null;
    }

    public TimeSpan ReadTimeSpan()
    {
      var ticks = this.ReadLong();
      return TimeSpan.FromTicks(ticks);
    }

    public TimeSpan? ReadNullableTimeSpan()
    {
      if (ReadBool()) return ReadTimeSpan();
      return null;
    }

    public Guid ReadGuid()
    {
      var arr = this.ReadByteArray();//inefficient copy!!
      return new Guid(arr);
    }

    public Guid? ReadNullableGuid()
    {
      if (ReadBool()) return ReadGuid();
      return null;
    }

    public Data.GDID ReadGDID()
    {
      var era = ReadUint();
      var id = ReadUlong();
      return new Data.GDID(era, id);
    }

    public Data.GDID? ReadNullableGDID()
    {
      if (ReadBool()) return ReadGDID();
      return null;
    }

    public FID ReadFID()
    {
      var id = m_Stream.ReadBEUInt64();
      return new FID(id);
    }

    public FID? ReadNullableFIDt()
    {
      if (ReadBool()) return ReadFID();
      return null;
    }


    public Pile.PilePointer ReadPilePointer()
    {
      var node = this.ReadInt();
      var seg = this.ReadInt();
      var adr = this.ReadInt();
      return new Pile.PilePointer(node, seg, adr);
    }

    public Pile.PilePointer? ReadNullablePilePointer()
    {
      if (ReadBool()) return ReadPilePointer();
      return null;
    }

    public NLSMap ReadNLSMap()
    {
      var cnt = ReadUshort();
      if (cnt <= 0) return new NLSMap();
      if (cnt > NLSMap.MAX_ISO_COUNT) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "Exceeded NLSMap.MAX_ISO_COUNT");

      var data = new NLSMap.NDPair[cnt];
      for (var i = 0; i < cnt; i++)
      {
        var iso0 = this.ReadByte();
        var iso1 = this.ReadByte();
        var iso2 = this.ReadByte();
        var iso = (iso0 << 16) + (iso1 << 8) + (iso2);
        var name = this.ReadString();
        var descr = this.ReadString();
        data[i] = new NLSMap.NDPair(iso, name, descr);
      }

      return new NLSMap(data);
    }

    public NLSMap? ReadNullableNLSMap()
    {
      if (ReadBool()) return ReadNLSMap();
      return null;
    }

    public Financial.Amount ReadAmount()
    {
      var iso = ReadString();
      var val = ReadDecimal();

      return Financial.Amount.Deserialize(iso, val);
    }

    public Financial.Amount? ReadNullableAmount()
    {
      if (ReadBool()) return ReadAmount();
      return null;
    }

    public Collections.StringMap ReadStringMap()
    {
      if (!ReadBool()) return null;

      var senseCase = ReadBool();

      var dict = Collections.StringMap.MakeDictionary(senseCase);

      var count = ReadInt();
      for (var i = 0; i < count; i++)
      {
        var key = ReadString();
        var value = ReadString();
        dict[key] = value;
      }

      return new Collections.StringMap(senseCase, dict);
    }

    public Atom ReadAtom() => new Atom(ReadUlong());

    public Atom? ReadNullableAtom()
    {
      if (ReadBool()) return ReadAtom();
      return null;
    }

    #region Collection
    /// <summary>
    /// Reads a collection of T using a functor
    /// </summary>
    public TCollection ReadCollection<TCollection, T>(Func<BixReader, T> read) where TCollection : class, ICollection<T>, new()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_COLLECTION_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, typeof(T).Name, Format.MAX_COLLECTION_LEN));

      var result = new TCollection();

      for (int i = 0; i < len; i++)
      {
        var elm = read(this);
        result.Add(elm);
      }

      return result;
    }
    #endregion

  }
}
