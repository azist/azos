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
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Represents abstraction of a hashed password, the concrete password algorithm provide implementation (e.g. bytebuffer, dictionary, string, etc.)
  /// </summary>
  [Azos.Serialization.Slim.SlimSerializationProhibited]
  public sealed class HashedPassword : IJsonWritable, IEnumerable<KeyValuePair<string, object>>
  {
    #region CONSTS
    public const string KEY_ALG = "alg";
    public const string KEY_FAM = "fam";
    #endregion

    #region Static

    /// <summary>
    /// Creates instance from Json string
    /// </summary>
    public static HashedPassword FromString(string str)
    {
      if (str.IsNullOrWhiteSpace())
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "HashedPassword.FromString(str==null|empty)");

      var password = new HashedPassword();

      var json = str.JsonToDataObject() as JsonDataMap;

      if (json == null || json[KEY_ALG].AsString().IsNullOrWhiteSpace())
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "HashedPassword.FromString(!map|!algo)");

      password.m_Content = json;

      return password;
    }

    /// <summary>
    /// Compares two strings for equality in length-constant time, so even if the strings do not match at the first char,
    /// all chars are still scanned, in other words the execution time of this function does NOT depend on the number
    /// of matched characters.
    /// See: https://stackoverflow.com/questions/21100985/why-is-the-slowequals-function-important-to-compare-hashed-passwords
    /// </summary>
    /// <returns>True if both string are equal char-by-char</returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool AreStringsEqualInLengthConstantTime(string a, string b)
    {
      const int ITERATIONSET = 250;
      if (a == null) a = string.Empty;
      if (b == null) b = string.Empty;
      var result = a.Length == b.Length;

      //compare data in ITERATION SETS even if the data is much smaller
      var total = 0;
      while(total < a.Length) total += ITERATIONSET;
      while(total < b.Length) total += ITERATIONSET;

      double magic = 0d;
      for(var i=0; i<total; i++)
      {
        var ca = i < a.Length ? a[i] : (char)0;
        var cb = i < b.Length ? b[i] : (char)0;
        if (ca != cb) result = false;

        //the formula is introduced to ensure no optimization by JIT, despite the MethodImpl pragma
        //the usage of the formula ensures that the `magic` state can not be optimized out or
        //statically pre-computed by JIT, ensuring that loop body does not collapse
        magic += Math.Sin( Math.PI * ((ca - cb) / (double)char.MaxValue) ); //sin(0) = 0  when a=b
      }

      return result && (magic < total);
    }

    /// <summary>
    /// Compares two byte strings for equality in length-constant time, so even if the strings do not match at the first byte,
    /// all bytes are still scanned, in other words the execution time of this function does NOT depend on the number
    /// of matched bytes.
    /// See: https://stackoverflow.com/questions/21100985/why-is-the-slowequals-function-important-to-compare-hashed-passwords
    /// </summary>
    /// <returns>True if both byte[] are equal byte-by-byte</returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool AreStringsEqualInLengthConstantTime(byte[] a, byte[] b)
    {
      const int ITERATIONSET = 250;
      if (a == null) a = new byte[0];
      if (b == null) b = new byte[0];
      var result = a.Length == b.Length;

      //compare data in ITERATION SETS even if the data is much smaller
      var total = 0;
      while (total < a.Length) total += ITERATIONSET;
      while (total < b.Length) total += ITERATIONSET;

      double magic = 0d;
      for (var i = 0; i < total; i++)
      {
        var ca = i < a.Length ? a[i] : 0;
        var cb = i < b.Length ? b[i] : 0;
        if (ca != cb) result = false;

        //the formula is introduced to ensure no optimization by JIT, despite the MethodImpl pragma
        //the usage of the formula ensures that the `magic` state can not be optimized out or
        //statically pre-computed by JIT, ensuring that loop body does not collapse
        magic += Math.Sin(Math.PI * ((ca - cb) / (double)byte.MaxValue)); //sin(0) = 0  when a=b
      }

      return result && (magic < total);
    }


    #endregion

    #region .ctor

    private HashedPassword() { }

    public HashedPassword(string algoName, PasswordFamily family)
    {
      m_Content = new JsonDataMap(false);
      m_Content[KEY_ALG] = algoName.NonBlank(nameof(algoName));
      m_Content[KEY_FAM] = family;
    }

    #endregion

    #region Field
    private JsonDataMap m_Content;
    #endregion

    #region Properties

    /// <summary>
    /// Algorithm instance name provides the "version" of algorithm used along with all of its configured parameters,
    /// such as iteration count for PBKDF etc...
    /// </summary>
    public string AlgoName => m_Content[KEY_ALG].AsString();
    public PasswordFamily Family => m_Content[KEY_FAM].AsEnum(PasswordFamily.Unspecified);

    public object this[string key]
    {
      get { return m_Content[key]; }
      set
      {
        if (key.EqualsOrdIgnoreCase(KEY_ALG))
          throw new SecurityException(GetType().Name + ".this[algo].readonly");
        if (key.EqualsOrdIgnoreCase(KEY_FAM))
          throw new SecurityException(GetType().Name + ".this[fam].readonly");

        m_Content[key] = value;
      }
    }
    #endregion

    #region Public
    public void Add(string key, object value) => m_Content.Add(key, value);

    public override string ToString() => m_Content.ToJson(JsonWritingOptions.CompactASCII);

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
     => JsonWriter.WriteMap(wri, m_Content, nestingLevel, options);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => m_Content.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Content.GetEnumerator();
    #endregion
  }
}
