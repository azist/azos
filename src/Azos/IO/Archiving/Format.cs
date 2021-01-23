/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IO.Archiving
{
  public static class Format
  {
    public const string VOLUME_HEADER = "#!/usr/bin/env bix\n";

    public const int VOLUME_PAD_LEN = 35;
    public const byte VOLUME_PAD_ASCII = 0x20;

    public const int PAGE_MIN_LEN = 1024;//1 Kb is the minimum page size
    public const int PAGE_DEFAULT_LEN = 96 * 1024;//96 Kb default (promoted to LOH)
    public const int PAGE_MAX_LEN = 128 * 1024 * 1024;//~128 Mb is the maximum page size
    public const int PAGE_MAX_BUFFER_LEN = 256 * 1024 * 1024;//~2568 Mb is the maximum page size

    public const byte PAGE_HEADER_1 = (byte)'P';
    public const byte PAGE_HEADER_2 = (byte)'G';


    public const int ENTRY_MIN_LEN = 2/*hdr*/ + 5 /*len*/ + 1/*payload*/;
    public const int ENTRY_MAX_LEN = 48 * 1000 * 1024;//~48 Mb is the maximum entry size

    public const byte ENTRY_HEADER_1 = (byte)'@';
    public const byte ENTRY_HEADER_2 = (byte)'>';

    public const byte ENTRY_HEADER_EOF_1 = 0x00;
    public const byte ENTRY_HEADER_EOF_2 = 0x00;

  }
}
