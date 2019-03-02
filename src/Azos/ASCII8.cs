using System;

namespace Azos
{
  /// <summary>
  /// Provides an efficient representation of a string which contains from 1 up to 8 ASCII-only digit/letter characters
  /// packed and stored as ulong system primitive. Typically used for system IDs of assets
  /// such as log and instrumentation channels. <para>The framework treats the type efficiently in many areas
  /// such as binary, BSON and JSON serialization. Short ID strings represented as ASCII8 en masse greatly
  /// relieve the GC pressure. </para>The ranges of acceptable characters are: '0..9|a..z' upper or lower case, and '-','_','.' which
  /// are the only allowed separators
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type was purposely designed for bulk streaming/batch service applications which need to move
  /// large number of objects (100Ks / second) containing short strings, for example log messages,
  /// instrumentation data messages/ and the like. Since the value fits into CPU register and does not produce
  /// references with consequential garbage collection the performance may be improved sometimes 5x-10x.
  /// </para>
  /// <para>
  /// The trick is to NOT convert strings into ASCII via .Encode() (that is why it is a static method, not a .ctor)
  /// but instead rely on static readonly (constant) values for naming channels, applications and other system assets.
  /// Instead of emitting "app1" string value in every log message we can now emit and store just a ulong 8 byte primitive.
  /// The packing works right to left, so the ulong may be var-bit compressed (e.g. using ULEB, Slim etc.).
  /// </para>
  /// <para>
  /// The string value is constrained to ASCII-only digits, upper or lower case letters and the following separators:  '-','_','.'
  /// </para>
  /// </remarks>
  [Serializable]
  public struct ASCII8 : IEquatable<ASCII8>, Data.Access.IDistributedStableHashProvider
  {

    /// <summary>
    /// Zero constant
    /// </summary>
    public static readonly ASCII8 ZERO = new ASCII8();

    /// <summary>
    /// Returns true if the character is valid per rule: [0..9|A..Z|a..z|.|_|-]
    /// </summary>
    public static bool IsValidChar(char c) =>
            (c >= '0' && c <= '9') ||
            (c >= 'A' && c <= 'Z') ||
            (c >= 'a' && c <= 'z') ||
            (c == '_' || c=='-' || c=='.');

    /// <summary>
    /// Encodes the string value into ulong. The value must contain ASCII only 1 to 8 characters
    /// and may not be whitespace only. Null is encoded as ASCII8(0)
    /// </summary>
    public static ASCII8 Encode(string value)
    {
      if (value==null) return new ASCII8(0ul);

      value.NonBlankMinMax(1, 8, nameof(value));

      var ax = 0ul;
      for(var i=0; i<value.Length; i++)
      {
        var c = value[i];

        if (!IsValidChar(c))
          throw new AzosException(StringConsts.ARGUMENT_ERROR + "ID8.Encode(![0..9|A..Z|a..z|.|_|-])");

        ax |= ((ulong)c << (i * 8));
      }
      return new ASCII8(ax);
    }

    /// <summary>
    /// Constructs the ASCII8 from ulong primitive
    /// </summary>
    public ASCII8(ulong id)
    {
      ID = id;
      m_Value = null;
    }

    /// <summary>
    /// Returns ulong ID value
    /// </summary>
    public readonly ulong ID;

    /// <summary>
    /// True when ID = 0, not representing any string
    /// </summary>
    public bool IsZero => ID == 0;


    /// <summary>
    /// Returns true when the value is either zero or a string of only acceptable ASCII chars
    /// </summary>
    public bool IsValid
    {
      get
      {
        if (IsZero) return true;

        var ax = ID;

        for (var i = 0; i < 8; i++)
        {
          var c = ax & 0xff;
          if (c == 0) break;

          if (!IsValidChar((char)c)) return false;

          ax >>= 8;
        }

        return true;
      }
    }

    //cached accessor
    [NonSerialized] private string m_Value;

    /// <summary>
    /// Returns string of up to 8 ASCII characters which this ID was constructed from
    /// </summary>
    public string Value => m_Value != null ? m_Value : m_Value = getValue();

    public bool Equals(ASCII8 other) => this.ID == other.ID;
    public override bool Equals(object obj)
    {
      if (obj is ASCII8 another) return this.Equals(another);
      return false;
    }
    public ulong GetDistributedStableHash() => ID;
    public override int GetHashCode() => ID.GetHashCode();

    public override string ToString() => IsZero ? "ASC8(zero)" : "ASC8(0x{0:X8}, `{1}`)".Args(ID, Value);


    public static bool operator == (ASCII8 lhs, ASCII8 rhs) =>  lhs.Equals(rhs);
    public static bool operator != (ASCII8 lhs, ASCII8 rhs) => !lhs.Equals(rhs);

    public static implicit operator ASCII8(ulong ul) => new ASCII8(ul);

    private unsafe string getValue()
    {
      if (IsZero) return null;

      char* data = stackalloc char[9];

      var ax = ID;

      for(var i=0; i<8; i++)
      {
        var c = ax & 0xff;
        if (c==0) break;

        if (!IsValidChar((char)c))
          throw new AzosException(StringConsts.ARGUMENT_ERROR + "ID8.Decode(![0..9|A..Z|a..z|.|_|-])");

        data[i] = (char)c;
        ax >>= 8;
      }

      return new string(data);
    }
  }
}
