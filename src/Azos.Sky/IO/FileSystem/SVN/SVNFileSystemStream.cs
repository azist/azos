/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

namespace Azos.IO.FileSystem.SVN
{
  public class SVNFileSystemStream : FileSystemStream
  {
    #region .ctor

      public SVNFileSystemStream(FileSystemFile file, Action<FileSystemStream> disposeAction) : base(file, disposeAction)
      {
        m_wdFile = ((SVNFileSystem.SVNFSH)file.Handle).Item as WebDAV.File;
        SVNFileSystem fs = file.FileSystem as SVNFileSystem;
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private readonly WebDAV.File m_wdFile;

      private MemoryStream m_Stream;

      private MemoryStream BufferStream
      {
        get
        {
          if (m_Stream == null)
          {
            m_Stream = new MemoryStream();
            m_wdFile.GetContent(m_Stream);
            m_Stream.Position = 0;
          }

          return m_Stream;
        }
      }

    #endregion

    #region Protected

      protected override void Dispose(bool disposing)
      {
        if (m_Stream != null)
        {
          m_Stream.Dispose();
        }

        base.Dispose(disposing);
      }

      protected override long DoGetLength()
      {
        return BufferStream.Length;
      }

      protected override long DoGetPosition()
      {
        return BufferStream.Position;
      }

      protected override void DoSetPosition(long position)
      {
        BufferStream.Position = position;
      }

      protected override int DoRead(byte[] buffer, int offset, int count)
      {
        return BufferStream.Read(buffer, offset, count);
      }

      protected override long DoSeek(long offset, System.IO.SeekOrigin origin)
      {
        return BufferStream.Seek(offset, origin);
      }

      protected override void DoFlush()
      {
        throw new NotImplementedException();
      }

      protected override void DoSetLength(long value)
      {
        throw new NotImplementedException();
      }

      protected override void DoWrite(byte[] buffer, int offset, int count)
      {
        throw new NotImplementedException();
      }

    #endregion

  } //SVNFileSystemStream

}
