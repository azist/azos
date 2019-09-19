using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Defines size limits/length constraints for entity fields/collections etc.
  /// </summary>
  public static class ValueLists
  {
    public const string ENTITY_VALUE_LIST = "A:Account,G:Group,I:Index,L:Login,P:Policy,R:Role,T:Token";

    public const string ENTITY_VERSION_STATUS_VALUE_LIST = "C:Created,U:Updated,D:Deleted";

    public const string ACCOUNT_TYPE_VALUE_LIST = "H:Human,S:Service,G:Group,O:Organization,S:System";
    public const string ACCOUNT_LEVEL_VALUE_LIST = "I:Invalid,U:User,A:Admin,S:System";
  }
}
