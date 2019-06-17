/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Azos.IO
{
  /// <summary>
  /// Represents a base for stream readers and writers.
  /// Streamer object instances ARE NOT THREAD-safe
  /// </summary>
  public abstract class Streamer
  {
    public static readonly UTF8Encoding UTF8Encoding = new UTF8Encoding(false, false);

    protected Streamer(Encoding encoding = null)
    {
      m_Encoding = encoding ?? UTF8Encoding;

      m_Buff32 = SlimFormat.ts_Buff32;
      if (m_Buff32==null)
      {
        var buf = new byte[32];
        m_Buff32 = buf;
        SlimFormat.ts_Buff32 = buf;
      }

    }

    protected byte[] m_Buff32;

    protected Stream m_Stream;
    protected Encoding m_Encoding;



    /// <summary>
    /// Returns format that this streamer implements
    /// </summary>
    public abstract StreamerFormat Format
    {
      get;
    }


    /// <summary>
    /// Returns underlying stream if it is bound or null
    /// </summary>
    public Stream Stream
    {
      get { return m_Stream; }
    }

    /// <summary>
    /// Returns stream string encoding
    /// </summary>
    public Encoding Encoding
    {
      get { return m_Encoding; }
    }



    /// <summary>
    /// Sets the stream as the target for output/input.
    /// This call must be coupled with UnbindStream()
    /// </summary>
    public void BindStream(Stream stream)
    {
      if (stream==null)
        throw new AzosIOException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".BindStream(stream==null)");

      if (m_Stream!=null && m_Stream!=stream)
        throw new AzosIOException(StringConsts.ARGUMENT_ERROR+GetType().FullName+" must unbind prior stream first");

      m_Stream = stream;
    }

    /// <summary>
    /// Unbinds the current stream. This call is coupled with BindStream(stream)
    /// </summary>
    public void UnbindStream()
    {
      if (m_Stream==null) return;

      if (this is WritingStreamer) m_Stream.Flush();
      m_Stream = null;
    }

  }
}
