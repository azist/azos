
namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Defines size limits/length constraints for entity fields/collections etc.
  /// </summary>
  public static class Sizes
  {
    public const int NAME_MIN = 3;
    public const int NAME_MAX = 32;

    public const int DESCRIPTION_MAX = 256;
    public const int NOTE_MAX = 4 * 1024;

    public const int ENTITY_ID_MIN = 3;
    public const int ENTITY_ID_MAX = 32;

    public const int ACCOUNT_TITLE_MAX = 32;

    public const int LOGIN_ID_MIN = 3;
    public const int LOGIN_ID_MAX = 1024;//this may store bearer token

    public const int LOGIN_ID_SCREENNAME_MAX = 32;
    public const int LOGIN_ID_EMAIL_MAX = 128;
    public const int LOGIN_ID_PHONE_MAX = 48;

    public const int LOGIN_PWD_MIN = 2;//{}
    public const int LOGIN_PWD_MAX = 1024;

    public const int LOGIN_PWD_HISTORY_COUNT_MAX = 64;

    public const int USER_AGENT_MAX = 128;
    public const int HOST_MAX = 64;


    public const int PROPERTY_COUNT_MAX = 256;
    public const int PROPERTY_NAME_MAX = 80;
    public const int PROPERTY_VALUE_MAX = 256;


    public const int RIGHTS_DATA_MAX = 256 * 1024;

    public const int TRAIT_NAME_MAX = PROPERTY_NAME_MAX;
    public const int TRAIT_VALUE_MAX = PROPERTY_VALUE_MAX;


    public const int EXT_PROVIDER_COUNT_MAX = 64;
    public const int EXT_PROVIDER_NAME_MAX  = 32;
    public const int EXT_PROVIDER_VALUE_MAX = 1024;//may need long for key
  }
}
