
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Azos.Web;
using Azos.ApplicationModel;
using Azos.Templatization;


namespace Azos.Wave.Templatization
{
      /// <summary>
      /// Defines a base class for web-related templates
      /// </summary>
      public abstract class WaveTemplate : Template<WorkContext, IRenderingTarget, object>
      {
         /// <summary>
         /// Escapes JS literal, see Azos.Web.Utils.EscapeJSLiteral
         /// </summary>
         public static string EscapeJSLiteral(string value)
         {
           return value.EscapeJSLiteral();
         }

         /// <summary>
         /// Converts string to HTML-encoded string
         /// </summary>
         public static string HTMLEncode(string value)
         {
           return WebUtility.HtmlEncode(value);
         }



         public WaveTemplate() : base()
         {

         }

         public WaveTemplate(WorkContext context) : base (context)
         {

         }

         public Response Response { get { return Context.Response; } }

         /// <summary>
         /// Returns session if it is available or null
         /// </summary>
         public WaveSession Session { get { return Context.Session; } }

         /// <summary>
         /// Returns CSRF token for session if it is available or null
         /// </summary>
         public string CSRFToken
         {
           get
           {
             var session = this.Session;
             if (session==null) return null;
             return session.CSRFToken;
           }
         }

         /// <summary>
         /// Override to indicate whetner the instance of the template may be reused for processing of other requests
         /// (possibly by parallel threads). Override to return false if there is any per-request state shared in instance fields
         /// False by default so multiple requests can not reuse the instance
         /// </summary>
         public override bool CanReuseInstance
         {
             get { return false; }
         }

         /// <summary>
         /// Override to provides response content type. Default value is HTML
         /// </summary>
         public virtual string ContentType
         {
             get { return Azos.Web.ContentType.HTML; }
         }


        /// <summary>
        /// Renders template by generating content into ResponseRenderingTarget
        /// </summary>
        public void Render(ResponseRenderingTarget target, object model)
        {
          Context.Response.ContentType = ContentType;
          base.Render(target, model);
        }

        /// <summary>
        /// Renders template by generating content into WorkContext
        /// </summary>
        public void Render(WorkContext work, object model)
        {
          this.BindGlobalContexts(work);
          Render(new ResponseRenderingTarget(work), model);
        }

        /// <summary>
        /// Renders template to string
        /// </summary>
        public string RenderToString(bool encodeHtml = true, object model = null)
        {
          var target = new Azos.Templatization.StringRenderingTarget(encodeHtml);
          base.Render(target, model);
          return target.Value;
        }

        /// <summary>
        /// Renders template to string in a WorkContext
        /// </summary>
        public string RenderToString(WorkContext work, bool encodeHtml = true, object model = null)
        {
          this.BindGlobalContexts(work);
          var target = new Azos.Templatization.StringRenderingTarget(encodeHtml);
          base.Render(target, model);
          return target.Value;
        }


        protected override bool DoPostRender(Exception error)
        {
          if (error==null) return false;
          throw new WaveTemplateRenderingException("{0}.DoPostRender: {1}".Args(GetType().FullName, error.ToMessageWithType()), error);
        }

    }

    /// <summary>
    /// Wave template that is scoped to a TModel type for Model (Rendering Context)
    /// </summary>
    public class WaveTemplate<TModel> : WaveTemplate
    {
        public WaveTemplate() : base()
        {

        }

        public WaveTemplate(WorkContext context) : base (context)
        {

        }

        /// <summary>
        /// Shortcut to TemplateRenderingContext which is set per call to Render(TModel)
        /// </summary>
        public TModel Model { get { return (TModel)RenderingContext;}}

        /// <summary>
        /// Renders template by generating content into ResponseRenderingTarget
        /// </summary>
        /// <param name="target">A ResponseRenderingTarget target to render output into</param>
        /// <param name="model">A model object for this rendering call</param>
        public void Render(ResponseRenderingTarget target, TModel model)
        {
          base.Render(target, model);
        }


        /// <summary>
        /// Renders template by generating content into WorkContext
        /// </summary>
        public void Render(WorkContext work, TModel model)
        {
          Render(new ResponseRenderingTarget(work), model);
        }

    }



}
