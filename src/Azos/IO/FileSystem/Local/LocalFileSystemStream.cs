
using System;
using System.IO;

namespace Azos.IO.FileSystem.Local
{
  internal class LocalFileSystemStream : FileSystemStream
  {

    public LocalFileSystemStream(FileSystemFile file, Action<FileSystemStream> disposeAction) : base(file, disposeAction)
    {
      var fa = FileAccess.ReadWrite;

      var hndl = file.Handle as LocalFileSystem.FSH;
      if (hndl!=null)
      {
        if (((FileInfo)hndl.m_Info).IsReadOnly)
          fa = FileAccess.Read;
      }
      m_FS = new FileStream(file.Path, FileMode.OpenOrCreate, fa, FileShare.ReadWrite);
    }

    protected override void Dispose(bool disposing)
    {
      m_FS.Dispose();
      base.Dispose(disposing);
    }

    private FileStream m_FS;


    protected override void DoFlush()
    {
      m_FS.Flush();
    }

    protected override long DoGetLength()
    {
      return m_FS.Length;
    }

    protected override long DoGetPosition()
    {
      return m_FS.Position;
    }

    protected override void DoSetPosition(long position)
    {
      m_FS.Position = position;
    }

    protected override int DoRead(byte[] buffer, int offset, int count)
    {
      return m_FS.Read(buffer, offset, count);
    }

    protected override long DoSeek(long offset, SeekOrigin origin)
    {
      return m_FS.Seek(offset, origin);
    }

    protected override void DoSetLength(long value)
    {
      m_FS.SetLength(value);
    }

    protected override void DoWrite(byte[] buffer, int offset, int count)
    {
      m_FS.Write(buffer, offset, count);
    }
  }
}
