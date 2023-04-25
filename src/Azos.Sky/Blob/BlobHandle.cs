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

namespace Azos.Sky.Blob
{
  /// <summary>
  /// Represents a handle through which you work with blobs. It is a stream which you can seek/read/write.
  /// The instances needs to be deterministically disposed
  /// </summary>
  public sealed class BlobHandle : System.IO.Stream
  {


    private GDID m_Gdid;
    private EntityId m_Id;
    private IConfigSectionNode m_Headers;
    private Tag[] m_Tags;
    private bool m_ReadOnly;

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => !m_ReadOnly;
    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    protected override void Dispose(bool disposing)
    {
     // m_FS.Dispose();
      base.Dispose(disposing);
    }

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
