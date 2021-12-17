/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.AuthKit
{
  public static class Constraints
  {
    /// <summary>
    /// Gdid generation namespace
    /// </summary>
    public const string ID_NS_AUTHKIT = "sky.akit";
    public const string ID_SEQ_USER  = "user";
    public const string ID_SEQ_LOGIN = "login";

    //EntityIds
    public static readonly Atom SYS_AUTHKIT = Atom.Encode("sky-auth");
    public static readonly Atom SCH_GDID    = Atom.Encode("gdid");
    public static readonly Atom SCH_ID      = Atom.Encode("id");
    public static readonly Atom ETP_USER    = Atom.Encode("user");
    public static readonly Atom ETP_LOGIN   = Atom.Encode("login");


    /// <summary>
    /// AuthKit event namespace
    /// </summary>
    public const string EVT_NS_AUTHKIT = "aukit";

    /// <summary>
    /// Name of the queue for login-related events
    /// </summary>
    public const string EVT_QUEUE_LOGIN = "login";

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

    public const int PROVIDER_DATA_MAX_LEN = 256 * 1024;

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
