/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading.Tasks;

using Azos.Apps;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Outlines the contract for stores which service MinIdp (Minimum identity provider)
  /// </summary>
  public interface IMinIdpStore : IApplicationComponent
  {
    Task<MinIdpUserData> GetByIdAsync(string id);
    Task<MinIdpUserData> GetByUriAsync(string uri);
    Task<MinIdpUserData> GetBySysAsync(SysAuthToken sysToken);
  }


  /// <summary>
  /// Sets contract for DTO - data stored in MinIdp system
  /// </summary>
  public sealed class MinIdpUserData
  {
    public SysAuthToken SysToken => new SysAuthToken(Realm.Value, SysId.ToString());

    public ulong SysId        { get; set; }
    public Atom Realm         { get; set; }
    public UserStatus Status  { get; set; }
    public DateTime CreateUtc { get; set; }
    public DateTime ModifyUtc { get; set; }
    public DateTime EndUtc    { get; set; }
    public string Id          { get; set; }
    public string ScreenName  { get; set; }
    public string Password    { get; set; }
    public string Name        { get; set; }
    public string Description { get; set; }
    public string Role        { get; set; }
    public Rights Rights      { get; set; }
    public string Note        { get; set; }
  }

}
