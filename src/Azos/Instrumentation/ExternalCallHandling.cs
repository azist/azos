/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Security;

namespace Azos.Instrumentation
{

  /// <summary>
  /// Describes an entity which handles external calls in a specified context.
  /// The typical use pattern is based on ExternalCallHandler&lt;TContext&gt; implementation
  /// which delegates the requests into specific instances of ExternalCallRequest
  /// </summary>
  /// <remarks>
  /// Note: this interface is designed for system administration needs and should not be used as an implementation technique of
  /// business domain logic. Consequently, the model is constrained to console-like interaction.
  /// </remarks>
  public interface IExternalCallHandler
  {
    /// <summary>
    /// Generates a help/description response for the specified external call request type
    /// </summary>
    ExternalCallResponse DescribeRequest(Type request);

    /// <summary>
    /// Generates a help/description response for the specified external call request type
    /// </summary>
    ExternalCallResponse DescribeRequest(IConfigSectionNode request);

    /// <summary>
    /// Dispatches the requested external call into an appropriate request handler which handles the request
    /// and returns the response object.  Returns null if the requested call could not be matched to its handler (e.g. unknown command)
    /// </summary>
    ExternalCallResponse HandleRequest(IConfigSectionNode request);

    /// <summary>
    /// Returns a list of request handler types supported. You can use this to output "help" page or provide custom
    /// tooling around external calls supported
    /// </summary>
    IEnumerable<Type> SupportedRequestTypes { get; }
  }


  /// <summary>
  /// Provides a default implementation of IExternalCallHandler in a TContext.
  /// You typically allocate this class in .ctor of the class/component that implements IExtrenallyCallable interface
  /// returning this instance. The .ctor takes an open array of supported command handler types
  /// which need to be of ExternalCallRequest&lt;TContext&gt; sub type.
  /// </summary>
  /// <typeparam name="TContext">The type of the allocation context, which is typically the target of external call</typeparam>
  public class ExternalCallHandler<TContext> : IExternalCallHandler where TContext : class
  {
    /// <summary>
    /// Creates handler instance in the scope of application needed for authorization, and TContext.
    /// </summary>
    /// <param name="app">Application context for DI and authorization</param>
    /// <param name="context">Call target context</param>
    /// <param name="parent">Parent handler which gets called if this one could not handle the request or null</param>
    /// <param name="supportedRequestTypes">An array of types which this handler recognizes/ Must be of ExternalCallRequest&lt;TContext&gt; subtype</param>
    public ExternalCallHandler(IApplication app, TContext context, IExternalCallHandler parent, params Type[] supportedRequestTypes)
    {
      m_App = app.NonNull(nameof(app));
      Context = context.NonNull(nameof(context));
      m_ParentHandler = parent;
      m_SupportedRequestTypes = supportedRequestTypes.NonNull(nameof(supportedRequestTypes))
                                              .ForEach(t => t.IsOfType<ExternalCallRequest<TContext>>());
    }

    private IApplication m_App;
    protected IEnumerable<Type> m_SupportedRequestTypes;
    private IExternalCallHandler m_ParentHandler;

    /// <summary>
    /// Application context
    /// </summary>
    public IApplication App => m_App;

    /// <summary>
    /// Target call context
    /// </summary>
    public TContext Context { get; private set; }

    /// <summary>
    /// Chained call handler which will be called for requests which could not be handled by this one or null
    /// </summary>
    public IExternalCallHandler Parent => m_ParentHandler;

    /// <summary>
    /// Types which this handler recognizes for processing
    /// </summary>
    public IEnumerable<Type> SupportedRequestTypes => m_SupportedRequestTypes;

    /// <summary>
    /// Describes request type
    /// </summary>
    public virtual ExternalCallResponse DescribeRequest(Type request)
      => DescribeRequest((request.NonNull(nameof(request)).Name + "{ }").AsLaconicConfig());

     /// <summary>
     /// Describes request vector
     /// </summary>
    public virtual ExternalCallResponse DescribeRequest(IConfigSectionNode request)
    {
      var result = DoProcessRequest(request, false);

      if (result == null && m_ParentHandler != null)
        result = m_ParentHandler.DescribeRequest(request);

      return result;
    }

