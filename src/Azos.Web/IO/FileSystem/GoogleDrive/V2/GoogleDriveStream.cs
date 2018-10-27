/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

namespace Azos.IO.FileSystem.GoogleDrive.V2
{
  public class GoogleDriveStream : FileSystemStream
  {
    #region .ctor

      public GoogleDriveStream(FileSystemFile file, Action<FileSystemStream> disposeAction)
        : base(file, disposeAction)
      {
        m_Handle = (GoogleDriveHandle)file.Handle;
        m_Session = file.Session as GoogleDriveSession;
      }

    #endregion

    #region Private Fields / Properties

      private bool m_IsChanged;
      private MemoryStream m_Stream;
      private GoogleDriveHandle m_Handle;
      private GoogleDriveSession m_Session;

      private MemoryStream BufferStream
      {
          get
          {
              if (m_Stream == null)
              {
                  m_Stream = new MemoryStream();
                  m_Session.Client.GetFile(m_Handle.Id, m_Stream);
                  m_Stream.Position = 0;
                  m_IsChanged = false;
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
          DoFlush();
          m_Stream.Dispose();
        }

        base.Dispose(disposing);
      }

      protected override void DoFlush()
      {
        if (m_IsChanged)
        {
          m_Session.Client.UpdateFile(m_Handle.Id, BufferStream);
          m_IsChanged = false;
        }
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

      protected override long DoSeek(long offset, SeekOrigin origin)
      {
        return BufferStream.Seek(offset, origin);
      }

      protected override void DoSetLength(long value)
      {
        if (BufferStream.Length != value)
        {
          BufferStream.SetLength(value);
          m_IsChanged = true;
        }
      }

      protected override void DoWrite(byte[] buffer, int offset, int count)
      {
        BufferStream.Write(buffer, offset, count);
        m_IsChanged = true;
      }

    #endregion
  }
}
