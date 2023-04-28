/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;
using Azos.Data.Adlib;

namespace Azos.Sky.Blob
{
  /// <summary>
  /// Represents a handle through which you work with blobs. It is a stream with seek/read/write capabilities.
  /// The instances needs to be deterministically disposed.
  /// The implementation does not provide buffering so you need to optimize tiny/scattered reads in the calling code.
  /// </summary>
  /// <remarks>You can extend this class along with <see cref="IBlobStoreLogic"/> custom implementation</remarks>
  public class BlobHandle : Stream
  {
    protected BlobHandle(IBlobStoreLogic store,
                         BlobHandleDescriptor descriptor,
                         VolatileBlobInfo vinfo,
                         bool readOnly)
    {
      m_Store = store.NonNull(nameof(store));
      m_RGdid = descriptor.NonNull(nameof(descriptor)).RGdid.HasRequiredValue(nameof(BlobHandleDescriptor.RGdid));
      m_Id = Constraints.ValidBlobId(descriptor.Id);
      m_BlockSize = Constraints.GuardBlockSize(descriptor.BlockSize);
      m_Headers = descriptor?.Headers.Node ?? Configuration.NewEmptyRoot(Constraints.CONFIG_HEADERS_SECTION);
      m_Tags = descriptor.Tags ?? new Tag[0];
      m_CreatedBy = descriptor.CreatedBy;
      m_CreatedUtc = descriptor.CreatedUtc;
      m_EndUtc = descriptor.EndUtc;
      m_ReadOnly = readOnly;
      UpdateLatestStatus(vinfo);
    }

    protected sealed override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      var store = Interlocked.Exchange(ref m_Store, null);
      if (store != null) Destructor(store);
    }

    /// <summary>
    /// Override to dispose additional resources when handle gets closed.
    /// Called once for the specified store
    /// </summary>
    protected virtual void Destructor(IBlobStoreLogic store)
    {
    }



    private IBlobStoreLogic m_Store;

    private readonly int m_BlockSize;

    private RGDID m_RGdid;
    private bool m_NotFound;
    private EntityId m_Id;
    private IConfigSectionNode m_Headers;
    private Tag[] m_Tags;
    private bool m_ReadOnly;
    private EntityId m_CreatedBy;
    private DateTime m_CreatedUtc;
    private DateTime? m_EndUtc;


    private long m_Position;
    private EntityId m_LastModifiedBy;
    private DateTime m_LastModifiedUtc;
    private long m_LastLength;


    /// <summary> Store which opened the handle </summary>
    public IBlobStore Store => m_Store;

    /// <summary> Id of the blob </summary>
    public EntityId Id => m_Id;

    /// <summary> Rgdid (system identifier with shard routing) of the blob </summary>
    public RGDID RGdid => m_RGdid;

    /// <summary> Headers </summary>
    public IConfigSectionNode Headers => m_Headers;

    /// <summary> Tags </summary>
    public IEnumerable<Tag> Tags => m_Tags;

    /// <summary> Block size set at create. It can not change </summary>
    public int BlockSize => m_BlockSize;

    /// <summary>
    /// Returns true when the blob is not found (anymore) for example when it was deleted by someone else
    /// </summary>
    public bool NotFound => m_NotFound;


    public EntityId CreatedBy => m_CreatedBy;
    public DateTime CreatedUtc => m_CreatedUtc;

    public EntityId LastModifiedBy => m_LastModifiedBy;
    public DateTime LastModifiedUtc => m_LastModifiedUtc;


    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => !m_ReadOnly;

    /// <summary> Last known length </summary>
    public override long Length => m_LastLength;

    public override void SetLength(long value)
    {
      m_Position = value;
      m_LastLength = value;
    }

    public override long Position
    {
      get => m_Position;
      set => m_Position = value.AtMinimum(0);
    }

    public override void Flush() => FlushAsync(default).Await();
    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override long Seek(long offset, SeekOrigin origin)
    {
      switch (origin)
      {
        case SeekOrigin.Begin:   Position = offset; break;
        case SeekOrigin.Current: Position = Position + offset; break;
        case SeekOrigin.End:     Position = Length + offset; break;
      }
      return Position;
    }

    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count, CancellationToken.None).AwaitResult();
    public override void Write(byte[] buffer, int offset, int count) => WriteAsync(buffer, offset, count, CancellationToken.None).Await();

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      var store = m_Store.NonDisposed(nameof(BlobHandle));
      CheckBufferArgs(buffer, offset, count);
      var (info, data) = await store.ReadBlockAsync(m_RGdid,
                                                    Position,
                                                    count,
                                                    cancellationToken).ConfigureAwait(false);
      UpdateLatestStatus(info);

      if (data == null)
      {
        m_NotFound = true;
        return 0;
      }

      if (data.Length == 0) return 0;
      Buffer.BlockCopy(data, 0, buffer, offset, data.Length);

      m_Position += data.Length;
      return data.Length;
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      var store = m_Store.NonDisposed(nameof(BlobHandle));

      CanWrite.IsTrue("CanWrite");

      CheckBufferArgs(buffer, offset, count);

      var info = await store.WriteBlockAsync(m_RGdid,
                                             Position,
                                             new ArraySegment<byte>(buffer, offset, count),
                                             cancellationToken).ConfigureAwait(false);

      if (info != null)
      {
        UpdateLatestStatus(info);
        m_Position += count;
      }
      else
      {
        m_NotFound = true;
      }
    }

    protected static void CheckBufferArgs(byte[] buffer, int offset, int count)
    {
      buffer.NonNull(nameof(buffer));
      (offset >= 0 && offset < buffer.Length).IsTrue("off [0 .. b.len)");
      (count >= 0 && count <= buffer.Length - offset).IsTrue("count [0 .. b.len - off]");
    }

    /// <summary>
    /// Updates latest information form <see cref="VolatileBlobInfo"/> object returned from the store
    /// </summary>
    protected virtual void UpdateLatestStatus(VolatileBlobInfo info)
    {
      if (info == null) return;
      m_LastLength = info.TotalLength;
      m_LastModifiedBy = info.ModifiedBy;
      m_LastModifiedUtc = info.ModifiedUtc;
    }
  }
}
