/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// General ancestor for Before Action Filters
  /// </summary>
  public abstract class BeforeActionFilterBaseAttribute : ActionFilterAttribute
  {
    /// <summary>
    /// Override to add logic/filtering right after the invocation of action method. Must return TRUE to stop processing chain
    /// </summary>
    protected internal override bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
      => false;

    /// <summary>
    /// Override to add logic/filtering finally after the invocation of action method. Must return TRUE to stop processing chain
    /// </summary>
    protected internal override void ActionInvocationFinally(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
    }
  }

  /// <summary>
  /// Only allows GET requests
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class HttpGetAttribute : BeforeActionFilterBaseAttribute
  {
    public HttpGetAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.IsGET)
      {
        work.Response.StatusCode = WebConsts.STATUS_405;
        work.Response.StatusDescription = WebConsts.STATUS_405_DESCRIPTION;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Only allows POST requests
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class HttpPostAttribute : BeforeActionFilterBaseAttribute
  {
    public HttpPostAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.IsPOST)
      {
        work.Response.StatusCode = WebConsts.STATUS_405;
        work.Response.StatusDescription = WebConsts.STATUS_405_DESCRIPTION;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Only allows PUT requests
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class HttpPutAttribute : BeforeActionFilterBaseAttribute
  {
    public HttpPutAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.IsPUT)
      {
        work.Response.StatusCode = WebConsts.STATUS_405;
        work.Response.StatusDescription = WebConsts.STATUS_405_DESCRIPTION;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Only allows DELETE requests
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class HttpDeleteAttribute : BeforeActionFilterBaseAttribute
  {
    public HttpDeleteAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.IsDELETE)
      {
        work.Response.StatusCode = WebConsts.STATUS_405;
        work.Response.StatusDescription = WebConsts.STATUS_405_DESCRIPTION;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Only allows PATCH requests
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class HttpPatchAttribute : BeforeActionFilterBaseAttribute
  {
    public HttpPatchAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.IsDELETE)
      {
        work.Response.StatusCode = WebConsts.STATUS_405;
        work.Response.StatusDescription = WebConsts.STATUS_405_DESCRIPTION;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Only allows PUT or POST requests
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class HttpPutOrPostAttribute : BeforeActionFilterBaseAttribute
  {
    public HttpPutOrPostAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.IsPUT && !work.IsPOST)
      {
        work.Response.StatusCode = WebConsts.STATUS_405;
        work.Response.StatusDescription = WebConsts.STATUS_405_DESCRIPTION;
        return true;
      }
      return false;
    }
  }

  /// <summary>
  /// Only allows requests that contain Accept application/json header
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class AcceptsJsonAttribute : BeforeActionFilterBaseAttribute
  {
    public AcceptsJsonAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.RequestedJSON)
      {
        work.Response.StatusCode = WebConsts.STATUS_406;
        work.Response.StatusDescription = WebConsts.STATUS_406_DESCRIPTION;
        return true;
      }
      return false;
    }
  }

}
