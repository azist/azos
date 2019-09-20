using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Business;

namespace Azos.IAM.Protocol
{
  public sealed class GroupListFilter : FilterModel<IEnumerable<GroupInfo>>
  {
    [Inject] IAdminLogic m_Admin;

    [Field(description: "Filters by name pattern")]
    public string Name {  get; set; }

    /// <summary>
    /// When set filters groups by account which is directly connected to it
    /// </summary>
    public GDID G_ChildAccount { get; set; }

    protected override async Task<SaveResult<IEnumerable<GroupInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<GroupInfo>>(await m_Admin.FilterListAsync(this));
  }

  public sealed class GroupInfo : TransientModel
  {
    [Field]
    public GDID GDID { get; set; }

    [Field]
    public GDID G_Parent { get; set; }

    [Field]
    public string Name{  get; set;}

    [Field]
    public DateTime? AsOfDate { get; set; }
  }
}
