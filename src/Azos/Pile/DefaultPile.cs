using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


using Azos.IO;
using Azos.Conf;
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

      public DefaultPile(string name = null) : base(name)
      {
      }

      public DefaultPile(object director, string name = null) : base(director, name)
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
