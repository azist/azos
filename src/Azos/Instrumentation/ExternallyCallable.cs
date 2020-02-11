using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Security;

namespace Azos.Instrumentation
{

  public interface IExternalCallHandler
  {
    ExternalCallResponse HandleRequest(IConfigSectionNode request);

    IEnumerable<Type> SupportedRequestTypes { get; }
  }

  public class ExternalCallHandler<TContext> : IExternalCallHandler where TContext : class
  {
    public ExternalCallHandler(IApplication app, TContext context, params Type[] supportedRequestTypes)
    {
      m_App = app.NonNull(nameof(app));
      Context = context.NonNull(nameof(context));
      m_SupportedRequestTypes = supportedRequestTypes.NonNull(nameof(supportedRequestTypes))
                                              .ForEach(t => t.IsOfType<ExternalCallRequest<TContext>>());
    }

    private IApplication m_App;
    protected IEnumerable<Type> m_SupportedRequestTypes;

    public IApplication App => m_App;
    public TContext Context { get; private set; }

    public IEnumerable<Type> SupportedRequestTypes => m_SupportedRequestTypes;


    public virtual ExternalCallResponse DescribeRequest(IConfigSectionNode request)
     => DoProcessRequest(request, false);

    public virtual ExternalCallResponse HandleRequest(IConfigSectionNode request)
     => DoProcessRequest(request, true);

    protected virtual ExternalCallResponse DoProcessRequest(IConfigSectionNode request, bool execute)
    {
      if (request == null || !request.Exists) return null;//unknown request
      var tr = SupportedRequestTypes.FirstOrDefault(t => t.Name.EqualsOrdIgnoreCase(request.Name));
      if (tr == null) return null;//unknown

      //Execution demands handler authorization
      if (execute)
      {
        Permission.AuthorizeAndGuardAction(App, tr);
      }

      var handler = (Activator.CreateInstance(tr, Context) as ExternalCallRequest<TContext>).NonNull(
                    "Type reg error for `{0}` is not `{1}`".Args(
                         GetType().DisplayNameWithExpandedGenericArgs(),
                         typeof(ExternalCallRequest<TContext>).DisplayNameWithExpandedGenericArgs()));

      handler.Configure(request);



      var response = execute ? handler.Execute() : handler.Describe();

      return response;
    }
  }

  public abstract class ExternalCallRequest<TContext> : IConfigurable where TContext : class
  {
    protected ExternalCallRequest(TContext context)
    {
      Context = context.NonNull(nameof(context));
    }

    public TContext Context { get; private set; }

    public abstract ExternalCallResponse Execute();

    public abstract ExternalCallResponse Describe();


    public virtual void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }

  public sealed class ExternalCallResponse
  {
    public int    StatusCode        { get; private set; }
    public int    StatusDescription { get; private set; }

    public string Content           { get; private set; }
    public string ContentType       { get; private set; }
  }

  /// <summary>
  /// Denotes entities that handle external calls - a form of RPC used for component administration.
  /// An implementer takes a request in a form of a structured config vector having its root node set to command name
  /// and returns an ExternalCallResponse object. This API is purposely sync-only for simplicity and command handlers
  /// should not take long time to execute. Keep in mind, that this mechanism should NOT be used for regular
  /// component operations, such as business operations, instead this should only be used for remote administration
  /// to unify the instrumentation interface.
  /// </summary>
  /// <remarks>
  /// This interface is a form of inversion of control used in Sky cmdlets: instead of implementing different cmdlets
  /// each coupled to specific component types, the general purpose `cman` cmdlet can delegate component-specific admin
  /// behavior into that component type
  /// </remarks>
  public interface IExternallyCallable
  {
    /// <summary>
    /// Returns an implementation of IExternalCallHandler initialized in the context of this entity
    /// </summary>
    IExternalCallHandler GetExternalCallHandler();
  }
}
