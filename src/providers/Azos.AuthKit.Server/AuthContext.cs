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

namespace Azos.AuthKit.Server
{
  /// <summary>
  /// Provides transitive data used between various methods calls during user authentication.
  /// The class is paired with IIdpHandlerLogic which makes a new instance of this or
  /// appropriate derivative class.
  /// Your auth system may extend this object also overriding <see cref="IIdpHandlerLogic.MakeNewUserAuthenticationContext(Atom, AuthenticationRequestContext)"/>
  /// </summary>
  public class AuthContext
  {
    public AuthContext(Atom realm, AuthenticationRequestContext ctx)
    {
      Realm = realm.IsTrue(v => !v.IsZero && v.IsValid, "Valid realm");
      RequestContext = ctx;
    }

    public readonly Atom Realm;
    public readonly AuthenticationRequestContext RequestContext;

    public bool HasResult{ get; set; }

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

    public string ScreenName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Note { get; set; }

    public ConfigVector Props { get; set; }
    public ConfigVector LoginProps { get; set; }

    public ConfigVector Rights { get; set; }
    public ConfigVector LoginRights { get; set; }


    public string ResultRole { get; set; }
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
        Props = this.ResultProps,
        Rights = this.ResultRights,
      };
    }
  }
}
