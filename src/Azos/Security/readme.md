 # Security and Access Control

 Back to [Documentation Index](/src/documentation-index.md)

This section describes application security, authentication, authorization, permissions and cryptography.

See also:
- [Dependency Injection](/src/Azos/Apps/Injection)
- [Configuration](/src/Azos/Conf)

The security topic is a very broad and complex one as modern systems may need to use various 
approaches to application security mechanisms such as authentication and authorization. 
Integration capabilities with other 3rd party providers is a must in almost every application these days.
Modern business applications typically use a combination of various contemporary and legacy technologies
as they need to span existing enterprise apps which may not be strangled/rewritten for practicality reasons.

## App Chassis - Security Manager
This Azos project was purposely designed to help build complex business systems which must address myriads of
diverse requirements spanning modern and legacy systems.

**Azos framework provides** a flexible uniform way of implementing a custom **security system of any level of complexity**
as needed for a particular application, or application system such as a distributed cloud application(s).

The application [Chassis](/src/Azos/Apps) provides a uniform way of implementing security system via [`ISecurityManager`](ISecurityManager.cs) contract
which provides the following main services described below.


## Authentication
Authentication is a process of establishing who the user/caller is. Authentication is required in all applications irregardless 
of the method and IDP (Identity Provider) used. In practice, the authentication methods require various forms of **IDP (Identity Provider) integrations**, 
whilst many organizations to this day maintain their business **user records in custom-built legacy** system (e.g. SQL database tables).

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



## Authorization
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