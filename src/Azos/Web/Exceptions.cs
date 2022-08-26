/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;
using Azos.Serialization.JSON;

namespace Azos.Web
{
  /// <summary>
  /// Base exception class thrown by Azos.Web-related topics
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
  public class WebCallException : WebException, IHttpStatusProvider, IExternalStatusProvider
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

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = this.DefaultBuildErrorStatusProviderMap(includeDump, "web.call");
      result[CoreConsts.EXT_STATUS_KEY_URI] = Uri;
      result[CoreConsts.EXT_STATUS_KEY_METHOD] = Method;
      result[CoreConsts.EXT_STATUS_KEY_HTTP_CODE] = HttpStatusCode;
      result[CoreConsts.EXT_STATUS_KEY_HTTP_DESCRIPTION] = HttpStatusDescription;

      if (includeDump)
      {
        result[CoreConsts.EXT_STATUS_KEY_CONTENT] = ErrorResponseContent;
      }

      return result;
    }
  }


  /// <summary>
  /// Thrown to indicate various Http status conditions such as the ones which arise on servers and server logic handlers
  /// </summary>
  [Serializable]
  public class HTTPStatusException : WebException, IHttpStatusProvider, IExternalStatusProvider
  {
    public const string STATUS_CODE_FLD_NAME = "HTTPSE-SC";
    public const string STATUS_DESCRIPTION_FLD_NAME = "HTTPSE-SD";

    public static HTTPStatusException BadRequest_400(string descr = null)
    {
      var d = WebConsts.STATUS_400_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_400, d);
    }

    public static HTTPStatusException Unauthorized_401(string descr = null)
    {
      var d = WebConsts.STATUS_401_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_401, d);
    }

    public static HTTPStatusException Forbidden_403(string descr = null)
    {
      var d = WebConsts.STATUS_403_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_403, d);
    }

    public static HTTPStatusException NotFound_404(string descr = null)
    {
      var d = WebConsts.STATUS_404_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_404, d);
    }

    public static HTTPStatusException MethodNotAllowed_405(string descr = null)
    {
      var d = WebConsts.STATUS_405_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_405, d);
    }

    public static HTTPStatusException NotAcceptable_406(string descr = null)
    {
      var d = WebConsts.STATUS_406_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_406, d);
    }

    public static HTTPStatusException TooManyRequests_429(string descr = null)
    {
      var d = WebConsts.STATUS_429_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_429, d);
    }

    public static HTTPStatusException InternalError_500(string descr = null)
    {
      var d = WebConsts.STATUS_500_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": " + descr);

      return new HTTPStatusException(WebConsts.STATUS_500, d);
    }

    public HTTPStatusException(int statusCode, string statusDescription) : base("{0} - {1}".Args(statusCode, statusDescription))
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    public HTTPStatusException(int statusCode, string statusDescription, string message) : base("{0} - {1} : {2}".Args(statusCode, statusDescription, message))
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    public HTTPStatusException(int statusCode, string statusDescription, string message, Exception inner) : base("{0} - {1} : {2}".Args(statusCode, statusDescription, message), inner)
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    protected HTTPStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      StatusCode = info.GetInt32(STATUS_CODE_FLD_NAME);
      StatusDescription = info.GetString(STATUS_DESCRIPTION_FLD_NAME);
    }

    /// <summary>
    /// Http status code
    /// </summary>
    public readonly int StatusCode;

    /// <summary>
    /// Http status description
    /// </summary>
    public readonly string StatusDescription;

    int IHttpStatusProvider.HttpStatusCode => StatusCode;
    string IHttpStatusProvider.HttpStatusDescription => StatusDescription;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(STATUS_CODE_FLD_NAME, StatusCode);
      info.AddValue(STATUS_DESCRIPTION_FLD_NAME, StatusDescription);
      base.GetObjectData(info, context);
    }

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = this.DefaultBuildErrorStatusProviderMap(includeDump, "wave.mvc");
      result[CoreConsts.EXT_STATUS_KEY_HTTP_CODE] = StatusCode;
      result[CoreConsts.EXT_STATUS_KEY_HTTP_DESCRIPTION] = StatusDescription;

      return result;
    }
  }


}
