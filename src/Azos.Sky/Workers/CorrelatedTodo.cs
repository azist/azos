using System;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Extends Todo toallow for correlation of multiple instances on SysCorrelationKey.
  /// This is used for example to aggregate various asynchronous events together.
  /// Example: batch frequent email notifications into one
  /// </summary>
  [Serializable]
  public abstract class CorrelatedTodo : Todo
  {
    /// <summary>
    /// Denotes values returned by a call to Merge(host, todo)
    /// </summary>
    public enum MergeResult
    {
      /// <summary>
      /// The Merge operation did not merge anything, so the another correlated todo should be inserted as a new one leaving the original intact
      /// </summary>
      None = 0,

      /// <summary>
      /// The original should be left as-is and another instance should be dropped as if it never existed
      /// </summary>
      IgnoreAnother = 1,

      /// <summary>
      /// The original should be updated with the merged content from another. Another is discarded (as it is already merged into original)
      /// </summary>
      Merged = 2
    }


    protected CorrelatedTodo() { }

    /// <summary>
    /// Provides the correlation key which is used for merging the CorrelatedTodo instances.
    /// The key technically may be null
    /// </summary>
    public string SysCorrelationKey { get; set; }


    /// <summary>
    /// Executes merge operation returning what whould happen to the original and another todo.
    /// This method MUST execute be VERY FAST and only contain merge logic, do not make externall IO calls -
    /// all business data must already be contained in the original and another instance
    /// </summary>
    protected internal abstract MergeResult Merge(ITodoHost host, DateTime utcNow, CorrelatedTodo another);
  }

}
