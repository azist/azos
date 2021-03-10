/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Conf;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Provides a file storage destination implementation
  /// </summary>
  public abstract class FileSink : Sink
  {
    private  const string DEFAULT_FILENAME = "{0:yyyyMMdd}.log";

    protected FileSink(ISinkOwner owner) : base(owner){ }
    protected FileSink(ISinkOwner owner, string name, int order) : base(owner, name, order){ }

    protected string        m_Path;
    protected string        m_FileName;

    /// <summary> Primary target stream used for writing into main file </summary>
    protected Stream        m_Stream;
    protected string        m_StreamFileName;
    private bool            m_Recreate;

    /// <summary>
    /// The name of the file without path may use {0} for date: {0:yyyyMMdd}-$($name).csv.log
    /// </summary>
    [Config]
    public virtual string FileName
    {
      get { return m_FileName; }
      set
      {
        if (m_FileName==value) return;
        m_FileName = value;
        m_Recreate = true;
      }
    }

    /// <summary>
    /// Directory where file should be created. Will create the directory chain if it doesn't exist
    /// </summary>
    [Config]
    public virtual string Path
    {
      get { return m_Path; }
      set
      {
        if (m_Path==value) return;
        m_Path = value;
        m_Recreate = true;
      }
    }


    protected override void DoStart()
    {
      base.DoStart();
      ensureStream();
    }

    protected override void DoWaitForCompleteStop()
    {
      closeStream();
      base.DoWaitForCompleteStop();
    }


    private DateTime m_PrevDate;
    protected internal override void DoPulse()
    {
      base.DoPulse();

      if (m_Stream==null) return;

      var utcNow = DateTime.UtcNow;
      if ((utcNow - m_PrevDate).TotalSeconds < 10) return;
      m_PrevDate = utcNow;

      if (m_StreamFileName != GetDestinationFileName())
      {
        m_Recreate = true;
        ensureStream();
      }
    }


    /// <summary>
    /// Called after output stream has been opened
    /// </summary>
    protected abstract void DoOpenStream();

    /// <summary>
    /// Called just before output stream is closed
    /// </summary>
    protected abstract void DoCloseStream();

    /// <summary>
    /// Called when message is to be written to stream
    /// </summary>
    protected abstract void DoWriteMessage(Message msg);

    /// <summary>
    /// Override DoFormatMessage() instead
    /// </summary>
    protected internal sealed override void DoSend(Message msg)
    {
      ensureStream();
      DoWriteMessage(msg);
    }

    protected virtual string DefaultFileName => DEFAULT_FILENAME;


    protected virtual string GetDestinationFileName()
    {
      var path = m_Path;
      if (path.IsNotNullOrWhiteSpace())
      {
        try
        {
          path = path.Args( LocalizedTime );
        }
        catch(Exception error)
        {
          throw new LogException(StringConsts.LOGSVC_FILE_SINK_PATH_ERROR.Args(Name, path, error.ToMessageWithType()), error);
        }
      }

      var fn = m_FileName;

      if (fn.IsNullOrWhiteSpace())
          fn = Name;

      if (fn.IsNullOrWhiteSpace())
          fn = DefaultFileName;

      try
      {
        fn = fn.Args( LocalizedTime );
      }
      catch(Exception error)
      {
        throw new LogException(StringConsts.LOGSVC_FILE_SINK_FILENAME_ERROR.Args(Name, fn, error.ToMessageWithType()), error);
      }

      CheckPath(path);

      var result = path.IsNullOrWhiteSpace() ? fn : CombinePaths(path, fn);
      return result;
    }

    /// <summary>
    /// Override to check and possibly pre-create the requested path.
    /// The default implementation uses Directory class for local file access
    /// </summary>
    protected virtual void CheckPath(string path)
    {
      if (path.IsNotNullOrWhiteSpace() && !Directory.Exists(path))
        IOUtils.EnsureAccessibleDirectory(path);
    }

    /// <summary>
    /// Override to combine paths, the default implementation uses Path.Combine
    /// </summary>
    protected virtual string CombinePaths(string p1, string p2)
    {
      return System.IO.Path.Combine(p1, p2);
    }

    private void ensureStream()
    {
      if (m_Stream!=null && !m_Recreate) return;
      closeStream();
      m_Recreate = false;
      var fn = GetDestinationFileName();

      m_Stream = MakeStream(fn);
      m_StreamFileName = fn;
      DoOpenStream();
    }

    /// <summary>
    /// Factory method: override to create specific stream type. The default implementation creates a local FileStream instance
    /// </summary>
    protected virtual Stream MakeStream(string fileName)
    {
      return new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
    }

    private void closeStream()
    {
      DoCloseStream();
      DisposeAndNull(ref m_Stream);
    }
  }
}
