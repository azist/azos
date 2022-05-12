/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;

using static Azos.Canonical;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Provides persisted model for forest tree node.
  /// This class is usually submitted to server via HTTP Post or Put to make persisted changes
  /// in the config forest data store
  /// </summary>
  [Bix("a5950275-e12f-4f6c-83b7-1f6862ac3308")]
  [Schema(Description = "Provides persisted model for forest tree node")]
  ////[UniqueSequence("azos", "forest")]
  public sealed class TreeNode : PersistedEntity<IForestSetupLogic, ChangeResult>
  {
    /// <summary>
    /// Returns a forest id (EntityId.System) of the forest which contains this tree which contains this node
    /// </summary>
    [Field(required: true, Description = "Returns a forest id (EntityId.System) of the forest which contains this tree which contains this node")]
    public Atom Forest { get; set; }

    /// <summary>
    /// Returns tree id which contains this node
    /// </summary>
    [Field(required: true, Description = "Returns tree id which contains this node")]
    public Atom Tree { get; set; }

    public override EntityId Id => !Forest.IsZero && Forest.IsValid
                                      ? new EntityId(Forest, Tree, Constraints.SCH_GNODE, this.Gdid.ToString())
                                      : EntityId.EMPTY;

    [Field(Description = "Parent node id, root has GDID.ZERO")]
    public GDID G_Parent { get; set; }

    /// <summary>
    /// The path segment of this node relative to its parent node
    /// </summary>
    [Field(required: true,
           minLength: Constraints.SEGMENT_MIN_LEN,
           maxLength: Constraints.SEGMENT_MAX_LEN,
           Description = "The path segment of this node relative to its parent node")]
    public string PathSegment { get; set; }

    /// <summary>
    /// UTC timestamp when this record becomes effective, that is - logically starts to apply
    /// </summary>
    [Field(required: true, Description = "UTC timestamp when this record becomes effective. " +
     "Do not confuse with Utc version date. Version date specifies when the change was made, whereas" +
     " StartUtc captures a point in time when entity starts to logically apply/exist." +
     " Business processes are interested in records arranged by StartUtc in most cases.")]
    public DateTime? StartUtc { get; set; }

    /// <summary>
    /// Configuration content for entity properties <seealso cref="ConfigVector"/>
    /// Entity properties belong to this node and do not get inherited
    /// </summary>
    [Field(required: true,
           minLength: Constraints.CONFIG_MIN_LEN,
           maxLength: Constraints.CONFIG_MAX_LEN,
           Description = "Entity properties")]
    public ConfigVector Properties { get; set; }

    /// <summary>
    /// Configuration content for this node level <seealso cref="ConfigVector"/>
    /// Node config overrides config from parent nodes
    /// </summary>
    [Field(required: true,
           minLength: Constraints.CONFIG_MIN_LEN,
           maxLength: Constraints.CONFIG_MAX_LEN,
           Description = "Configuration content for this node level. Config is inherited from parent levels")]
    public ConfigVector Config { get; set; }

    protected override ValidState DoBeforeValidateOnSave()
    {
      var result = new ValidState(DataStoreTargetName, ValidErrorMode.Batch);

      if (G_Parent == GDID.ZERO)
      {
        if (FormMode == FormMode.Insert)
        {
          result = new ValidState(result, new DocValidationException(nameof(TreeNode), "Root tree node insert is prohibited. May only update root nodes"));
        }

        if (PathSegment != Constraints.VERY_ROOT_PATH_SEGMENT)
        {
          result = new ValidState(result, new FieldValidationException(this, nameof(PathSegment), $"Value must be `{Constraints.VERY_ROOT_PATH_SEGMENT}` for root tree node"));
        }

        if (!this.Gdid.IsZero && this.Gdid != Constraints.G_VERY_ROOT_NODE)
        {
          result = new ValidState(result, new FieldValidationException(this, nameof(Gdid), $"Gdid field must either be null or `{Constraints.G_VERY_ROOT_NODE}` for the root tree node"));
        }

        if (this.Gdid.IsZero) this.Gdid = Constraints.G_VERY_ROOT_NODE;
      }
      else
      {
        if(this.Gdid == Constraints.G_VERY_ROOT_NODE)
        {
          result = new ValidState(result, new FieldValidationException(this, nameof(Gdid), $"Parent modification for very root node is prohibited"));
        }
      }
      return result;
    }

    protected override async Task<ValidState> DoAfterValidateOnSaveAsync(ValidState state)
    {
      var result = await base.DoAfterValidateOnSaveAsync(state).ConfigureAwait(false);
      if (result.ShouldContinue)
      {
        state = await m_SaveLogic.ValidateNodeAsync(this, state).ConfigureAwait(false);
      }
      return result;
    }

    protected override Task DoBeforeSaveAsync()
    {
      // Not needed as we override the logic below because we generate gdid differently here using forest ns
      ////await base.DoBeforeSaveAsync().ConfigureAwait(false);

      //Generate new GDID only AFTER all checks are passed not to waste gdid instance
      //in case of validation errors
      if (FormMode == FormMode.Insert && m_GdidGenerator != null)
      {
        do Gdid = m_GdidGenerator.Provider.GenerateOneGdid(Constraints.ID_NS_CONFIG_FOREST_PREFIX + Forest.Value, Tree.Value);
        while(
           (Gdid == Constraints.G_VERY_ROOT_NODE) ||
           (Gdid.Authority == Constraints.GDID_RESERVED_ID_AUTHORITY && Gdid.Counter < Constraints.GDID_RESERVED_ID_COUNT));//skip the reserved value for root node gdid
      }

      return Task.CompletedTask;
    }

    protected override async Task<ChangeResult> SaveBody(IForestSetupLogic logic)
     => await logic.SaveNodeAsync(this).ConfigureAwait(false);

  }
}
