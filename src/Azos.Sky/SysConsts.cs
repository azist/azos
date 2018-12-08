/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Reflection;

namespace Azos.Sky
{

  /// <summary>
  /// Defines system constants. Do not localize!
  /// </summary>
  public static class SysConsts
  {
    /// <summary>
    /// Defines maximum allowed clock drift in the cluster system.
    /// Certain algorithms depend on this value for ordering and avoidance of locking logic.
    /// The injected time source has to guarantee that all clocks on all machines in the system
    /// differ by at most this number of milliseconds.
    /// </summary>
    public const int MAX_ALLOWED_CLOCK_DRIFT_MS = 2000;

    /// <summary>
    /// Provides a safe margin that various cluster algorithms have to consider to avoid locking
    /// and other ordering optimizations
    /// </summary>
    public const int CLOCK_DRIFT_SAFE_MARGIN_MS = 2 * MAX_ALLOWED_CLOCK_DRIFT_MS;



    public const string UNKNOWN_ENTITY = "<unknown>";
    public const string NULL = "<null>";

    public const string GDID_NS_DYNAMIC_HOST = "SYS-SkyDynamicHost";
    public const string GDID_NAME_DYNAMIC_HOST = "DYNHOST";

    public const string GDID_NS_WORKER = "SYS-SkyWORKER";
    public const string GDID_NAME_WORKER_TODO = "TODO";
    public const string GDID_NAME_WORKER_PROCESS = "PROCESS";
    public const string GDID_NAME_WORKER_SIGNAL = "SIGNAL";

    public const string GDID_NS_MESSAGING = "SYS-SkyMESSAGING";
    public const string GDID_NAME_MESSAGING_MESSAGE = "MESSAGE";

    /// <summary>
    /// The name of application config root section that sky-related components should nest under.
    /// This is needed not to cause an collision in inner section names with possible existing root-level non-sky names
    /// </summary>
    public const string APPLICATION_CONFIG_ROOT_SECTION = "sky";

    public const string EXT_PARAM_GROUP_METABASE = "metabase";
    public const string EXT_PARAM_GROUP_WORKER = "worker";

    public const string LOG_TOPIC_METABASE = "mtbs";
    public const string LOG_TOPIC_ID_GEN = "idgen";
    public const string LOG_TOPIC_APP_MANAGEMENT = "AppMgmt";
    public const string LOG_TOPIC_HOST_GOV = "hgov";
    public const string LOG_TOPIC_ZONE_GOV = "zgov";
    public const string LOG_TOPIC_LOCKING = "lck";
    public const string LOG_TOPIC_INSTRUMENTATION = "instr";
    public const string LOG_TOPIC_WWW = "www";
    public const string LOG_TOPIC_SVC = "svc";
    public const string LOG_TOPIC_LOCALIZATION = "loclz";
    public const string LOG_TOPIC_MDB = "mdb";
    public const string LOG_TOPIC_KDB = "kdb";
    public const string LOG_TOPIC_DYNHOST_GOV = "dynh";
    public const string LOG_TOPIC_HOST_SET = "hostSet";
    public const string LOG_TOPIC_WORKER = "wrkr";
    public const string LOG_TOPIC_TODO = "todo";
    public const string LOG_TOPIC_PM = "pman";
    public const string LOG_TOPIC_WMSG = "wmsg";


    /// <summary>
    /// Supplied in command line from ARD to signify that AHGOV was launched by ARD
    /// </summary>
    public const string ARD_PARENT_CMD_PARAM = "ARDPARENT";

    /// <summary>
    /// Supplied in command line from ARD to signify that AHGOV was launched by ARD and last update failed
    /// </summary>
    public const string ARD_UPDATE_PROBLEM_CMD_PARAM = "UPDATEPROBLEM";

    public const string HGOV_ARD_DIR = "ard";
    public const string HGOV_RUN_NETF_DIR = "run-netf";
    public const string HGOV_RUN_CORE_DIR = "run-core";
    public const string HGOV_UPDATE_DIR = "upd";
    public const string HGOV_UPDATE_FINISHED_FILE = "update.finished";
    public const string HGOV_UPDATE_FINISHED_FILE_OK_CONTENT = "OK.";


    public static readonly string[] APP_NAMES_FORBIDDEN_ON_DYNAMIC_HOSTS = { SysConsts.APP_NAME_GDIDA, SysConsts.APP_NAME_ZGOV };

