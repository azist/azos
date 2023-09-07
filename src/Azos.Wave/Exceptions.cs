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
  public class MvcActionException : MvcException, IExternalStatusProvider
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

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = this.DefaultBuildErrorStatusProviderMap(includeDump, "wave.mvc");
      result[CoreConsts.EXT_STATUS_KEY_CONTROLLER] = Controller;
      result[CoreConsts.EXT_STATUS_KEY_ACTION] = Action;

      return result;
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
    public const string FILTER_TYPE_FLD_NAME  = "FPE-FT";
    public const string FILTER_NAME_FLD_NAME  = "FPE-FN";
    public const string FILTER_ORDER_FLD_NAME = "FPE-FO";
    public const string FILTER_PATH_FLD_NAME  =  "FPE-FP";
    public const string HANDLER_TYPE_FLD_NAME = "FPE-HT";
    public const string HANDLER_NAME_FLD_NAME = "FPE-HN";

    public FilterPipelineException(WorkFilter filter, WorkFilter.CallChain chain, Exception inner) : base(inner.Message, inner)
    {
      FilterType = filter.GetType();
      FilterName = filter.Name;
      FilterOrder = filter.Order;

      //#783 20230218 DKh ---------------------
      var path = new StringBuilder(">", 128);
      for(var i=0; i <= chain.CurrentIndex; i++)
      {
        path.Append('>');
        path.Append(chain.Filters[i].Name);
      }

      FilterPath = path.ToString();
      //---------------------------------------


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
      FilterPath  = info.GetString(FILTER_PATH_FLD_NAME);
      HandlerType = (Type)info.GetValue(HANDLER_TYPE_FLD_NAME, typeof(Type));
      HandlerName = info.GetString(HANDLER_NAME_FLD_NAME);
    }

    public readonly Type   FilterType;
    public readonly string FilterName;
    public readonly int    FilterOrder;
    public readonly string FilterPath;//#783
    public readonly Type   HandlerType;
    public readonly string HandlerName;

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
        return FilterPath + ">  " +(cause != null ? cause.ToMessageWithType() : SysConsts.NULL_STRING);
      }
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(FILTER_TYPE_FLD_NAME, FilterType);
      info.AddValue(FILTER_NAME_FLD_NAME, FilterName);
      info.AddValue(FILTER_ORDER_FLD_NAME, FilterOrder);
      info.AddValue(FILTER_PATH_FLD_NAME,  FilterPath);
      info.AddValue(HANDLER_TYPE_FLD_NAME, HandlerType);
      info.AddValue(HANDLER_NAME_FLD_NAME, HandlerName);
      base.GetObjectData(info, context);
    }
  }


  /// <summary>
  /// Provides various extension methods
  /// </summary>
  public static class ExceptionExtensions
  {
    /// <summary>
    /// Describes exception into JsonDataMap suitable for use as a client response
    /// </summary>
    public static JsonDataMap ToClientResponseJsonMap(this Exception error, bool withDump)
    {
      if (error == null) return null;

      var actual = error;
      if (actual is FilterPipelineException fpe)
        actual = fpe.RootException;

      var result = new JsonDataMap();
      result[CoreConsts.EXT_STATUS_KEY_OK] = false;

      var http = error.SearchThisOrInnerExceptionOf<IHttpStatusProvider>();
      if (http != null)
      {
        result[CoreConsts.EXT_STATUS_KEY_HTTP_CODE] = http.HttpStatusCode;
        result[CoreConsts.EXT_STATUS_KEY_HTTP_DESCRIPTION] = http.HttpStatusDescription;
      }

      result[CoreConsts.EXT_STATUS_KEY_ERROR] = actual.GetType().Name;
      result[CoreConsts.EXT_STATUS_KEY_IS_AUTH] = Security.AuthorizationException.IsDenotedBy(error);

      var esp = error.SearchThisOrInnerExceptionOf<IExternalStatusProvider>();
      if (esp != null)
      {
        var data = esp.ProvideExternalStatus(withDump);

        if (data != null)
          result[CoreConsts.EXT_STATUS_KEY_DATA] = data;
      }

      if (withDump)
      {
        result[CoreConsts.EXT_STATUS_KEY_DEV_DUMP] = new WrappedExceptionData(error,
                                                       captureStack: true,
                                                       captureExternalStatus: false,
                                                       captureCallFlow: true);
      }

      return result;
    }
  }
}