/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
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
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

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
        throw new NotSupportedException("Sync Response destructor is prohibited");
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
          await m_AspResponse.StartAsync().ConfigureAwait(false);
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
    /// Provides access to ASP.Net response object
    /// </summary>
    public HttpResponse AspResponse => m_AspResponse;


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
    /// Body error phrase communicated via header
    /// </summary>
    public string BodyError
    {
      get
      {
        var hdr = Work.Server.HttpBodyErrorHeader;
        if (hdr.IsNotNullOrWhiteSpace()) return m_AspResponse.Headers[hdr].ToString();
        return null;
      }
      set
      {
        var hdr = Work.Server.HttpBodyErrorHeader;
        if (hdr.IsNotNullOrWhiteSpace())
        {
          m_AspResponse.Headers[hdr] = value;
        }
      }
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

    /// <summary>
    /// Response cookies
    /// </summary>
    public IResponseCookies Cookies => m_AspResponse.Cookies;

    #endregion

    #region Public

    /// <summary>
    /// Writes a string into response
    /// </summary>
    public async Task WriteAsync(string content, bool setContentType = true)
    {
      if (content.IsNullOrEmpty()) return;

      if (setContentType) SetTextualContentType(Azos.Web.ContentType.TEXT);

      await setWasWrittenToAsync().ConfigureAwait(false);
      await StrUtils.TextBytes.WriteToStreamAsync(getStream(), content, Encoding).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes a string into response with \\n at the end
    /// </summary>
    public async Task WriteLineAsync(string content)
      => await WriteAsync((content ?? string.Empty) + "\n").ConfigureAwait(false);

    /// <summary>
    /// Writes an object as JSON. Does nothing if object is null
    /// </summary>
    public async Task WriteJsonAsync(object data, JsonWritingOptions options = null)
    {
      if (data==null) return;
      SetTextualContentType(Azos.Web.ContentType.JSON);
      await setWasWrittenToAsync().ConfigureAwait(false);
#warning NonClosingStreamWrap needs to be removed, instead use "TextWriter.keepOpen" property #731
      await JsonWriter.WriteAsync(data, new NonClosingStreamWrap( getStream() ), options, Encoding).ConfigureAwait(false);
    }

    /// <summary>
    /// Write the file to the client so client can download it. May set Buffered=false to use chunked encoding for big files
    /// </summary>
    public async Task WriteFileAsync(string localFileName, int bufferSize = DEFAULT_DOWNLOAD_BUFFER_SIZE, bool attachment = false, CancellationToken? cancelToken = null)
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
      ContentType = Work.App.GetContentTypeMappings().MapFileExtension(ext).ContentType;
      if (attachment)
      {
        Headers.Add(WebConsts.HTTP_HDR_CONTENT_DISPOSITION, "attachment; filename={0}".Args(fi.Name));
      }

      await setWasWrittenToAsync().ConfigureAwait(false);


      if (bufferSize<MIN_DOWNLOAD_BUFFER_SIZE) bufferSize=MIN_DOWNLOAD_BUFFER_SIZE;
      else if (bufferSize>MAX_DOWNLOAD_BUFFER_SIZE) bufferSize=MAX_DOWNLOAD_BUFFER_SIZE;

      using var fs = new FileStream(localFileName, FileMode.Open, FileAccess.Read);

      var dest = getStream();
      if (dest is MemoryStream ms)
      {
        if (ms.Position==0)
        {
          ms.SetLength(fsize);//pre-allocate memory stream
          ms.Position = ms.Length;

          if (cancelToken.HasValue)
            await fs.ReadAsync(ms.GetBuffer(), 0, (int)ms.Position, cancelToken.Value).ConfigureAwait(false);
          else
            await fs.ReadAsync(ms.GetBuffer(), 0, (int)ms.Position).ConfigureAwait(false);

          return;
        }
      }

      if (cancelToken.HasValue)
        await fs.CopyToAsync(dest, bufferSize, cancelToken.Value).ConfigureAwait(false);
      else
        await fs.CopyToAsync(dest, bufferSize).ConfigureAwait(false);
    }


    /// <summary>
    /// Write the contents of the stream to the client so client can download it. May set Buffered=false to use chunked encoding for big files
    /// </summary>
    public async Task WriteStreamAsync(Stream stream, int bufferSize = DEFAULT_DOWNLOAD_BUFFER_SIZE, string attachmentName = null, CancellationToken? cancelToken = null)
    {
      if (stream==null)
        throw new WaveException(StringConsts.ARGUMENT_ERROR+"Response.WriteStream(stream==null)");


      if (attachmentName.IsNotNullOrWhiteSpace())
      {
        Headers.Add(WebConsts.HTTP_HDR_CONTENT_DISPOSITION, "attachment; filename={0}".Args(attachmentName));
      }

      await setWasWrittenToAsync().ConfigureAwait(false);

      if (bufferSize<MIN_DOWNLOAD_BUFFER_SIZE) bufferSize=MIN_DOWNLOAD_BUFFER_SIZE;
      else if (bufferSize>MAX_DOWNLOAD_BUFFER_SIZE) bufferSize=MAX_DOWNLOAD_BUFFER_SIZE;


      var dest = getStream();

      if (cancelToken.HasValue)
        await stream.CopyToAsync(dest, bufferSize, cancelToken.Value).ConfigureAwait(false);
      else
        await stream.CopyToAsync(dest, bufferSize).ConfigureAwait(false);
    }


    /// <summary>
    /// Returns output stream for direct output and marks response as being written into
    /// </summary>
    public async Task<Stream> GetDirectOutputStreamForWritingAsync()
    {
      await setWasWrittenToAsync().ConfigureAwait(false);
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
    public Task FlushAsync(CancellationToken? cancelToken = null)
    {
      if (!WasWrittenTo || Buffered) return Task.CompletedTask;

      if (cancelToken.HasValue)
        return m_AspResponse.Body.FlushAsync(cancelToken.Value);
      else
        return m_AspResponse.Body.FlushAsync();
    }


    /// <summary>
    /// Configures response with redirect status and headers. This method DOES NOT ABORT the work pipeline,so
    ///  the processing of filters and handlers continues unless 'work.Aborted = true' is issued in code.
    ///  See also 'RedirectAndAbort(url)'
    /// </summary>
    public void Redirect(string url, WebConsts.RedirectCode code = WebConsts.RedirectCode.Found_302)
    {
      Headers.Location = url.NonBlank(nameof(url));
      StatusCode        = WebConsts.GetRedirectStatusCode(code);
      StatusDescription = WebConsts.GetRedirectStatusDescription(code);
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
    /// Adds Http header value/s
    /// </summary>
    public void AddHeader(string name, string value)
      => Headers.Add(name.NonBlank(nameof(name)), value.NonNull(nameof(value)));

    public void AddHeader(string name, IEnumerable<string> values)
      => Headers.Add(name.NonBlank(nameof(name)), new StringValues(values.NonNull(nameof(values)) is string[] sa ? sa : values.ToArray()));

    public void AddHeader(string name, StringValues values)
      => Headers.Add(name.NonBlank(nameof(name)), values);

    /// <summary>
    /// Appends cookie to the response
    /// </summary>
    public void AppendCookie(string key, string value, CookieOptions options = null)
    {
      if (options != null)
        m_AspResponse.Cookies.Append(key.NonBlank(nameof(key)), value.NonNull(nameof(value)), options);
      else
        m_AspResponse.Cookies.Append(key.NonBlank(nameof(key)), value.NonNull(nameof(value)));
    }

    /// <summary>
    /// Sets cache control header
    /// </summary>
    public bool SetCacheControlHeaders(CacheControl control, bool force = true, string vary = null)
    {
      //SeekOrigin:
      //Microsoft.AspNetCore.Http.Headers.ResponseHeaders.
      //Microsoft.Net.Http.Headers.CacheControlHeaderValue

      if (!force && StringValues.IsNullOrEmpty(Headers.CacheControl)) return false;
      var value = control.HTTPCacheControl;
      if (value.IsNullOrWhiteSpace()) return false;

      Headers.CacheControl = value;
      if (control.Cacheability == CacheControl.Type.NoCache)
      {
        Headers.Pragma = "no-cache";
        Headers.Expires = "0";
        Headers.Vary = "*";
      }
      else
      {
        if (vary.IsNotNullOrWhiteSpace())
          Headers.Vary = vary;
      }
      return true;
    }

    /// <summary>
    /// Sets headers so all downstream, layers (browsers, proxies) do not cache response.
    /// If Force==true(default) then overrides existing headers with no cache.
    /// Returns true when headers were set
    /// </summary>
    public bool SetNoCacheHeaders(bool force = true) => SetCacheControlHeaders(CacheControl.NoCache, force);
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

      if (!m_AspResponse.HasStarted && !m_Buffered)
      {
        await m_AspResponse.StartAsync().ConfigureAwait(false);
      }
    }
    #endregion
  }
}
