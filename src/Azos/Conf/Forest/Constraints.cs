/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Defines constraints for config forest such as data min/max length etc.
  /// </summary>
  public static class Constraints
  {
    public const string ID_NS_CONFIG_FOREST_PREFIX = "az-cforest-";

    public static readonly Atom SCH_PATH = Atom.Encode("path");
    public static readonly Atom SCH_GNODE = Atom.Encode("gnode");
    public static readonly Atom SCH_GVER = Atom.Encode("gver");

    public const int PATH_SEGMENT_MAX_COUNT = 0xff;
    public const int SEGMENT_MIN_LEN = 1;
    public const int SEGMENT_MAX_LEN = 64;

    public const int CONFIG_MIN_LEN = 6; // {r:{}}
    public const int CONFIG_MAX_LEN = 512 * 1024;

  }
}
