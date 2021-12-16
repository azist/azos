/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Wave.Mvc;

namespace Azos.AuthKit.Server.Web
{
  [NoCache]
  public sealed class UserCore : ApiProtocolController
  {
    [Inject] IIdpUserCoreLogic m_Logic;

    [ActionOnPost(Name = "filter"), AcceptsJson]
    public async Task<object> PostUserFilter(UserListFilter filter)
      => await ApplyFilterAsync(filter).ConfigureAwait(false);

    [ActionOnPost()]
    public async Task<object> PostUserEntity(UserEntity user)
      => await SaveNewAsync(user).ConfigureAwait(false);

    [ActionOnPut()]
    public async Task<object> PutUserEntity(UserEntity user)
    => await SaveEditAsync(user).ConfigureAwait(false);
  }
}
