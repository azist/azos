/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Collections;
using Azos.Data;
using Azos.Data.Business;
using Azos.Security;
using Azos.Security.MinIdp;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.AuthKit.Server
{
  /// <summary>
  /// Provides transitive data used between various methods calls during user authentication.
  /// The class is paired with IIdpHandlerLogic which makes a new instance of this or
  /// appropriate derivative class.
  /// Your auth system may extend this object also overriding <see cref="IIdpHandlerLogic.MakeNewUserAuthenticationContext(AuthenticationRequestContext)"/>
  /// </summary>
  public class AuthContext
  {
    /// <summary>
    /// Describes auth outcome with optional JSON map details
    /// </summary>
    public struct Outcome
    {
      public static Outcome Ok() => new Outcome(true, null);

      public static Outcome Ok(string description) => Ok(1, description);
      public static Outcome Ok(int code, string description, Action<JsonDataMap> details = null)
      {
        var map = new JsonDataMap { { "c", code }, { "d", description } };
        if (details != null) details(map);
        return new Outcome(true, map);
      }
      public static Outcome Negative(int code, string description, Action<JsonDataMap> details = null)
      {
        var map = new JsonDataMap { { "c", code }, { "d", description } };
        if (details != null) details(map);
        return new Outcome(false, map);
      }

      private Outcome(bool ok, JsonDataMap map) {  OK = ok; Data = map; }
      public readonly bool OK;
      public readonly JsonDataMap Data;
    }


    public AuthContext(Atom realm, AuthenticationRequestContext ctx)
    {
      Realm = realm.IsTrue(v => !v.IsZero && v.IsValid, "Valid realm");
      RequestContext = ctx;
    }

    public readonly Atom Realm;
    public readonly AuthenticationRequestContext RequestContext;

    /// <summary>
    /// ResultCodes a kin to HTTP status codes 200-300 represent success
    /// </summary>
    public Outcome Result {  get; private set; }

    /// <summary>
    /// True when the record indicates logically positive auth result outcome - when user is valid etc..
    /// False when, in spite of this instance presence it does not represent a valid auth result (e.g. invalid login)
    /// </summary>
    public bool HasResult => Result.OK;


    /// <summary>
    /// Sets auth result along with short textual description
    /// </summary>
    public void SetResult(Outcome outcome)
    {
      Result = outcome;
    }

    public LoginProvider Provider {  get; set; }

    public GDID G_User { get; set; }

    public string SysId { get; set; }
    public string SysTokenData { get; set; }

    public UserStatus Status { get; set; }
    public DateTime CreateUtc { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    public GDID G_Login { get; set; }
    public EntityId  LoginId {  get; set; }
    public string    LoginPassword { get; set; }
    public DateTime LoginStartUtc { get; set; }
    public DateTime LoginEndUtc { get; set; }

    public EntityId? OrgUnit { get; set; }
    public string ScreenName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Note { get; set; }

    public DateRange? LockSpanUtc { get; set; }
    public EntityId? LockActor { get; set; }
    public string LockNote { get; set; }

    public DateRange? LoginLockSpanUtc { get; set; }
    public EntityId? LoginLockActor { get; set; }
    public string LoginLockNote { get; set; }

    public ConfigVector Props { get; set; }
    public ConfigVector LoginProps { get; set; }

    public ConfigVector Rights { get; set; }
    public ConfigVector LoginRights { get; set; }

    public string ResultRole { get; set; }
    public ConfigVector ResultRoleConfig { get; set; }//#803
    public ConfigVector ResultRights { get; set; }
    public ConfigVector ResultProps{ get; set; }


    /// <summary> Makes an object which embodies the result of policy application: MinIdp object </summary>
    public virtual MinIdpUserData MakeResult()
    {
      return new MinIdpUserData
      {
        SysId = this.SysId,
        Realm = this.Realm,
        SysTokenData = this.SysTokenData,
        Status = this.Status,
        CreateUtc = this.CreateUtc,
        StartUtc =  this.StartUtc,
        EndUtc   =  this.EndUtc,

        LoginId = this.LoginId,
        LoginPassword = this.LoginPassword,
        LoginStartUtc = this.LoginStartUtc,
        LoginEndUtc = this.LoginEndUtc,

        ScreenName = this.ScreenName,
        Name = this.Name,
        Description = this.Description,
        Note = this.Note,

        Role = this.ResultRole,
        OrgUnit = this.OrgUnit.HasValue ? this.OrgUnit.Value.AsString : null,//#809
        Props = this.ResultProps,
        Rights = this.ResultRights,
      };
    }
  }
}
