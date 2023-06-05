/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

using Azos.Web;
using Azos.Graphics;
using Azos.Conf;
using Azos.Collections;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Wave.Mvc;
using Azos.Wave.Templatization;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Handles MVC-related requests
  /// </summary>
  public class MvcHandler : TypeLookupHandler<Controller>
  {
    #region .ctor
    protected MvcHandler(WorkHandler director, string name, int order, WorkMatch match = null) : base(director, name, order, match)
    {
    }

    protected MvcHandler(WorkHandler director, IConfigSectionNode confNode) : base(director, confNode)
    {
    }
    #endregion

    #region Fields
    private Registry<ControllerInfo> m_Controllers = new Registry<ControllerInfo>();
    #endregion

    #region Protected
    protected override async Task DoTargetWorkAsync(Controller target, WorkContext work)
    {
      target.m_WorkContext = work;//This also establishes Controller.App
      App.DependencyInjector.InjectInto(target);//Inject app-rooted context


      var action = GetActionName(target, work);

      object[] args;

      //1. try controller instance to resolve action
      var mi = target.FindMatchingAction(work, action, out args);

      //2. if controller did not resolve then resolve by framework (most probable case)
      if (mi==null)
      {
        (mi, args) = await FindMatchingActionAsync(target, work, action).ConfigureAwait(false);
      }


      if (mi==null)
        throw new HTTPStatusException(WebConsts.STATUS_404,
                                      WebConsts.STATUS_404_DESCRIPTION,
                                      StringConsts.MVC_CONTROLLER_ACTION_UNMATCHED_HANDLER_ERROR.Args(target.GetType().FullName, action));

      Security.Permission.AuthorizeAndGuardAction(App.SecurityManager, mi, work.Session, () => work.NeedsSession());

      object result = null;
      bool handled = false;
      try
      {
        try
        {
          (handled, result) = await target.BeforeActionInvocationAsync(work, action, mi, args, result).ConfigureAwait(false);
          if (!handled)
          {
            result = mi.Invoke(target, args);

            //-----------------
            // 20190303 DKh temp code until Wave is refactored to full async pipeline
            //-----------------
            if (result is Task task)
            {
              await task.ConfigureAwait(false);//20220802 #479

              var taskResult = task.TryGetCompletedTaskResultAsObject();
              if (taskResult.ok) result = taskResult.result;//unwind the result
              else if (task.IsCanceled) result = null;
              else if (task.IsFaulted) throw task.Exception;
            }
            //-----------------


            (handled, result) = await target.AfterActionInvocationAsync(work, action, mi, args, result).ConfigureAwait(false);
          }
        }
        finally
        {
          result = await target.ActionInvocationFinally(work, action, mi, args, result).ConfigureAwait(false);
        }
      }
      catch(Exception error)
      {
        if (error is TargetInvocationException tie)
        {
          var cause = tie.InnerException;
          if (cause!=null) error = cause;
        }

        if (error is AggregateException age)
        {
          var cause = age.Flatten().InnerException;
          if (cause != null) error = cause;
        }
        throw MvcActionException.WrapActionBodyError(target.GetType().FullName, action, error);
      }

      //20200610 DKh if (mi.ReturnType == typeof(void)) return;
      if (mi.ReturnType==typeof(void) || mi.ReturnType==typeof(Task)) return;

      try
      {
        try
        {
          await ProcessResultAsync(target, work, result).ConfigureAwait(false);
        }
        catch(Exception error)
        {
          throw MvcActionException.WrapActionResultError(target.GetType().FullName, action, result, error);
        }
      }
      finally
      {
        await DisposeIfDisposableAndNullAsync(ref result).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Handles the error by re-throwing MVCException with wrapped inner exception.
    /// This method must NOT include any stack trace as text because it indicates system problems.
    /// Use Debug log destination (which prints inner stack traces) if more details are needed
    /// </summary>
    protected override void DoError(WorkContext work, Exception error)
    {
      if (error is MvcException) throw error;

      throw new MvcException(StringConsts.MVC_HANDLER_WORK_PROCESSING_ERROR.Args(error.ToMessageWithType()), error);
    }

    /// <summary>
    /// Gets name of MVC action from work and controller. Controller may override name of variable
    /// </summary>
    protected virtual string GetActionName(Controller controller, WorkContext work)
    {
      var action = work.MatchedVars[controller.ActionVarName].AsString();
      if (action.IsNullOrWhiteSpace()) action = controller.DefaultActionName;
      return action;
    }

    /// <summary>
    /// Finds matching method that has the specified action name and best matches the supplied input
    /// </summary>
    protected virtual async Task<(MethodInfo, object[])> FindMatchingActionAsync(Controller controller, WorkContext work, string action)
    {
      var tp = controller.GetType();

      var cInfo = m_Controllers[ControllerInfo.TypeToKeyName(tp)]; //Lock free lookup

      if (cInfo==null)
      {
        cInfo = new ControllerInfo(tp);
        m_Controllers.Register(cInfo);
      }


      var gInfo = cInfo.Groups[action];

      if (gInfo==null) //action unknown
      {
        throw new HTTPStatusException(WebConsts.STATUS_404,
                                      WebConsts.STATUS_404_DESCRIPTION,
                                      StringConsts.MVC_CONTROLLER_ACTION_UNKNOWN_ERROR.Args(tp.DisplayNameWithExpandedGenericArgs(), action));
      }

      foreach(var ai in gInfo.Actions)
        foreach(var match in ai.Attribute.Matches)
        {
          var matched = match.Make(work);
          if (matched != null)
          {
            var attr = ai.Attribute;
            var result = ai.Method;

            //set json options before body is read by BindParameters
            work.JsonOptions = ai.JsonOptions ?? cInfo.JsonOptions;

            var args = await BindParametersAsync(controller, action, attr, result, work);

            return (result, args);
          }
        }

      return (null, null);
    }

    /// <summary>
    /// Fills method invocation param array with args doing some interpretation for widely used types like JSONDataMaps, DataDocs etc..
    /// </summary>
    protected virtual async Task<object[]> BindParametersAsync(Controller controller, string action, ActionBaseAttribute attrAction, MethodInfo method,  WorkContext work)
    {
      var mpars = method.GetParameters();
      var args = new object[mpars.Length];

      if (mpars.Length == 0) return null;

      var requested = await work.GetWholeRequestAsJsonDataMapAsync().ConfigureAwait(false);

      var strictParamBinding = attrAction.StrictParamBinding;

      //check for complex type
      for(var i = 0; i < mpars.Length; i++)
      {
        var mp = mpars[i];
        var ctp = mp.ParameterType;
        var got = requested[mp.Name];

        if (ctp==typeof(object) || typeof(IDictionary<string, object>).IsAssignableFrom(ctp))
        {
          args[i] = got;
          continue;
        }

        if (typeof(TypedDoc).IsAssignableFrom(ctp) && got is JsonDataMap docMap)
        {
          try
          {
            args[i] = JsonReader.ToDoc(ctp, docMap);
            continue;
          }
          catch(Exception error)
          {
            throw new HTTPStatusException(WebConsts.STATUS_400,
                                          WebConsts.STATUS_400_DESCRIPTION,
                                          error.ToMessageWithType(),
                                          error);
          }
        }
      }//for

      //go arg by arg
      for(var i = 0; i < args.Length; i++)
      {
        if (args[i] != null) continue;

        var mp = mpars[i];

        var got = requested[mp.Name];

        if (got == null)
        {
          if (mp.HasDefaultValue) args[i] = mp.DefaultValue;
          continue;
        }

        if (got is byte[])
        {
          if (mp.ParameterType==typeof(byte[]))
          {
            args[i] = got;
            continue;
          }
          if (mp.ParameterType==typeof(Stream) || mp.ParameterType==typeof(MemoryStream))
          {
            args[i] = new MemoryStream((byte[])got, false);
            continue;
          }
          if (strictParamBinding)
            throw new HTTPStatusException(WebConsts.STATUS_400,
                                      WebConsts.STATUS_400_DESCRIPTION,
                                      StringConsts.MVC_CONTROLLER_ACTION_PARAM_BINDER_ERROR
                                                  .Args(
                                                        controller.GetType().DisplayNameWithExpandedGenericArgs(),
                                                        "strict",
                                                        action,
                                                        mp.Name,
                                                        mp.ParameterType.DisplayNameWithExpandedGenericArgs(), "byte[]" ));
        }//got byte[]

        var strVal = got.AsString();
        try
        {
          args[i] = strVal.AsType(mp.ParameterType, strictParamBinding);
        }
        catch
        {
          const int MAX_LEN = 30;
          if (strVal.Length>MAX_LEN) strVal = strVal.Substring(0, MAX_LEN)+"...";
          throw new HTTPStatusException(WebConsts.STATUS_400,
                                        WebConsts.STATUS_400_DESCRIPTION,
                                      StringConsts.MVC_CONTROLLER_ACTION_PARAM_BINDER_ERROR
                                                  .Args(
                                                        controller.GetType().DisplayNameWithExpandedGenericArgs(),
                                                        strictParamBinding ? "strict" : "relaxed",
                                                        action,
                                                        mp.Name,
                                                        mp.ParameterType.DisplayNameWithExpandedGenericArgs(), strVal ));
        }
      }//for args

      return args;
    }

    /// <summary>
    /// Turns result object into appropriate response
    /// </summary>
    protected virtual async Task ProcessResultAsync(Controller controller, WorkContext work, object result)
    {
      if (result==null) return;

      if (result is string txtresult)
      {
        await work.Response.WriteAsync(txtresult).ConfigureAwait(false);
        return;
      }

      if (result is WaveTemplate tpl)
      {
        if (!tpl.CanReuseInstance)
          App.DependencyInjector.InjectInto(tpl);

        tpl.Render(work, App);
        return;
      }

      if (result is Image img)
      {
        work.Response.ContentType = ContentType.PNG;
        var stream = await work.Response.GetDirectOutputStreamForWritingAsync().ConfigureAwait(false);
        img.Save(stream, PngImageFormat.Standard);
        return;
      }

      if (result is IActionResult aresult)
      {
        await aresult.ExecuteAsync(controller, work).ConfigureAwait(false);
        return;
      }

      //#874 20230605 DKh
      if (work.RequestedBixon)
      {
        await work.Response.WriteBixonAsync(result, JsonWritingOptions.CompactRowsAsMap).ConfigureAwait(false);
        return;
      }

      //default serialize object as JSON
      await work.Response.WriteJsonAsync(result, JsonWritingOptions.CompactRowsAsMap).ConfigureAwait(false);
    }
    #endregion
  }
}
