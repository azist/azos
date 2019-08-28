using System;
using System.Linq;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Security
{
  /// <summary>
  /// Provides abstraction for permissions that operate in a context of a data store, so their ACL/rights vectors contain sub-sections.
  /// The data store context is supplied via session.DataContextName which is comprised of segments delimited by commas/spaces, each segment string
  /// addressing target data context's by name. For example: "client1, devmaster"
  /// The root permission level definition must be granted first, then system tries to find specifically named context each prefixed with `data::`.
  /// If it can not be found, the system defaults to the node `data::*` if it is found, otherwise access is denied.
  /// </summary>
  /// <example>
  /// <code>
  ///  myPermission
  ///  {
  ///    level = 1
  ///    data::*{level=1 }//this will be used if no other rule matches
  ///    data::client1{level=2 custom{ ...}}
  ///    data::devmaster{level=3 custom{ ...}}
  ///  }
  /// </code>
  /// </example>
  public abstract class DataContextualPermission : TypedPermission
  {
    public const string DATA_ACL_PREFIX = "data::";
    public const string DATA_ACL_ANY = "data::*";
    public static readonly char[] DELIMS = new []{' ', ',',';'};

    protected DataContextualPermission(int level) : base(level) { }

    protected sealed override bool DoCheckAccessLevel(IApplication app, ISession session, AccessLevel access)
    {
      if (!base.DoCheckAccessLevel(app, session, access)) return false;

      var dataContextName = session.NonNull(nameof(session)).DataContextName;
      if (dataContextName.IsNullOrWhiteSpace()) return false; // false;//deny for unspecified stores

      var segments = dataContextName.Split(DELIMS);

      var wereAny = false;
      for(var i=0; i<segments.Length; i++)
      {
        var segment = segments[i];
        if (segment.IsNullOrWhiteSpace()) continue;

        wereAny = true;

        var nds = access.Data[DATA_ACL_PREFIX + segment];

        if (!nds.Exists)//no data-specific override found - try to find ANY
          nds = access.Data[DATA_ACL_ANY];

        if (!nds.Exists) return false;//no data-specific or ANY override found - denied

        var passed = DoCheckDataStoreAccessLevel(app, session, dataContextName, nds, access);
        if (!passed) return false;
      }

      return wereAny;
    }

    /// <summary>
    /// Override to perform additional detailed checks in the scope of dataContextName.
    /// The base implementation just checks if the required level is sufficient on the node
    /// </summary>
    /// <param name="app">App scope</param>
    /// <param name="session">Non-null session scope under which permission is checked</param>
    /// <param name="dataContextName">String data context name as supplied from session. Non null/blank</param>
    /// <param name="dsRights">Existing rights/ACL node from  permission's ACL</param>
    /// <param name="access">Permission's rot access grant+ACL</param>
    /// <returns>True if action is authorized to be performed</returns>
    protected virtual bool DoCheckDataStoreAccessLevel(IApplication app,
                                                       ISession session,
                                                       string dataContextName,
                                                       IConfigSectionNode dsRights,
                                                       AccessLevel access)
    {
      return dsRights.AttrByName(AccessLevel.CONFIG_LEVEL_ATTR).ValueAsInt(0) >= Level;
    }
  }
}
