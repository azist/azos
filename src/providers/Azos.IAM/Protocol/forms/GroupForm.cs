using System;
using System.Threading.Tasks;

using Azos.Data;

namespace Azos.IAM.Protocol
{

  public sealed class GroupForm : ChangeForm<GroupForm.Data>
  {
    public sealed class Data : EntityBody
    {
      public GDID G_Parent {  get; set; }
      public string ID     {  get; set; }
      public GDID G_Policy {  get; set; }
    }
  }


}
