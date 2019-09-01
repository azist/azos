using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data;
using Azos.Data.Business;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Finds entities on their traits
  /// </summary>
  public sealed class EntityListTraitFilter : FilterModel<IEnumerable<EntityInfo>>
  {
    [Inject] IAdminLogic m_Logic;

    [Field(required: true, description: "A C-like expression of a form: `(name=value || name=value)`")]
    public string FilterExpression {  get; set; }

    protected override async Task<SaveResult<IEnumerable<EntityInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<EntityInfo>>(await m_Logic.FilterAsync(this));
  }

  public sealed class EntityInfo : TransientModel
  {
    [Field]
    public string EntityName { get; set; }

    [Field]
    public GDID G_Entity { get; set; }
  }
}
