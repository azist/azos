
using Azos.Data;

namespace Azos.IAM.Protocol
{

  public sealed class RoleForm : ChangeForm<GroupEntityBody>{ }

  public sealed class RoleEntityBody : EntityBodyWithRights
  {
    [Field(required: true,
           kind: DataKind.ScreenName,
           minLength: Sizes.ENTITY_ID_MIN,
           maxLength: Sizes.ENTITY_ID_MAX,
           description: "Unique Role ID. Do not change the ID as all links to it will become invalid")]
    public string ID { get; set; }
  }


}