    /// <summary>
    /// Handles (executes) the request. returns null for unknown/unrecognized/unsupported request type
    /// </summary>
    public virtual ExternalCallResponse HandleRequest(IConfigSectionNode request)
    {
      var result = DoProcessRequest(request, true);

      if (result == null && m_ParentHandler != null)
        result = m_ParentHandler.HandleRequest(request);

      return result;
    }

    /// <summary>
    /// Override to process request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="execute">Execute vs Describe</param>
    /// <returns>Response object or null if request is unmatched/unsupported</returns>
    protected virtual ExternalCallResponse DoProcessRequest(IConfigSectionNode request, bool execute)
    {
      if (request == null || !request.Exists) return null;//unknown request

      //Find the matching handler type out of SUPPORTED (no type injection possible)
      var tr = SupportedRequestTypes.FirstOrDefault(t => t.Name.EqualsOrdIgnoreCase(request.Name));// command-name{ ... }
      if (tr == null) return null;//unknown

      //Execution MANDATES handler authorization of the calling principal
      if (execute)
      {
        Permission.AuthorizeAndGuardAction(App, tr);
      }

      //Create instance of the appropriate handler
      var handler = (Activator.CreateInstance(tr, Context) as ExternalCallRequest<TContext>).NonNull(
                    "Type reg error for `{0}` is not `{1}`".Args(
                         GetType().DisplayNameWithExpandedGenericArgs(),
                         typeof(ExternalCallRequest<TContext>).DisplayNameWithExpandedGenericArgs()));

      //perform DI of app container
      App.InjectInto(handler);

      //configure handler from request properties
      handler.Configure(request);

      //Execute or Describe (which is a form of execution with a different purpose)
      var response = execute ? handler.Execute() : handler.Describe();

      return response;
    }
  }

  /// <summary>
  /// Provides general base for individual call handlers dispatched by ExternalCallHandler
  /// </summary>
  public abstract class ExternalCallRequest<TContext> : IConfigurable where TContext : class
  {
    protected ExternalCallRequest(TContext context)
    {
      Context = context.NonNull(nameof(context));
    }

    /// <summary>
    /// The target call context
    /// </summary>
    public TContext Context { get; private set; }

    /// <summary>
    /// Override to execute the call request
    /// </summary>
    /// <returns></returns>
    public abstract ExternalCallResponse Execute();

    /// <summary>
    /// Override to generate help/descriptive response
    /// </summary>
    public abstract ExternalCallResponse Describe();

    /// <summary>
    /// Override to configure, default applies Config attribute
    /// </summary>
    public virtual void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);
  }

  /// <summary>
  /// Embodies a response for ExternalCall. This is purposely a sealed class for simplicity.
  /// Keep in mind that external calls should NOT be used as implementation technique of business domain logic.
  /// The model is consequently constrained on purpose to prevent possible abuse.
  /// </summary>
  public sealed class ExternalCallResponse
  {
    /// <summary>
    /// Creates an OK/200 response
    /// </summary>
    public ExternalCallResponse(string contentType, string content)
    {
      StatusCode = 200;
      StatusDescription = "OK";
      ContentType = contentType;
      Content = content;
    }

    /// <summary>
    /// Instance with the specified status
    /// </summary>
    public ExternalCallResponse(int status, string statusDescription, string contentType, string content)
    {
      StatusCode = status;
      StatusDescription = statusDescription;
      ContentType = contentType;
      Content = content;
    }

    /// <summary>
    /// Although this mechanism is not built specifically for web, it uses HTTP return codes by convention
    /// </summary>
    public int    StatusCode        { get; private set; }

    /// <summary>
    /// Status description similar to Http status phrase
    /// </summary>
    public string StatusDescription { get; private set; }

    /// <summary>
    /// Content of ContentType
    /// </summary>
    public string Content           { get; private set; }

    /// <summary>
    /// Content type uses MIME types by convention
    /// </summary>
    public string ContentType       { get; private set; }
  }


}
