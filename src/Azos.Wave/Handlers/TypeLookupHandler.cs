/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.IO;
using System.Reflection;

using Azos.Data;
using Azos.Conf;
using Azos.Text;

namespace Azos.Wave.Handlers
{
    /// <summary>
    /// Represents a base handler for all handlers that dynamically resolve type that performs actual work
    /// </summary>
    public abstract class TypeLookupHandler<TTarget> : WorkHandler where TTarget : class
    {
       #region CONSTS
         public const string VAR_TARGET_TYPE = "type";
         public const string VAR_INSTANCE_ID = "instanceID";

         public const string CONFIG_DEFAULT_TYPE_ATTR = "default-type";
         public const string CONFIG_CLOAK_TYPE_ATTR = "cloak-type";
         public const string CONFIG_NOT_FOUND_REDIRECT_URL_ATTR = "not-found-redirect-url";

       #endregion


       #region .ctor

         protected TypeLookupHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                              : base(dispatcher, name, order, match)
         {

         }

         protected TypeLookupHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
         {
          if (confNode==null)
            throw new WaveException(StringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(confNode==null)");

          foreach(var ntl in confNode.Children.Where(cn=>cn.IsSameName(TypeLocation.CONFIG_TYPE_LOCATION_SECTION)))
            m_TypeLocations.Register( FactoryUtils.Make<TypeLocation>(ntl, typeof(TypeLocation), args: new object[]{ ntl }) );

          m_DefaultTypeName = confNode.AttrByName(CONFIG_DEFAULT_TYPE_ATTR).Value;
          m_CloakTypeName = confNode.AttrByName(CONFIG_CLOAK_TYPE_ATTR).Value;
          m_NotFoundRedirectURL = confNode.AttrByName(CONFIG_NOT_FOUND_REDIRECT_URL_ATTR).Value;
         }

       #endregion

       #region Fields

         private string m_DefaultTypeName;
         private string m_CloakTypeName;
         private string m_NotFoundRedirectURL;
         private volatile TypeLookup m_Lookup = new TypeLookup();
         private TypeLocations m_TypeLocations = new TypeLocations();

       #endregion


       #region Properties

            /// <summary>
            /// Indicates whether instance IDs are supported in requests. Default is false.
            /// Override to return true for handlers that support target instance state between requests
            /// </summary>
            public virtual bool SupportsInstanceID
            {
              get { return false; }
            }

            /// <summary>
            /// Returns a registry of type locations
            /// </summary>
            public TypeLocations TypeLocations
            {
              get { return m_TypeLocations;}
            }

            /// <summary>
            /// Provides default type name
            /// </summary>
            public string DefaultTypeName
            {
              get { return m_DefaultTypeName ?? string.Empty;}
              set { m_DefaultTypeName = value;}
            }

            /// <summary>
            /// Provides type name which is used if the prior one was not found. This allows to block 404 errors,
            /// i.e. if page with requested name is not found then always return the specified page
            /// </summary>
            public string CloakTypeName
            {
              get { return m_CloakTypeName ?? string.Empty;}
              set { m_CloakTypeName = value;}
            }

            /// <summary>
            /// Provides redirect URL where the user gets redirected when type name could not be resolved.
            /// Note: CloakTypeName is used first when set.
            /// </summary>
            public string NotFoundRedirectURL
            {
              get { return m_NotFoundRedirectURL ?? string.Empty;}
              set { m_NotFoundRedirectURL = value;}
            }


       #endregion


       #region Protected

    /// <summary>
    /// Sealed. Override DoTargetWork(TTarget, WorkContext) to do actual work
    /// </summary>
    protected sealed override void DoHandleWork(WorkContext work)
    {
      Exception error = null;

      TTarget target = null;

      try
      {
        try
        {
          var tt = GetTargetType(work);

          if (tt==null || tt.IsAbstract)
          {
            if (m_NotFoundRedirectURL.IsNotNullOrWhiteSpace())
            {
              work.Response.RedirectAndAbort(m_NotFoundRedirectURL);
              return;
            }

            error = Do404(work);
            return;
          }

          Security.Permission.AuthorizeAndGuardAction(App, tt, work.Session, () => work.NeedsSession() );

          target = CreateTargetInstance(work, tt);

          if (target==null)
          {
            error = Do404(work);
            return;
          }

          DoTargetWork(target, work);

          if (target is IDisposable dtarget)
            dtarget.Dispose();
        }
        catch(Exception err)
        {
          error = err;
        }
      }
      finally
      {
        if (error!=null)
          DoError(work, error);
      }
    }



           /// <summary>
           /// Performs work on the target instance
           /// </summary>
           protected abstract void DoTargetWork(TTarget target, WorkContext work);



           /// <summary>
           /// Override to resolve route/URL parameters to type
           /// </summary>
           protected virtual Type GetTargetType(WorkContext work)
           {
             var tname = GetTargetTypeNameFromWorkContext(work);

             var result = getTargetType(work, tname);
             if(result!=null && !result.IsAbstract) return result;

             if (m_CloakTypeName.IsNotNullOrWhiteSpace())
              result = getTargetType(work, m_CloakTypeName);

             return result;
           }

           /// <summary>
           /// Override to get type name from WorkContext. Default implementation looks for MatchedVars[VAR_TARGET_TYPE]
           /// </summary>
           protected virtual string GetTargetTypeNameFromWorkContext(WorkContext work)
           {
             var result = work.MatchedVars[VAR_TARGET_TYPE].AsString();

             if (result.IsNullOrWhiteSpace())
              result = DefaultTypeName;

             //20160217 DKh
             var match = work.Match;
             if (match!=null && match.TypeNsPrefix.IsNotNullOrWhiteSpace())
             {
                var pfx = match.TypeNsPrefix;

                if (pfx[pfx.Length-1]!='/' && pfx[pfx.Length-1]!='\\') pfx = pfx + '/';

                result = pfx + result;
             }

             return result;
           }


           /// <summary>
           /// Factory method - Override to create and initialize more particular template implementation (i.e. based on model)
           /// </summary>
           protected virtual TTarget CreateTargetInstance(WorkContext work, Type tt)
           {
              return  Activator.CreateInstance(tt) as TTarget;
           }

           /// <summary>
           /// Override to handle 404 condition, i.e. may write into response instead of generating a exception.
           /// The default implementation returns a HttpStatusException with 404 code
           /// </summary>
           protected virtual HTTPStatusException Do404(WorkContext context)
           {
              return new HTTPStatusException(WebConsts.STATUS_404, WebConsts.STATUS_404_DESCRIPTION);
           }

           /// <summary>
           /// Override to handle error processing, i.e. may elect to write error data into response.
           /// The default implementation throws the error. It is recommended to handle errors with filters instead
           /// </summary>
           protected virtual void DoError(WorkContext work, Exception error)
           {
              throw error;
           }

       #endregion


       #region .pvt .impl

            private Type getTargetType(WorkContext work, string typeName)
            {
              const string PORTAL_PREFIX = @"!#PORTAL\";

              if (typeName.IsNullOrWhiteSpace()) return null;
              Type result = null;
              string key;

              if (work.Portal==null)
                key = typeName;
              else
                key = PORTAL_PREFIX + work.Portal.Name + typeName;


              //1 Lookup in cache
              if (m_Lookup.TryGetValue(key, out result)) return result;

              //2 Lookup in locations
              result = lookupTargetInLocations(work, typeName);
              if (result!=null)
              {
                var lookup = new TypeLookup(m_Lookup);//thread safe copy
                lookup[key] =  result;//other thread may have added already

                System.Threading.Thread.MemoryBarrier();
                m_Lookup = lookup;//atomic
                return result;
              }

              return null;//404 error - type not found
            }


            private Type lookupTargetInLocations(WorkContext work, string typeName)
            {
              if (!isValidTypeNameKey(typeName)) return null;

              string portal = null;
              if (work.Portal!=null)
               portal = work.Portal.Name;

              var clrTName = getCLRTypeName(typeName);

              while(true)
              {
                foreach(var loc in  m_TypeLocations.OrderedValues)
                {
                  if (portal!=null)
                  {
                    if (!portal.EqualsOrdIgnoreCase(loc.Portal)) continue;
                  }
                  else
                  {
                    if (loc.Portal.IsNotNullOrWhiteSpace()) continue;
                  }

                  var asm = loc.Assembly;
                  if (asm==null)
                    asm = Assembly.LoadFrom(loc.AssemblyName);

                  //explicit ns pattern is required
                  var namespaces = loc.Namespaces;
                  if (namespaces==null) continue;

                  var matches = asm.GetTypes()
                                   .Where(t =>
                                            t.IsPublic &&
                                           !t.IsAbstract &&
                                           !t.IsGenericTypeDefinition &&
                                            t.Name.EqualsOrdIgnoreCase(clrTName) &&
                                            namespaces.Any(ns => t.Namespace.MatchPattern(ns, senseCase: true))
                                         )
                                   .ToArray();

                  if (matches.Length==0) continue;//not found in this typelocation
                  if (matches.Length>1)
                    WriteLog(Log.MessageType.Warning,
                             nameof(lookupTargetInLocations),
                             StringConsts.TYPE_MULTIPLE_RESOLUTION_WARNING.Args(typeName, matches[0].AssemblyQualifiedName));

                  return matches[0];
                }

                if (portal == null) break;
                portal = null;//2nd iteration
              }//while


              return null;
            }


            private bool isValidTypeNameKey(string key)
            {
              if (key==null) return false;

              for(var i=0; i<key.Length; i++)
              {
                var c = key[i];
                if (char.IsLetterOrDigit(c)) continue;
                if (c=='/' || c=='\\' || c=='-' || c=='_' || c=='.') continue;
                return false;
              }

              return true;
            }

            private string getCLRTypeName(string key)
            {
              var cname = Path.GetFileNameWithoutExtension(key);
              var ns = Path.GetDirectoryName(key);

              ns = ns.Replace('/','.').Replace('\\','.').Trim('.');

              var fullName =  string.IsNullOrWhiteSpace(ns)? cname :  ns + '.'+ cname;

              return fullName.Replace('-', '_');
            }



       #endregion
    }
}
