/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.IO.ErrorHandling
{
  /// <summary>
  /// Implements Adler32 checksum algorithm based on Mark Adlers work
  /// </summary>
  /// <remarks>
  /// This class is used for efficient string hashing for sharding. Use extreme caution before making any changes
  /// as it may effect all shard mappings
  /// </remarks>
  public struct Adler32
  {
    #region CONSTS

    //https://github.com/madler/zlib/blob/master/adler32.c
    // #define BASE 65521 /* largest prime smaller than 65536 */
    // #define NMAX 5552  /* NMAX is the largest n such that 255n(n+1)/2 + (n+1)(BASE-1) <= 2^32-1 */

    const uint ADLER_BASE = 65521;
    const int NMAX = 5552;

    #endregion

    #region Static
    /// <summary>
    /// Computes Adler32 for encoded string
    /// </summary>
    public static uint ForEncodedString(string text, System.Text.Encoding encoding)
    {
      if (encoding==null) encoding = System.Text.Encoding.UTF8;
      var buff =  encoding.GetBytes( text );
      return Adler32.ForBytes( buff );
    }

    /// <summary>
    /// Computes Adler32 for binary string representation (in-memory)
    /// </summary>
    public static uint ForString(string text)
    {
      if (text.IsNullOrWhiteSpace()) return 0;

      var adler = new Adler32();
      adler.Add( text );
      return adler.m_Value;
    }

    /// <summary>
    /// Computes Adler32 for byte array
    /// </summary>
    public static uint ForBytes(byte[] buff)
    {
      var adler = new Adler32();
      adler.Add( buff );
      return adler.m_Value;
    }

    /// <summary>
    /// Computes Adler32 for byte array
    /// </summary>
    public static uint ForBytes(byte[] buff, int offset) => ForBytes(buff, offset, buff.Length - offset);

    /// <summary>
    /// Computes Adler32 for byte array
    /// </summary>
    public static uint ForBytes(byte[] buff, int offset, int count)
    {
      var adler = new Adler32();
      adler.Add(buff, offset, count);
      return adler.m_Value;
    }

    #endregion


    #region Private Fields
    private uint m_Value;
    private bool m_Started;
    #endregion

    #region Properties

    public uint Value { get { return m_Value;} }

    #endregion

    #region Public

    /// <summary>
    /// Addes byte[] to checksum
    /// </summary>
    public void Add(byte[] buff)
    {
       Add(buff, 0, buff.Length);
    }

    public void Add(byte[] buff, int offset, int count)
    {
      if (!m_Started)
      {
        m_Started = true;
        m_Value = 1;//structs ca not have dflt ctor, needed to init on first call not to make a class instance
      }

      uint lo = m_Value & 0xFFFF;
      uint hi = m_Value >> 16;

      while (count > 0)
      {
        // Modulo calc is deferred
        var n = count < NMAX ? count : NMAX;

        count -= n;
        while (--n >= 0)
        {
          lo += (uint)buff[offset];
          hi += lo;
          offset++;
        }
        lo %= ADLER_BASE;
        hi %= ADLER_BASE;
      }

      m_Value = (hi << 16) | lo;
    }



    /// <summary>
    /// Adds string to checksum
    /// </summary>
    public void Add(string buff)
    {
      Add(buff, 0, buff.Length);
    }

    public void Add(string buff, int offset, int count)
    {
      if (!m_Started)
      {
        m_Started = true;
        m_Value = 1;//structs ca not have dflt ctor, needed to init on first call not to make a class instance
      }

      uint lo = m_Value & 0xFFFF;
      uint hi = m_Value >> 16;

      while (count > 0)
      {
        // Modulo calc is deferred
        var n = count < NMAX ? count : NMAX;

        count -= n;
        while (--n >= 0)
        {
          lo += (uint)buff[offset];
          hi += lo;
          offset++;
        }
        lo %= ADLER_BASE;
        hi %= ADLER_BASE;
      }

      m_Value = (hi << 16) | lo;
    }

    #endregion


  }

}
