/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.AuthKit
{
  public static class Constraints
  {
    public const string CONFIG_CLAIMS_SECTION = "claims";// props{  claims{ pub{...} } }
    public const string CONFIG_PUBLIC_SECTION = "pub";

    public const int ENTITY_ID_MAX_LEN = 256;

    public const int USER_NAME_MIN_LEN = 1;
    public const int USER_NAME_MAX_LEN = 64;

    public const int USER_DESCR_MIN_LEN = 1;
    public const int USER_DESCR_MAX_LEN = 128;

    public const int LOGIN_ID_MAX_LEN = 700;

    public const int LOGIN_PWD_MIN_LEN = 2;// { }
    public const int LOGIN_PWD_MAX_LEN = 2048;// { }


    public const int RIGHTS_MIN_LEN = 6; // {r:{}}
    public const int RIGHTS_MAX_LEN = 256 * 1024;

    public const int PROPS_MIN_LEN = 6; // {r:{}}
    public const int PROPS_MAX_LEN = 128 * 1024;

    public const int DESCRIPTION_MAX_LEN = 128;
    public const int NOTE_SHORT_MAX_LEN = 256;
    public const int NOTE_MAX_LEN = 4 * 1024;

    public const int CALLER_ADDR_MAX_LEN = 64;
    public const int CALLER_AGENT_MAX_LEN = 256;
  }
}
