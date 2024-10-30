/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;



using Azos.Serialization.JSON;

namespace Azos.IO
{
  /// <summary>
  /// Writes primitives and other supported types to Slim-format stream. Use factory method of SlimFormat instance to create a new instance of SlimWriter class
  /// </summary>
  public class SlimWriter : WritingStreamer
  {
    protected internal SlimWriter() : base(null) { }


    /// <summary>
    /// Returns SlimFormat that this writer implements
    /// </summary>
    public override StreamerFormat Format => SlimFormat.Instance;


    public override void Flush()
    {
      m_Stream.Flush();
    }


    public override void Write(bool value)
    {
      m_Stream.WriteByte( value ? (byte)0xff : (byte)0);
    }

    public override void Write(bool? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(byte? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        this.Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(byte[] buffer)
    {
      if (buffer==null)
      {
        this.Write(false);
        return;
      }
      this.Write(true);
      var len = buffer.Length;
      if (len>SlimFormat.MAX_BYTE_ARRAY_LEN)
        throw new AzosIOException(StringConsts.SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "bytes", SlimFormat.MAX_BYTE_ARRAY_LEN));

      this.Write(len);
      m_Stream.Write(buffer, 0, len);
    }


    public override void Write(int[] value)
    {
      if (value==null)
      {
        this.Write(false);
        return;
      }
      this.Write(true);
      var len = value.Length;
      if (len>SlimFormat.MAX_INT_ARRAY_LEN)
        throw new AzosIOException(StringConsts.SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "ints", SlimFormat.MAX_INT_ARRAY_LEN));

      this.Write(len);
      for(int i=0; i<len; i++)
        this.Write(value[i]); //WITH compression
    }


    public override void Write(long[] value)
    {
      if (value==null)
      {
        this.Write(false);
        return;
      }
      this.Write(true);
      var len = value.Length;
      if (len>SlimFormat.MAX_LONG_ARRAY_LEN)
        throw new AzosIOException(StringConsts.SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "longs", SlimFormat.MAX_LONG_ARRAY_LEN));

      this.Write(len);
      for(int i=0; i<len; i++)
        this.Write(value[i]); //WITH compression
    }


    public override void Write(double[] value)
    {
      if (value==null)
      {
        this.Write(false);
        return;
      }
      this.Write(true);
      var len = value.Length;
      if (len>SlimFormat.MAX_DOUBLE_ARRAY_LEN)
        throw new AzosIOException(StringConsts.SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "doubles", SlimFormat.MAX_DOUBLE_ARRAY_LEN));

