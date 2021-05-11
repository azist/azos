/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Platform.Abstraction.Graphics;

namespace Azos.Graphics
{
  public sealed partial class Canvas : DisposableObject
  {
    /// <summary> All Canvas objects derived from this one </summary>
    public abstract class Asset : DisposableObject, IPALCanvasAsset
    {
      public abstract IPALCanvasAsset AssetHandle { get; }
    }

    /// <summary> All Canvas objects derived from this one </summary>
    public abstract class Asset<THandle> : Asset where THandle : class, IPALCanvasAsset
    {
      protected Asset(THandle handle)
      {
        m_Handle = handle;
      }

      protected override void Destructor()
      {
        base.Destructor();
        DisposeAndNull(ref m_Handle);
      }

      private THandle m_Handle;

      public THandle Handle { get { EnsureObjectNotDisposed();  return m_Handle;} }

      public override IPALCanvasAsset AssetHandle => Handle;
    }

  }
}
