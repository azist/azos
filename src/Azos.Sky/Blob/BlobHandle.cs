/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;
using Azos.Data.Adlib;
using Azos.Data.Business;
using Azos.Serialization.JSON;

namespace Azos.Sky.Blob
{
  /// <summary>
  /// Represents a handle through which you work with blobs. It is a stream which you can seek/read/write.
  /// The instances needs to be deterministically disposed
  /// </summary>
  public abstract class BlobHandle : System.IO.Stream
  {
    protected BlobHandle(IBlobStoreLogic logic, int bufferSize, bool readOnly)
    {
      m_Store = logic.NonNull(nameof(logic));
      m_Buffer = new byte[bufferSize.KeepBetween(1024, 1024 * 1024)];
      m_ReadOnly = readOnly;
    }

    protected sealed override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      var store = System.Threading.Interlocked.Exchange(ref m_Store, null);
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

    private byte[] m_Buffer;

    private GDID m_Gdid;
    private EntityId m_Id;
    private IConfigSectionNode m_Headers;
    private Tag[] m_Tags;
    private bool m_ReadOnly;
    private EntityId m_CreatedBy;
    private DateTime m_CreatedUtc;


    private EntityId m_LastModifiedBy;
    private DateTime m_LastModifiedUtc;
    private long m_LastLength;


    /// <summary>
    /// Store which opened the handle
    /// </summary>
    public IBlobStore Store => m_Store;


    /// <summary>
    /// Local buffer size
    /// </summary>
    public int BufferSize => m_Buffer.Length;

    public EntityId CreatedBy => m_CreatedBy;
    public DateTime CreatedUtc => m_CreatedUtc;

    public EntityId LastModifiedBy => m_LastModifiedBy;
    public DateTime LastModifiedUtc => m_LastModifiedUtc;


    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => !m_ReadOnly;

    /// <summary> Last known length </summary>
    public override long Length => m_LastLength;

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }



    public override void Flush()
    {
      throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }

    public override long Seek(long offset, System.IO.SeekOrigin origin)
    {
      throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotImplementedException();
    }
  }

}
