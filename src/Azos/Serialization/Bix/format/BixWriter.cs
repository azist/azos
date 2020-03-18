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
  /// Facilitates writing primitives into stream in Bix Format
  /// </summary>
  public struct BixWriter
  {
    public BixWriter(Stream stream) => m_Stream = stream;

    private readonly Stream m_Stream;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush() => m_Stream.Flush();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte value) => m_Stream.WriteByte(value);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(bool value) => Write(value ? Format.TRUE : Format.FALSE);


    public void Write(byte[] buffer)
    {
      var len = buffer.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bytes", Format.MAX_BYTE_ARRAY_LEN));

      Write(len);
      m_Stream.Write(buffer, 0, len);
    }

    public void Write(sbyte[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "sbyte", Format.MAX_BYTE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(short[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "short", Format.MAX_SHORT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void Write(ushort[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ushort", Format.MAX_SHORT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void Write(int[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ints", Format.MAX_INT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void Write(uint[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "uints", Format.MAX_INT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void Write(long[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "longs", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void Write(ulong[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ulongs", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void Write(double[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_DOUBLE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "doubles", Format.MAX_DOUBLE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(float[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_FLOAT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "floats", Format.MAX_FLOAT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Decimal(decimal[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_DECIMAL_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "decimals", Format.MAX_DECIMAL_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(char ch) => Write((ushort)ch);

    public void Write(char[] value)
    {
      var str = new string(value);
      this.Write(str);
    }

    public void Write(string[] value)
    {
      var len = value.Length;
      if (len > Format.MAX_STRING_ARRAY_CNT)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "strings", Format.MAX_STRING_ARRAY_CNT));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }


    public void Write(decimal value)
    {
      var bits = decimal.GetBits(value);
      this.Write(bits[0]);
      this.Write(bits[1]);
      this.Write(bits[2]);

      byte sign = (bits[3] & 0x80000000) != 0 ? (byte)0x80 : (byte)0x00;
      byte scale = (byte)((bits[3] >> 16) & 0x7F);

      this.Write((byte)(sign | scale));
    }

    public unsafe void Write(double value)
    {
      var buf = Format.GetBuff32();
      ulong core = *(ulong*)(&value);

      buf[0] = (byte)core;
      buf[1] = (byte)(core >> 8);
      buf[2] = (byte)(core >> 16);
      buf[3] = (byte)(core >> 24);
      buf[4] = (byte)(core >> 32);
      buf[5] = (byte)(core >> 40);
      buf[6] = (byte)(core >> 48);
      buf[7] = (byte)(core >> 56);

      m_Stream.Write(buf, 0, 8);
    }

    public unsafe void Write(float value)
    {
      var buf = Format.GetBuff32();
      uint core = *(uint*)(&value);
      buf[0] = (byte)core;
      buf[1] = (byte)(core >> 8);
      buf[2] = (byte)(core >> 16);
      buf[3] = (byte)(core >> 24);
      m_Stream.Write(buf, 0, 4);
    }

    public void Write(int value)
    {
      byte b = 0;

      if (value < 0)
      {
        b = 1;
        value = ~value;//turn off minus bit but dont +1
      }

      b = (byte)(b | ((value & 0x3f) << 1));
      value = value >> 6;
      var has = value != 0;
      if (has)
        b = (byte)(b | 0x80);
      m_Stream.WriteByte(b);
      while (has)
      {
        b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public void Write(long value)
    {
      byte b = 0;

      if (value < 0)
      {
        b = 1;
        value = ~value;//turn off minus bit but dont +1
      }

      b = (byte)(b | ((value & 0x3f) << 1));
      value = value >> 6;
      var has = value != 0;
      if (has)
        b = (byte)(b | 0x80);
      m_Stream.WriteByte(b);
      while (has)
      {
        b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public void Write(sbyte value) => m_Stream.WriteByte((byte)value);

    public void Write(short value)
    {
      byte b = 0;

      if (value < 0)
      {
        b = 1;
        value = (short)~value;//turn off minus bit but dont +1
      }

      b = (byte)(b | ((value & 0x3f) << 1));
      value = (short)(value >> 6);
      var has = value != 0;
      if (has)
        b = (byte)(b | 0x80);
      m_Stream.WriteByte(b);
      while (has)
      {
        b = (byte)(value & 0x7f);
        value = (short)(value >> 7);
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public void Write(string value)
    {
      var len = value.Length;
      if (len > Format.MAX_STR_LEN)//This is much faster than Encoding.GetByteCount()
      {
        var encoded = Format.ENCODING.GetBytes(value);
        Write(encoded.Length);
        m_Stream.Write(encoded, 0, encoded.Length);
        return;
      }

      //reuse pre-allocated buffer
      var buf = Format.GetStrBuff();
      var bcnt = Format.ENCODING.GetBytes(value, 0, len, buf, 0);

      Write(bcnt);
      m_Stream.Write(buf, 0, bcnt);
    }

    public void Write(uint value)
    {
      var has = true;
      while (has)
      {
        byte b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public void Write(ulong value)
    {
      var has = true;
      while (has)
      {
        byte b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public void Write(ushort value)
    {
      var has = true;
      while (has)
      {
        byte b = (byte)(value & 0x7f);
        value = (ushort)(value >> 7);
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public void Write(DateTime value)
    {
      m_Stream.WriteBEUInt64((ulong)value.Ticks);
      m_Stream.WriteByte((byte)value.Kind);
    }

    public void Write(TimeSpan value)
    {
      Write(value.Ticks);
    }

    public void Write(Guid value)
    {
      Write(value.ToByteArray());
    }

    public  void Write(Data.GDID value)
    {
      Write(value.Era);
      Write(value.ID);
    }

    public void Write(FID value)
    {
      m_Stream.WriteBEUInt64(value.ID);
    }

    public void Write(Pile.PilePointer value)
    {
      Write(value.NodeID);
      Write(value.Segment);
      Write(value.Address);
    }

    public void Write(NLSMap map)
    {
      if (map.m_Data == null)
      {
        Write((ushort)0);
        return;
      }

      Write((ushort)map.m_Data.Length);
      for (var i = 0; i < map.m_Data.Length; i++)
      {
        var nd = map.m_Data[i];
        Write((byte)((nd.ISO & 0xff0000) >> 16));
        Write((byte)((nd.ISO & 0x00ff00) >> 08));
        Write((byte)((nd.ISO & 0xFF)));
        Write(nd.Name);
        Write(nd.Description);
      }
    }

    public void Write(Financial.Amount value)
    {
      Write(value.CurrencyISO);
      Write(value.Value);
    }

    public void Write(Collections.StringMap map)
    {
      Write(map.CaseSensitive);
      Write((int)map.Count);

      foreach (var kvp in map)
      {
        Write(kvp.Key);
        Write(kvp.Value);
      }
    }

    public void Write(Atom value) => Write(value.ID);

  }
}
