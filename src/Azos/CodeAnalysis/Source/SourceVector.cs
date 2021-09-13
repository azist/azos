/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Represents a pointer to the named source code  and character position
  /// </summary>
  public struct SourceVector
  {
    public readonly string SourceName;
    public readonly SourcePosition Position;

    public SourceVector(string srcName, SourcePosition position)
    {
      SourceName = srcName;
      Position = position;
    }

  }
}
