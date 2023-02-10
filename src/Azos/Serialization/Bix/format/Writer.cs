/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

using Azos.Data;
using Azos.Financial;
using Azos.Pile;
using Azos.Serialization.JSON;

namespace Azos.Serialization.Bix
{
  public static class Writer
  {
    public static readonly Dictionary<Type, Action<BixWriter, object>> WRITERS = new Dictionary <Type, Action<BixWriter, object>>
    {
      {typeof(byte)                        ,                   (w, v) => Write(w, (byte)v)                    },
      {typeof(byte?)                       ,                   (w, v) => Write(w, (byte?)v)                   },
      {typeof(byte[])                      ,                   (w, v) => Write(w, (byte[])v)                  },
      {typeof(byte?[])                     ,                   (w, v) => Write(w, (byte?[])v)                 },
      {typeof(ICollection<byte>)           ,                   (w, v) => Write(w, (ICollection<byte>)v)       },
      {typeof(ICollection<byte?>)          ,                   (w, v) => Write(w, (ICollection<byte?>)v)      },

      {typeof(bool)                        ,                   (w, v) => Write(w, (bool)v)                    },
      {typeof(bool?)                       ,                   (w, v) => Write(w, (bool?)v)                   },
      {typeof(bool[])                      ,                   (w, v) => Write(w, (bool[])v)                  },
      {typeof(bool?[])                     ,                   (w, v) => Write(w, (bool?[])v)                 },
      {typeof(ICollection<bool>)           ,                   (w, v) => Write(w, (ICollection<bool>)v)       },
      {typeof(ICollection<bool?>)          ,                   (w, v) => Write(w, (ICollection<bool?>)v)      },

      {typeof(sbyte)                        ,                   (w, v) => Write(w, (sbyte)v)                    },
      {typeof(sbyte?)                       ,                   (w, v) => Write(w, (sbyte?)v)                   },
      {typeof(sbyte[])                      ,                   (w, v) => Write(w, (sbyte[])v)                  },
      {typeof(sbyte?[])                     ,                   (w, v) => Write(w, (sbyte?[])v)                 },
      {typeof(ICollection<sbyte>)           ,                   (w, v) => Write(w, (ICollection<sbyte>)v)       },
      {typeof(ICollection<sbyte?>)          ,                   (w, v) => Write(w, (ICollection<sbyte?>)v)      },

      {typeof(short)                        ,                   (w, v) => Write(w, (short)v)                    },
      {typeof(short?)                       ,                   (w, v) => Write(w, (short?)v)                   },
      {typeof(short[])                      ,                   (w, v) => Write(w, (short[])v)                  },
      {typeof(short?[])                     ,                   (w, v) => Write(w, (short?[])v)                 },
      {typeof(ICollection<short>)           ,                   (w, v) => Write(w, (ICollection<short>)v)       },
      {typeof(ICollection<short?>)          ,                   (w, v) => Write(w, (ICollection<short?>)v)      },

      {typeof(ushort)                        ,                   (w, v) => Write(w, (ushort)v)                    },
      {typeof(ushort?)                       ,                   (w, v) => Write(w, (ushort?)v)                   },
      {typeof(ushort[])                      ,                   (w, v) => Write(w, (ushort[])v)                  },
      {typeof(ushort?[])                     ,                   (w, v) => Write(w, (ushort?[])v)                 },
      {typeof(ICollection<ushort>)           ,                   (w, v) => Write(w, (ICollection<ushort>)v)       },
      {typeof(ICollection<ushort?>)          ,                   (w, v) => Write(w, (ICollection<ushort?>)v)      },

      {typeof(int)                        ,                   (w, v) => Write(w, (int)v)                    },
      {typeof(int?)                       ,                   (w, v) => Write(w, (int?)v)                   },
      {typeof(int[])                      ,                   (w, v) => Write(w, (int[])v)                  },
      {typeof(int?[])                     ,                   (w, v) => Write(w, (int?[])v)                 },
      {typeof(ICollection<int>)           ,                   (w, v) => Write(w, (ICollection<int>)v)       },
      {typeof(ICollection<int?>)          ,                   (w, v) => Write(w, (ICollection<int?>)v)      },

      {typeof(uint)                        ,                   (w, v) => Write(w, (uint)v)                    },
      {typeof(uint?)                       ,                   (w, v) => Write(w, (uint?)v)                   },
      {typeof(uint[])                      ,                   (w, v) => Write(w, (uint[])v)                  },
      {typeof(uint?[])                     ,                   (w, v) => Write(w, (uint?[])v)                 },
      {typeof(ICollection<uint>)           ,                   (w, v) => Write(w, (ICollection<uint>)v)       },
      {typeof(ICollection<uint?>)          ,                   (w, v) => Write(w, (ICollection<uint?>)v)      },


      {typeof(long)                        ,                   (w, v) => Write(w, (long)v)                    },
      {typeof(long?)                       ,                   (w, v) => Write(w, (long?)v)                   },
      {typeof(long[])                      ,                   (w, v) => Write(w, (long[])v)                  },
      {typeof(long?[])                     ,                   (w, v) => Write(w, (long?[])v)                 },
      {typeof(ICollection<long>)           ,                   (w, v) => Write(w, (ICollection<long>)v)       },
      {typeof(ICollection<long?>)          ,                   (w, v) => Write(w, (ICollection<long?>)v)      },

      {typeof(ulong)                        ,                   (w, v) => Write(w, (ulong)v)                    },
      {typeof(ulong?)                       ,                   (w, v) => Write(w, (ulong?)v)                   },
      {typeof(ulong[])                      ,                   (w, v) => Write(w, (ulong[])v)                  },
      {typeof(ulong?[])                     ,                   (w, v) => Write(w, (ulong?[])v)                 },
      {typeof(ICollection<ulong>)           ,                   (w, v) => Write(w, (ICollection<ulong>)v)       },
      {typeof(ICollection<ulong?>)          ,                   (w, v) => Write(w, (ICollection<ulong?>)v)      },

      {typeof(double)                        ,                   (w, v) => Write(w, (double)v)                    },
      {typeof(double?)                       ,                   (w, v) => Write(w, (double?)v)                   },
      {typeof(double[])                      ,                   (w, v) => Write(w, (double[])v)                  },
      {typeof(double?[])                     ,                   (w, v) => Write(w, (double?[])v)                 },
      {typeof(ICollection<double>)           ,                   (w, v) => Write(w, (ICollection<double>)v)       },
      {typeof(ICollection<double?>)          ,                   (w, v) => Write(w, (ICollection<double?>)v)      },

      {typeof(float)                        ,                   (w, v) => Write(w, (float)v)                    },
      {typeof(float?)                       ,                   (w, v) => Write(w, (float?)v)                   },
      {typeof(float[])                      ,                   (w, v) => Write(w, (float[])v)                  },
      {typeof(float?[])                     ,                   (w, v) => Write(w, (float?[])v)                 },
      {typeof(ICollection<float>)           ,                   (w, v) => Write(w, (ICollection<float>)v)       },
      {typeof(ICollection<float?>)          ,                   (w, v) => Write(w, (ICollection<float?>)v)      },

      {typeof(decimal)                        ,                   (w, v) => Write(w, (decimal)v)                    },
      {typeof(decimal?)                       ,                   (w, v) => Write(w, (decimal?)v)                   },
      {typeof(decimal[])                      ,                   (w, v) => Write(w, (decimal[])v)                  },
      {typeof(decimal?[])                     ,                   (w, v) => Write(w, (decimal?[])v)                 },
      {typeof(ICollection<decimal>)           ,                   (w, v) => Write(w, (ICollection<decimal>)v)       },
      {typeof(ICollection<decimal?>)          ,                   (w, v) => Write(w, (ICollection<decimal?>)v)      },

      {typeof(char)                        ,                   (w, v) => Write(w, (char)v)                    },
      {typeof(char?)                       ,                   (w, v) => Write(w, (char?)v)                   },
      {typeof(char[])                      ,                   (w, v) => Write(w, (char[])v)                  },
      {typeof(char?[])                     ,                   (w, v) => Write(w, (char?[])v)                 },
      {typeof(ICollection<char>)           ,                   (w, v) => Write(w, (ICollection<char>)v)       },
      {typeof(ICollection<char?>)          ,                   (w, v) => Write(w, (ICollection<char?>)v)      },

      {typeof(string)                        ,                   (w, v) => Write(w, (string)v)                    },
      {typeof(string[])                      ,                   (w, v) => Write(w, (string[])v)                  },
      {typeof(ICollection<string>)           ,                   (w, v) => Write(w, (ICollection<string>)v)       },

      {typeof(DateTime)                        ,                   (w, v) => Write(w, (DateTime)v)                    },
      {typeof(DateTime?)                       ,                   (w, v) => Write(w, (DateTime?)v)                   },
      {typeof(DateTime[])                      ,                   (w, v) => Write(w, (DateTime[])v)                  },
      {typeof(DateTime?[])                     ,                   (w, v) => Write(w, (DateTime?[])v)                 },
      {typeof(ICollection<DateTime>)           ,                   (w, v) => Write(w, (ICollection<DateTime>)v)       },
      {typeof(ICollection<DateTime?>)          ,                   (w, v) => Write(w, (ICollection<DateTime?>)v)      },

      {typeof(TimeSpan)                        ,                   (w, v) => Write(w, (TimeSpan)v)                    },
      {typeof(TimeSpan?)                       ,                   (w, v) => Write(w, (TimeSpan?)v)                   },
      {typeof(TimeSpan[])                      ,                   (w, v) => Write(w, (TimeSpan[])v)                  },
      {typeof(TimeSpan?[])                     ,                   (w, v) => Write(w, (TimeSpan?[])v)                 },
      {typeof(ICollection<TimeSpan>)           ,                   (w, v) => Write(w, (ICollection<TimeSpan>)v)       },
      {typeof(ICollection<TimeSpan?>)          ,                   (w, v) => Write(w, (ICollection<TimeSpan?>)v)      },

      {typeof(Guid)                        ,                   (w, v) => Write(w, (Guid)v)                    },
      {typeof(Guid?)                       ,                   (w, v) => Write(w, (Guid?)v)                   },
      {typeof(Guid[])                      ,                   (w, v) => Write(w, (Guid[])v)                  },
      {typeof(Guid?[])                     ,                   (w, v) => Write(w, (Guid?[])v)                 },
      {typeof(ICollection<Guid>)           ,                   (w, v) => Write(w, (ICollection<Guid>)v)       },
      {typeof(ICollection<Guid?>)          ,                   (w, v) => Write(w, (ICollection<Guid?>)v)      },

      {typeof(GDID)                        ,                   (w, v) => Write(w, (GDID)v)                    },
      {typeof(GDID?)                       ,                   (w, v) => Write(w, (GDID?)v)                   },
      {typeof(GDID[])                      ,                   (w, v) => Write(w, (GDID[])v)                  },
      {typeof(GDID?[])                     ,                   (w, v) => Write(w, (GDID?[])v)                 },
      {typeof(ICollection<GDID>)           ,                   (w, v) => Write(w, (ICollection<GDID>)v)       },
      {typeof(ICollection<GDID?>)          ,                   (w, v) => Write(w, (ICollection<GDID?>)v)      },

      {typeof(RGDID)                        ,                   (w, v) => Write(w, (RGDID)v)                    },
      {typeof(RGDID?)                       ,                   (w, v) => Write(w, (RGDID?)v)                   },
      {typeof(RGDID[])                      ,                   (w, v) => Write(w, (RGDID[])v)                  },
      {typeof(RGDID?[])                     ,                   (w, v) => Write(w, (RGDID?[])v)                 },
      {typeof(ICollection<RGDID>)           ,                   (w, v) => Write(w, (ICollection<RGDID>)v)       },
      {typeof(ICollection<RGDID?>)          ,                   (w, v) => Write(w, (ICollection<RGDID?>)v)      },

      {typeof(FID)                        ,                   (w, v) => Write(w, (FID)v)                    },
      {typeof(FID?)                       ,                   (w, v) => Write(w, (FID?)v)                   },
      {typeof(FID[])                      ,                   (w, v) => Write(w, (FID[])v)                  },
      {typeof(FID?[])                     ,                   (w, v) => Write(w, (FID?[])v)                 },
      {typeof(ICollection<FID>)           ,                   (w, v) => Write(w, (ICollection<FID>)v)       },
      {typeof(ICollection<FID?>)          ,                   (w, v) => Write(w, (ICollection<FID?>)v)      },

      {typeof(PilePointer)                        ,                   (w, v) => Write(w, (PilePointer)v)                    },
      {typeof(PilePointer?)                       ,                   (w, v) => Write(w, (PilePointer?)v)                   },
      {typeof(PilePointer[])                      ,                   (w, v) => Write(w, (PilePointer[])v)                  },
      {typeof(PilePointer?[])                     ,                   (w, v) => Write(w, (PilePointer?[])v)                 },
      {typeof(ICollection<PilePointer>)           ,                   (w, v) => Write(w, (ICollection<PilePointer>)v)       },
      {typeof(ICollection<PilePointer?>)          ,                   (w, v) => Write(w, (ICollection<PilePointer?>)v)      },

      {typeof(NLSMap)                        ,                   (w, v) => Write(w, (NLSMap)v)                    },
      {typeof(NLSMap?)                       ,                   (w, v) => Write(w, (NLSMap?)v)                   },
      {typeof(NLSMap[])                      ,                   (w, v) => Write(w, (NLSMap[])v)                  },
      {typeof(NLSMap?[])                     ,                   (w, v) => Write(w, (NLSMap?[])v)                 },
      {typeof(ICollection<NLSMap>)           ,                   (w, v) => Write(w, (ICollection<NLSMap>)v)       },
      {typeof(ICollection<NLSMap?>)          ,                   (w, v) => Write(w, (ICollection<NLSMap?>)v)      },

      {typeof(Amount)                        ,                   (w, v) => Write(w, (Amount)v)                    },
      {typeof(Amount?)                       ,                   (w, v) => Write(w, (Amount?)v)                   },
      {typeof(Amount[])                      ,                   (w, v) => Write(w, (Amount[])v)                  },
      {typeof(Amount?[])                     ,                   (w, v) => Write(w, (Amount?[])v)                 },
      {typeof(ICollection<Amount>)           ,                   (w, v) => Write(w, (ICollection<Amount>)v)       },
      {typeof(ICollection<Amount?>)          ,                   (w, v) => Write(w, (ICollection<Amount?>)v)      },

      {typeof(Atom)                        ,                   (w, v) => Write(w, (Atom)v)                    },
      {typeof(Atom?)                       ,                   (w, v) => Write(w, (Atom?)v)                   },
      {typeof(Atom[])                      ,                   (w, v) => Write(w, (Atom[])v)                  },
      {typeof(Atom?[])                     ,                   (w, v) => Write(w, (Atom?[])v)                 },
      {typeof(ICollection<Atom>)           ,                   (w, v) => Write(w, (ICollection<Atom>)v)       },
      {typeof(ICollection<Atom?>)          ,                   (w, v) => Write(w, (ICollection<Atom?>)v)      },

      {typeof(EntityId)                        ,                   (w, v) => Write(w, (EntityId)v)                    },
      {typeof(EntityId?)                       ,                   (w, v) => Write(w, (EntityId?)v)                   },
      {typeof(EntityId[])                      ,                   (w, v) => Write(w, (EntityId[])v)                  },
      {typeof(EntityId?[])                     ,                   (w, v) => Write(w, (EntityId?[])v)                 },
      {typeof(ICollection<EntityId>)           ,                   (w, v) => Write(w, (ICollection<EntityId>)v)       },
      {typeof(ICollection<EntityId?>)          ,                   (w, v) => Write(w, (ICollection<EntityId?>)v)      },
    };


