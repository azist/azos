/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;

namespace Azos.Sky.Blob
{
  /// <summary>
  /// Declares the fundamental constraints for BLOB storage
  /// </summary>
  public static class Constraints
  {
    public const string CONFIG_HEADERS_SECTION = "h";


    public const int BLOCK_SIZE_MIN = 16 * 1024;
    public const int BLOCK_SIZE_MAX = 4 * 1024 * 1024;
    public const int BLOCK_SIZE_DEFAULT =  256 * 1024;

    public const int TAG_COUNT_MAX = 32;


    /// <summary> Maximum length of id </summary>
    public const int ID_LENGTH_MAX = 128;

    /// <summary>
    /// Keeps block size (in bytes) within MIN...MAX bounds. 0 = DEFAULT block size
    /// </summary>
    public static int GuardBlockSize(int sz) => sz <= 0 ? BLOCK_SIZE_DEFAULT : sz.KeepBetween(BLOCK_SIZE_MIN, BLOCK_SIZE_MAX);

    /// <summary>
    /// Checks EntityId for validity as blob id
    /// </summary>
    public static EntityId ValidBlobId(EntityId id)
    {
      id.HasRequiredValue(nameof(id));
      id.System.HasRequiredValue("id.space");
      id.Type.HasRequiredValue("id.volume");
      id.Address.NonBlankMax(ID_LENGTH_MAX, "id.address");
      return id;
    }

  }
}
