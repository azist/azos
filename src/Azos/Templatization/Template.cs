/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Templatization
{
    /// <summary>
    /// A general template interface.
    /// A template is a class that gets instantiated at some point to Render() its content into IRenderingTarget instance.
    /// Templates are not necessarily text-based, i.e. they can be image-based or based on various kinds of binary files
    /// </summary>
    public interface ITemplate
    {
       /// <summary>
       /// Custom context for the lifetime of this template
       /// </summary>
       object Context
       {
         get;
       }


       /// <summary>
       /// Indicates whether an instance of template class may be reused for invocation of Render() more than once
       /// (possibly with different rendering target and/or rendering context)
       /// </summary>
       bool CanReuseInstance
       {
         get;
       }


       /// <summary>
       /// Renders template by generating content into target
       /// </summary>
       /// <param name="target">A target that rendering is done into</param>
       /// <param name="renderingContext">A context object for this rendering call</param>
       void Render(IRenderingTarget target, object renderingContext);
    }



    /// <summary>
    /// A general ancestor for any template. All templates derive from this class directly or indirectly.
    /// A template is a class that gets instantiated at some point to Render() its content into IRenderingTarget instance.
    /// Templates are not necessarily text-based, i.e. they can be image-based or based on various kinds of binary files
    /// </summary>
    public abstract class Template<TContext, TTarget, TRenderingContext> : ITemplate
                             where TContext : class
                             where TTarget : class,IRenderingTarget
    {

       protected Template()
       {

       }

       protected Template(TContext context)
       {
           BindGlobalContexts(context);
       }

       private TContext m_Context;

       [ThreadStatic]  private static TTarget ts_Target;
       [ThreadStatic]  private static TRenderingContext ts_RenderingContext;


       /// <summary>
       /// Custom context for the lifetime of this template
       /// </summary>
       public TContext Context
       {
         get { return m_Context; }
       }

       object ITemplate.Context
       {
         get { return m_Context; }
       }


       /// <summary>
       /// Returns thread-local target which is specific for this call to Render()
       /// </summary>
       public TTarget Target
       {
         get { return ts_Target;}
       }

       /// <summary>
       /// Returns thread-local rendering context which is specific for this call to Render()
       /// </summary>
       public TRenderingContext RenderingContext
       {
         get { return ts_RenderingContext;}
       }

       /// <summary>
       /// Indicates whether an instance of template class may be reused for invocation of Render() more than once (possibly with different rendering target and/or rendering context)
       /// </summary>
       public abstract bool CanReuseInstance
       {
         get;
       }


       /// <summary>
       /// Renders template by generating content into target
       /// </summary>
       /// <param name="target">A target to render output into</param>
       /// <param name="renderingContext">A context object for this rendering call</param>
       public void Render(TTarget target, TRenderingContext renderingContext)
       {
         if (target==null)
          throw new TemplatizationException(StringConsts.ARGUMENT_ERROR + "Render(target=null)");

         try
         {
           ts_Target = target;
           ts_RenderingContext = renderingContext;

           DoPreRender();
           DoRender();
           DoPostRender(null);
         }
         catch(Exception error)
         {
            var rethrow = DoPostRender(error);
            if (rethrow) throw;
         }
         finally
         {
            ts_Target = null;
            ts_RenderingContext = default(TRenderingContext);
         }
       }


       void ITemplate.Render(IRenderingTarget target, object renderingContext)
       {
         Render(target as TTarget, (TRenderingContext)renderingContext);
       }


       /// <summary>
       /// Infrastructure. Sets Context property. Normally this method should never be called by developers
       /// </summary>
       public void BindGlobalContexts(TContext context)
       {
         m_Context = context;
         DoContextBinding();
       }


       /// <summary>
       /// Infrastructure. Override to perform extra steps after Context property gets set.
       /// Normally this method should never be called by developers
       /// </summary>
       protected virtual void DoContextBinding()
       {

       }


       /// <summary>
       /// Performs pre-rendering actions
       /// </summary>
       protected virtual void DoPreRender() {}


       /// <summary>
       /// Performs actual rendering
       /// </summary>
       protected virtual void DoRender() {}


       /// <summary>
       /// Performs post-rendering actions. Return true to rethrow error
       /// </summary>
       protected virtual bool DoPostRender(Exception error) { return true;}


    }


}
