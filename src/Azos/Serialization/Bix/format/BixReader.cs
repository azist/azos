/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Runtime.CompilerServices;

using Azos.Serialization.JSON;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Facilitates reading primitives from stream in Bix Format
  /// </summary>
  public struct BixReader
  {
    public BixReader(Stream stream) => m_Stream = stream;

    private readonly Stream m_Stream;


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
    public bool ReadBool() => ReadByte() != 0;

    public bool? ReadNullableBool()
    {
      if (ReadBool()) return ReadBool();
      return null;
    }

    public byte[] ReadByteArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bytes", Format.MAX_BYTE_ARRAY_LEN));

      var buf = new byte[len];

      ReadFromStream(buf, len);

      return buf;
    }

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

    public sbyte[] ReadSbyteArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "sbytes", Format.MAX_BYTE_ARRAY_LEN));

      var result = new sbyte[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadSbyte();

      return result;
    }

    public sbyte?[] ReadNullableSbyteArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "sbytes?", Format.MAX_BYTE_ARRAY_LEN));

      var result = new sbyte[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableSbyte();

      return result;
    }

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

    public ushort[] ReadUshortArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ushorts", Format.MAX_SHORT_ARRAY_LEN));

      var result = new ushort[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadUshort();

      return result;
    }

    public ushort?[] ReadNullableUshortArray()
    {
      if (!ReadBool()) return null;

      var len = ReadInt();
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_READ_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ushorts?", Format.MAX_SHORT_ARRAY_LEN));

      var result = new ushort?[len];

      for (int i = 0; i < len; i++)
        result[i] = ReadNullableUshort();

      return result;
    }


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

    public sbyte ReadSbyte()
    {
      var b = m_Stream.ReadByte();
      if (b < 0) throw new BixException(StringConsts.BIX_STREAM_CORRUPTED_ERROR + "ReadSbyte(): eof");
      return (sbyte)b;
    }

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

    public string ReadString()
    {
      var has = this.ReadBool();
      if (!has) return null;

      var bsz = this.ReadInt();
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

    public DateTime ReadDateTime()
    {
      var ticks = (long)m_Stream.ReadBEUInt64();
      var kind = (DateTimeKind)m_Stream.ReadByte();
      return new DateTime(ticks, kind);
    }

    public TimeSpan ReadTimeSpan()
    {
      var ticks = this.ReadLong();
      return TimeSpan.FromTicks(ticks);
    }

    public Guid ReadGuid()
    {
      var arr = this.ReadByteArray();//inefficient copy!!
      return new Guid(arr);
    }

    public Data.GDID ReadGDID()
    {
      var era = ReadUint();
      var id = ReadUlong();
      return new Data.GDID(era, id);
    }

    public FID ReadFID()
    {
      var id = m_Stream.ReadBEUInt64();
      return new FID(id);
    }

    public Pile.PilePointer ReadPilePointer()
    {
      var node = this.ReadInt();
      var seg = this.ReadInt();
      var adr = this.ReadInt();
      return new Pile.PilePointer(node, seg, adr);
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

    public Financial.Amount ReadAmount()
    {
      var iso = ReadString();
      var val = ReadDecimal();

      return Financial.Amount.Deserialize(iso, val);
    }

    public Collections.StringMap ReadStringMap()
    {
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

  }
}
