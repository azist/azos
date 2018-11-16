/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Provides a file storage sinks implementation
  /// </summary>
  public abstract class TextFileSink : FileSink
  {

    protected TextFileSink(ISinkOwner owner) : base(owner)
    {
    }

    protected TextFileSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
    }


    private StreamWriter m_Writer;

    protected override void DoOpenStream()
    {
      m_Writer = new StreamWriter(m_Stream);
    }

    protected override void DoCloseStream()
    {
      if (m_Writer!=null)
      {
        m_Writer.Flush();
        m_Writer.Close();
        DisposableObject.DisposeAndNull(ref m_Writer);
      }
    }

    /// <summary>
    /// Warning: don't override this method in derived destinations, use
    /// DoFormatMessage() instead!
    /// </summary>
    protected override void DoWriteMessage(Message msg)
    {
      var txtMsg = DoFormatMessage(msg);
      m_Writer.WriteLine( txtMsg );
    }

    protected internal override void DoPulse()
    {
      base.DoPulse();
      m_Writer.Flush();
    }

    /// <summary>
    /// Called when message is to be written to stream
    /// </summary>
    protected abstract string DoFormatMessage(Message msg);
  }
}
