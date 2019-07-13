/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Conf;

namespace Azos.Security
{
  /// <summary>
  /// Invoked by permission checker to get session instance
  /// </summary>
  public delegate ISession GetSessionFunc();


  /// <summary>
  /// Represents a general permission abstraction - where permission type represents the path/name of the permission
  ///  in User's rights and .ctor takes specific parameters to check while authorizing user.
  ///  Permission-derived class represents a certain permission type, whereas its instance represents a check for some desired access level.
  ///  To authorize certain actions, one creates an instance of Permission-derived class passing in its .ctor required
  ///   access levels, then calls `Check()` method that returns true if the action is authorized.
  ///
  /// This design provides a great deal of flexibility, i.e. for complex security cases developers may inherit leaf-level permissions from intermediate ones
  ///   that have logic tied to session-level variables, this way user's access may vary by permission/session state, e.g. a user may have
  ///    "Patient.Master" level 4 access in database "A", while having access denied to the same named permission in database "B".
  /// User's database, or system instance is a flag in user-session context
  /// </summary>
  [AttributeUsage(AttributeTargets.Class |
                  AttributeTargets.Interface |
                  AttributeTargets.Constructor |
                  AttributeTargets.Method |
                  AttributeTargets.Field |
                  AttributeTargets.Property, Inherited = true, AllowMultiple=true)]
  [CustomMetadata(typeof(PermissionCustomMetadataProvider))]
  public abstract class Permission : Attribute
  {
    #region CONSTS
    public const string CONFIG_PERMISSIONS_SECTION = "permissions";
    public const string CONFIG_PERMISSION_SECTION = "permission";
    #endregion

    #region Static

    /// <summary>
    /// Checks the action represented by MemberInfo by checking the permission-derived attributes and returns false if
    /// any of authorization attributes do not pass
    /// </summary>
    public static bool AuthorizeAction(IApplication app, MemberInfo actionInfo, ISession session = null, GetSessionFunc getSessionFunc = null)
    {
      return FindAuthorizationFailingPermission(app, actionInfo, session, getSessionFunc) == null;
    }


    private static Dictionary<MemberInfo, Permission[]> s_AttrCache = new Dictionary<MemberInfo,Permission[]>();


    /// <summary>
    /// Checks the action represented by MemberInfo by checking the permission-derived attributes and returns false if
    /// any of authorization attributes do not pass
    /// </summary>
    public static Permission FindAuthorizationFailingPermission(IApplication app, MemberInfo actionInfo, ISession session = null, GetSessionFunc getSessionFunc = null)
    { //20150124 DKh - added caching instead of reflection. Glue inproc binding speed improved 20%
      Permission[] permissions;
      if (!s_AttrCache.TryGetValue(actionInfo, out permissions))
      {
        permissions = actionInfo.GetCustomAttributes(typeof(Permission), true).Cast<Permission>().ToArray();
        var dict = new Dictionary<MemberInfo,Permission[]>(s_AttrCache);
        dict[actionInfo] = permissions;
        s_AttrCache = dict;//atomic
      }

      for(var i=0; i<permissions.Length; i++)
      {
        var permission = permissions[i];
        if (i==0 && session==null && getSessionFunc!=null) session = getSessionFunc();
        if (!permission.Check(app, session)) return permission;
      }
      return null;
    }

    /// <summary>
    /// Guards the action represented by MemberInfo by checking the permission-derived attributes and throwing exception if
    /// any of authorization attributes do not pass
    /// </summary>
    public static void AuthorizeAndGuardAction(IApplication app,
                                               MemberInfo actionInfo,
                                               ISession session = null,
                                               GetSessionFunc getSessionFunc = null)
    {
      var failed = FindAuthorizationFailingPermission(app, actionInfo, session, getSessionFunc);

      if (failed!=null)
        throw new AuthorizationException(string.Format(StringConsts.SECURITY_AUTHROIZATION_ERROR, failed,  actionInfo.ToDescription()));
    }

    /// <summary>
    /// Helper method which allows to write multiple permissions as open-array method param.
    /// Returns empty enumerable if null is passed in
    /// </summary>
    public static IEnumerable<Permission> All(params Permission[] permissions) => permissions!=null ? permissions : Enumerable.Empty<Permission>();

