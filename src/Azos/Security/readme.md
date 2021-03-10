 # Security and Access Control

 Back to [Documentation Index](/src/documentation-index.md)

This section covers a broad span of application security:
- authentication and IDP (Identity Providers)
- authorization, access control, roles, rights and permissions
- password management: generating, verifying, changing
- message protection algorithms (cryptography)

See also:
- [Dependency Injection](/src/Azos/Apps/Injection)
- [Configuration](/src/Azos/Conf)

The security topic is a very broad and complex one as modern systems may need to employ various 
application security mechanisms such as the ones needed for authentication and authorization. 
Integration capabilities with other 3rd party providers is a must have feature in almost every application these days.
Modern business applications typically use a combination of various contemporary and legacy technologies
as they need to cover existing enterprise apps which may not be strangled/rewritten for practicality reasons.

## App Chassis - Security Manager
The Azos project was purposely designed to help build complex business systems which must address myriads of
diverse requirements spanning modern and legacy systems.

**Azos framework provides** a flexible uniform way of implementing a custom **security system of any level of complexity**
as needed for a particular application, or application system such as a distributed cloud application(s).

Application [Chassis](/src/Azos/Apps) defines a uniform way of implementing app-wide security system via 
[`ISecurityManager`](ISecurityManager.cs) contract which provides the security services described in more detail below.

### Quick Overview
This is a quick overview how application security works in Azos.

#### Sessions and Users
In Azos, all code execution happens under a `Session` scope, even the ones that do not need session still have `NOPSession` instance for
uniformity and design simplification. See [ISession](/src/Azos/Apps/ISession.cs) 

> Do not confuse the general concept of a session with legacy "fat" session concept from the past (e.g. web sessions in classic ASP.Net). 
> In Azos a session is a lite transient object which does not introduce any performance degradation. At the same time
> you can make a stateful session object which gets persisted (e.g. in state database), however this pattern is rarely needed these days.

Sessions object provides data about the user interaction session with a particular application, for example: one may store language, formatting and other
preferences in a session object, which can default from user. For example, scripting systems, such as `RemoteTerminal` implementations use sessions to store
their session variable values. When user logs-in they set their principal and create session object which may be preserved between calls.
**Session objects are always available for every call flow**, be it synchronous or 
**asynchronous** (using await) ones. The session is always accessible via `Ambient.CurrentCallSession { get; }` system property which should be rarely (if ever)
used in business code. Business code should pass sessions as a class properties or method parameters instead. 

In addition, `Session` exposes `DataContextName` property which is much needed in many modern applications as a **"tenant id"** or 
"database instance name". Your application may support authorization access checks based on a target data context, inheriting its permissions from 
[`DataContextualPermission`](authorization/DataContextualPermission.cs)

> Contrary to what many purists say, Ambient is NOT an anti-pattern if it is used judiciously for a limited set of app-wide cross-cutting
> concerns like Session/User/CallFlow scope. Azos makes a fully conscious design decision to use it in system code, and as a matter of fact
> most other frameworks use the same pattern under the hood (e.g. DI is based on an internal service locator). Just like the App Chassis pattern, 
> the proper use of Ambient pattern does not make testing harder, to the contrary - it **structures application cross-cutting services 
> much better around a predefined set of concepts available in any application setting**

`Session` object exposes a [`User`](User.cs) property which embodies data about the "user" (aka "the subject principal") of the session. These concepts have nothing to do with
OS-provided impersonation, however you may have a system where `User` principal is set from the OS one - that is up to a concrete `ISecurityManager`
implementation. Just like the session object which can be accessed via `Ambient` class, the subject user is accessible via `Ambient.CurrentCallUser { get; }`
however, **the business code should pass-in the User and session objects via object properties/method parameters for business purposes**. The Ambient
design pattern is provided for system-level code (similar to `Thread.CurrentPrincipal`).

#### Authentication
Authentication is a process of establishing who the user principal is which results in setting `User` property of a session object.
The `Session` and `User` objects are set by system-level security code, such as network authentication boundaries(Wave/Glue auth filters).

Authentication is required in all applications irregardless of the method and IDP (Identity Provider) used. In practice, the authentication methods require various forms of **IDP (Identity Provider) integrations**, 
whilst many organizations to this day maintain their business **user records in custom-built legacy** system (e.g. SQL database tables).

