using System;

namespace Azos.Pile
{
  /// <summary>
  /// Used only for advanced GC optimization in big memory applications and should not be used in business logic directly.
  /// Encodes a CLR string as a value type holding up to the specified number of Unicode characters in-place.
  /// This structure is used e.g. as dictionary keys, effectively hiding string keys from GC. The technique greatly
  /// relieves the memory pressure in applications which have to store 10s of millions of strings for a long time (migrating to Gen2+ gc)
  /// </summary>
  public unsafe struct ValueString48 : IEquatable<ValueString48>, IEquatable<string>
  {
    public const int SIZE = 48;

    public ValueString48(string v)
    {
      if (v==null) //null string = null buffer
      {
        fixed (char* c = m_Buffer)
        {
          *c = (char)0;
        }
        return;
      }

      var len = v.Length;

      if (len > SIZE)
        throw new AzosException(StringConsts.ARGUMENT_ERROR+"{0}.ctor(str.Length > {1})".Args(GetType().Name, SIZE));

      fixed (char* c = m_Buffer)
      {
        var p = c;
        for (var i = 0; i < len; i++, p++)
          *p = v[i];
      }
    }

    private fixed char m_Buffer[SIZE];

    /// <summary>
    /// Returns the length of the stored string up to the maximum pre-allocated size
    /// </summary>
    public int Length{ get{ fixed (char* c = m_Buffer) return getLen(c); }}

    /// <summary>
    /// Returns CLR string content. No caching is done and a new string instance is allocated on every access.
    /// Empty buffer equals null string
    /// </summary>
    public string StringValue
    {
      get
      {
        fixed (char* c = m_Buffer)
        {
          var len = getLen(c);
          return len==0 ? null : new string(c, 0, len);
        }
      }
    }

    private int getLen(char* ptr)
    {
      for (var len = 0; len < SIZE; len++, ptr++)
        if (*ptr == (char)0) return len;

      return SIZE;
    }


    public override string ToString() => StringValue;

    public override int GetHashCode()
    {
      fixed (char* c = m_Buffer)
      {
        var len = 0;
        var ptr = c;
        int hash = 0;
        for (; len < SIZE; len++, ptr++)
        {
          var chr = *ptr;
          if (chr == (char)0) break;
          hash ^= (chr << (len & 0b1111));
        }

        return hash ^ (len * 1217);
      }
    }

    public override bool Equals(object obj) => obj is ValueString48 vs ? this.Equals(vs) : false;

    public bool Equals(ValueString48 other)
    {
      fixed (char* b1 = m_Buffer)
      {
        var p1 = (long*)b1;
        var p2 = (long*)other.m_Buffer;
        for(var i=0; i < SIZE / sizeof(long); i++, p1++, p2++) if (*p1 != *p2) return false;
      }

      return true;
    }

    public bool Equals(string other)
    {
      fixed (char* b1 = m_Buffer)
      {
        if (other==null) return *b1 == (char)0;

        if (other.Length!=getLen(b1)) return false;

        var p = b1;
        for(var i=0; i<other.Length; i++, p++)
         if (other[i]!=*p) return false;

        return true;
      }
    }

    public static bool operator ==(ValueString48 a, ValueString48 b) => a.Equals(b);
    public static bool operator !=(ValueString48 a, ValueString48 b) => !a.Equals(b);

    public static bool operator ==(ValueString48 a, string b) => a.Equals(b);
    public static bool operator !=(ValueString48 a, string b) => !a.Equals(b);

    public static bool operator ==(string a, ValueString48 b) => b.Equals(a);
    public static bool operator !=(string a, ValueString48 b) => !b.Equals(a);
  }
}
