
using System.IO;

namespace Azos.IO
{
  /// <summary>
  /// Implements a stream wrapper that does not close/get disposed.
  /// This stream is needed for TextWriter defect that always closes stream in its destructor
  /// </summary>
  public sealed class NonClosingStreamWrap : Stream
  {
    /// <summary>
    /// Allocates a wrapper around some other stream so it can be used with TextWriter that always closes the underlying stream
    /// in cases when the underlying stream needs to remain open after TextWriter is done
    /// </summary>
    public NonClosingStreamWrap(Stream target)
    {
      Target = target;
    }

    protected override void Dispose(bool disposing)
    {

    }

    /// <summary>
    /// Target stream that this stream wraps
    /// </summary>
    public readonly Stream Target;


    public override void Close()
    {

    }

    public override bool CanRead
    {
      get { return Target.CanRead; }
    }

    public override bool CanSeek
    {
      get { return Target.CanSeek; }
    }

    public override bool CanWrite
    {
      get { return Target.CanWrite; }
    }

    public override void Flush()
    {
      Target.Flush();
    }

    public override long Length
    {
      get { return Target.Length; }
    }

    public override long Position
    {
      get
      {
        return Target.Position;
      }
      set
      {
        Target.Position = value;
      }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      return Target.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      return Target.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      Target.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      Target.Write(buffer, offset, count);
    }



  }
}
