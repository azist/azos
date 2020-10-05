/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.Bix;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Outlines the contract for stores serving the underlying IDP data for MinIdp (Minimum identity provider)
  /// </summary>
  /// <remarks>
  /// This interface provides a consumption-only channel for building MinIdp security managers.
  /// The admin/data change functionality is purposely left out from this contract and is typically
  /// provided by specific implementations of IMinIdpStore using externally callable command pattern
  /// </remarks>
  public interface IMinIdpStore : IApplicationComponent
  {
    Task<MinIdpUserData> GetByIdAsync(Atom realm, string id);
    Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri);
    Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken);
  }

  /// <summary>
  /// Outlines the contract for stores serving the underlying IDP data for MinIdp (Minimum identity provider)
  /// </summary>
  public interface IMinIdpStoreImplementation : IMinIdpStore, IDaemon
  {
  }


  /// <summary>
  /// Sets contract for DTO - data stored in MinIdp system. This class is not meant to be exposed
  /// in various public contexts as it represents and internal data tuple for MinIdp implementation
  /// </summary>
  [Bix("21AF1418-ED04-40E7-9B71-0B1EAAA8AE33")]
  [Schema(Description = @"Sets contract for DTO - data stored in MinIdp system. This doc is not meant to be exposed 
  in various public contexts as it represents and internal data tuple for MinIdp implementation")]
  public sealed class MinIdpUserData : TypedDoc
  {
    public SysAuthToken SysToken => new SysAuthToken(Realm.Value, SysId.ToString());

    [Field] public ulong SysId        { get; set; }//tbl_user.pk <--- clustered primary key BIGINT
    [Field] public Atom  Realm        { get; set; }//tbl_user.realm  vchar(8)
    [Field] public UserStatus Status  { get; set; }//tbl_user.stat tinyint 1 byte
    [Field] public DateTime CreateUtc { get; set; }//tbl_user.cd
    [Field] public DateTime StartUtc  { get; set; }//tbl_user.sd
    [Field] public DateTime EndUtc    { get; set; }//tbl_user.ed

    [Field] public string    LoginId       { get; set; }//tbl_login.id    vchar(36)
    [Field] public string    LoginPassword { get; set; }//tbl_login.pwd   vchar(2k) -- contains PWD JSON
    [Field] public DateTime? LoginStartUtc { get; set; }//tbl_login.sd
    [Field] public DateTime? LoginEndUtc   { get; set; }//tbl_login.ed

    [Field] public string ScreenName  { get; set; }//tbl_user.screenName vchar(36)
    [Field] public string Name        { get; set; }//tbl_user.name   vchar(64)
    [Field] public string Description { get; set; }//tbl_user.descr  vchar(96)
    [Field] public string Role        { get; set; }//tbl.role.id   vchar 25
    [Field] public string Rights      { get; set; }//tbl_role.rights  blob (256k)
    [Field] public string Note        { get; set; }//tbl_user.note  blob (4k)
  }

}