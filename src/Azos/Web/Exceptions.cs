/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Web
{
  /// <summary>
  /// Base exception class thrown by Azos.Web assembly
  /// </summary>
  [Serializable]
  public class WebException : AzosException
  {
    public WebException() { }
    public WebException(string message) : base(message) { }
    public WebException(string message, Exception inner) : base(message, inner) { }
    protected WebException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown for making web calls to external servers
  /// </summary>
  [Serializable]
  public class WebCallException : WebException, IHttpStatusProvider
  {
    public const string URI_FLD_NAME = "WCALL-URI";
    public const string METHOD_FLD_NAME = "WCALL-METHOD";
    public const string HTTPCODE_FLD_NAME = "WCALL-CODE";
    public const string HTTPDESCR_FLD_NAME = "WCALL-DESCR";
    public const string RESPONSE_FLD_NAME = "WCALL-ERR-RESP";


    /// <summary>
    /// Destination Uri of the call
    /// </summary>
    public string Uri { get; private set; }

    /// <summary>
    /// Call Http method
    /// </summary>
    public string Method { get; private set; }

    /// <summary>
    /// Returns the content of the error response, typically shortened
    /// </summary>
    public string ErrorResponseContent { get ; private set;}

    /// <summary>
    /// Resulting HTTP status code
    /// </summary>
    public int HttpStatusCode { get; private set; }

    /// <summary>
    /// Status description
    /// </summary>
    public string HttpStatusDescription { get; private set; }


    public WebCallException() { }
    public WebCallException(string message, string uri, string method, int httpCode, string httpDescription, string errorResponseContent) : base(message)
    {
      Uri = uri;
      Method = method;
      HttpStatusCode = httpCode;
      HttpStatusDescription = httpDescription;
      ErrorResponseContent = errorResponseContent;
    }

    public WebCallException(string message, string uri, string method, int httpCode, string httpDescription, string errorResponseContent, Exception inner) : base(message, inner)
    {
      Uri = uri;
      Method = method;
      HttpStatusCode = httpCode;
      HttpStatusDescription = httpDescription;
      ErrorResponseContent = errorResponseContent;
    }

    protected WebCallException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      Uri = info.GetString(URI_FLD_NAME);
      Method = info.GetString(METHOD_FLD_NAME);
      HttpStatusCode = info.GetInt32(HTTPCODE_FLD_NAME);
      HttpStatusDescription = info.GetString(HTTPDESCR_FLD_NAME);
      ErrorResponseContent = info.GetString(RESPONSE_FLD_NAME);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.NonNull(nameof(info));

      info.AddValue(URI_FLD_NAME, Uri);
      info.AddValue(METHOD_FLD_NAME, Method);
      info.AddValue(HTTPCODE_FLD_NAME, HttpStatusCode);
      info.AddValue(HTTPDESCR_FLD_NAME, HttpStatusDescription);
      info.AddValue(RESPONSE_FLD_NAME, ErrorResponseContent);
      base.GetObjectData(info, context);
    }
  }
}
