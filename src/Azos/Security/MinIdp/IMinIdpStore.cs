/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Time;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Outlines the contract for stores serving the underlying IDP data for MinIdp (Minimum identity provider)
  /// </summary>
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
  /// Sets contract for DTO - data stored in MinIdp system
  /// </summary>
  public sealed class MinIdpUserData
  {
    public SysAuthToken SysToken => new SysAuthToken(Realm.Value, SysId.ToString());

    public ulong SysId        { get; set; }//tbl_user.pk <--- clustered primary key BIGINT
    public Atom  Realm        { get; set; }//tbl_user.realm  vchar(8)
    public UserStatus Status  { get; set; }//tbl_user.stat tinyint 1 byte
    public DateTime CreateUtc { get; set; }//tbl_user.cd
    public DateTime StartUtc  { get; set; }//tbl_user.sd
    public DateTime EndUtc    { get; set; }//tbl_user.ed

    public string LoginId         { get; set; }//tbl_login.id    vchar(36)
    public string LoginPassword   { get; set; }//tbl_login.pwd   vchar(2k) -- contains PWD JSON
    public DateTime? LoginStartUtc { get; set; }//tbl_login.sd
    public DateTime? LoginEndUtc   { get; set; }//tbl_login.ed


    public string ScreenName  { get; set; }//tbl_user.screenName vchar(36)
    public string Name        { get; set; }//tbl_user.name   vchar(64)
    public string Description { get; set; }//tbl_user.descr  vchar(96)
    public string Role        { get; set; }//tbl.role.id   vchar 25
    public Rights Rights      { get; set; }//tbl_role.rights  blob (256k)
    public string Note        { get; set; }//tbl_user.note  blob (4k)
  }

}