    public const string APP_NAME_HGOV = "ahgov";
    public const string APP_NAME_ZGOV = "azgov";
    public const string APP_NAME_GDIDA = "agdida";
    public const string APP_NAME_WS = "aws";
    public const string APP_NAME_SH = "ash";
    public const string APP_NAME_PH = "aph";
    public const string APP_NAME_LOG = "alog";

    public const string DEFAULT_WORLD_REGION_PATH = "/World.r";
    public const string DEFAULT_WORLD_GLOBAL_NOC_PATH = DEFAULT_WORLD_REGION_PATH + "/Global.noc";
    public const string DEFAULT_WORLD_GLOBAL_ZONE_PATH = DEFAULT_WORLD_GLOBAL_NOC_PATH + "/Default.z";

    public const string DEFAULT_APP_CONFIG_ROOT = "application";

    public const int REMOTE_TERMINAL_TIMEOUT_MS = 10 * //min
                                                  60 * //sec
                                                  1000; //msec

    //Default Bindings
    public const string APTERM_BINDING = "apterm";
    public const string SYNC_BINDING   = "sync";
    public const string ASYNC_BINDING  = "async";

    public const string DEFAULT_BINDING = ASYNC_BINDING;

    public const int APP_TERMINAL_PORT_OFFSET = 16;

    public const int DEFAULT_ZONE_GOV_WEB_PORT = 8081;
    public const int DEFAULT_ZONE_GOV_SVC_SYNC_PORT = 2000;
    public const int DEFAULT_ZONE_GOV_SVC_ASYNC_PORT = 2001;
    public const int DEFAULT_ZONE_GOV_APPTERM_PORT = DEFAULT_ZONE_GOV_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;

    public const int DEFAULT_HOST_GOV_WEB_PORT = 8082;
    public const int DEFAULT_HOST_GOV_SVC_SYNC_PORT = 3000;
    public const int DEFAULT_HOST_GOV_SVC_ASYNC_PORT = 3001;
    public const int DEFAULT_HOST_GOV_APPTERM_PORT = DEFAULT_HOST_GOV_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;

    public const int DEFAULT_GDID_AUTH_WEB_PORT = 8083;
    public const int DEFAULT_GDID_AUTH_SVC_SYNC_PORT = 4000;
    public const int DEFAULT_GDID_AUTH_SVC_ASYNC_PORT = 4001;
    public const int DEFAULT_GDID_AUTH_APPTERM_PORT = DEFAULT_GDID_AUTH_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;

    public const int DEFAULT_ASH_WEB_PORT = 8084;
    public const int DEFAULT_ASH_APPTERM_PORT = 5000 + APP_TERMINAL_PORT_OFFSET;
    public const int DEFAULT_ASH_SVC_BASE_SYNC_PORT = 6000;
    public const int DEFAULT_ASH_SVC_BASE_ASYNC_PORT = 7000;

    public const int DEFAULT_AWS_WEB_PORT = 8085;
    public const int DEFAULT_AWS_SVC_SYNC_PORT = 8000;
    public const int DEFAULT_AWS_SVC_ASYNC_PORT = 8001;
    public const int DEFAULT_AWS_APPTERM_PORT = DEFAULT_AWS_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;

    public const int DEFAULT_PH_WEB_PORT = 8086;
    public const int DEFAULT_PH_SVC_SYNC_PORT = 9000;
    public const int DEFAULT_PH_SVC_ASYNC_PORT = 9001;
    public const int DEFAULT_PH_APPTERM_PORT = DEFAULT_PH_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;

    public const int DEFAULT_LOG_WEB_PORT = 8087;
    public const int DEFAULT_LOG_SVC_SYNC_PORT = 10000;
    public const int DEFAULT_LOG_SVC_ASYNC_PORT = 10001;
    public const int DEFAULT_LOG_APPTERM_PORT = DEFAULT_LOG_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;

    public const int DEFAULT_TELEMETRY_WEB_PORT = 8088;
    public const int DEFAULT_TELEMETRY_SVC_SYNC_PORT = 11000;
    public const int DEFAULT_TELEMETRY_SVC_ASYNC_PORT = 11001;
    public const int DEFAULT_TELEMETRY_APPTERM_PORT = DEFAULT_TELEMETRY_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;