      this.Write(len);
      for(int i=0; i<len; i++)
        this.Write(value[i]);
    }

    public override void Write(float[] value)
    {
      if (value==null)
      {
        this.Write(false);
        return;
      }
      this.Write(true);
      var len = value.Length;
      if (len>SlimFormat.MAX_FLOAT_ARRAY_LEN)
        throw new AzosIOException(StringConsts.SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "floats", SlimFormat.MAX_FLOAT_ARRAY_LEN));

      this.Write(len);
      for(int i=0; i<len; i++)
        this.Write(value[i]);
    }

    public override void Write(decimal[] value)
    {
      if (value==null)
      {
        this.Write(false);
        return;
      }
      this.Write(true);
      var len = value.Length;
      if (len>SlimFormat.MAX_DECIMAL_ARRAY_LEN)
        throw new AzosIOException(StringConsts.SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "decimals", SlimFormat.MAX_DECIMAL_ARRAY_LEN));

      this.Write(len);
      for(int i=0; i<len; i++)
        this.Write(value[i]);
    }


    public override void Write(char ch)
    {
      this.Write((short)ch);
    }

    public override void Write(char? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }



    public override void Write(char[] buffer)
    {
      if (buffer==null)
      {
        this.Write(false);
        return;
      }

      var buf = m_Encoding.GetBytes(buffer);
      this.Write(buf);
    }


    public override void Write(string[] array)
    {
      if (array==null)
      {
        this.Write(false);
        return;
      }
      this.Write(true);
      var len = array.Length;
      if (len>SlimFormat.MAX_STRING_ARRAY_CNT)
        throw new AzosIOException(StringConsts.SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR.Args(len, "strings", SlimFormat.MAX_STRING_ARRAY_CNT));

      this.Write(len);
      for(int i=0; i<len; i++)
        this.Write(array[i]);
    }

    public override void Write(decimal value)
    {
      var bits = decimal.GetBits(value);
      this.Write( bits[0] );
      this.Write( bits[1] );
      this.Write( bits[2] );

      byte sign = (bits[3] & 0x80000000) != 0 ? (byte)0x80 : (byte)0x00;
      byte scale = (byte) ((bits[3] >> 16) & 0x7F);

      this.Write( (byte)(sign | scale) );
    }

    public override void Write(decimal? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public unsafe override void Write(double value)
    {
      ulong core = *(ulong*)(&value);

      m_Buff32[0] = (byte)core;
      m_Buff32[1] = (byte)(core >> 8);
      m_Buff32[2] = (byte)(core >> 16);
      m_Buff32[3] = (byte)(core >> 24);
      m_Buff32[4] = (byte)(core >> 32);
      m_Buff32[5] = (byte)(core >> 40);
      m_Buff32[6] = (byte)(core >> 48);
      m_Buff32[7] = (byte)(core >> 56);

      m_Stream.Write(m_Buff32, 0, 8);
    }

    public override void Write(double? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public unsafe override void Write(float value)
    {
      uint core = *(uint*)(&value);
      m_Buff32[0] = (byte)core;
      m_Buff32[1] = (byte)(core >> 8);
      m_Buff32[2] = (byte)(core >> 16);
      m_Buff32[3] = (byte)(core >> 24);
      m_Stream.Write(m_Buff32, 0, 4);
    }

    public override void Write(float? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(int value)
    {
      byte b = 0;

      if (value<0)
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
      while(has)
      {
        b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public override void Write(int? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(long value)
    {
      byte b = 0;

      if (value<0)
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
      while(has)
      {
        b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public override void Write(long? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(sbyte value)
    {
      m_Stream.WriteByte((byte)value);
    }

    public override void Write(sbyte? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(short value)
    {
      byte b = 0;

      if (value<0)
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
      while(has)
      {
        b = (byte)(value & 0x7f);
        value = (short)(value >> 7);
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public override void Write(short? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(string value)
    {
      if (value==null)
      {
        this.Write(false);
        return;
      }

      this.Write(true);

      var len = value.Length;
      if (len>SlimFormat.MAX_STR_LEN)//This is much faster than Encoding.GetByteCount()
      {
        var buf = m_Encoding.GetBytes(value);
        this.Write(buf.Length);
        m_Stream.Write(buf, 0, buf.Length);
        return;
      }

      //try to reuse pre-allocated buffer
      if (SlimFormat.ts_StrBuff==null) SlimFormat.ts_StrBuff = new byte[SlimFormat.STR_BUF_SZ];
      var bcnt = m_Encoding.GetBytes(value, 0, len, SlimFormat.ts_StrBuff, 0);

      this.Write(bcnt);
      m_Stream.Write(SlimFormat.ts_StrBuff, 0, bcnt);
    }

    public override void Write(uint value)
    {
      var has = true;
      while(has)
      {
        byte b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public override void Write(uint? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(ulong value)
    {
      var has = true;
      while(has)
      {
        byte b = (byte)(value & 0x7f);
        value = value >> 7;
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public override void Write(ulong? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(ushort value)
    {
      var has = true;
      while(has)
      {
        byte b = (byte)(value & 0x7f);
        value = (ushort)(value >> 7);
        has = value != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }
    }

    public override void Write(ushort? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }



    public override void Write(MetaHandle value)
    {
      var meta = value.Metadata.HasValue;

      var handle = value.m_Handle;

      byte b = 0;

      if (meta) b = 1;

      b = (byte)(b | ((handle & 0x3f) << 1));
      handle = (handle >> 6);
      var has = handle != 0;
      if (has)
          b = (byte)(b | 0x80);
      m_Stream.WriteByte(b);
      while(has)
      {
        b = (byte)(handle & 0x7f);
        handle = (handle >> 7);
        has = handle != 0;
        if (has)
          b = (byte)(b | 0x80);
        m_Stream.WriteByte(b);
      }

      if (meta)
      {
        var vis = value.Metadata.Value;
        this.Write( vis.StringValue );

        if (vis.StringValue==null)
          this.Write( vis.IntValue );
      }
    }


    public override void Write(MetaHandle? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }



    public override void Write(DateTime value)
    {
      //Prior to 20150626 DKh
      //this.Write(value.ToBinary());

      m_Stream.WriteBEUInt64( (ulong)value.Ticks );
      m_Stream.WriteByte( (byte)value.Kind );
    }

    public override void Write(DateTime? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(DateTimeOffset value)
    {
      Write(value.DateTime);
      Write(value.Offset);
    }

    public override void Write(DateTimeOffset? value)
    {
      if (value.HasValue)
      {
        Write(true);
        Write(value.Value);
        return;
      }
      Write(false);
    }


    public override void Write(TimeSpan value)
    {
      this.Write(value.Ticks);
    }

    public override void Write(TimeSpan? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(Guid value)
    {
      this.Write(value.ToByteArray());
    }

    public override void Write(Guid? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(Data.GDID value)
    {
      this.Write(value.Era);
      this.Write(value.ID);
    }

    public override void Write(Data.GDID? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(Data.RGDID value)
    {
      this.Write(value.Route);
      this.Write(value.Gdid);
    }

    public override void Write(Data.RGDID? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(Azos.Glue.Protocol.TypeSpec spec)
    {
        this.Write( spec.m_Name );
        m_Stream.WriteBEUInt64( spec.m_Hash );
    }

    public override void Write(Azos.Glue.Protocol.MethodSpec spec)
    {
        this.Write( spec.m_MethodName );
        m_Stream.WriteBEUInt64( spec.m_ReturnType );
        this.Write( spec.m_Signature );
        m_Stream.WriteBEUInt64( spec.m_Hash );
    }


    public override void Write(FID value)
    {
      m_Stream.WriteBEUInt64( value.ID );
    }

    public override void Write(FID? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(Pile.PilePointer value)
    {
      this.Write(value.NodeID);
      this.Write(value.Segment);
      this.Write(value.Address);
    }

    public override void Write(Pile.PilePointer? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(VarIntStr value)
    {
      this.Write(value.StringValue);
      if (value.StringValue==null)
        this.Write(value.IntValue);
    }

    public override void Write(VarIntStr? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(NLSMap map)
    {
      if (map.m_Data==null)
      {
        this.Write((ushort)0);
        return;
      }

      this.Write((ushort)map.m_Data.Length);
      for(var i=0; i<map.m_Data.Length; i++)
      {
        var nd = map.m_Data[i];
        this.Write(nd.ISO);
        this.Write(nd.Name);
        this.Write(nd.Description);
      }
    }

    public override void Write(NLSMap? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(Financial.Amount value)
    {
      this.Write(value.ISO);
      this.Write(value.Value);
    }

    public override void Write(Financial.Amount? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

    public override void Write(Collections.StringMap map)
    {
      if (map==null)
      {
        this.Write(false);
        return;
      }

      this.Write(true);
      this.Write(map.CaseSensitive);
      this.Write((int)map.Count);

      foreach(var kvp in map)
      {
        this.Write(kvp.Key);
        this.Write(kvp.Value);
      }
    }

    public override void Write(Atom value)
    {
      this.Write(value.ID);
    }

    public override void Write(Atom? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }


    public override void Write(Data.EntityId value)
    {
      this.Write(value.System);
      this.Write(value.Type);
      this.Write(value.Schema);
      this.Write(value.Address);
    }

    public override void Write(Data.EntityId? value)
    {
      if (value.HasValue)
      {
        this.Write(true);
        Write(value.Value);
        return;
      }
      this.Write(false);
    }

  }
}
