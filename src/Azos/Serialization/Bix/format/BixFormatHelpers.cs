/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.IO;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Creates a read scope around pre-allocated buffer.
  /// The scopes can be nested each preserving their own state.
  /// It must be deterministically disposed with a call to <see cref="Dispose"/> (or using).
  /// The scope is 100% synchronous
  /// </summary>
  public struct BixReaderBufferScope : IDisposable
  {
    [ThreadStatic] private static BufferSegmentReadingStream ts_Stream;

    private static BufferSegmentReadingStream getStream()
    {
      var result = ts_Stream;
      if (result == null)
      {
        result = new BufferSegmentReadingStream();
      }

      ts_Stream = null;
      return result;
    }

    public BixReaderBufferScope(byte[] buffer)
    {
      buffer.NonNull(nameof(buffer));
      var stream = getStream();
      stream.UnsafeBindBuffer(buffer, 0, buffer.Length);
      Reader = new BixReader(stream);
    }

    public BixReaderBufferScope(ArraySegment<byte> buffer)
    {
      buffer.Array.NonNull(nameof(buffer));
      var stream = getStream();
      stream.UnsafeBindBuffer(buffer);
      Reader = new BixReader(stream);
    }

    public readonly BixReader Reader;

    public void Reset() => ((BufferSegmentReadingStream)Reader.m_Stream).UnsafeReset();

    public void Dispose()
    {
      if (Reader.m_Stream == null) return;
      var was = (BufferSegmentReadingStream)Reader.m_Stream;
      was.UnbindBuffer();
      ts_Stream = was;
    }
  }

  /// <summary>
  /// Creates a write scope around dynamically-allocated buffer.
  /// The scopes can be nested each preserving their own state.
  /// It must be deterministically disposed with a call to <see cref="Dispose"/> (or using).
  /// The scope is 100% synchronous. Use <see cref="Buffer"/> to get a copy of what has been written
  /// </summary>
  public struct BixWriterBufferScope : IDisposable
  {
    public static BixWriterBufferScope DefaultCapacity => new BixWriterBufferScope(16 * 1024);

    public BixWriterBufferScope(int capacity)
    {
      m_Stream = new MemoryStream(capacity.KeepBetween(0, 8 * 1024 * 1024));
      m_Writer = new BixWriter(m_Stream);
    }

    private readonly MemoryStream m_Stream;
    private readonly BixWriter m_Writer;

    public BixWriter Writer
    {
      get
      {
        m_Writer.IsAssigned.IsTrue("Assigned");
        return m_Writer;
      }
    }

    /// <summary>
    /// Starts writing over
    /// </summary>
    public void Reset(bool keepWrittenContent = false)
    {
      m_Stream.NonNull(nameof(m_Stream));
      m_Stream.Position = 0;
      if (!keepWrittenContent)
      {
        m_Stream.SetLength(0);
      }
    }

    /// <summary>
    /// Returns a copy of written buffer
    /// </summary>
    public byte[] Buffer
      => m_Stream.NonNull(nameof(m_Stream)).ToArray();

    /// <summary>
    /// Returns a portion of memory buffer which has been written up to the point of the call
    /// </summary>
    public (byte[] buffer, int count) BufferSegment
      => (m_Stream.NonNull(nameof(m_Stream)).GetBuffer(), (int)m_Stream.Length);

    public void Dispose() => m_Stream?.Dispose();
  }

}
