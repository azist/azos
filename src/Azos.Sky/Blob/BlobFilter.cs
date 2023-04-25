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
using Azos.Data;
using Azos.Data.AST;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.Sky.Blob
{
  [Bix("5aeda833-27b1-4ce5-b499-898bd3daabe4")]
  [Schema(Description = "Provides model for filtering files")]
  public sealed class BlobFilter : FilterModel<IEnumerable<BlobInfo>>
  {
    /// <summary>
    /// File space
    /// </summary>
    [Field(required: true, Description = "File space")]
    public Atom Space { get; set; }

    /// <summary>
    /// File volume
    /// </summary>
    [Field(required: true, Description = "File volume")]
    public Atom Volume { get; set; }

    /// <summary>
    /// Gets specific file by its GDID
    /// </summary>
    [Field(description: "File header GDID")]
    public GDID Gdid { get; set; }

    [Field(description: "Tag filter expression tree")]
    public Expression TagFilter { get; set; }

    [InjectModule] IBlobStoreLogic m_Logic;

    protected async override Task<SaveResult<IEnumerable<BlobInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<BlobInfo>>(await m_Logic.FindBlobsAsync(this).ConfigureAwait(false));


  }
}
