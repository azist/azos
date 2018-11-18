/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Serialization.Slim;

namespace Azos.Pile
{
  /// <summary>
  /// Provides default implementation of IPile which stores objects in the local machine RAM
  /// </summary>
  [SlimSerializationProhibited]
  public sealed class DefaultPile : DefaultPileBase
  {
    #region .ctor

      public DefaultPile(IApplication app, string name = null) : base(app, name)
      {
      }

      public DefaultPile(IApplicationComponent director, string name = null) : base(director, name)
      {
      }

    #endregion

    #region Properties

      /// <summary>
      /// Returns PilePersistence.Memory
      /// </summary>
      public override ObjectPersistence Persistence { get{ return ObjectPersistence.Memory; }}
    #endregion

    #region Protected

      /// <summary>
      /// Creates a segment that stores data in local memory array byte buffers
      /// </summary>
      internal override DefaultPileBase._segment MakeSegment(int segmentNumber)
      {
        var memory = new LocalMemory(SegmentSize);
        var result = new DefaultPileBase._segment(this, memory, true);
        return result;
      }
    #endregion

  }
}
