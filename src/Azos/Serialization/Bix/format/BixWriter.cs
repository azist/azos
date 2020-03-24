/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
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

    public bool IsAssigned => m_Stream!=null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush() => m_Stream.Flush();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(TypeCode type) => m_Stream.WriteByte((byte)type);

    #region BYTE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte value) => m_Stream.WriteByte(value);

    public void Write(byte? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<byte> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte[] buffer) => WriteBuffer(buffer);//aliases needed for dispatch script uniformity

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBuffer(byte[] buffer)
    {
      if (buffer == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = buffer.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "byte", Format.MAX_BYTE_ARRAY_LEN));

      Write(len);
      m_Stream.Write(buffer, 0, len);
    }

    public void WriteCollection(ICollection<byte?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(byte?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "byte?", Format.MAX_BYTE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    #endregion

    #region BOOL
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(bool value) => Write(value ? Format.TRUE : Format.FALSE);

    public void Write(bool? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<bool> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(bool[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bool", Format.MAX_BYTE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void WriteCollection(ICollection<bool?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(bool?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bool?", Format.MAX_BYTE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region SBYTE
    public void Write(sbyte value) => m_Stream.WriteByte((byte)value);

    public void Write(sbyte? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<sbyte> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(sbyte[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "sbyte", Format.MAX_BYTE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void WriteCollection(ICollection<sbyte?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(sbyte?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_BYTE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "sbyte?", Format.MAX_BYTE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region SHORT

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

    public void Write(short? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<short> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(short[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "short", Format.MAX_SHORT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void WriteCollection(ICollection<short?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(short?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "short?", Format.MAX_SHORT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }


    #endregion

    #region USHORT

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

    public void Write(ushort? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<ushort> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(ushort[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ushort", Format.MAX_SHORT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void WriteCollection(ICollection<ushort?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(ushort?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ushort?", Format.MAX_SHORT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }
    #endregion

    #region INT
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

    public void Write(int? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<int> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(int[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "int", Format.MAX_INT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void WriteCollection(ICollection<int?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(int?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "int?", Format.MAX_INT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }
    #endregion

    #region UINT
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

    public void Write(uint? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<uint> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(uint[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "uint", Format.MAX_INT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void WriteCollection(ICollection<uint?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(uint?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_INT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "uint?", Format.MAX_INT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }
    #endregion

    #region LONG
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

    public void Write(long? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<long> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(long[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "long", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void WriteCollection(ICollection<long?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(long?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "long?", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }
    #endregion

    #region ULONG

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    public void Write(ulong? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<ulong> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(ulong[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ulong", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }

    public void WriteCollection(ICollection<ulong?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(ulong?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ulong?", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]); //WITH compression of every element
    }
    #endregion

    #region DOUBLE

    public unsafe void Write(double value)
    {
#warning Why not use direct pointer cast and write UINT instead of buffer? or write byte-by-byte?
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

    public void Write(double? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<double> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(double[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_DOUBLE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "double", Format.MAX_DOUBLE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void WriteCollection(ICollection<double?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(double?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_DOUBLE_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "double?", Format.MAX_DOUBLE_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region FLOAT

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

    public void Write(float? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<float> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(float[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_FLOAT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "float", Format.MAX_FLOAT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void WriteCollection(ICollection<float?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(float?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_FLOAT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "float?", Format.MAX_FLOAT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    #endregion

    #region DECIMAL
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

    public void Write(decimal? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<decimal> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(decimal[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_DECIMAL_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "decimal", Format.MAX_DECIMAL_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void WriteCollection(ICollection<decimal?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(decimal?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_DECIMAL_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "decimal?", Format.MAX_DECIMAL_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region CHAR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(char ch) => Write((ushort)ch);

    public void Write(char? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public void WriteCollection(ICollection<char> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(char[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var str = new string(value);
      this.Write(str);
    }

    public void WriteCollection(ICollection<char?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(char?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_SHORT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "char?", Format.MAX_SHORT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region STRING
#warning TRY to use stream of CHArs instead of encoding
    public void Write(string value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len == 0)
      {
        Write((int)0);
        return;
      }

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

    public void WriteCollection(ICollection<string> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(string[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_STRING_ARRAY_CNT)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "string", Format.MAX_STRING_ARRAY_CNT));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region DATETIME

    public void Write(DateTime value)
    {
      m_Stream.WriteBEUInt64((ulong)value.Ticks);
      m_Stream.WriteByte((byte)value.Kind);
    }

    public void WriteCollection(ICollection<DateTime> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(DateTime[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "datetime", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(DateTime? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<DateTime?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(DateTime?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "datetime?", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region TIMESPAN
    public void Write(TimeSpan value)
    {
      Write(value.Ticks);
    }

    public void WriteCollection(ICollection<TimeSpan> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(TimeSpan[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "timespan", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(TimeSpan? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<TimeSpan?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(TimeSpan?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "timespan?", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region GUID
    public unsafe void Write(Guid value)
    {
      var buf = Format.GetBuff32();
      buf.FastEncodeGuid(0, value);
      m_Stream.Write(buf, 0, 16);
    }

    public void WriteCollection(ICollection<Guid> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Guid[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_GUID_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "guid", Format.MAX_GUID_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(Guid? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<Guid?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Guid?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_GUID_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "guid?", Format.MAX_GUID_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region GDID
    public void Write(Data.GDID value)
    {
      Write(value.Era);
      Write(value.ID);
    }

    public void WriteCollection(ICollection<Data.GDID> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Data.GDID[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_GDID_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "gdid", Format.MAX_GDID_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(Data.GDID? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<Data.GDID?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Data.GDID?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_GDID_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "gdid?", Format.MAX_GDID_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region FID
    public void Write(FID value)
    {
      m_Stream.WriteBEUInt64(value.ID);
    }

    public void WriteCollection(ICollection<FID> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(FID[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "fid", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(FID? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<FID?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(FID?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "fid?", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    #endregion

    #region PilePointer
    public void Write(Pile.PilePointer value)
    {
      Write(value.NodeID);
      Write(value.Segment);
      Write(value.Address);
    }

    public void WriteCollection(ICollection<Pile.PilePointer> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Pile.PilePointer[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_PPTR_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "pptr", Format.MAX_PPTR_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(Pile.PilePointer? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<Pile.PilePointer?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Pile.PilePointer?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_PPTR_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "pptr?", Format.MAX_PPTR_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region NLSMap
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

    public void WriteCollection(ICollection<NLSMap> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(NLSMap[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_NLS_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "nls", Format.MAX_NLS_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(NLSMap? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<NLSMap?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(NLSMap?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_NLS_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "nls?", Format.MAX_NLS_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region Amount
    public void Write(Financial.Amount value)
    {
      Write(value.CurrencyISO);
      Write(value.Value);
    }

    public void WriteCollection(ICollection<Financial.Amount> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Financial.Amount[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_AMOUNT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "amount", Format.MAX_AMOUNT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(Financial.Amount? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<Financial.Amount?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Financial.Amount?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_AMOUNT_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "amount?", Format.MAX_AMOUNT_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }
    #endregion

    #region Atom
    public void Write(Atom value) => Write(value.ID);

    public void WriteCollection(ICollection<Atom> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Atom[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "atom", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    public void Write(Atom? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }

    public void WriteCollection(ICollection<Atom?> value) => WriteCollection(value, (bix, elm) => bix.Write(elm));
    public void Write(Atom?[] value)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Length;
      if (len > Format.MAX_LONG_ARRAY_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "atom?", Format.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for (int i = 0; i < len; i++)
        this.Write(value[i]);
    }

    #endregion

    #region JSON (object)
    public void WriteJson(object value, string targetName)
    {
      var target = JsonWritingOptions.CompactRowsAsMap;
      if (targetName.IsNotNullOrWhiteSpace())
      {
        target = new JsonWritingOptions(target){ RowMapTargetName = targetName };
      }

      JsonWriter.Write(value, m_Stream, target, Format.ENCODING);
    }
    #endregion

    #region Collection
    /// <summary>
    /// Writes a collection of T using a functor.
    /// While its true that arrays are also collections a separate method is needed to have an ability to treat arrays
    /// differently using direct memory-buffer copies- something which can not be done with ICollection(T)
    /// </summary>
    public void WriteCollection<T>(ICollection<T> value, Action<BixWriter, T> write)
    {
      if (value == null)
      {
        Write(false);
        return;
      }
      Write(true);

      var len = value.Count;
      if (len > Format.MAX_COLLECTION_LEN)
        throw new BixException(StringConsts.BIX_WRITE_X_COLLECTION_MAX_SIZE_ERROR.Args(len, typeof(T).Name, Format.MAX_COLLECTION_LEN));

      Write(len);
      foreach (var elm in value) write(this, elm);
    }
    #endregion

  }
}