These security boundaries obtain a token from the caller (e.g. via Authorization header) and then call the `ISecurityManager.Authenticate`
method which yields a `User` object which is then typically assigned into `session.User` property.

User-facing apps may authenticate users via HTML form post and maintain their identity token via a browser session
cookie object.

The API apps typically authenticate users using `Access Tokens` (such as JWT) which may rely on a
client-side data embedded in a token itself (and protected cryptographically), such as the ones used with 
**claim-based identity**, or may require the use of **server-side token authorities** (aka `TokenRing` in Azos) which for additional security
are cross-checked via a back channel for every API call. Server-side token authorities are more secure and more complex to implement because:

- They do not divulge any information even in an encrypted form because server tokens are nothing but randomly generated IDs which point-to security data maintain on the server
- Server tokens may be revoked/deleted from the store at any time without having to rely on the client-token expiration or blacklisting
- Server side token introduce state which needs to be checked for every call, this does introduce some performance penalty which can be mitigated by using in-process volatile short-term caching
- There are simply more components with server-side token authorities, therefore they should be used only when really needed

The number of authentication modes and methods is really unlimited, hence Azos provides an abstraction via 
[Credentials](credentials/Credentials.cs) abstract class which has the following concretions:
- `BlankCredenitals` - A NOP credential signifying the absence of any meaningful credentials
- `BearerCredentials` - contain a `Token` field which is typically supplied via `Authorization: Bearer` HTTP header scheme
- `IDPasswordCredentials` - contain plain ID/password plain text credentials typically supplied by HTML form post, or `Authorization: Basic` HTTP header scheme
- `SocialNetTokenCredentials` - a social `(NetId,token)` tuple used for login via social networks
- `EntityUriCredentials` - point to a resource (such as a group/room etc.) using its URI 

The authentication is a process of obtaining a [`User`](User.cs) subject for the supplied credentials.
```CSharp
interface ISecurityManager
  /// <summary>
  /// Authenticates user by checking the supplied credentials against the
  /// authentication store that this manager represents.
  /// If credential are invalid then UserKind.Invalid is returned.
  /// </summary>
  /// <param name="credentials">User credentials.
  /// Particular manager implementation may elect to support multiple credential types, i.e.
  /// IdPassword, Twitter, Facebook, OAuth, LegacySystemA/B/C etc.
  /// </param>
  /// <returns>
  /// User object. Check User.Status for UserStatus.Invalid flag to see if authentication succeeded
  /// </returns>
  User Authenticate(Credentials credentials);
```


#### Authorization
From the authorization standpoint, business applications are notorious for having many security-addressable
pieces and parts, for example: many apps have their fields, buttons, and even drop-down choices restricted/limited during data entry.

It is not uncommon to have requirements to implement a **resource-based** security, such as **row-level access control**.
A typical example is insurance carrier groups, having their carrier representatives be able to see only members within certain groups.
Usually these kind of systems have complex rights assignments like "Include all but these", "Exclude all but these..."

A `User` principal object is accessible via `Session.User` property and represents an authenticated user principal.

> Many think that `User` implies a human user. It does not. A user may also be a robot, a process, an entity etc. The term can be used 
> along with `Principal` interchangeably. 
 
> You can use `Session.LastLoginType = (Human | Robot)` to determine how the session was initiated. See [ISession](/src/Azos/Apps/ISession.cs)

User object exposes `Rights` property which is used as a round-trip bag between `ISecurityManager` calls to `Authenticate` and `Authorize`.
This is a very important design point. The "rights" depend on particular `ISecurityManager` implementation - it is up to that implementation
to interpret user `Rights` per specific system:
```csharp
  /// <summary>
  /// Authorizes user by finding appropriate access level to permission by supplied path.
  /// Depending on particular implementation, rights may be fully or partially cached in memory.
  /// Note: this authorization call returns AccessLevel object that may contain a complex data structure.
  /// The final assertion of user's ability to perform a certain action is encapsulated in Permission.Check() method.
  /// Call Permission.AuthorizeAndGuardAction(MemberInfo, ISession) to guard classes and methods from unauthorized access
  /// </summary>
  /// <param name="user">A user to perform authorization for</param>
  /// <param name="permission">An instance of permission to get</param>
  /// <returns>AccessLevel granted to the specified permission</returns>
  AccessLevel Authorize(User user, Permission permission);
```

