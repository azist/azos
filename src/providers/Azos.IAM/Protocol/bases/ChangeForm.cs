
using System;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.JSON;

namespace Azos.IAM.Protocol
{

  /// <summary>
  /// Base form that is submitted to server to change the persisted state.
  /// The descendants are either "commands" that derive directly, or descendants of ChangeForm&lt;TEntity&gt;
  /// </summary>
  public abstract class ChangeForm : PersistedModel<ChangeResult>
  {
    [Field(required: true)] public Guid IdempotencyKey { get; set; }

    [Field(required: true)] public GDID G_Actor { get; set; }
    [Field(required: true)] public string ActorUserAgent { get; set; }
    [Field(required: true)] public string ActorHost { get; set; }
    [Field(required: true)] public string ActionNote { get; set; }

    [Inject] IAdminLogic m_Admin;

    protected override async Task<SaveResult<ChangeResult>> DoSaveAsync() => await m_Admin.ApplyChangeAsync(this);
  }

  /// <summary>
  /// Base form that is submitted to server to change its state
  /// </summary>
  public abstract class ChangeForm<TEntity> : ChangeForm where TEntity : EntityBody
  {
    [Field(required: true, description: "Entity body data")]
    public TEntity Body { get; set; }
  }
}
