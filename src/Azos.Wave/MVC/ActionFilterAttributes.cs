/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using Azos.Conf;

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
  [CustomMetadata(@"filter{
   name='HttpGet'
   title='Filters GET requests'
   description='Returns 405 if request method is not GET'
  }")]
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
  [CustomMetadata(@"filter{
   name='HttpPost'
   title='Filters POST requests'
   description='Returns 405 if request method is not POST'
  }")]
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
  [CustomMetadata(@"filter{
   name='HttpPut'
   title='Filters PUT requests'
   description='Returns 405 if request method is not PUT'
  }")]
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
  [CustomMetadata(@"filter{
   name='HttpDelete'
   title='Filters DELETE requests'
   description='Returns 405 if request method is not DELETE'
  }")]
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
  [CustomMetadata(@"filter{
   name='HttpPatch'
   title='Filters PATCH requests'
   description='Returns 405 if request method is not PATCH'
  }")]
  public sealed class HttpPatchAttribute : BeforeActionFilterBaseAttribute
  {
    public HttpPatchAttribute() { }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.IsPATCH)
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
  [CustomMetadata(@"filter{
   name='HttpPutOrPost'
   title='Filters PUT or POST requests'
   description='Returns 405 if request method is not PUT or POST'
  }")]
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
  [CustomMetadata(@"filter{
   name='AcceptsJson'
   title='Filters requests that contain Accept JSON header'
   description='Returns 406 is Accept header was not set to `application/json`'
  }")]
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


  /// <summary>
  /// Only allows requests that have entity body (e.g. POST payload).
  /// If MaxContentLength limit is set then the 'Content-Length' header is required and its value is compared against the limit
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  [CustomMetadata(@"filter{
   name='HasEntityBody'
   title='Filters requests that have entity body (e.g. POST payload)'
   description='Returns 400 if there is no body or 413 if entity is too large when MaxContentLength is set'
  }")]
  public sealed class HasEntityBodyAttribute : BeforeActionFilterBaseAttribute
  {
    public HasEntityBodyAttribute() { }

    /// <summary>
    /// When set to >0 value requires the 'Content-Length' header and imposes a limit on its value
    /// </summary>
    public long MaxContentLength { get; set; }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (!work.Request.HasEntityBody)
      {
        work.Response.StatusCode = WebConsts.STATUS_400;
        work.Response.StatusDescription = WebConsts.STATUS_400_DESCRIPTION;
        return true;
      }

      if (MaxContentLength<=0) return false;

      var got = work.Request.ContentLength64;
      if (got<0)
      {
        work.Response.StatusCode = WebConsts.STATUS_411;
        work.Response.StatusDescription = WebConsts.STATUS_411_DESCRIPTION;
        return true;
      }

      if (got > MaxContentLength)
      {
        work.Response.StatusCode = WebConsts.STATUS_413;
        work.Response.StatusDescription = WebConsts.STATUS_413_DESCRIPTION;
        return true;
      }

      return false;
    }

    public override bool ShouldProvideInstanceMetadata(IMetadataGenerator context, ConfigSectionNode dataRoot)
      => MaxContentLength > 0;

    public override ConfigSectionNode ProvideInstanceMetadata(IMetadataGenerator context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
    {
      dataRoot.AddAttributeNode("max-content-length", MaxContentLength);
      return dataRoot;
    }
  }

}