The [`AccessLevel`](authorization/AccessLevel.cs) represents the actual authorization data - an **access control list** (ACL), which is in 
effect at the time of the call. The actual Authorization determination is performed in the [`Permission`](authorization/Permissions.cs) `Check` method.

The ACLs/rights store a configuration object of the following form:
```cshrap
namespace1
{
  namespaceX
  {
    permission1{ level=1 ... }
  }
}
```

Permissions have `Path` property which is a ACL config tree path, consequently when you create a typed permission in, for example: 
`MySystem.Security.MyPermissions.BookMasterPermission` here is what an ACL grant may look like:
```csharp
  MySystem
  {
    Security{ MyPermissions{  BookMaster{ level = 4 }}}
  }
```

> Permissions are classes derived from `Attribute` so they can decorate classes and methods declaratively or be allocated imperatively
> in code. This is a form of **inversion of control** - instead of handling all logic in one large class the system delegates the
> work to the relevant entities, having `ISecurityManager` acting as a facade to the IDP/authorization stores, permissions representing
> pieces of security-addressable functionality along with virtual `Check()` method which encapsulates the actual logic.

> NOTE: Permission `Check()` executes in `ISession` scope, having `Session.User` object as a property. This allows to build complex
> policy-based authorization schemes which depend on **Users, Rights and User Session objects**

The authorization functionality is built into WAVE and Glue surfaces, so you can add permissions at the declaration level:
```csharp

  [WorkerPermission]
  public interface INotificationService
  {
    bool SendMsg(...);

    [SupervisorPermission]
    bool SendUrgentMsg(...);
  }

  [SchedulingPermission] //VIEW or up
  public class Appointment : ApiProtocolController
  {
    [ActionOnGet]
    public async Task<object> GetApt(string id){ .... }

    [ActionOnPost, 
     SchedulingPermission(SchedulingLevel.Create), // CREATE mode to post new entries
     AdminPermission] // and ADMIN Permission
    public async Task<object> PostApt(Appointment data){ .... }
  }
```

> NOTE: Permissions are more granular than role-based security. It is always possible to derive a more general entity from its details, whereas
> the opposite operation is not possible. If your system does not need to use granular permissions you can create permissions which
> logically equate to roles for simplicity. In Azos a "role" is a named set of permissions and authorization is done based on permissions, 
> not roles.


Sometimes applications need to authorize actions using imperative code constructs, for example in the below example a determination
of `DataContext` which authorization depends upon, is performed later in the code flow:
```csharp
  public async Task<ResponseMessage> ProcessAsync(RequestMessage request)
  {
    //quickly pre-check request if it is logically malformed
    await preValidateRequest(request);

    //determine data context name BEFORE we authorize the processing
    var dataContext = routeDataContext(request);

    //create our security check when we know dataContext
    var demand = new RequestProcessingPermission(ProcessingLevel.Process, dataContext.Name);

    //Authorize AFTER we know the data context
    await App.Authorize(demand);//this throws AuthorizationException

    //do actual work which was authorized at this point
    var result = await processCore(request, dataContext);

    return result;
  }
```

> Notice in the example above, there is no passing around `User` or `Session` objects. This is because
> the system uses Ambient context under the hood as it knows the current call flow principal. 
> Notice that you can now use any kind of design because you do not need to carry `Session` around
> many function calls that otherwise do not need it. The code above is 100% agnostic of its host - 
> it does not care whether it is a web, console app or any other host process type

See also:
- [Configuration](/src/Azos/Conf)
- [Dependency Injection](/src/Azos/Apps/Injection)
- [Logging](/src/Azos/Log)


External resources:
- [Authentication (Wikipedia)](https://en.wikipedia.org/wiki/Authentication)
- [Authorization (Wikipedia)](https://en.wikipedia.org/wiki/Authorization)
- [Role-based Access Control (Wikipedia)](https://en.wikipedia.org/wiki/Role-based_access_control)
- [Claims-based Identity (Wikipedia)](https://en.wikipedia.org/wiki/Claims-based_identity)


Back to [Documentation Index](/src/documentation-index.md)