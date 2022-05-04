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
    public const int SPACE_NAME_MIN_LEN = 1;
    public const int SPACE_NAME_MAX_LEN = 32;

    public static readonly Atom SCH_GITEM = Atom.Encode("gi");
  }
}