    /// <summary>
    /// Guards the action represented by enumerable of permissions by checking all permissions and throwing exception if
    /// any of authorization attributes do not pass
    /// </summary>
    public static void AuthorizeAndGuardAction(IApplication app,
                                               IEnumerable<Permission> permissions,
                                               string actionName,
                                               ISession session = null,
                                               GetSessionFunc getSessionFunc = null)
    {
      if (permissions==null) return;


      if (session==null && permissions.Any() && getSessionFunc!=null) session = getSessionFunc();

      var failed = permissions.FirstOrDefault(perm => perm!=null && !perm.Check(app, session));

      if (failed!=null)
        throw new AuthorizationException(string.Format(StringConsts.SECURITY_AUTHROIZATION_ERROR, failed,  actionName ?? CoreConsts.UNKNOWN));
    }

    /// <summary>
    /// Guards the action  by checking a single permission and throwing exception if any of authorization attributes do not pass
    /// </summary>
    public static void AuthorizeAndGuardAction(IApplication app,
                                               Permission permission,
                                               string actionName,
                                               ISession session = null,
                                               GetSessionFunc getSessionFunc = null)
    {
      if (permission == null) return;

      if (session == null && getSessionFunc != null) session = getSessionFunc();

      var failed = !permission.Check(app, session);

      if (failed)
        throw new AuthorizationException(string.Format(StringConsts.SECURITY_AUTHROIZATION_ERROR, failed, actionName ?? CoreConsts.UNKNOWN));
    }

    /// <summary>
    /// Makes multiple permissions from conf node
    /// </summary>
    public static IEnumerable<Permission> MultipleFromConf(IConfigSectionNode node,
                                                          string shortNodeName = null,
                                                          string typePattern = null
                                                          )
    {
      if (node==null || !node.Exists) return Enumerable.Empty<Permission>();

      var result = new List<Permission>();

      foreach(var pnode in node.Children.Where(cn => cn.IsSameName(CONFIG_PERMISSION_SECTION) ||
                                              (shortNodeName.IsNotNullOrWhiteSpace() && cn.IsSameName(shortNodeName) )))
      {
        result.Add( FactoryUtils.MakeUsingCtor<Permission>(pnode, typePattern) );
      }

      return result;
    }


    #endregion

    #region .ctor
    /// <summary>
    /// Creates the check instance against the minimum access level for this permission
    /// </summary>
    protected Permission(int level)
    {
      m_Level = level;
    }
    #endregion

    #region Fields
    private int m_Level;
    #endregion

    #region Properties
    /// <summary>
    /// Returns the permission name - the last segment of the path
    /// </summary>
    public abstract string Name
    {
        get;
    }

    /// <summary>
    /// Returns the permission description - base implementation returns permission name
    /// </summary>
    public virtual string Description
    {
        get { return Name;}
    }

    /// <summary>
    /// Returns a top-rooted path to this permission (without name)
    /// </summary>
    public abstract string Path
    {
        get;
    }

    /// <summary>
    /// Returns full permission path - a concatenation of its path and name
    /// </summary>
    public string FullPath
    {
        get
        {
          var path = Path;
          if (path.EndsWith("/"))
            return path + Name;
          else
            return path + "/" + Name;
        }
    }

    /// <summary>
    /// Specifies the minimum access level for the permission check to pass
    /// </summary>
    public int Level
    {
        get { return m_Level; }
    }

    #endregion

    #region Public

    /// <summary>
    /// Shortcut method that creates a temp/mock BaseSession object thus checking permission in mock BaseSession context
    /// </summary>
    public bool Check(IApplication app, User user)
    {
      if (user==null || !user.IsAuthenticated) return false;
#warning May avoid heap allocation here by implementing SessionStub as struct
      var session = new BaseSession(Guid.NewGuid(), app.Random.NextRandomUnsignedLong);
      session.User = user;
      return this.Check(app, session);
    }