    public static bool IsWriteSupported(Type t) => WRITERS.ContainsKey(t);

    public static Action<BixWriter, object> GetWriteFunctionForType(Type t) => WRITERS.TryGetValue(t, out var f) ? f : null;

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

    #region INT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, int value) { writer.Write(TypeCode.Int32); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, int? value) { writer.Write(TypeCode.Int32Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, int[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Int32); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, int?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Int32Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<int> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Int32); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<int?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Int32Null); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, int value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, int? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, int[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, int?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<int> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<int?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region UINT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, uint value) { writer.Write(TypeCode.Uint32); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, uint? value) { writer.Write(TypeCode.Uint32Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, uint[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Uint32); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, uint?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Uint32Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<uint> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Uint32); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<uint?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Uint32Null); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, uint value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, uint? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, uint[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, uint?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<uint> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<uint?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region LONG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, long value) { writer.Write(TypeCode.Int64); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, long? value) { writer.Write(TypeCode.Int64Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, long[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Int64); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, long?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Uint64Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<long> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Int64); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<long?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Int64Null); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, long value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, long? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, long[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, long?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<long> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<long?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region ULONG
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ulong value) { writer.Write(TypeCode.Uint64); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ulong? value) { writer.Write(TypeCode.Uint64Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ulong[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Uint64); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ulong?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Uint64Null); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<ulong> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Uint64); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<ulong?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Uint64Null); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ulong value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ulong? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ulong[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ulong?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<ulong> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<ulong?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region DOUBLE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, double value) { writer.Write(TypeCode.Double); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, double? value) { writer.Write(TypeCode.DoubleNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, double[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Double); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, double?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.DoubleNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<double> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Double); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<double?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.DoubleNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, double value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, double? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, double[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, double?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<double> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<double?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region FLOAT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, float value) { writer.Write(TypeCode.Float); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, float? value) { writer.Write(TypeCode.FloatNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, float[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Float); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, float?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.FloatNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<float> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Float); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<float?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.FloatNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, float value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, float? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, float[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, float?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<float> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<float?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region DECIMAL
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, decimal value) { writer.Write(TypeCode.Decimal); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, decimal? value) { writer.Write(TypeCode.DecimalNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, decimal[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Decimal); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, decimal?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.DecimalNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<decimal> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Decimal); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<decimal?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.DecimalNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, decimal value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, decimal? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, decimal[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, decimal?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<decimal> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<decimal?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region CHAR
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, char value) { writer.Write(TypeCode.Char); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, char? value) { writer.Write(TypeCode.CharNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, char[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Char); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, char?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.CharNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<char> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Char); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<char?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.CharNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, char value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, char? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, char[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, char?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<char> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<char?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region STRING
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, string value) { writer.Write(TypeCode.String); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, string[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.String); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<string> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.String); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, string value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, string[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<string> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region DATETIME
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, DateTime value) { writer.Write(TypeCode.DateTime); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, DateTime? value) { writer.Write(TypeCode.DateTimeNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, DateTime[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.DateTime); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, DateTime?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.DateTimeNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<DateTime> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.DateTime); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<DateTime?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.DateTimeNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, DateTime value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, DateTime? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, DateTime[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, DateTime?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<DateTime> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<DateTime?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region TIMESPAN
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, TimeSpan value) { writer.Write(TypeCode.TimeSpan); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, TimeSpan? value) { writer.Write(TypeCode.TimeSpanNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, TimeSpan[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.TimeSpan); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, TimeSpan?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.TimeSpanNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<TimeSpan> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.TimeSpan); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<TimeSpan?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.TimeSpanNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, TimeSpan value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, TimeSpan? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, TimeSpan[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, TimeSpan?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<TimeSpan> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<TimeSpan?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region GUID
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Guid value) { writer.Write(TypeCode.Guid); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Guid? value) { writer.Write(TypeCode.GuidNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Guid[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Guid); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Guid?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.GuidNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<Guid> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Guid); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<Guid?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.GuidNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Guid value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Guid? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Guid[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Guid?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<Guid> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<Guid?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region GDID
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, GDID value) { writer.Write(TypeCode.GDID); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, GDID? value) { writer.Write(TypeCode.GDIDNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, GDID[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.GDID); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, GDID?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.GDIDNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<GDID> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.GDID); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<GDID?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.GDIDNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, GDID value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, GDID? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, GDID[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, GDID?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<GDID> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<GDID?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region RGDID
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, RGDID value) { writer.Write(TypeCode.RGDID); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, RGDID? value) { writer.Write(TypeCode.RGDIDNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, RGDID[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.RGDID); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, RGDID?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.RGDIDNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<RGDID> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.RGDID); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<RGDID?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.RGDIDNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, RGDID value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, RGDID? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, RGDID[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, RGDID?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<RGDID> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<RGDID?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region FID
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, FID value) { writer.Write(TypeCode.FID); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, FID? value) { writer.Write(TypeCode.FIDNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, FID[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.FID); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, FID?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.FIDNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<FID> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.FID); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<FID?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.FIDNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, FID value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, FID? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, FID[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, FID?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<FID> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<FID?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region PILEPOINTER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, PilePointer value) { writer.Write(TypeCode.PilePointer); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, PilePointer? value) { writer.Write(TypeCode.PilePointerNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, PilePointer[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.PilePointer); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, PilePointer?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.PilePointerNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<PilePointer> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.PilePointer); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<PilePointer?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.PilePointerNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, PilePointer value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, PilePointer? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, PilePointer[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, PilePointer?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<PilePointer> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<PilePointer?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region NLSMAP
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, NLSMap value) { writer.Write(TypeCode.NLSMap); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, NLSMap? value) { writer.Write(TypeCode.NLSMapNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, NLSMap[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.NLSMap); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, NLSMap?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.NLSMapNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<NLSMap> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.NLSMap); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<NLSMap?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.NLSMapNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, NLSMap value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, NLSMap? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, NLSMap[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, NLSMap?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<NLSMap> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<NLSMap?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region AMOUNT
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Amount value) { writer.Write(TypeCode.Amount); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Amount? value) { writer.Write(TypeCode.AmountNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Amount[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Amount); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Amount?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.AmountNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<Amount> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Amount); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<Amount?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.AmountNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Amount value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Amount? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Amount[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Amount?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<Amount> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<Amount?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region ATOM
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Atom value) { writer.Write(TypeCode.Atom); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Atom? value) { writer.Write(TypeCode.AtomNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Atom[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.Atom); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, Atom?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.AtomNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<Atom> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.Atom); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<Atom?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.AtomNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Atom value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Atom? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Atom[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, Atom?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<Atom> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<Atom?> value) { writer.Write(name); Write(writer, value); }
    #endregion

    #region EntityId
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, EntityId value) { writer.Write(TypeCode.EntityId); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, EntityId? value) { writer.Write(TypeCode.EntityIdNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, EntityId[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.EntityId); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, EntityId?[] value) { writer.Write(TypeCode.Array); writer.Write(TypeCode.EntityIdNull); writer.Write(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<EntityId> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.EntityId); writer.WriteCollection(value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write(BixWriter writer, ICollection<EntityId?> value) { writer.Write(TypeCode.Collection); writer.Write(TypeCode.EntityIdNull); writer.WriteCollection(value); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, EntityId value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, EntityId? value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, EntityId[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, EntityId?[] value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<EntityId> value) { writer.Write(name); Write(writer, value); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteField(BixWriter writer, ulong name, ICollection<EntityId?> value) { writer.Write(name); Write(writer, value); }
    #endregion


    #region JSON / Object
    public static void WriteJson(BixWriter writer, object value, BixContext ctx) { writer.Write(TypeCode.JsonObject); writer.WriteJson(value, ctx.TargetName); }
    #endregion

    #region Any / Polymorphic

    public static void WriteAnyField(BixWriter writer, ulong name, object value, BixContext ctx) { writer.Write(name); WriteAny(writer, value, ctx); }

    /// <summary>
    /// This functions writes "any" value from object. It is orders of magnitude slower than statically-linked type-safe code
    /// as is only used as the last resort for general cases like object[]
    /// </summary>
    public static void WriteAny(BixWriter writer, object value, BixContext ctx)
    {
      if (value == null)
      {
        WriteNull(writer);
        return;
      }

      //1 - TypedDoc
      if (value is TypedDoc doc)
      {
        WriteDoc(writer, doc, ctx, isRoot: false);
        return;
      }

      //2 - Custom BixWritable
      if (value is IBixWritable wri)//must be AFTER doc
      {
        if (wri.WriteToBix(writer, ctx)) return;
      }

      var t = value.GetType(); //handle polymorphic

      //3 - Try to get direct type
      var fn = GetWriteFunctionForType(t);
      if (fn != null)
      {
        fn(writer, value);
        return;
      }


      //4 - Try to dispatch of ICollection<T>
      var sequence = value as ICollection;
      if (sequence != null)//quick reject filter
      {
        foreach(var ti in t.GetInterfaces().Where(ti => ti.GetGenericTypeDefinition() == typeof(ICollection<>)))
        {
          fn = GetWriteFunctionForType(ti);//handle ICollection<T>
          if (fn != null)
          {
            fn(writer, value);
            return;
          }
        }
      }

      //4 - try MAP and generic collection[T]
      if (value is IDictionary<string, object> map){ WriteMap(writer, map, ctx); return; }
      if (sequence != null) { WriteSequence(writer, sequence, ctx); return; }

      //5 - as a last resort default to Json
      WriteJson(writer, value, ctx);
    }


    public static void WriteMap(BixWriter writer, IDictionary<string, object> value, BixContext ctx)
    {
      writer.Write(TypeCode.Map);

      if (value == null)
      {
        writer.Write(false);
        return;
      }
      writer.Write(true);

      ctx.IncreaseNesting();//this throws on depth

      writer.Write(value is JsonDataMap jdm ? jdm.CaseSensitive : true);
      writer.Write((uint)value.Count);

      foreach (var kvp in value)
      {
        writer.Write(kvp.Key);
        WriteAny(writer, kvp.Value, ctx);
      }

      ctx.DecreaseNesting();
    }

    public static void WriteSequence(BixWriter writer, ICollection value, BixContext ctx)
    {
      writer.Write(TypeCode.Sequence);

      if (value == null)
      {
        writer.Write(false);
        return;
      }
      writer.Write(true);

      ctx.IncreaseNesting();//this throws on depth

      writer.Write((uint)value.Count);

      foreach (var one in value)
      {
        WriteAny(writer, one, ctx);
      }

      ctx.DecreaseNesting();
    }
    #endregion

    #region DOC

    public static void WriteDocField(BixWriter writer, ulong name, TypedDoc doc, BixContext ctx) { writer.Write(name); WriteDoc(writer, doc, ctx, false); }

    public static void WriteDoc(BixWriter writer, TypedDoc doc, BixContext ctx, bool isRoot)
    {
      if (doc == null)
      {
        writer.Write(TypeCode.Doc);
        writer.Write(false);
        return;
      }

      ctx.IncreaseNesting();//this throws on depth

      var (ttp, handled) = ctx.OnDocWrite(writer, doc);//does amorphous.beforeSave()
      if (handled) return;

      if (doc is IBixWritable wri)
      {
        if (wri.WriteToBix(writer, ctx)) return;//handled by custom writer
      }

      if (!Bixer.s_Index.TryGetValue(ttp, out var core))
        throw new BixException(StringConsts.BIX_TYPE_NOT_SUPPORTED_ERROR.Args(ttp));

      var emitTypeId = ctx.PolymorphicMembers || (isRoot && ctx.PolymorphicRoot);

      writer.Write( emitTypeId ? TypeCode.DocWithType : TypeCode.Doc);
      writer.Write(true);//not null

      if (emitTypeId)
      {
        var tid = core.Attribute.TypeFguid;
        writer.Write(tid.S1);
        writer.Write(tid.S2);
      }

      core.Serialize(writer, doc, ctx);

      ctx.DecreaseNesting();
    }


    public static void WriteDocSequenceField(BixWriter writer, ulong name, IEnumerable<TypedDoc> value, BixContext ctx) { writer.Write(name); WriteDocSequence(writer, value, ctx); }

    public static void WriteDocSequence(BixWriter writer, IEnumerable<TypedDoc> value, BixContext ctx)
    {
      writer.Write(TypeCode.Sequence);

      if (value == null)
      {
        writer.Write(false);
        return;
      }
      writer.Write(true);

      ctx.IncreaseNesting();//this throws on depth

      var cnt = value is ICollection col ? col.Count : value.Count();
      writer.Write((uint)cnt);

      foreach (var one in value)
      {
        WriteDoc(writer, one, ctx, false);
      }

      ctx.DecreaseNesting();
    }

    #endregion

    #region HEADER/FOOTER

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteHeader(BixWriter writer, BixContext ctx)
    {
      var flags = 0;
      if (ctx.PolymorphicRoot)   flags |= 0b0010;
      if (ctx.PolymorphicMembers) flags |= 0b0001;

      writer.Write(Format.HDR1);
      writer.Write((byte)(Format.HDR2 | flags));
      writer.Write(ctx.Version);
    }

    #endregion

  }
}
