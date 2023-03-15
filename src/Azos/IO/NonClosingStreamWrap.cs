/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
      return Target.FlushAsync(cancellationToken);
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

    public override int ReadByte() => Target.ReadByte();

    public override int Read(byte[] buffer, int offset, int count) => Target.Read(buffer, offset, count);

    public override long Seek(long offset, SeekOrigin origin) => Target.Seek(offset, origin);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return Target.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
      return Target.ReadAsync(buffer, cancellationToken);
    }

    public override int Read(Span<byte> buffer)
    {
      return Target.Read(buffer);
    }

    public override void SetLength(long value)
    {
      Target.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      Target.Write(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      return Target.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
      return Target.WriteAsync(buffer, cancellationToken);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
      Target.Write(buffer);
    }

    public override void WriteByte(byte value)
    {
      Target.WriteByte(value);
    }

  }
}
