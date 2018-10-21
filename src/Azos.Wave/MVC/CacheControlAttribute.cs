
using System;
using System.Reflection;

using Azos.Web;

namespace Azos.Wave.MVC
{

  /// <summary>
  /// Decorates controller classes or actions that set NoCache headers in response. By default Force = true
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
  public sealed class CacheControlAttribute : ActionFilterAttribute
  {
    public CacheControlAttribute() : base(0) {}
    public CacheControlAttribute(int order) : base(order) {}

    public CacheControl.Type Cacheability { get; set; }
    public int? MaxAgeSec { get; set; }
    public int? SharedMaxAgeSec { get; set; }

    public bool NoStore { get; set; }
    public bool NoTransform { get; set; }
    public bool MustRevalidate { get; set; }
    public bool ProxyRevalidate { get; set; }

    public bool Force{ get; set; }

    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      return false;
    }

    protected internal override bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      work.Response.SetCacheControlHeaders(new CacheControl
      {
        Cacheability = Cacheability,
        MaxAgeSec = MaxAgeSec,
        SharedMaxAgeSec = SharedMaxAgeSec,
        NoStore = NoStore,
        NoTransform = NoTransform,
        MustRevalidate = MustRevalidate,
        ProxyRevalidate = ProxyRevalidate
      }, Force);
      return false;
    }

    protected internal override void ActionInvocationFinally(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
    }
  }
}
