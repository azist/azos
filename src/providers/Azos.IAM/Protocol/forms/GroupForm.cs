using System;
using System.Threading.Tasks;

using Azos.Data;

namespace Azos.IAM.Protocol
{

  public sealed class GroupForm : ChangeForm<GroupEntityBody>{ }

  public sealed class GroupEntityBody : EntityBodyWithRights
  {
    [Field(required: false,
         description: "Points to parent group if this is a child group or Zero")]
    public GDID G_Parent { get; set; }

    [Field(required: true,
           kind: DataKind.ScreenName,
           minLength: Sizes.ENTITY_ID_MIN,
           maxLength: Sizes.ENTITY_ID_MAX)]
    public string ID { get; set; }

    [Field(required: false, description: "Points to policy affecting this group; if null then policy is taken from parent group")]
    public GDID G_Policy { get; set; }
  }


}
