# AZos IAM Server

This article provides a top-level overview of Azos IAM Server Solution, describing its capabilities and use cases.

It is recommended that you first get familiar with the following resources and concepts:
- [IAM - Identity and Access Management](https://en.wikipedia.org/wiki/Identity_management) (Wikipedia)
- [Authentication](https://en.wikipedia.org/wiki/Authentication) (Wikipedia)
- [Authorization](https://en.wikipedia.org/wiki/Authorization) (Wikipedia)
- [Multi-Factor Authentication](https://en.wikipedia.org/wiki/Multi-factor_authentication) (Wikipedia)
- [OAuth](https://en.wikipedia.org/wiki/OAuth) (Wikipedia)
- [OpenID Connect](https://youtu.be/WVCzv50BslE) (YouTube)


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


## Policies
Policies define a set of rules/settings for handling various aspects of security-related logic.
The IAM Server identifies each policy by a unique `ID`. Policy data is a config vector. It is stored in a time period collection.

Policies get linked to `Groups->Account` hierarchy, having policy reference not required. 
During various operations the system calculated the effective policy by taking it from the top of the hierarchy, and
tries to override with every more specific level, therefore if policy is not linked at a particular level, it will be defaulted from
the effective policy already calculated.


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

### Account LOCK-OUT for  X wrong log-in attempts
###   Number of log-in attempts

### Should not be able to re-use LOGIN/EMAIL after it is inactivated
### Password change schedule
### Minimum password edit distance

### Permission Assignment Dat spans
in a date/time span - maybe add this to root permission (`sd`,`ed` along with `level`) -
   or maybe this should be delegated to specific app



