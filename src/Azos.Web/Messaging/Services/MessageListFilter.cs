/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.AST;
using Azos.Data.Business;

namespace Azos.Web.Messaging.Services
{
  /// <summary>
  /// Filter object submitted to server for message list search
  /// </summary>
  public sealed class MessageListFilter : FilterModel<IEnumerable<MessageInfo>>
  {
    [Field(Description = "ArchiveId used for storage")]
    public string ArchiveId { get; set; }

    [Field(Description = "Message ID")]
    public string ID { get; set; }

    [Field(Description = "Message RelatedID")]
    public string RelatedID { get; set; }

    [Field(Description = "Provides an optional complex expression tree applied to message tag list")]
    public Expression TagFilter { get; set;}

    [Inject] IMessageArchiveLogic m_Archive;

    protected async override Task<SaveResult<IEnumerable<MessageInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<MessageInfo>>(await m_Archive.GetMessageListAsync(this).ConfigureAwait(false));
  }
}