    /// <summary>
    /// Shortcut method that creates a temp/mock BaseSession object thus checking permission in mock BaseSession context
    /// </summary>
    public Task<bool> CheckAsync(IApplication app, User user)
    {
      if (user == null || !user.IsAuthenticated) return Task.FromResult(false);
#warning May avoid heap allocation here by implementing SessionStub as struct
      var session = new BaseSession(Guid.NewGuid(), app.Random.NextRandomUnsignedLong);
      session.User = user;
      return this.CheckAsync(app, session);
    }

    /// <summary>
    /// Checks the permission for requested action as specified in particular permission .ctor.
    /// The check is performed in the scope of supplied session, or if no session was supplied then
    ///  current execution context session is assumed
    /// </summary>
    /// <returns>True when action is authorized, false otherwise</returns>
    public virtual bool Check(IApplication app, ISession sessionInstance = null)
    {
      var session = sessionInstance ?? ExecutionContext.Session ?? NOPSession.Instance;
      var user = session.User;

      //System user passes all permission checks
      if (user.Status==UserStatus.System) return true;

      var manager = app.SecurityManager;

      var access = manager.Authorize(user, this);

      if (!access.IsAssigned) return false;

      return DoCheckAccessLevel(app, session, access);
    }

    /// <summary>
    /// Checks the permission for requested action as specified in particular permission .ctor.
    /// The check is performed in the scope of supplied session, or if no session was supplied then
    ///  current execution context session is assumed. An Async version which uses async manager.AuthorizeAsync() call
    /// </summary>
    /// <returns>True when action is authorized, false otherwise</returns>
    public virtual async Task<bool> CheckAsync(IApplication app, ISession sessionInstance = null)
    {
      var session = sessionInstance ?? ExecutionContext.Session ?? NOPSession.Instance;
      var user = session.User;

      //System user passes all permission checks
      if (user.Status == UserStatus.System) return true;

      var manager = app.SecurityManager;

      var access = await manager.AuthorizeAsync(user, this);

      if (!access.IsAssigned) return false;

      return DoCheckAccessLevel(app, session, access);
    }

    public override string ToString()
    {
      return FullPath;
    }

    #endregion

    #region Protected

    /// <summary>
    /// Override to perform access level checks per user's AccessLevel instance.
    /// True if  accessLevel satisfies permission requirements.
    /// The default implementation checks the access.Level
    /// </summary>
    protected virtual bool DoCheckAccessLevel(IApplication app, ISession session, AccessLevel access)
    {
      return access.Level >= m_Level;
    }
    #endregion
  }

  /// <summary>
  /// A general ancestor for all typed permissions - the ones declared in code
  /// </summary>
  public abstract class TypedPermission : Permission
  {
    public const string PERMISSION_SUFFIX = "Permission";//do not localize
    /// <summary>
    /// Creates the check instance against the minimum access level for this typed permission
    /// </summary>
    protected TypedPermission(int level) : base (level)
    {
    }

    public override string Name
    {
      get
      {
        var name = GetType().Name;
        if (name.EndsWith(PERMISSION_SUFFIX) && name.Length > PERMISSION_SUFFIX.Length)//do not localize
          name = name.Remove(name.Length - PERMISSION_SUFFIX.Length);

        return name;
      }
    }

    public override string Path
    {
      get { return '/' + GetType().Namespace.Replace('.', '/'); }
    }
  }


  /// <summary>
  /// Represents a permission check instance which is a-typical and is based on string arguments
  /// </summary>
  public sealed class AdHocPermission : Permission
  {
    public AdHocPermission(string path, string name, int level) : base (level)
    {
      path = path ?? "/";
      name = name ?? CoreConsts.UNKNOWN;

      if (!path.StartsWith("/")) path = '/' + path;

      m_Path = path.Replace('.', '/').Replace('\\', '/');
      m_Name = name;
    }

    private string m_Name;
    private string m_Path;


    /// <summary>
    /// Returns the permission name - the last segment of the path
    /// </summary>
    public override string Name
    {
        get { return m_Name; }
    }

    /// <summary>
    /// Returns a top-rooted path to this permission (without name)
    /// </summary>
    public override string Path
    {
        get { return m_Path; }
    }
  }

}
