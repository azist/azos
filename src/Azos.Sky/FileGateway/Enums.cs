/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.FileGateway
{
  /// <summary>
  /// Specifies the file creation mode
  /// </summary>
  public enum CreateMode
  {
    /// <summary>
    /// If file exists it throws. The file may not exist at the time of the call
    /// </summary>
    Create,

    /// <summary>
    /// If file exists, it first truncates it to zero bytes, then post new content which replaces the existing file
    /// </summary>
    Replace,

    /// <summary>
    /// If file does not exist, it gets created, if it exists, it is kept as is and written-to at the specified chunk preserving other chunks
    /// </summary>
    Save,
  }

  /// <summary>
  /// File or Directory
  /// </summary>
  public enum ItemType { File = 1, Directory = 2 };
}
