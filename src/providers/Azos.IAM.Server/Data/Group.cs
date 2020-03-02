using Azos.Data;
using Azos.IAM.Protocol;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Represents a set of accounts, a unit of account hierarchical organization.
  /// </summary>
  public sealed class Group : EntityWithRights
  {
    [Field(required: false,
           description: "Points to parent group if this is a child group or Zero",
           metadata: "idx{name='main' order=0 unique=true dir=asc}")]
    [Field(typeof(Group), nameof(G_Parent), TMONGO, backendName: "g_par")]
    public GDID G_Parent { get; set;}

    [Field(required: true,
           kind: DataKind.ScreenName,
           minLength: Sizes.ENTITY_ID_MIN,
           maxLength: Sizes.ENTITY_ID_MAX,
           metadata: "idx{name='main' order=1 unique=true dir=asc}")]
    [Field(typeof(Group), nameof(ID), TMONGO, backendName: "id")]
    public string ID { get; set; }

    [Field(required: false, description: "Points to policy affecting this group; if null then policy is taken from parent group")]
    [Field(typeof(Group), nameof(G_Policy), TMONGO, backendName: "g_pol")]
    public GDID G_Policy { get; set; }
  }

}
