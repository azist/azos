/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Defines Bix format constants
  /// </summary>
  public static class Format
  {
    public static readonly UTF8Encoding ENCODING = new UTF8Encoding(false, false);

    public static readonly Atom VERSION = Atom.Encode("0");

    public const int MAX_BYTE_ARRAY_LEN = 128 * //mb
                                          1024 * //kb
                                          1024;

    public const int MAX_SHORT_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 2;
    public const int MAX_INT_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 4;
    public const int MAX_LONG_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 8;
    public const int MAX_DOUBLE_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 8;
    public const int MAX_FLOAT_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 4;
    public const int MAX_DECIMAL_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 13;
    public const int MAX_STRING_ARRAY_CNT = MAX_BYTE_ARRAY_LEN / 48;
    public const int MAX_GUID_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 16;
    public const int MAX_GDID_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 12;
    public const int MAX_PPTR_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 12;
    public const int MAX_NLS_ARRAY_LEN = 1024;
    public const int MAX_AMOUNT_ARRAY_LEN = MAX_BYTE_ARRAY_LEN / 16;
    public const int MAX_COLLECTION_LEN = 64 * 1024;


    public const int STR_BUF_SZ = 96 * 1024;// ensure placement in LOH
                                            // in many business cases bix writes pretty big chunks of text, e.g.:
                                            // NLS pairs containing full page product markup (2-8 Kbytes of text per ISO Lang)
                                            // pre-serialized JSON fragments i.e. 6-8 kb

    public const int MAX_STR_LEN = (STR_BUF_SZ / 3) - 16; //3 bytes per UTF8 character - 16 BOM etc.
                                                          //this is done on purpose NOT to call
                                                          //Encoding.GetByteCount() to save time


    public const byte HDR1 = 0xB1;
    public const int HDR2 = 0b1010_0000;

    public const byte NULL = 0x00;

    public const byte TRUE = 0xff;
    public const byte FALSE = 0x00;

    [ThreadStatic] private static byte[] ts_Buff32;
    [ThreadStatic] private static byte[] ts_StrBuff;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte[] GetBuff32()
    {
      var buf = ts_Buff32;
      if (buf == null)
      {
        buf = new byte[32];
        ts_Buff32 = buf;
      }
      return buf;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static byte[] GetStrBuff()
    {
      var buf = ts_StrBuff;
      if (buf == null)
      {
        buf = new byte[STR_BUF_SZ];
        ts_StrBuff = buf;
      }
      return buf;
    }

  }
}
