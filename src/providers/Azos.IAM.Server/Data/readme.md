# AZos IAM Server

This article provides a top-level overview of Azos IAM Server Solution, describing its capabilities and use cases.

It is recommended that you first get familiar with the following resources and concepts:
- [IAM - Identity and Access Management](https://en.wikipedia.org/wiki/Identity_management) (Wikipedia)
- [Authentication](https://en.wikipedia.org/wiki/Authentication) (Wikipedia)
- [Authorization](https://en.wikipedia.org/wiki/Authorization) (Wikipedia)
- [Multi-Factor Authentication](https://en.wikipedia.org/wiki/Multi-factor_authentication) (Wikipedia)
- [OAuth](https://en.wikipedia.org/wiki/OAuth) (Wikipedia)
- [OpenID Connect](https://youtu.be/WVCzv50BslE) (YouTube)

## IAM Server Summary
Azos IAM (AZIAM) solution is a full stack Identity and Access Management (IAM) service provider. It is implemented 
on AZos unified platform, as such, sharing many app features (config, logging, telemetry, cluster etc.)

### Capabilities
- Multiple **realms** - single logical server installation supports multiple logical data stores
- **Temporal assignments** - all assignments are set per time period and can be scheduled (e.g. valid from X until Y.)
- **Multi-factor authentication** with adaptive status per-login
- **OAuth/OpenID Connect**
- **Identity federation** - e.g. an Account with AD, OAuth/OpenID, Social, and other Providers
- **Custom IDP integration** (e.g. existing users stored in XYZ)
- Accounts with **multiple login methods**
- **Hierarchical account groups** with ACL derivation
- Various Account types e.g. - "user", "department" etc.
- Detailed AC (Access Control Lists) with custom permissions
- ACL permissions support any kind of data structures: from bool Y/N flags to custom access config vectors e.g. - **Geofences**, time-of day/week rights etc.
- **Role-Based-Security** via role ACL mix-ins
- **Detailed authorization overrides** down to account login level (Group->Group(s)->Account->Login)
- **Full change auditing** for all entities
- **Custom entity attributes** aka. `Entity Properties`, e.g. "Certification.Medicare = true", "Facility.Lockbox = 1234" etc..
- Custom entity **property and trait indexing** - ability to efficiently query entities on their traits
- **Policies** - a set of rules/settings applied in time assignment spans to entities
- Policy-based Group/Account/Login lock-out, password reset, complexity enforcement, can not reuse X old, min edit distance etc.
- **Async notifications via web hooks** - host system gets notified of the security events, such as lock-out etc.

### Datastore
AZIAM stores its data in a backend-agnostic store, performing near-real-time multi-master data replication of all of the entity changes between 
multiple data centers. By default, AZIAM server comes with the `Mongo Db` store (**note:** *Provider-specific sharding and replication 
 is **NOT used on purpose**. Consequentially, one does not need to get expensive licenses for, say MS Sql or Oracle used as an AZIAM backing store.*).

### IAM Protocol / Client
AZIAM server provides its services via a well-factored `REST` interface. You can use `Azos.IAM` client library for ease of use,
but this is not required, as you can use any HTTP(s)/REST capable client to work with the server.

AZIAM server provides asynchronous notification capabilities via web hooks. Web hooks can be defined per `realm`.


## Rights / Access Control List (ACL)
https://en.wikipedia.org/wiki/Access-control_list
^clarification:^ *The term `ACL` is used here in a much broader sense - it is a security rights configuration vector*

Rights aka ACL are configuration vectors containing information about subject (e.g. group/account/login) capabilities.
The ACL data is rich and allows for the most complex security scenarios as automatically supported by Azos Permissions.
*(Note: you do not have to use Azos as a security client for Azos IAM Server, however if you do, you get all unified 
system benefits, such as no need to check permissions manually on a client side etc.)*

## Groups
Azos IAM server supports concepts of `Groups`. Any account can be a part of only one group at any single time.
This is not a limitation, because groups organize accounts physically, whereas `Roles` provide mix-in rights on a ad-hoc basis.

## Roles
Roles represents a named "kits" of access rights. The get mix-in to ACL by their IDs, as every Role has a unique mnemonic `ID`.
You should not change `ID`s for roles as this would render all role links invalid with consequential rights loss on ACLs.

## Accounts
`Account` entity represents a titled account in the system. This does not necessarily correspond with physical users. Accounts
may represent departments, user groups (such as a chat room), and other org units

## Logins
The system supports multiple (more than one) log-in methods, such as the ones used for alternative credential formats and identity federation.
The following account login types are typical:
- Screenname(ID) / Password (A "screenname" is an ID which identifies accounts uniquely, it is always publicly visible, hence the name)
- EMail / Password
- Connected social networks: Twitter, Facebook etc...
- Connected 3rd party OAuth/OpenID identity providers, e.g.: Google, Sales Force
- (Phase II) Active Directory Accounts and other LDAP Accounts
- Various custom 3rd party IDP provider plugins




## Policies
Policies define a set of rules/settings for handling various aspects of security-related logic.
The IAM Server identifies each policy by a unique `ID`. Policy data is a config vector. It is stored in a time period collection.

Policies get linked to `Groups->Account` hierarchy, having policy reference not required. 
During various operations the system calculated the effective policy by taking it from the top of the hierarchy, and
tries to override with every more specific level, therefore if policy is not linked at a particular level, it will be defaulted from
the effective policy already calculated.

## Entity Indexing
AZIAM Server is designed to support a variety of systems/use cases, as such, it supports custom entity properties.
`Index` table is provided to index custom entity properties and other entity traits (specific to a particular system use-cases).
The index is populated on data mutation operations as specified in the mutation Api call.

## Change Auditing
An `Audit` table retains a detailed log of all changes to all entities in the system (except for `Audit` and other system internal tables).
The audit log table captures previous and new values field-by-field. The detalization level is controlled by the effective policy.

All entities with rights also contain as set of `Audit*` fields that capture the last change performed. This is needed to always have
the minimum auditing data even if `Audit` table gets purged completely.

The server deletes out-dated `Audit` data as specified by the effective policy.


## Token Ring Services


## Muti-Factor Authentication
The multi-factor (e.g. 2FA) authentication flow operates as follows:
1. User tries to log-in using the appropriate credential type (e.g. id/pwd)
2. Depending on the credential type used, the applicable policy is applied. 
 **NOTE:** The policy is always fetched even using login ID only (even if the password is bad), this is
 needed to possibly lock account after X invalid attempts (see below)
3. The policy defines account login requirements and limits etc..
4. The `LoginStatus` table is updated to reflect the latest status change
5. If the login is not acceptable (e.g. wrong password), the error is returned
6. If the login is acceptable, but the policy requires 2FA, the IAM server `.Authenticate(cred)` method
 returns a `User` subject with an `Invalid` status having its rights contain the pragma/permission
 to with a 2FA token which contains the pointer to secret/code that was conveyed to the user using the secondary
 authentication channel (e.g. a Text Message, an Email) - details are defined by policy.
7. Users are now presented with an interstitial UI to enter the secret conveyed to them over the secondary channel.
8. The secret answer is combined with the 2FA token supplied in the prior step using `.MultifactorSecretCredentials(2fatoken, secret)`
  into `.Authenticate(cred)`, if the secret is correct, the system returns a valid `User` principal object 

The following p-code is an example of the above:
```csharp
...
  var idp = new IDPasswordCredentials("user@service.com", "password123");//taken from the original login form POST
  var user = await secman.AuthenticateAsync(idp);
  if (user.Status > UserStatus.Invalid) return user;//User is Good, we are done

  var mftoken = SecUtils.CheckForMultifactorToken(user.Rights);
  if (mftoken==null) return user;//user is permanently bad, and there is nothing else we can do, just return bad user

  //mftoken contains: {"token", "secret", "channels"}  you send the "secret" to the user
  //in any of acceptable ways as returned in "channels" (e.g. SMS vs phone).
  //Never return "secret" as a part of the interstitial prompt/page itself!!!
  //Note: the channel communication IS NOT provided by IAM serve, you need to use whatever messaging infrastructure
  //you application supports

  //Display a page, with hidden field populated by mftoken.token, and a text field
  //where user needs to post a "secret" answer that was supplied to him over the 2nd channel (e.g. SMS)
  displayInterstitial(mftoken);
...
  //user posts:
  var mfcred = new MultifactorSecretCredentials(mftoken, secret);//which came from HTTP POST
  var user = await secman.AuthenticateAsync(mfcred);
  if (user.Status > UserStatus.Invalid) return user;//User is Good, we have successfully passed 2FA

  //if the user is invalid, process continues as per above...
  //also, as governed by policy, IAM server may return a different secret that you would need to send on a different channel
```

## Various Policies and Rules

###  Password change every X days, Last password change
The information is kept in `Login` table.
This is enforced by the account-effective policy.


### Can not reuse X old passwords
The system keeps however many passwords needed in the `Login` table. Upon the password change, the system checks to see if such 
combination was already used. Per above, the X is supplied by the effective policy

### Entity LOCK-OUT for  X wrong log-in attempts
The effective policy determines conditions when entities become locked-out.
For example, for an `Account` entity it is the maximum number of wrong/bad login attempts after which the whole account 
becomes locked-out. There are two types of lock-out: **Permanent** and **auto-reset**. Permanent lock-outs require system/admin/user
intervention to resolve (e.g. a process similar to "reset password" to unlock an account). Auto-reset account locks are lifted automatically 
after the reset data which is governed by the effective policy

### Number of log-in attempts

### Should not be able to re-use LOGIN/EMAIL after it is inactivated
### Password change schedule
### Minimum password edit distance

### Permission Assignment Dat spans
in a date/time span - maybe add this to root permission (`sd`,`ed` along with `level`) -
   or maybe this should be delegated to specific app



