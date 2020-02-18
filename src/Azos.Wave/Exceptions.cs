/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using System.Text;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Wave
{
  /// <summary>
  /// Base exception thrown by the WAVE framework
  /// </summary>
  [Serializable]
  public class WaveException : AzosException
  {
    public WaveException() { }
    public WaveException(string message) : base(message) { }
    public WaveException(string message, Exception inner) : base(message, inner) { }
    protected WaveException(SerializationInfo info, StreamingContext context): base(info, context) { }
  }



  /// <summary>
  /// Base exception thrown by the Mvc-related framework
  /// </summary>
  [Serializable]
  public class MvcException : WaveException
  {
    public MvcException() { }
    public MvcException(string message) : base(message) { }
    public MvcException(string message, Exception inner) : base(message, inner) { }
    protected MvcException(SerializationInfo info, StreamingContext context): base(info, context) { }
  }


  /// <summary>
  /// Wraps inner exceptions capturing stack trace in inner implementing blocks
  /// </summary>
  [Serializable]
  public class MvcActionException : MvcException
  {
    public const string CONTROLLER_FLD_NAME = "MVCAE-C";
    public const string ACTION_FLD_NAME = "MVCAE-A";

    public static MvcActionException WrapActionBodyError(string controller, string action, Exception src)
    {
      if (src==null) throw new WaveException(StringConsts.ARGUMENT_ERROR+typeof(MvcActionException).Name+"Wrap(src=null)");

      return new MvcActionException(controller,
                                    action,
                                    "Controller action body: '{0}'.'{1}'. Exception: {2}".Args(controller, action, src.ToMessageWithType()),
                                    src);
    }

    public static MvcActionException WrapActionResultError(string controller, string action, object result, Exception src)
    {
      if (src==null) throw new WaveException(StringConsts.ARGUMENT_ERROR+typeof(MvcActionException).Name+"Wrap(src=null)");

      return new MvcActionException(controller,
                                    action,
                                    "Controller action result processing: '{0}'.'{1}' -> {2}. Exception: {3}".Args(controller,
                                                                                                                   action,
                                                                                                                   result==null ? "<null>" : result.GetType().FullName,
                                                                                                                   src.ToMessageWithType()),
                                    src);
    }

    protected MvcActionException(string controller, string action, string msg, Exception inner): base(msg, inner)
    {
      Controller = controller;
      Action = action;
    }

    protected MvcActionException(SerializationInfo info, StreamingContext context): base(info, context)
    {
      Controller = info.GetString(CONTROLLER_FLD_NAME);
      Action = info.GetString(ACTION_FLD_NAME);
    }

    public readonly string Controller;
    public readonly string Action;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(CONTROLLER_FLD_NAME, Controller);
      info.AddValue(ACTION_FLD_NAME, Action);
      base.GetObjectData(info, context);
    }
  }

  /// <summary>
  /// Wraps WAVE template rendering exceptions
  /// </summary>
  [Serializable]
  public class WaveTemplateRenderingException : WaveException
  {
    public WaveTemplateRenderingException(string message, Exception inner): base(message, inner) { }
    protected WaveTemplateRenderingException(SerializationInfo info, StreamingContext context): base(info, context) { }
  }

  /// <summary>
  /// Thrown by filter pipeline
  /// </summary>
  [Serializable]
  public class FilterPipelineException : WaveException
  {
    public const string FILTER_TYPE_FLD_NAME = "FPE-FT";
    public const string FILTER_NAME_FLD_NAME = "FPE-FN";
    public const string FILTER_ORDER_FLD_NAME = "FPE-FO";
    public const string HANDLER_TYPE_FLD_NAME = "FPE-HT";
    public const string HANDLER_NAME_FLD_NAME = "FPE-HN";

    public FilterPipelineException(WorkFilter filter, Exception inner) : base(inner.Message, inner)
    {
      FilterType = filter.GetType();
      FilterName = filter.Name;
      FilterOrder = filter.Order;
      if (filter.Handler != null)
      {
        HandlerType = filter.Handler.GetType();
        HandlerName = filter.Handler.Name;
      }
    }

    protected FilterPipelineException(SerializationInfo info, StreamingContext context): base(info, context)
    {
      FilterType = (Type)info.GetValue(FILTER_TYPE_FLD_NAME, typeof(Type));
      FilterName = info.GetString(FILTER_NAME_FLD_NAME);
      FilterOrder = info.GetInt32(FILTER_ORDER_FLD_NAME);
      HandlerType = (Type)info.GetValue(HANDLER_TYPE_FLD_NAME, typeof(Type));
      HandlerName = info.GetString(HANDLER_NAME_FLD_NAME);
    }

    public readonly Type FilterType;
    public readonly string FilterName;
    public readonly int FilterOrder;
    public readonly Type HandlerType;
    public readonly string HandlerName;

    /// <summary>
    /// Returns a mnemonic filter sequence where the root exception originated from
    /// </summary>
    public string FilterPath
    {
      get
      {
         var result = new StringBuilder(":>", 128);

         Exception error = this;
         while(error is FilterPipelineException fpe)
         {
            result.Append(fpe.FilterName);
            result.Append(">");
            error = error.InnerException;
         }

         return result.ToString();
      }
    }

    /// <summary>
    /// Returns unwound root exception - unwrapping it from FilterPipelineException
    /// </summary>
    public Exception RootException
    {
      get
      {
         if (InnerException is FilterPipelineException fpe)
           return fpe.RootException;
         else
           return InnerException;
      }
    }

    public override string Message
    {
      get
      {
        var cause = RootException;
        return FilterPath + " " +(cause != null ? cause.ToMessageWithType() : SysConsts.NULL_STRING);
      }
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(FILTER_TYPE_FLD_NAME, FilterType);
      info.AddValue(FILTER_NAME_FLD_NAME, FilterName);
      info.AddValue(FILTER_ORDER_FLD_NAME, FilterOrder);
      info.AddValue(HANDLER_TYPE_FLD_NAME, HandlerType);
      info.AddValue(HANDLER_NAME_FLD_NAME, HandlerName);
      base.GetObjectData(info, context);
    }
  }

  /// <summary>
  /// Thrown to indicate various Http status conditions
  /// </summary>
  [Serializable]
  public class HTTPStatusException : WaveException, IHttpStatusProvider
  {
    public const string STATUS_CODE_FLD_NAME = "HTTPSE-SC";
    public const string STATUS_DESCRIPTION_FLD_NAME = "HTTPSE-SD";

    public static HTTPStatusException BadRequest_400(string descr = null)
    {
      var d = WebConsts.STATUS_400_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_400, d);
    }

    public static HTTPStatusException Unauthorized_401(string descr = null)
    {
      var d = WebConsts.STATUS_401_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_401, d);
    }

    public static HTTPStatusException Forbidden_403(string descr = null)
    {
      var d = WebConsts.STATUS_403_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_403, d);
    }

    public static HTTPStatusException NotFound_404(string descr = null)
    {
      var d = WebConsts.STATUS_404_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_404, d);
    }

    public static HTTPStatusException MethodNotAllowed_405(string descr = null)
    {
      var d = WebConsts.STATUS_405_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_405, d);
    }

    public static HTTPStatusException NotAcceptable_406(string descr = null)
    {
      var d = WebConsts.STATUS_406_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_406, d);
    }

    public static HTTPStatusException TooManyRequests_429(string descr = null)
    {
      var d = WebConsts.STATUS_429_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

      return new HTTPStatusException(WebConsts.STATUS_429, d);
    }

    public static HTTPStatusException InternalError_500(string descr = null)
    {
      var d = WebConsts.STATUS_500_DESCRIPTION;
      if (descr.IsNotNullOrWhiteSpace()) d += (": "+descr);

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
  }

  /// <summary>
  /// Provides various extension methods
  /// </summary>
  public static class ExceptionExtensions
  {
    /// <summary>
    /// Describes exception into JsonDataMap for responding to client
    /// </summary>
    public static JsonDataMap ToClientResponseJsonMap(this Exception error, bool withDump)
    {
      if (error == null) return null;

      var actual = error;
      if (actual is FilterPipelineException fpe)
        actual = fpe.RootException;

      var result = new JsonDataMap();
      result["OK"] = false;

      if (actual is IHttpStatusProvider st)
      {
        result["HttpStatusCode"] = st.HttpStatusCode;
        result["HttpStatusDescription"] = st.HttpStatusDescription;
      }

      result["Error"] = actual.GetType().Name;
      result["IsAuthorization"] = Security.AuthorizationException.IsDenotedBy(actual);

      if (withDump)
      {
        result["dev-cause"] = new WrappedExceptionData(actual, false);
        result["dev-dump"] = new WrappedExceptionData(error, true);
      }

      if (actual is IExternalStatusProvider esp)
        result["data"] = esp.ProvideExternalStatus(withDump);


      return result;
    }
  }
}