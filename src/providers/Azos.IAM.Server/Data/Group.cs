using Azos.Data;

namespace Azos.IAM.Server.Data
{
  /// <summary>
  /// Represents a set of users, a unit of user hierarchy organization.
  /// </summary>
  public sealed class Group : EntityWithRights
  {
    [Field(required: false,
           description: "Points to parent group if this is a child group or Zero",
           metadata: "idx{name='main' order=0 unique=true dir=asc}")]
    [Field(typeof(Group), nameof(G_Parent), TMONGO, backendName: "g_par")]
    public GDID G_Parent { get; set;}

    [Field(required: true, kind: DataKind.ScreenName, metadata: "idx{name='main' order=1 unique=true dir=asc}")]
    [Field(typeof(Group), nameof(ID), TMONGO, backendName: "id")]
    public string ID { get; set; }

    [Field(required: false, description: "Points to policy affecting this group; if null then policy is taken from parent group")]
    [Field(typeof(Group), nameof(G_Policy), TMONGO, backendName: "g_pol")]
    public GDID G_Policy { get; set; }
  }

}
