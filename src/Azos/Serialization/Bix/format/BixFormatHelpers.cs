/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using Azos.IO;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Creates a read scope around pre-allocated buffer.
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
        ts_Stream = result;
      }
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

    public void Dispose() => ((BufferSegmentReadingStream)Reader.m_Stream).UnbindBuffer();
  }

  /// <summary>
  /// Creates a write scope around dynamically-allocated buffer.
  /// It must be deterministically disposed with a call to <see cref="Dispose"/> (or using).
  /// The scope is 100% synchronous. Use <see cref="Buffer"/> to get a copy of what has been written
  /// </summary>
  public struct BixWriterBufferScope : IDisposable
  {
    public BixWriterBufferScope(int capacity = 0)
    {
      m_Stream = new MemoryStream(capacity.KeepBetween(0, 8 * 1024 * 1024));
      Writer = new BixWriter(m_Stream);
    }

    private readonly MemoryStream m_Stream;
    public readonly BixWriter Writer;

    public byte[] Buffer => m_Stream.ToArray();

    public void Dispose() => m_Stream.Dispose();
  }

}
