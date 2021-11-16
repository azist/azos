/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Provides a transient model (an read-only info) for a forest tree node
  /// </summary>
  [Bix("a2b414f6-9a2d-4031-9458-151cd9edf3e6")]
  public sealed class TreeNodeInfo : TransientModel
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

    /// <summary>
    /// Immutable primary global distributed Id (surrogate pk) for this node within a forest tree
    /// </summary>
    [Field(required: true, key: true, Description = "Immutable primary global distributed Id (surrogate pk) for this node within a forest tree")]
    public GDID Gdid { get; set; }

    /// <summary>
    /// Immutable primary global distributed Id (surrogate pk) for this nodes parent within a forest tree. A root node has a value of GDID.ZERO
    /// </summary>
    [Field(required: true, Description = "Immutable primary global distributed Id (surrogate pk) for this nodes parent within a forest tree. A root node has a value of GDID.ZERO")]
    public GDID G_Parent { get; set; }

    /// <summary>
    /// `EntityId` of `GNODE` schema for this tree node
    /// </summary>
    [Field(required: true, key: true, Description = "`EntityId` of `GNODE` schema for this tree node")]
    public EntityId Id => new EntityId(Forest, Tree, Constraints.SCH_GNODE, Gdid.ToString());

    /// <summary>
    /// `EntityId` of `GNODE` schema for this tree nodes parent if one has a parent
    /// </summary>
    [Field(required: true, key: true, Description = "`EntityId` of `GNODE` schema for this tree nodes parent if one has a parent")]
    public EntityId ParentId => new EntityId(Forest, Tree, Constraints.SCH_GNODE, G_Parent.ToString());

    /// <summary>
    /// The path segment of this node relative to its parent node, see `FullPath`
    /// </summary>
    [Field(required: true, Description = "The path segment of this node relative to its parent node, see `FullPath`")]
    public string PathSegment { get; set; }

    /// <summary>
    /// Full path of this tree node starting from the very root
    /// </summary>
    [Field(required: true, Description = "Full path of this tree node starting from the very root")]
    public string FullPath { get; set; }

    /// <summary>
    /// `EntityId` of `path` schema for this tree node
    /// </summary>
    [Field(required: true, key: true, Description = "`EntityId` of `path` schema for this tree nodes parent if one has a parent")]
    public EntityId FullPathId => new EntityId(Forest, Tree, Constraints.SCH_PATH, FullPath);

    /// <summary>
    /// Version of this node data record
    /// </summary>
    [Field(required: true, Description = "Version of this node data record")]
    public VersionInfo DataVersion { get; set; }

    [Field(required: true, Description = "UTC timestamp when this tree node record becomes effective. " +
     "Do not confuse with Utc version date. Version date specifies when the change was made, whereas" +
     " StartUtc captures a point in time when entity starts to logically apply/exist." +
     " Business processes are interested in records arranged by StartUtc in most cases.")]
    public DateTime StartUtc { get; set; }

    /// <summary>
    /// Entity properties <seealso cref="ConfigVector"/>.
    /// Note: properties belong to this node and do not get inherited from parent nodes
    /// </summary>
    [Field(required: true, Description = "Entity properties")]
    public ConfigVector Properties { get; set; }

    /// <summary>
    /// Configuration for this level of tree hierarchy. <seealso cref="ConfigVector"/>
    /// </summary>
    [Field(required: true, Description = "Configuration for this level of tree hierarchy")]
    public ConfigVector LevelConfig { get; set; }

    /// <summary>
    /// Configuration calculated from hierarchy chain. <seealso cref="ConfigVector"/>
    /// </summary>
    [Field(required: true, Description = "Configuration calculated from hierarchy chain")]
    public ConfigVector EffectiveConfig { get; set; }


    /// <summary>
    /// Clones this instance (by copying appropriate fields) into persisted model
    /// </summary>
    public TreeNode CloneIntoPersistedModel()
    {
      var result = new TreeNode();
      result.Gdid = this.Gdid;
      result.PathSegment = this.PathSegment;
      result.Properties = this.Properties.Content;//a copy
      result.Config = this.LevelConfig.Content;//a copy
      result.StartUtc = this.StartUtc;
      result.Tree = this.Tree;
      result.Forest = this.Forest;
      return result;
    }
  }
}
