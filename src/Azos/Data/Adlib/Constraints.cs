using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Data.Adlib
{
  /// <summary>
  /// Defines limits/constraints for data server
  /// </summary>
  public static class Constraints
  {
    public const int MAX_HEADERS_LENGTH = 8 * 1024;
    public const int MAX_CONTENT_LENGTH = 4 * 1024 * 1024;
    public const int MAX_TAG_COUNT = 128;
    public const int MAX_SHARD_TOPIC_LEN = 128;

    public const int SPACE_NAME_MIN_LEN = 1;
    public const int SPACE_NAME_MAX_LEN = 32;

    public static readonly Atom SCH_GITEM = Atom.Encode("gi");
  }
}
