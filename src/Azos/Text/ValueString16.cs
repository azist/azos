using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Azos.Text
{
  /// <summary>
  /// Encodes a CLR string as a value type holding up to the specified number of UTF8-encoded bytes in-place.
  /// This structure is used e.g. as dictionary keys, effectively hiding strings from GC. The technique greatly
  /// relieves the memory pressure in applications which have to store 10s of millions of strings for a long time
  /// </summary>
  //[StructLayout(LayoutKind.Sequential, Pack=1)]
  public unsafe struct ValueString16 : IEquatable<ValueString16>//, IEquatable<string>
  {
    // internal static readonly Encoding ENCODING = new UTF8Encoding(false);

    public ValueString16(string v)
    {
      v.NonBlankMax(16);//mojet byt null, ne boilshe 16 simvolov
      fixed (char* c = m_Buffer)
      {
        var p = c;
        for (var i = 0; i < v.Length; i++, p++)
          *p = v[i];
      }
    }

    private fixed char m_Buffer[16];

    public string StringValue
    {
      get
      {
        fixed (char* c = m_Buffer)
        {
          var i =0;
          while(i<16 && *(c+i)!=(char)0) i++;
          return new string(c, 0, i);
        }
      }
    }

    public override string ToString() => StringValue;

    public bool Equals(ValueString16 other)
    {
      fixed (char* b1 = m_Buffer)
      {
        var p1 = b1;
        var p2 = other.m_Buffer;
        for(var i=0; i<16; i++, p1++, p2++) if (*p1 != *p2) return false;
      }

      return true;
    }

    //private ulong m_Seg1;
    //private ulong m_Seg2;


    //public unsafe string Original
    //{
    //  get
    //  {
    //    fixed(ulong* p = &Seg1) return ENCODING.GetString((byte*)p, 16);
    //  }
    //}


  }
}
