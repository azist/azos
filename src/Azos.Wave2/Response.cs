/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Azos.IO;
using Azos.Log;
using Azos.Web;
using Azos.Serialization.JSON;

using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Azos.Wave
{
  /// <summary>
  /// Represents Response object used to generate web responses to client
  /// </summary>
  public sealed class Response : DisposableObject
  {
    #region CONSTS
    public const int DEFAULT_DOWNLOAD_BUFFER_SIZE = 128*1024;
    public const int MIN_DOWNLOAD_BUFFER_SIZE = 1*1024;
    public const int MAX_DOWNLOAD_BUFFER_SIZE = 1024*1024;
    public const int MAX_DOWNLOAD_FILE_SIZE = 32 * 1024 * 1024;

    public static readonly Encoding UTF8_WEB_ENCODING = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    #endregion

    #region .ctor
    internal Response(WorkContext work, HttpResponse httpResponse)
    {
      Work = work;
      m_Encoding = UTF8_WEB_ENCODING;
      m_AspResponse = httpResponse;
      m_AspResponse.Headers[HeaderNames.Server] = Work.Server.Name;
    }

    protected override void Destructor()
    {
      if (!((IDisposableLifecycle)this).DisposedByFinalizer)
        throw new NotSupportedException("Sync Response destructior is prohibited");
    }

    protected override async ValueTask DestructorAsync()
    {
      try
      {
        var srv = Work.Server;
        var ie = srv.m_InstrumentationEnabled;

        if (ie && m_WasWrittenTo)
        Interlocked.Increment(ref srv.m_stat_WorkContextWrittenResponse);

        if (m_Buffer != null)
        {
          var sz = m_Buffer.Position;
          m_AspResponse.ContentLength = sz;

          await m_AspResponse.Body.WriteAsync(m_Buffer.GetBuffer(), 0, (int)sz).ConfigureAwait(false);

          m_Buffer = null;

          if (ie)
          {
            Interlocked.Increment(ref srv.m_stat_WorkContextBufferedResponse);
            Interlocked.Add(ref srv.m_stat_WorkContextBufferedResponseBytes, sz);
          }
        }
      }
      catch(Exception error)
      {
        Work.Log(MessageType.Error, error.ToMessageWithType(), "Response.dctor()", error);
      }
    }
    #endregion

    #region Fields
    public readonly WorkContext Work;
    private HttpResponse m_AspResponse;

    private Encoding m_Encoding;
    private bool m_WasWrittenTo;
    private bool m_Buffered;
    private MemoryStream m_Buffer;
    #endregion

    #region Properties
    /// <summary>
    /// Returns true if some output has been performed
    /// </summary>
    public bool WasWrittenTo => m_WasWrittenTo;

    /// <summary>
    /// Determines whether the content is buffered locally. This property can not be set after
    ///  the response has been written to
    /// </summary>
    public bool Buffered
    {
      get => m_Buffered;
      set
      {
        if (m_WasWrittenTo)
        {
          throw new WaveException(StringConsts.RESPONSE_WAS_WRITTEN_TO_ERROR+".Buffered.set()");
        }
        m_Buffered = value;
      }
    }

    /// <summary>
    /// Gets/sets content encoding
    /// </summary>
    public Encoding Encoding
    {
      get => m_Encoding;
      set
      {
        if (m_WasWrittenTo)
        {
          throw new WaveException(StringConsts.RESPONSE_WAS_WRITTEN_TO_ERROR + ".Encoding.set()");
        }
        m_Encoding = value ?? UTF8_WEB_ENCODING;
      }
    }

    /// <summary>
    /// Http status code
    /// </summary>
    public int StatusCode
    {
      get => m_AspResponse.StatusCode;
      set => m_AspResponse.StatusCode = value;
    }

    /// <summary>
    /// Http status description communicated via header
    /// </summary>
    public string StatusDescription
    {
      get => m_AspResponse.Headers[Work.Server.HttpStatusTextHeader].ToString();
      set => m_AspResponse.Headers[Work.Server.HttpStatusTextHeader] = value;
    }

    /// <summary>
    /// Http content type get or set.
    /// Warning: Set textual content type using <see cref="SetTextualContentType(string)"/> which
    /// also sets the charset header value
    /// </summary>
    public string ContentType
    {
      get => m_AspResponse.ContentType;
      set => m_AspResponse.ContentType = value;
    }

    /// <summary>
    /// Set textual content type with charset taken from current encoding
    /// </summary>
    public void SetTextualContentType(string ctp)
     => m_AspResponse.ContentType = ctp.NonBlank(nameof(ctp)) + "; charset=" + Encoding.WebName;

    /// <summary>
    /// Returns http headers of the response
    /// </summary>
    public IHeaderDictionary Headers => m_AspResponse.Headers;

    #endregion

    #region Public

    /// <summary>
    /// Writes a string into response
    /// </summary>
    public async ValueTask WriteAsync(string content)
    {
      if (content.IsNotNullOrEmpty()) return;
      SetTextualContentType(Azos.Web.ContentType.TEXT);
      await setWasWrittenToAsync().ConfigureAwait(false);
      await StrUtils.TextBytes.WriteToStreamAsync(getStream(), content, Encoding).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes a string into response with \\n at the end
    /// </summary>
    public async ValueTask WriteLine(string content)
      => await WriteAsync((content ?? string.Empty) + "\n").ConfigureAwait(false);

    /// <summary>
    /// Writes an object as JSON. Does nothing if object is null
    /// </summary>
    public async Task WriteJsonAsync(object data, JsonWritingOptions options = null)
    {
      if (data==null) return;
      SetTextualContentType(Azos.Web.ContentType.JSON);
      await setWasWrittenToAsync().ConfigureAwait(false);
#warning async JsonWriter
      JsonWriter.Write(data, new NonClosingStreamWrap( getStream() ), options, Encoding);
    }

      /// <summary>
      /// Write the file to the client so client can download it. May set Buffered=false to use chunked encoding for big files
      /// </summary>
      public void WriteFile(string localFileName, int bufferSize = DEFAULT_DOWNLOAD_BUFFER_SIZE, bool attachment = false)
      {
        if (localFileName.IsNullOrWhiteSpace())
          throw new WaveException(StringConsts.ARGUMENT_ERROR+"Response.WriteFile(localFileName==null|empty)");

        var fi = new FileInfo(localFileName);

        if (!fi.Exists)
          throw new WaveException(StringConsts.RESPONSE_WRITE_FILE_DOES_NOT_EXIST_ERROR.Args(localFileName));

        var fsize = fi.Length;
        if (Buffered && fsize>MAX_DOWNLOAD_FILE_SIZE)
          throw new WaveException(StringConsts.RESPONSE_WRITE_FILE_OVER_MAX_SIZE_ERROR.Args(localFileName, MAX_DOWNLOAD_FILE_SIZE));

        var ext = Path.GetExtension(localFileName);
        setWasWrittenTo();
        m_NetResponse.ContentType = Work.App.GetContentTypeMappings().MapFileExtension(ext).ContentType;

        if (attachment)
          m_NetResponse.Headers.Add(WebConsts.HTTP_HDR_CONTENT_DISPOSITION, "attachment; filename={0}".Args(fi.Name));

        if (bufferSize<MIN_DOWNLOAD_BUFFER_SIZE) bufferSize=MIN_DOWNLOAD_BUFFER_SIZE;
        else if (bufferSize>MAX_DOWNLOAD_BUFFER_SIZE) bufferSize=MAX_DOWNLOAD_BUFFER_SIZE;

        using(var fs = new FileStream(localFileName, FileMode.Open, FileAccess.Read))
        {

          var dest = getStream();
          if (dest is MemoryStream)
          {
            var ms = ((MemoryStream)dest);
            if (ms.Position==0)
            {
              ms.SetLength(fsize);//pre-allocate memory stream
              ms.Position = ms.Length;
              fs.Read(ms.GetBuffer(), 0, (int)ms.Position);
              return;
            }
          }

          fs.CopyTo(dest, bufferSize);
        }
      }


      /// <summary>
      /// Write the contents of the stream to the client so client can download it. May set Buffered=false to use chunked encoding for big files
      /// </summary>
      public void WriteStream(Stream stream, int bufferSize = DEFAULT_DOWNLOAD_BUFFER_SIZE, string attachmentName = null)
      {
        if (stream==null)
          throw new WaveException(StringConsts.ARGUMENT_ERROR+"Response.WriteStream(stream==null)");

        setWasWrittenTo();

        if (attachmentName.IsNotNullOrWhiteSpace())
          m_NetResponse.Headers.Add(WebConsts.HTTP_HDR_CONTENT_DISPOSITION, "attachment; filename={0}".Args(attachmentName));

        if (bufferSize<MIN_DOWNLOAD_BUFFER_SIZE) bufferSize=MIN_DOWNLOAD_BUFFER_SIZE;
        else if (bufferSize>MAX_DOWNLOAD_BUFFER_SIZE) bufferSize=MAX_DOWNLOAD_BUFFER_SIZE;


        var dest = getStream();
        stream.CopyTo(dest, bufferSize);
      }


      /// <summary>
      /// Returns output stream for direct output and marks response as being written into
      /// </summary>
      public Stream GetDirectOutputStreamForWriting()
      {
        setWasWrittenTo();
        return getStream();
      }

      /// <summary>
      /// Cancels the buffered content. Throws if the response is not Buffered
      /// </summary>
      public void CancelBuffered()
      {
        if (!WasWrittenTo) return;
        if (!Buffered)
          throw new WaveException(StringConsts.RESPONSE_CANCEL_NON_BUFFERED_ERROR);
        m_Buffer = null;
        m_WasWrittenTo = false;
      }

      /// <summary>
      /// RESERVED FOR FUTURE USE. Flushes the internally buffered content
      /// </summary>
      public void Flush()
      {
       //There is currently (2014.03.26) no way to flush the HttpListenerResponse.OutputStream
      }


      /// <summary>
      /// Configures response with redirect status and headers. This method DOES NOT ABORT the work pipeline,so
      ///  the processing of filters and handlers continues unless 'work.Aborted = true' is issued in code.
      ///  See also 'RedirectAndAbort(url)'
      /// </summary>
      public void Redirect(string url, WebConsts.RedirectCode code = WebConsts.RedirectCode.Found_302)
      {
        m_NetResponse.Headers.Set(HttpResponseHeader.Location, url);
        m_NetResponse.StatusCode        = WebConsts.GetRedirectStatusCode(code);
        m_NetResponse.StatusDescription = WebConsts.GetRedirectStatusDescription(code);
      }

      /// <summary>
      /// Configures response with redirect status and headers. This method also aborts the work pipeline,so
      ///  the processing of filters and handlers does not continue. See also 'Redirect(url)'
      /// </summary>
      public void RedirectAndAbort(string url, WebConsts.RedirectCode code = WebConsts.RedirectCode.Found_302)
      {
        this.Redirect(url, code);
        Work.Aborted = true;
      }

      /// <summary>
      /// Adds Http header
      /// </summary>
      public void AddHeader(string name, string value)
      {
        m_NetResponse.AddHeader(name, value);
      }

      /// <summary>
      /// Appends cookie to the response
      /// </summary>
      public void AppendCookie(Cookie cookie)
      {
        m_NetResponse.AppendCookie(cookie);
      }

      public bool SetCacheControlHeaders(CacheControl control, bool force = true, string vary = null)
      {
        if (!force && m_NetResponse.Headers[HttpResponseHeader.CacheControl].IsNotNullOrWhiteSpace()) return false;
        var value = control.HTTPCacheControl;
        if (value.IsNullOrWhiteSpace()) return false;

        m_NetResponse.Headers[HttpResponseHeader.CacheControl] = value;
        if (control.Cacheability == CacheControl.Type.NoCache)
        {
          m_NetResponse.Headers[HttpResponseHeader.Pragma] = "no-cache";
          m_NetResponse.Headers[HttpResponseHeader.Expires] = "0";
          m_NetResponse.Headers[HttpResponseHeader.Vary] = "*";
        }
        else
        {
          if (vary.IsNotNullOrWhiteSpace())
            m_NetResponse.Headers[HttpResponseHeader.Vary] = vary;
        }
        return true;
      }

      /// <summary>
      /// Sets headers so all downstream, layers (browsers, proxies) do not cache response.
      /// If Force==true(default) then overrides existing headers with no cache.
      /// Returns true when headers were set
      /// </summary>
      public bool SetNoCacheHeaders(bool force = true)
      {
        return SetCacheControlHeaders(CacheControl.NoCache, force);
      }

    #endregion


    #region .pvt
    private Stream getStream()
    {
      if (m_Buffer != null) return m_Buffer;

      if (Buffered)
      {
        m_Buffer = new MemoryStream();
        return m_Buffer;
      }

      return m_AspResponse.Body;
    }

    private async ValueTask setWasWrittenToAsync()
    {
      m_WasWrittenTo = true;

      if (!m_AspResponse.HasStarted)
      {
        await m_AspResponse.StartAsync().ConfigureAwait(false);
      }
    }
    #endregion
  }
}
