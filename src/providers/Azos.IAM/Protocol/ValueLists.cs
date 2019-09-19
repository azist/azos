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


    public static class LoginType
    {
      /// <summary> System id: GDID</summary>
      public const string TYPE_SID = "sid";

      /// <summary> Screenname e.g.: "alex1980"; screen names are used in systems that provide email e.g. "alex1980@myservice.com"</summary>
      public const string TYPE_SCREENNAME = "scrn";

      /// <summary> Email address e.g. "alex1980@myservice.com"</summary>
      public const string TYPE_EMAIL = "eml";

      /// <summary> Telephone number e.g. "8885552223413"</summary>
      public const string TYPE_PHONE = "tel";

      /// <summary> Login via OAuth/OpenID connect through 3rd party (e.g. Twitter account)</summary>
      public const string TYPE_OAUTH = "oauth";

      /// <summary> Bearer token issued by the system </summary>
      public const string TYPE_BEARER = "bear";
    }
  }
}
