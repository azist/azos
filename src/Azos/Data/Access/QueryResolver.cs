/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Azos.Conf;

namespace Azos.Data.Access
{
    /// <summary>
    /// Infrastructure class - not for app developers.
    /// Resolves Query objects into query handlers. Query names are case-insensitive.
    /// This class is thread-safe
    /// </summary>
    public sealed class QueryResolver : ICRUDQueryResolver, IConfigurable
    {
        #region CONSTS
            public const string CONFIG_QUERY_RESOLVER_SECTION = "query-resolver";
            public const string CONFIG_HANDLER_LOCATION_SECTION = "handler-location";
            public const string CONFIG_SCRIPT_ASM_ATTR = "script-assembly";
            public const string CONFIG_NS_ATTR = "ns";
        #endregion

        #region .ctor
            public QueryResolver(ICRUDDataStoreImplementation dataStore)
            {
                m_DataStore = dataStore;
            }
        #endregion

        #region Fields
            private volatile bool m_Started;

            private string m_ScriptAssembly;
            private ICRUDDataStoreImplementation m_DataStore;
            private List<string> m_Locations = new List<string>();
            private Collections.Registry<CRUDQueryHandler> m_Handlers = new Collections.Registry<CRUDQueryHandler>();

        #endregion


        #region Properties

            /// <summary>
            /// Gets sets name of assembly that query scripts resolve from
            /// </summary>
            public string ScriptAssembly
            {
                get { return m_ScriptAssembly;}
                set
                {
                  checkNotStarted();
                  m_ScriptAssembly = value;
                }
            }

            public IList<string> HandlerLocations
            {
                get { return m_Locations;}
            }

            public Collections.IRegistry<CRUDQueryHandler> Handlers
            {
                get { return m_Handlers;}
            }

        #endregion


        #region Public

            /// <summary>
            /// Registers handler location. The Resolver must be not started yet. This method is NOT thread safe
            /// </summary>
            public void RegisterHandlerLocation(string location)
            {
              checkNotStarted();
              if (location.IsNullOrWhiteSpace() || m_Locations.Contains(location, StringComparer.InvariantCultureIgnoreCase )) return;
              m_Locations.Add(location);
            }

            /// <summary>
            /// Unregisters handler location returning true if it was found and removed. The Resolve must be not started yet. This method is NOT thread safe
            /// </summary>
            public bool UnregisterHandlerLocation(string location)
            {
              checkNotStarted();
              if (location.IsNullOrWhiteSpace()) return false;
              return m_Locations.RemoveAll((s) => s.EqualsIgnoreCase(location)) > 0;
            }


            public CRUDQueryHandler Resolve(Query query)
            {
                m_Started = true;
                var name = query.Name;
                try
                {
                    var result = m_Handlers[name];
                    if (result!=null) return result;

                    result = searchForType(name);

                    if (result==null)//did not find handler yet
                        result = searchForScript(name);

                    if (result==null)//did not find handler yet
                        throw new DataAccessException(StringConsts.CRUD_QUERY_RESOLUTION_NO_HANDLER_ERROR);

                    m_Handlers.Register(result);
                    return result;
                }
                catch(Exception error)
                {
                    throw new DataAccessException(StringConsts.CRUD_QUERY_RESOLUTION_ERROR.Args(name, error.ToMessageWithType()), error);
                }
            }

            public void Configure(IConfigSectionNode node)
            {
                checkNotStarted();

                m_ScriptAssembly = node.AttrByName(CONFIG_SCRIPT_ASM_ATTR).ValueAsString( Assembly.GetCallingAssembly().FullName );
                foreach(var lnode in node.Children.Where(cn => cn.IsSameName(CONFIG_HANDLER_LOCATION_SECTION)))
                {
                  var loc = lnode.AttrByName(CONFIG_NS_ATTR).Value;
                  if (loc.IsNotNullOrWhiteSpace())
                    m_Locations.Add( loc );
                  else
                    App.Log.Write(Log.MessageType.Warning, StringConsts.CRUD_CONFIG_EMPTY_LOCATIONS_WARNING, "CRUD", "QueryResolver.Configure()");
                }

            }

        #endregion

        #region .pvt

            private void checkNotStarted()
            {
                if (m_Started)
                 throw new DataAccessException(StringConsts.CRUD_QUERY_RESOLVER_ALREADY_STARTED_ERROR);
            }


            private CRUDQueryHandler searchForType(string name)
            {
                foreach(var loc in m_Locations)
                {
                    var ns = loc;
                    var asm = string.Empty;
                    var ic = loc.IndexOf(',');
                    if(ic>0)
                    {
                        ns = loc.Substring(0, ic);
                        asm = loc.Substring(ic+1);
                    }

                    var tname = asm.IsNullOrWhiteSpace() ? "{0}.{1}".Args(ns, name) : "{0}.{1}, {2}".Args(ns, name, asm);
                    var t = Type.GetType(tname, false, true);
                    if (t!=null)
                    {
                        if (typeof(CRUDQueryHandler).IsAssignableFrom(t))
                        {
                            return Activator.CreateInstance(t, m_DataStore, name) as CRUDQueryHandler;
                        }
                    }
                }
                return null;
            }

            private CRUDQueryHandler searchForScript(string name)
            {
                var asm = Assembly.Load(m_ScriptAssembly);
                var asmname = asm.FullName;
                var ic = asmname.IndexOf(',');
                if (ic>0)
                 asmname = asmname.Substring(0, ic);
                var resources = asm.GetManifestResourceNames();

                var resName = name + m_DataStore.ScriptFileSuffix;

                var res = resources.FirstOrDefault(r => r.EqualsIgnoreCase(resName) || r.EqualsIgnoreCase(asmname+"."+resName));

                if (res!=null)
                {
                    using (var stream = asm.GetManifestResourceStream(res))
                      using (var reader = new StreamReader(stream))
                      {
                         var script = reader.ReadToEnd();
                         var qsource = new QuerySource(name, script);
                         return m_DataStore.MakeScriptQueryHandler(qsource);
                      }
                }
                return null;
            }

        #endregion
    }


}
