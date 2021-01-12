using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IO.Archiving
{
  public static class Format
  {
    public const int ENTRY_MIN_LEN = 2/*hdr*/ + 5 /*len*/ + 1/*payload*/; 
    public const int ENTRY_MAX_LEN = 64 * 1000 * 1024;//~64 Mb is the maximum entry size

    public const byte ENTRY_HEADER_1 = (byte)'O';
    public const byte ENTRY_HEADER_2 = (byte)'k';

    public const byte ENTRY_HEADER_EOF_1 = 0x00;
    public const byte ENTRY_HEADER_EOF_2 = 0x00;

  }
}
