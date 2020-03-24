using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using Azos.Data;

namespace Azos.Serialization.Bix
{
  public static class Writer
  {
    static Writer()
    {
      //loop through all Write* methods and geather signature types
    }

    public static readonly HashSet<Type> SUPPORTED_TYPES = new HashSet<Type>();


    public static bool IsWriteSupported(Type t) => SUPPORTED_TYPES.Contains(t);

    #region NULL
    public static void WriteNull(BixWriter writer) => writer.Write(TypeCode.Null);
    #endregion

    #region BYTE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, byte value) { writer.Write(TypeCode.Byte);  writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, byte? value) { writer.Write(TypeCode.ByteNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, byte[] value) { writer.Write(TypeCode.Buffer); writer.Write(value); }//WATCHOUT - BUFFER is only used for byte[]!!!
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, byte?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.ByteNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<byte> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Byte); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<byte?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.ByteNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, byte value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, byte? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, byte[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, byte?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<byte> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<byte?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region BOOL
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, bool value) { writer.Write(TypeCode.Bool); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, bool? value) { writer.Write(TypeCode.BoolNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, bool[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Bool); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, bool?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.BoolNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<bool> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Bool); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<bool?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.BoolNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, bool value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, bool? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, bool[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, bool?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<bool> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<bool?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region SBYTE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, sbyte value) { writer.Write(TypeCode.Sbyte); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, sbyte? value) { writer.Write(TypeCode.SbyteNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, sbyte[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Sbyte); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, sbyte?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.SbyteNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<sbyte> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Sbyte); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<sbyte?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.SbyteNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, sbyte value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, sbyte? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, sbyte[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, sbyte?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<sbyte> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<sbyte?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region SHORT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, short value) { writer.Write(TypeCode.Int16); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, short? value) { writer.Write(TypeCode.Int16Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, short[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Int16); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, short?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Int16Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<short> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Int16); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<short?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Int16Null); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, short value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, short? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, short[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, short?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<short> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<short?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region USHORT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ushort value) { writer.Write(TypeCode.Uint16); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ushort? value) { writer.Write(TypeCode.Uint16Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ushort[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Uint16); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ushort?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Int16Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<ushort> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Uint16); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<ushort?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Uint16Null); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ushort value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ushort? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ushort[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ushort?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<ushort> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<ushort?> value) { writer.Write(name); Write(writer, value); }
    #endregion


  }
}