    public const int DEFAULT_WEB_MESSAGE_SYSTEM_WEB_PORT = 8089;
    public const int DEFAULT_WEB_MESSAGE_SYSTEM_SVC_SYNC_PORT  = 12000;
    public const int DEFAULT_WEB_MESSAGE_SYSTEM_SVC_ASYNC_PORT = 12001;
    public const int DEFAULT_WEB_MESSAGE_SYSTEM_SVC_APPTERM_PORT  = DEFAULT_WEB_MESSAGE_SYSTEM_SVC_SYNC_PORT + APP_TERMINAL_PORT_OFFSET;
    //Network Service Name Resolution

    public const string NETWORK_UTESTING = "utesting"; //used for local loop unit testing
    public const string NETWORK_INTERNOC = "internoc";
    public const string NETWORK_NOCGOV = "nocgov";
    public const string NETWORK_DBSHARD = "dbshard";

    public const string NETWORK_SVC_GDID_AUTHORITY = "gdida";
    public const string NETWORK_SVC_WEB_MANAGER_PREFIX = "webman-";
    public const string NETWORK_SVC_ZGOV_WEB_MANAGER = NETWORK_SVC_WEB_MANAGER_PREFIX + APP_NAME_ZGOV;
    public const string NETWORK_SVC_HGOV_WEB_MANAGER = NETWORK_SVC_WEB_MANAGER_PREFIX + APP_NAME_HGOV;
    public const string NETWORK_SVC_ZGOVTELEMETRY = "zgovtelemetry";
    public const string NETWORK_SVC_ZGOVLOG = "zgovlog";
    public const string NETWORK_SVC_ZGOVHOSTREG = "zgovhostreg";
    public const string NETWORK_SVC_ZGOVHOSTREPL = "zgovhostrepl";
    public const string NETWORK_SVC_ZGOVLOCKER = "zgovlocker";
    public const string NETWORK_SVC_HGOV = "hgov";
    public const string NETWORK_SVC_HGOVPINGER = "hgovpinger";
    public const string NETWORK_SVC_PROCESSCONTROLLER = "processcontroller";
    public const string NETWORK_SVC_LOGRECEIVER = "logreceiver";
    public const string NETWORK_SVC_TELEMETRYRECEIVER = "telemetryreceiver";
    public const string NETWORK_SVC_TESTER = "tester";
    public const string NETWORK_SVC_WEB_MESSAGE_SYSTEM = "webmessagesystem";

    public const string REGION_MNEMONIC_REGION = "reg";
    public const string REGION_MNEMONIC_NOC = "noc";
    public const string REGION_MNEMONIC_ZONE = "zone";
    public const string REGION_MNEMONIC_HOST = "host";


    public static readonly HashSet<char> NAME_INVALID_CHARS = new HashSet<char>
        {
            (char)0,
            (char)0x0d,
            (char)0x0a,
            Metabase.Metabank.HOST_DYNAMIC_SUFFIX_SEPARATOR,
            '@', '#', ',' , ';' , ':' , '%', '&',
            '/' , '\\' , '\'' , '"' , '|' ,
            '*' , '?',
            '<' , '>',
            '[' , ']',
            '{' , '}',
            '(' , ')',
        };

    public const string CONFIG_ENABLED_ATTR = "enabled";
  }

  /// <summary>
  /// Resolves environment variables using values declared in code in static class SysConstants.
  /// The name must be prepended with "SysConsts." prefix, otherwise the OS resolver is used
  /// </summary>
  public sealed class SystemVarResolver : Conf.IEnvironmentVariableResolver
  {
    public const string PREFIX = "SysConsts.";

    private static SystemVarResolver s_Instance = new SystemVarResolver();

    private SystemVarResolver()
    {

    }

    public static void Bind()
    {
      Conf.Configuration.ProcesswideEnvironmentVarResolver = SystemVarResolver.s_Instance;
    }

    /// <summary>
    /// Returns a singleton class instance
    /// </summary>
    public static SystemVarResolver Instance { get { return s_Instance; } }


    public bool ResolveEnvironmentVariable(string name, out string value)
    {
      value = null;
      if (name.StartsWith(PREFIX))
      {
        var sname = name.Substring(PREFIX.Length);
        var tp = typeof(SysConsts);
        var fld = tp.GetField(sname, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Static);
        if (fld != null)
        {
          var val = fld.GetValue(null);
          if (val != null)
          {
            value = val.ToString();
            return true;
          }
        }
      }
      return false;
    }
  }
}
