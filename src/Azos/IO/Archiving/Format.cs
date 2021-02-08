/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.IO.Archiving
{
  public static class Format
  {
    public const string VOLUME_HEADER = "#!/usr/bin/env bix\n";

    public const int VOLUME_PAD_LEN = 35;
    public const byte VOLUME_PAD_ASCII = 0x20;

    /// <summary>
    /// Establishes a minimum page size - 1024 bytes
    /// </summary>
    public const int PAGE_MIN_LEN = 1024;

    /// <summary>
    /// Establishes default data page size - 96 KBytes
    /// </summary>
    public const int PAGE_DEFAULT_LEN = 96 * 1024;//96 Kb default (promoted to LOH)

    /// <summary>
    /// Limits page size - 130 MBytes
    /// </summary>
    public const int PAGE_MAX_LEN = 130 * 1024 * 1024;//~130 Mb is the maximum page size

    /// <summary>
    /// Absolute limit on internal page buffer - 270 MBytes
    /// </summary>
    public const int PAGE_MAX_BUFFER_LEN = 270 * 1024 * 1024;//~270 Mb is the maximum page size

    public const byte PAGE_HEADER_1 = (byte)'P';
    public const byte PAGE_HEADER_2 = (byte)'G';


    public const int ENTRY_MIN_LEN = 2/*hdr*/ + 5 /*len*/ + 1/*payload*/;

    /// <summary>
    /// An absolute limit on maximum raw archive entry size: 64 MBytes.
    /// The library does not support archive entries larger than 64 Mb in principle
    /// </summary>
    public const int ENTRY_MAX_LEN = 64 * 1024 * 1024;//~64 Mb is the maximum entry size

    public const byte ENTRY_HEADER_1 = (byte)'@';
    public const byte ENTRY_HEADER_2 = (byte)'>';

    public const byte ENTRY_HEADER_EOF_1 = 0x00;
    public const byte ENTRY_HEADER_EOF_2 = 0x00;
  }
}
