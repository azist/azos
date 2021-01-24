using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Azos.Serialization.JSON;

namespace Azos
{
  /// <summary>
  /// Provides an efficient representation of a system string value which contains from 1 up to 8 ASCII-only
  /// digit or letter characters packed and stored as ulong system primitive. Typically used for system IDs of assets
  /// such as log and instrumentation channels, codes and classifications (e.g. ISO codes).
  /// The use of atoms obviates the costly string allocations for various codes which pretty much remain
  /// the same for the application lifetime (e.g. ISO language, currency, country codes, various system component names etc..)
  /// <para>
  /// The framework treats the type efficiently in many areas
  /// such as binary, BSON and JSON serialization. Short ID strings represented as Atom en masse greatly
  /// relieve the GC pressure in network/data processing apps.
  /// </para>
  /// The ranges of acceptable characters are: '0..9|a..z' upper or lower case, and '-','_' which
  /// are the only allowed separators.
  /// <para>
  /// WARNING: Atom type is designed to represent a finite distinct number of constant values (typically less than a few thousand), having
  /// most applications dealing with less than 100 atom values. Do not encode arbitrary strings as atoms as these
  /// bloat the system Atom intern pool
  /// </para>
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type was purposely designed for bulk streaming/batch service applications which need to move
  /// large number of objects (100Ks / second) containing short strings which identify a limited set of system objects,
  /// for example log message channel IDs, app IDs, instrumentation data channels/ and the like.
  /// Since the value fits into CPU register and does not produce references with consequential garbage collection the
  /// overall performance may be improved sometimes 5x-10x.
  /// </para>
  /// <para>
  /// The trick is to NOT convert strings into ASCII via .Encode() (that is why it is a static method, not a .ctor)
  /// but instead rely on static readonly (constant) values for naming channels, applications and other system assets.
  /// Instead of emitting "app1" string value in every log message we can now emit and store just a ulong 8 byte primitive.
  /// The packing works right to left, so the ulong may be var-bit compressed (e.g. using ULEB, Bix, Slim etc.).
  /// </para>
  /// <para>
  /// The string value is constrained to ASCII-only digits, upper or lower case letters and the following separators:  '-','_'
  /// </para>
  /// </remarks>
  [Serializable]
  public struct Atom : IEquatable<Atom>, Data.Idgen.IDistributedStableHashProvider, IJsonWritable, IJsonReadable
  {

    /// <summary>
    /// Zero constant
    /// </summary>
    public static readonly Atom ZERO = new Atom();

    /// <summary>
    /// Returns true if the character is valid per Atom rule: [0..9|A..Z|a..z|_|-]
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidChar(char c) =>
            (c >= '0' && c <= '9') ||
            (c >= 'A' && c <= 'Z') ||
            (c >= 'a' && c <= 'z') ||
            (c == '_' || c=='-');

    /// <summary>
    /// Encodes a string value into an Atom. The value must contain ASCII only 1 to 8 characters
    /// conforming to [0..9|A..Z|a..z|_|-] pattern and may not have whitespace.
    /// Null is encoded as Atom(0).
    /// <para>
    /// WARNING: There has to be a good reason to call this method in places other than constant declarations.
    /// The whole point of using atoms is to rely on pre-encoded constant values. Please evaluate carefully what your code does
    /// as dynamic atom encoding en mass does not make any sense
    /// </para>
    /// <para>
    /// WARNING: Atom type is designed to represent a finite distinct number of constant values (typically less than a few thousand), having
    /// most applications dealing with less than 100 atom values. Do not encode arbitrary strings as atoms as these
    /// bloat the system Atom intern pool
    /// </para>
    /// </summary>
    public static Atom Encode(string value)
    {
      if (value==null) return new Atom(0ul);

      value.NonBlankMinMax(1, 8, nameof(value));

      var ax = 0ul;
      for(var i=0; i<value.Length; i++)
      {
        var c = value[i];

        if (!IsValidChar(c))
          throw new AzosException(StringConsts.ARGUMENT_ERROR + "Atom.Encode(![0..9|A..Z|a..z|_|-])");

        ax |= ((ulong)c << (i * 8));
      }
      return new Atom(ax);
    }

    /// <summary>
    /// Tries to encode a string value into Atom. The value must contain ASCII only 1 to 8 characters
    /// conforming to [0..9|A..Z|a..z|_|-] pattern and may not contain whitespace.
    /// Null is encoded as Atom(0).
    /// <para>
    /// WARNING: There has to be a good reason to call this method in places other than constant declarations.
    /// The whole point of using atoms is to rely on pre-encoded constant values. Please evaluate carefully what your code does
    /// as dynamic atom encoding en mass does not make any sense
    /// </para>
    /// <para>
    /// WARNING: Atom type is designed to represent a finite distinct number of constant values (typically less than a few thousand), having
    /// most applications dealing with less than 100 atom values. Do not encode arbitrary strings as atoms as these
    /// bloat the system Atom intern pool
    /// </para>
    /// </summary>
    public static bool TryEncode(string value, out Atom atom)
    {
      atom = new Atom(0ul);

      if (value == null) return true;

      if (value.IsNullOrWhiteSpace() || value.Length>8)
      {
        return false;
      }

      var ax = 0ul;
      for (var i = 0; i < value.Length; i++)
      {
        var c = value[i];

        if (!IsValidChar(c))
          return false;

        ax |= ((ulong)c << (i * 8));
      }

      atom = new Atom(ax);
      return true;
    }

