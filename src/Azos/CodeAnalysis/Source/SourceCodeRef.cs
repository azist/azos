/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Represents a reference to the source code which may be named buffer or project source item (i.e. solution project item)
  /// </summary>
  public struct SourceCodeRef
  {
    /// <summary>
    /// Provides name for the source, this property is set to ProjectItem.Name when IProjectItem is supplied in .ctor
    /// </summary>
    public readonly string SourceName;

    /// <summary>
    /// References project source item, this property may be null
    /// </summary>
    public readonly IProjectItem ProjectItem;

    public SourceCodeRef(string srcName)
    {
      SourceName = srcName ?? CoreConsts.UNNAMED_MEMORY_BUFFER;
      ProjectItem = null;
    }

    public SourceCodeRef(IProjectItem srcItem)
    {
      ProjectItem = srcItem;
      SourceName = srcItem.Name ?? CoreConsts.UNNAMED_PROJECT_ITEM;
    }

    public override string ToString() => SourceName;

  }
}