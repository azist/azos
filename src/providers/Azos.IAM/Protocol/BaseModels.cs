

using Azos.Data;
using Azos.Data.Business;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Base class for entity bodies
  /// </summary>
  public abstract class EntityBody : FragmentModel
  {
    [Field(key: true, required: true, description: "Global Distributed ID of this Entity")]
    public GDID GDID { get; set; }
  }


  /// <summary>
  /// Base form that is submitted to server to change the persisted state.
  /// The descendants are either "commands" that derive directly, or descendants of ChangeForm&lt;TEntity&gt;
  /// </summary>
  public abstract class ChangeForm : PersistedModel<ChangeResult>
  {
    [Field(required: true)] public GDID G_Actor { get; set; }
    [Field(required: true)] public string ActorUserAgent { get; set; }
    [Field(required: true)] public string ActorHost { get; set; }
    [Field(required: true)] public string ActionNote { get; set; }
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
