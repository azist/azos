
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


    public override string ToString()
    {
      return SourceName;
    }
  }

}