    /// <summary>
    /// Tries to encode a string value or numeric ID represented as string into Atom.
    /// The string value must contain ASCII only 1 to 8 characters conforming to [0..9|A..Z|a..z|_|-] pattern
    /// and may not contain whitespace. The numeric ID should start with '#' prefix and may have optional  binary or hex prefixes, e.g. "#0x3234"
    /// Null is encoded as Atom(0).
    /// <para>
    /// WARNING: There has to be a good reason to call this method in places other than constant declarations.
    /// The whole point of using atoms is to rely on pre-encoded constant values. Please evaluate carefully what your code does
    /// as dynamic atom encoding en mass does not make any sense
    /// </para>
    /// <para>
    /// WARNING: Atom type is designed to represent a finite distinct number of constant values (typically less than a few thousand), having
    /// most applications dealing with less than 100 atom values. Do not encode arbitrary strings as atoms as these
    /// bloat the system Atom intern pool
    /// </para>
    /// </summary>
    public static bool TryEncodeValueOrId(string value, out Atom atom)
    {
      atom = new Atom(0ul);

      if (value == null) return true;

      value = value.Trim();

      if (value.StartsWith("#"))
      {
        value = value.Substring(1);
        var asul = Data.ObjectValueConversion.AsULong(value, ulong.MaxValue);
        if (asul < ulong.MaxValue)
        {
          atom =  new Atom(asul);
          return true;
        }
        return false;
      }

      return TryEncode(value, out atom);
    }

    /// <summary>
    /// Constructs an Atom from an ulong primitive
    /// </summary>
    public Atom(ulong id) => ID = id;

    /// <summary>
    /// Returns ulong ID value of the Atom. This value is the only value which Atom consists of (stores), hence Atoms fit in 64bit CPU registers 1:1
    /// </summary>
    public readonly ulong ID;

    /// <summary>
    /// True when ID = 0, not representing any string
    /// </summary>
    public bool IsZero => ID == 0;


    /// <summary>
    /// Returns true when the value is either zero or a string of valid Atom characters
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

    //intern pool for speed
    private static object s_Lock = new object();
    private static volatile Dictionary<ulong, string> s_Cache = new Dictionary<ulong, string>();

    /// <summary>
    /// Returns string of up to 8 ASCII characters which this ID was constructed from.
    /// The implementation performs thread-safe interning of strings keyed on Atom.ID
    /// </summary>
    public string Value
    {
      get
      {
        if (IsZero) return null;

        //lock-free lookup covers 99.99% of cases
        if (s_Cache.TryGetValue(ID, out var value)) return value;

        //the creation of new Atoms is slow
        lock(s_Lock)
        {
          if (s_Cache.TryGetValue(ID, out value)) return value;
          var dict = new Dictionary<ulong, string>(s_Cache);
          value = getValue();
          dict[ID] = value;
          System.Threading.Thread.MemoryBarrier();
          s_Cache = dict; //atomic swap
        }

        return value;
      }
    }

    public bool Equals(Atom other) => this.ID == other.ID;
    public override bool Equals(object obj)
    {
      if (obj is Atom another) return this.Equals(another);
      return false;
    }
    public ulong GetDistributedStableHash() => ID;
    public override int GetHashCode() => ID.GetHashCode();

    //important to keep this as Value because Atoms are used in many format strings (which uses toString())
    public override string ToString() => Value;


    public static bool operator == (Atom lhs, Atom rhs) =>  lhs.Equals(rhs);
    public static bool operator != (Atom lhs, Atom rhs) => !lhs.Equals(rhs);

    public static implicit operator Atom(ulong ul) => new Atom(ul);

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
          throw new AzosException(StringConsts.ARGUMENT_ERROR + "Atom.Decode(![0..9|A..Z|a..z|_|-])");

        data[i] = (char)c;
        ax >>= 8;
      }

      return new string(data);
    }

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
    => JsonWriter.EncodeString(wri, IsZero ? "#0" : Value, options);

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data==null) return (true, Atom.ZERO);

      if (data is ulong ul)
      {
        var atom = new Atom(ul);
        if (atom.IsValid) return (true, atom);
      }
      else
       if(data is string str && TryEncodeValueOrId(str, out var a)) return (true, a);

      return (false, null);
    }
  }
}
