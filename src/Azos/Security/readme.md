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
Integration capabilities with other 3rd party providers is a must in almost every application these days.
Modern business applications typically use a combination of various contemporary and legacy technologies
as they need to span existing enterprise apps which may not be strangled/rewritten for practicality reasons.

## App Chassis - Security Manager
This Azos project was purposely designed to help build complex business systems which must address myriads of
diverse requirements spanning modern and legacy systems.

**Azos framework provides** a flexible uniform way of implementing a custom **security system of any level of complexity**
as needed for a particular application, or application system such as a distributed cloud application(s).

Application [Chassis](/src/Azos/Apps) defines a uniform way of implementing app-wide security system via 
[`ISecurityManager`](ISecurityManager.cs) contract which provides the security services described in more detail below.

### Quick Overview
Here is a quick overview how application security works in Azos.

#### Sessions and Users
In Azos, all code execution happens under a `Session` scope, even the ones that do not need session still have `NOPSession` instance for
uniformity and simplification of design. 

> Do not confuse the general concept of a session with legacy "fat" session concept from the past (e.g. web sessions in classic ASP.Net). 
> In Azos a session is a lite transient object which does not introduce any performance degradation. At the same time
> you can make a stateful session object which gets persisted (e.g. in state database), however this pattern is rarely needed these days.

Sessions object provides data about the user interaction session with a particular application, for example: one may store language, formatting and other
preferences in a session object, which can default from user. For example, scripting systems, such as `RemoteTerminal` implementations use sessions to store
their session variable values. When user logs-in they set their principal and create session object which may be preserved between calls.
**Session objects are always available for every call flow**, be it synchronous or 
asynchronous (using await) ones. The session is always accessible via `Ambient.CurrentCallSession { get; }` system property which should be rarely (if ever)
used in business code. Business code should pass sessions as a class prop/method parameter instead. 

In addition, `Session` exposes `DataContextName` property which is much needed in many modern applications as "tenant id" or 
"database instance name". Your application may support access checks based on target data context, inheriting its permissions from 
[`DataContextualPermission`](authorization/DataContextualPermission.cs)

> Contrary to what many purists say, Ambient is NOT an anti-pattern if it is used judiciously for a limited set of app-wide cross-cutting
> concerns like Session/User/CallFlow scope. Azos makes a fully conscious design decision to use it in system code, and as a matter of fact
> most other frameworks use the same pattern under the hood (e.g. DI is based on an internal service locator). Just like the App Chassis pattern, 
> the proper use of Ambient pattern does not make testing harder, to the contrary - it **structures application cross-cutting services 
> much better around a predefined set of concepts available in any application**

`Session` object exposes a [`User`](User.cs) property which embodies data about the "user" (aka "the subject") of the session. These concepts have nothing to do with
OS-provided impersonation, however you may have a system where `User` principal is set from the OS one - that is up to a concrete `ISecurityManager`
implementation. Just like the session object which can be accessed via `Ambient` class, the subject user is accessible via `Ambient.CurrentCallUser { get; }`
however, the business code should pass-in the User and session objects via object properties/method parameters for business purposes. The Ambient
design pattern is provided for system-level code (similar to `Thread.CurrentPrincipal`).

#### Authentication
Authentication is a process of establishing who the user is which results in setting `User` property of a session object.
The `Session` and `User` objects are set by system-level security code, such as network authentication boundaries(Wave/Glue filters).

Authentication is required in all applications irregardless of the method and IDP (Identity Provider) used. In practice, the authentication methods require various forms of **IDP (Identity Provider) integrations**, 
whilst many organizations to this day maintain their business **user records in custom-built legacy** system (e.g. SQL database tables).

These security boundaries obtain a token from the caller (e.g. via Authorization header) and then transact the `ISecurityManager.Authenticate`
method which yields a `User` object which is then typically assigned into `session.User` property.

User-facing apps may authenticate users via HTML form post and maintain their identity token via a browser session
cookie object.

The API apps typically authenticate users using `Access Tokens` (such as JWT) which may rely on a
client-side data embedded in a token itself (and protected cryptographically), such as the ones used with 
**claim-based identity**, or may require used of **server-side token authorities** which for additional security
are cross-checked via a back channel for every API call.


The number of authentication modes and methods is really unlimited, hence Azos provides an abstraction via 
[Credentials](credentials/Credentials.cs) abstract class which has the following concretions:
- BlankCredenitals - A NOP credential signifying the absence of any meaningful credentials
- BearerCredentials - contain a `Token` field which is typically supplied via `Authorization: Bearer` HTTP header scheme
- IDPasswordCredentials - contain plain ID/password plain text credentials typically supplied by HTML form post, or `Authorization: Basic` HTTP header scheme
- SocialNetTokenCredentials - a social `(NetId,token)` tuple used for login via social networks
- EntityUriCredentials - point to a resource (such as a group/room etc.) using its URI 

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
pieces and parts, for example: many app have their fields, buttons, and even drop-down choices  
restricted/limited during data entry.

It is not uncommon to have a requirement to implement a **resource-based** security, such as **row-level access control**.
A typical example is insurance carrier groups, having their carrier representatives be able to see only members within the group.
Usually these kind of systems have complex assignments like "Include all but these", "Exclude all but these..."


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