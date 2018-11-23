/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos
{
  /// <summary>
  /// Central non-localizable Azos system constants
  /// </summary>
  public static class CoreConsts
  {
      public const string NULL_STRING = "<null>";

      public const int ABS_HASH_MASK = 0x7FFFFFFF;

      public const int MAX_BYTE_BUFFER_SIZE = 2147483647 - 256 - 15; // 2 Gbyte - 256 (reserved for object headers etc.) - 15 bytes (16-aligned)

      public const string CSRF_TOKEN_NAME = "__CSRFToken";

      public static readonly Geometry.LatLng DEFAULT_GEO_LOCATION = new Geometry.LatLng("41.4996374,-81.6936649", "Cleveland, OH, USA");

    #region External parameters
      public const string EXT_PARAM_CONTENT_LACONIC = "laconic://";
      public const string EXT_PARAM_CONTENT_JSON = "json://";

      public const string EXT_PARAM_GROUP_PILE = "pile";
      public const string EXT_PARAM_GROUP_GLUE = "glue";
      public const string EXT_PARAM_GROUP_LOCKING = "locking";
      public const string EXT_PARAM_GROUP_MESSAGING = "messaging";
      public const string EXT_PARAM_GROUP_OBJSTORE = "objectstore";
      public const string EXT_PARAM_GROUP_WEB = "web";
      public const string EXT_PARAM_GROUP_INSTRUMENTATION = "instrumentation";
      public const string EXT_PARAM_GROUP_CACHE = "cache";
      public const string EXT_PARAM_GROUP_DATA = "data";
      public const string EXT_PARAM_GROUP_LOG = "log";
      public const string EXT_PARAM_GROUP_TIME = "time";
      public const string EXT_PARAM_GROUP_PAY = "pay";
      public const string EXT_PARAM_GROUP_SHIPPING = "shipping";
      public const string EXT_PARAM_GROUP_SECURITY = "security";
      public const string EXT_PARAM_GROUP_SOCIAL = "social";
    #endregion

    #region Code Analysis
      public const string CS_LANGUAGE = "C#";
      public const string CS_EXTENSION = ".cs";

      public const string JS_LANGUAGE = "JavaScript";
      public const string JS_EXTENSION = ".js";

      public const string UNKNOWN = "<unknown>";

      public const string UNNAMED_MEMORY_BUFFER = "<unnamed memory buffer>";
      public const string UNNAMED_PROJECT_ITEM = "<unnamed project item>";
    #endregion

    #region Categories
      public const string DATA_CATEGORY = "Data";
      public const string PRESENTATION_CATEGORY = "Presentation";
      public const string SECURITY_CATEGORY = "Security";
      public const string INTERACTIONS_CATEGORY = "Interaction";
      public const string VALIDATION_CATEGORY = "Validation";

      public const string CHANGE_EVENTS_CATEGORY = "Change";

      public const string FORMULA_FIELD_NAME_PREFIX = "::";
    #endregion

    #region Topics
      //note: Topics must be short as they are emitted into every log line
      public const string LOG_NET_TOPIC = "NET";
      public const string APPLICATION_TOPIC = "App";
      public const string DATA_TOPIC = "Data";
      public const string LOG_TOPIC = "Log";
      public const string COLLECTIONS_TOPIC = "Coll";
      public const string INSTRUMENTATION_TIMEFRAME_TOPIC = "inst.timeframe";
      public const string OBJSTORE_TOPIC = "Objstr";
      public const string INSTRUMENTATION_TOPIC = "Instr";
      public const string THROTTLINGSVC_TOPIC = "Thrtl";
      public const string SCHEDULE_TOPIC = "Sched";//Scheduled jobs, i.e. cleanup files etc...
      public const string DEBUG_TOPIC = "Debug";
      public const string TRACE_TOPIC = "Trace";
      public const string SECURITY_TOPIC = "Sec";
      public const string ASSERT_TOPIC = "Assert";
      public const string TIME_TOPIC = "Time";
      public const string GLUE_TOPIC = "Glue";
      public const string ERLANG_TOPIC = "Erl";
      public const string CACHE_TOPIC = "Cache";
      public const string LOCALIZATION_TOPIC = "Lcl";
      public const string PILE_TOPIC = "Pile";
      public const string IO_TOPIC = "IO";
      public const string WEBMSG_TOPIC = "WMsg";
      public const string RUN_TOPIC = "Run";

      public const string LOG_CHANNEL_SECURITY = "security";

      public const string ISO_LANG_ENGLISH = "eng";
      public const string ISO_LANG_RUSSIAN = "rus";
      public const string ISO_LANG_SPANISH = "spa";
      public const string ISO_LANG_GERMAN  = "deu";
      public const string ISO_LANG_FRENCH  = "fre";

      public const string ISO_CURRENCY_USD  = "usd";
      public const string ISO_CURRENCY_EUR  = "eur";
      public const string ISO_CURRENCY_MXN  = "mxn";
      public const string ISO_CURRENCY_RUB  = "rub";

      public const string ISO_COUNTRY_USA      = "usa";
      public const string ISO_COUNTRY_RUSSIA   = "rus";
      public const string ISO_COUNTRY_GERMANY  = "deu";
      public const string ISO_COUNTRY_MEXICO   = "mex";
      public const string ISO_COUNTRY_CANADA   = "can";
      public const string ISO_COUNTRY_FRANCE   = "fra";
    #endregion

    #region Units
      public const string UNIT_NAME_OCCURENCE = "occr.";
      public const string UNIT_NAME_UNSPECIFIED = "unspc.";
      public const string UNIT_NAME_RECORD = "recs.";
      public const string UNIT_NAME_OBJECT = "obj.";
      public const string UNIT_NAME_SEGMENT = "seg.";
      public const string UNIT_NAME_BYTES = "bytes";
      public const string UNIT_NAME_PAGE = "pgs.";
      public const string UNIT_NAME_PAGE_PER_BUCKET = "pgs/bkt.cnt.";
      public const string UNIT_NAME_TIME = "times";
      public const string UNIT_NAME_CALL = "calls";
      public const string UNIT_NAME_SWEEP = "swps.";
      public const string UNIT_NAME_TRANSPORT = "tran.";
      public const string UNIT_NAME_TRANSACTION = "trxn.";
      public const string UNIT_NAME_CHANNEL = "chan.";
      public const string UNIT_NAME_BYTE = "bytes";
      public const string UNIT_NAME_MESSAGE = "msgs.";
      public const string UNIT_NAME_ERROR = "errors";
      public const string UNIT_NAME_SLOT = "slots";
      public const string UNIT_NAME_SEC = "sec.";
      public const string UNIT_NAME_MSEC = "msec.";
      public const string UNIT_NAME_PERCENT = "%";
      public const string UNIT_NAME_MB = "mb.";
      public const string UNIT_NAME_EVENT = "events";
      public const string UNIT_NAME_REQUEST = "req.";
      public const string UNIT_NAME_EXCEPTION = "excp.";
      public const string UNIT_NAME_RESPONSE = "resp.";
      public const string UNIT_NAME_WORK_CONTEXT = "w.ctx.";
      public const string UNIT_NAME_SESSION = "sess.";
      public const string UNIT_NAME_GEO_LOOKUP = "geo.lkp.";
      public const string UNIT_NAME_GEO_LOOKUP_HIT = "geo.hit.";
      public const string UNIT_NAME_MONEY = "money";
      public const string UNIT_NAME_TABLE = "table";
      public const string UNIT_NAME_NAMESPACE = "nmsp.";
    #endregion
  }
}
