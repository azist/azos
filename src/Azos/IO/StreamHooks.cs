/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Azos.IO
{
  /// <summary>
  /// Allows to tap-into stream function via functional-style hooks.
  /// You create this stream around another stream, now all stream contract operations go via the functors instead (which are set)
  /// or delegated to Target stream if functors are not set
  /// </summary>
  public class StreamHooks : Stream
  {
    public StreamHooks(Stream target, bool ownTarget = false)
    {
      m_Target = target.NonNull(nameof(target));
      m_OwnTarget = ownTarget;
    }

    protected override void Dispose(bool disposing)
    {
      if (m_OwnTarget) DisposableObject.DisposeAndNull(ref m_Target);
    }

    private Stream m_Target;
    private bool m_OwnTarget;

    /// <summary>
    /// Target stream that this stream wraps
    /// </summary>
    public Stream Target => m_Target;

    /// <summary>
    /// If true, disposes target
    /// </summary>
    public bool OwnTarget => m_OwnTarget;


    public Func<StreamHooks, int>                    Handle_ReadByte  { get; set; }
    public Action<StreamHooks, byte>                 Handle_WriteByte { get; set; }
    public Action<StreamHooks>                       Handle_Flush     { get; set; }
    public Func<StreamHooks, byte[], int, int, int>  Handle_Read      { get; set; }
    public Func<StreamHooks, long, SeekOrigin, long> Handle_Seek      { get; set; }
    public Action<StreamHooks, long>                 Handle_SetLength { get; set; }
    public Action<StreamHooks, byte[], int, int>     Handle_Write     { get; set; }
    public Action<StreamHooks, Stream, int>          Handle_CopyTo    { get; set; }

    public Func<StreamHooks, CancellationToken, Task>                          Handle_FlushAsync  { get; set; }
    public Func<StreamHooks, byte[], int, int, CancellationToken, Task<int>>   Handle_ReadAsync   { get; set; }
    public Func<StreamHooks, byte[], int, int, CancellationToken, Task>        Handle_WriteAsync  { get; set; }
    public Func<StreamHooks, Stream, int, CancellationToken, Task>             Handle_CopyToAsync { get; set; }

    public Func<StreamHooks, bool> Handle_CanReadGet  { get; set; }
    public Func<StreamHooks, bool> Handle_CanWriteGet { get; set; }
    public Func<StreamHooks, bool> Handle_CanSeekGet  { get; set; }
    public Func<StreamHooks, long> Handle_LengthGet   { get; set; }

    public Func<StreamHooks, long>    Handle_PositionGet { get; set; }
    public Action<StreamHooks, long>  Handle_PositionSet { get; set; }


    public override int ReadByte()
    {
      var h = Handle_ReadByte;
      return h != null ? h(this) : Target.ReadByte();
    }

    public override void WriteByte(byte b)
    {
      var h = Handle_WriteByte;
      if (h != null) h(this, b); else Target.WriteByte(b);
    }

    public override void Flush()
    {
      var h = Handle_Flush;
      if (h != null) h(this); else Target.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      var h = Handle_Read;
      return h != null ? h(this, buffer, offset, count) : Target.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      var h = Handle_Seek;
      return h != null ? h(this, offset, origin) : Target.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
      var h = Handle_SetLength;
      if (h != null) h(this, value); else Target.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      var h = Handle_Write;
      if (h != null) h(this, buffer, offset, count); else Target.Write(buffer, offset, count);
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
      var h = Handle_CopyTo;
      if (h != null) h(this, destination, bufferSize); else Target.CopyTo(destination, bufferSize);
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
      var h = Handle_FlushAsync;
      return (h != null) ? h(this, cancellationToken) : Target.FlushAsync(cancellationToken);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      var h = Handle_ReadAsync;
      return (h != null) ? h(this, buffer, offset, count, cancellationToken) : Target.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
      var h = Handle_WriteAsync;
      return (h != null) ? h(this, buffer, offset, count, cancellationToken) : Target.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
      var h = Handle_CopyToAsync;
      return (h != null) ? h(this, destination, bufferSize, cancellationToken) : Target.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override bool CanRead
    {
      get
      {
        var h = Handle_CanReadGet;
        return h != null ? h(this) : Target.CanRead;
      }
    }

    public override bool CanWrite
    {
      get
      {
        var h = Handle_CanWriteGet;
        return h != null ? h(this) : Target.CanWrite;
      }
    }

    public override bool CanSeek
    {
      get
      {
        var h = Handle_CanSeekGet;
        return h != null ? h(this) : Target.CanSeek;
      }
    }

    public override long Length
    {
      get
      {
        var h = Handle_LengthGet;
        return h != null ? h(this) : Target.Length;
      }
    }

    public override long Position
    {
      get
      {
        var h = Handle_PositionGet;
        return h != null ? h(this) : Target.Position;
      }
      set
      {
        var h = Handle_PositionSet;
        if ( h != null) h(this, value); else  Target.Position = value;
      }
    }
  }

  /// <summary>
  /// Provides various use cases for <see cref="StreamHooks"/> class, such as "CaseOfRandomReadAsync" which is used for testing
  /// </summary>
  public static class StreamHookUse
  {
    private static Encoding UTF8_NOBOM = new UTF8Encoding(false);//NO BOM

    /// <summary>
    /// Used for testing, encodes string content a MemoryStream using the specified encoding, or UTF8 by default.
    /// The stream is set at position zero
    /// </summary>
    public static byte[] EncodeStringToBufferNoBom(string content, Encoding encoding = null)
    {
      content.NonNull(nameof(content));
      if (encoding == null) encoding = UTF8_NOBOM;
      using var ms = new MemoryStream();
      using (var tw = new StreamWriter(ms, encoding, 4096, leaveOpen: true)) tw.Write(content);
      return ms.ToArray();
    }

    /// <summary>
    /// Builds a wrapper around a UTF8 text stream which introduces a random delay and/or random chunk size into stream sync reading process
    /// </summary>
    public static StreamHooks CaseOfRandomAsyncStringReading(string content, int msDelayFrom, int msDelayTo, int chunkSizeFrom, int chunkSizeTo, Encoding encoding = null)
    {
      content.NonNull(nameof(content));

      if (encoding==null) encoding = UTF8_NOBOM;

      var ms = new MemoryStream();
      using(var tw = new StreamWriter(ms, encoding, 4096, leaveOpen: true)) tw.Write(content);
      return CaseOfRandomAsyncReading(ms, msDelayFrom, msDelayTo, chunkSizeFrom, chunkSizeTo, ownTarget: true);
    }

    /// <summary>
    /// Builds a wrapper around a content bytes stream which introduces a random delay and/or random chunk size into stream sync reading process
    /// </summary>
    public static StreamHooks CaseOfRandomAsyncBufferReading(byte[] content, int msDelayFrom, int msDelayTo, int chunkSizeFrom, int chunkSizeTo, Encoding encoding = null)
    {
      content.NonNull(nameof(content));

      var ms = new MemoryStream(content);
      return CaseOfRandomAsyncReading(ms, msDelayFrom, msDelayTo, chunkSizeFrom, chunkSizeTo, ownTarget: true);
    }

    /// <summary>
    /// Builds a wrapper around a target stream which introduces a random delay and/or random chunk size into stream sync reading process
    /// </summary>
    public static StreamHooks CaseOfRandomAsyncReading(Stream target, int msDelayFrom, int msDelayTo, int chunkSizeFrom, int chunkSizeTo, bool ownTarget = false)
    => new StreamHooks(target, ownTarget)
    {
      Handle_ReadAsync = (hooks, buffer, offset, count, cancel)
        => HandlerOfRandomReadAsync(hooks, buffer, offset, count, cancel, msDelayFrom, msDelayTo, chunkSizeFrom, chunkSizeTo)
    };

    /// <summary>
    /// Handles stream async requests by delegating work to target stream with controllable random delay and random buffer chunks.
    /// Consider using <see cref="CaseOfRandomAsyncReading(Stream, int, int, int, int, bool)"/> higher order function to introduce this behavior
    /// around your target stream
    /// </summary>
    public static async Task<int> HandlerOfRandomReadAsync(StreamHooks hooks,
                                                         byte[] buffer,
                                                         int offset,
                                                         int count,
                                                         CancellationToken cancel,
                                                         int msDelayFrom,
                                                         int msDelayTo,
                                                         int chunkSizeFrom,
                                                         int chunkSizeTo)
    {
      if (msDelayFrom > 0 && msDelayTo > 0)
      {
        var rndMsDelay = Ambient.Random.NextScaledRandomInteger(msDelayFrom, msDelayTo);
        if (rndMsDelay > 0)
        {
          await Task.Delay(rndMsDelay, cancel).ConfigureAwait(false);
        }
      }

      var chunkSize = (chunkSizeFrom > 0 || chunkSizeTo > 0)
                         ? Ambient.Random
                                  .NextScaledRandomInteger(chunkSizeFrom, chunkSizeTo)
                                  .KeepBetween(1, count)
                         : count;

      var gotNow = await hooks.Target.ReadAsync(buffer, offset, chunkSize, cancel).ConfigureAwait(false);
      return gotNow;
    }
  }

}
