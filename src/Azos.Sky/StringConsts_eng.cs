/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky
{
  internal static class StringConsts
  {
    public const string ARGUMENT_ERROR = "Argument error: ";
    public const string COMPONENT_NAME_EMPTY_ERROR = "Component name can not be empty";
    public const string INTERNAL_IMPLEMENTATION_ERROR = "Error in the internal implementation: ";

    public const string HOST_NAME_EMPTY_ERROR = "Host name can not be empty";

    public const string METABASE_APP_NAME_ASSIGNMENT_ERROR = "Metabase application name must be assigned a non-null non-empty string only once at process entry point";

    public const string BOOT_LOCAL_CONFIGURATION_ROOT_DOES_NOT_EXIST_ERROR = "Application boot - local configuration root '{0}' does not exist";

    public const string RT_CMDLET_ACTIVATION_ERROR =
                  "Internal error. The cmdlet activation returned <null> for '{0}'";

    public const string RT_CMDLET_DONTKNOW_ERROR =
                  "Server: do not know how to handle cmdlet '{0}'. Use 'help' to see cmdlet list";
    public const string APP_LOADER_ERROR = "Application loader exception: ";

    public const string PERMISSION_DESCRIPTION_RemoteTerminalOperatorPermission =
       "Controls whether users can access remote terminals";

    public const string PERMISSION_DESCRIPTION_AppRemoteTerminalPermission =
       "Controls whether users can access remote terminal of application context";

    public const string GDIDGEN_ALL_AUTHORITIES_FAILED_ERROR = "GDIDGenerator failed to obtain GDIDBlock from any authority. Tried: \n";

    public const string GDIDGEN_SET_TESTING_ERROR = "GDIDGenerator can not set TestingAuthorityGlueNode as the block was already allocated";

    public const string GDIDAUTH_AUTHORITY_ASSIGNMENT_WARNING =
      "AUTHORITY = {0}. This is a warning because an extra care should be taken with AUTHORITY assignment";


    public const string GDIDAUTH_INSTANCE_ALREADY_ALLOCATED_ERROR = "GDIDAuthorityService instance is already allocated";
    public const string GDIDAUTH_INSTANCE_NOT_RUNNING_ERROR = "GDIDAuthorityService instance is not running or shutting down";
    public const string GDIDAUTH_LOCATIONS_CONFIG_ERROR = "GDIDAuthorityServiceBase locations configuration error: ";
    public const string GDIDAUTH_LOCATIONS_READ_FAILURE_ERROR = "GDIDAuthorityServiceBase was not able to read sequence value from any persistence locations. Exception(s): ";
    public const string GDIDAUTH_LOCATION_PERSISTENCE_FAILURE_ERROR = "GDIDAuthorityServiceBase was not able to persist sequence increment in any persistence locations. Exception(s): ";
    public const string GDIDAUTH_DISK_PATH_TOO_LONG_ERROR = "GDIDAuthorityService can not save sequence, path is too long. Use shorter sequence and scope names. Path: {0}";
    public const string GDIDAUTH_NAME_INVALID_CHARS_ERROR = "GDIDAuthorityService can not use the supplied name '{0}' as it contains invalid chars. Scope and sequence names, may only contain alphanumeric or ['-','.','_'] chars and may only start from and end with either a latin letter or a digit";
    public const string GDIDAUTH_NAME_INVALID_LEN_ERROR = "GDIDAuthorityService can not use the supplied name '{0}' as it is either null/blank or longer than the limit of {1}";
    public const string GDIDAUTH_IDS_INVALID_AUTHORITY_VALUE_ERROR = "GDIDAuthorityService.AuthorityIDs set to ivalid value of '{0}'. An array of at least one element having all of its element values between 0..0x0f is required";
    public const string GDIDAUTH_ID_DATA_PARSING_ERROR = "GDIDAuthorityService::_id parsing error of '{0}'. Inner: {1}";
    public const string GDIDAUTH_ERA_EXHAUSTED_ERROR = "GDIDAuthorityService CATASTROPHIC FAILURE scope '{0}', sequence '{1}'. The era is exhausted. No more generation possible";

    public const string GDIDAUTH_ERA_EXHAUSTED_ALERT = "GDIDAuthorityService CRITICAL ALERT scope '{0}', sequence '{1}'. The era is about to be exhausted";

    public const string GDIDAUTH_ERA_PROMOTED_WARNING = "GDIDAuthorityService Era in scope '{0}', sequence '{1}' has been promoted to {2}";


    public const string HOST_SET_BUILDER_SINGLETON_CONFIG_ERROR =
      "HostSet Builder singleton could not be created from config: \n `{0}`. \n Error: {1}";

    public const string HOST_SET_BUILDER_CONFIG_FIND_ERROR =
      "HostSet Builder '{0}' could not find config section for a named set '{1}' in any of the region zones starting at '{2}'; search parent: {3}, transcend NOC: {4}";

    public const string HOST_SET_DYNAMIC_HOST_NOT_SUPPORTED_ERROR =
      "HostSet '{0}' declares dynamic host '{1}' which is not supported";


    public const string METABASE_INSTANCE_ALREADY_ALLOCATED_ERROR = "Metabank class instance is already allocated in this app container: ";

    public const string METABASE_FS_CONNECTION_ERROR = "Metabase file system {0}('{1}','{2}') connection error: {3}";

    public const string METABASE_INVALID_OPERTATION_ERROR = "Invalid metabase operation: ";
    public const string METABASE_CONFIG_LOAD_ERROR = "Metabase config file '{0}' load error: {1}";
    public const string METABASE_FILE_NOT_FOUND_ERROR = "Could not find file '{0}' in any of the supported config formats";
    public const string METABASE_FILE_NOT_FOUND_EXACT_ERROR = "Could not find file by exact name '{0}'";

    public const string METABASE_PLATFORMS_OS_NOT_DEFINED_ERROR = "Metabase platforms file does not contain the definition for '{0}' operating system";
    public const string METABASE_PLATFORMS_OS_DUPLICATION_ERROR = "Metabase platforms file defines '{0}' operating system more than once";



    public const string METABASE_PATH_NOT_FOUND_ERROR = "Metabase operation '{0}' could not find path '{1}'";

    public const string METABASE_METADATA_CTOR_1_ERROR =
     "Metadata '{0}' could not be created. Level configuration root name should be called '{1}' but is called '{2}' instead";

    public const string METABASE_METADATA_CTOR_2_ERROR =
     "Metadata '{0}' could not be created. A 'name' attribute must be declared on the root config node level having its value equal to '{1}', however it is equal to '{2}'";

    public const string METABASE_METADATA_CTOR_3_ERROR =
     "Metadata '{0}' could not be created. The name '{1}' is invalid sky entity name. See .IsValidName() function. Path: {2}";


    public const string METABASE_STRUCTURE_NOTEXISTS_ERROR = "Metadabase structure error: '{0}' does not exist ";

    public const string METABASE_APP_CONFIG_APP_DOESNT_EXIST_ERROR =
      "Metadabase section '{0}' contains application config file '{1}' that references non-existing application '{2}'";

    public const string METABASE_BAD_HOST_ROLE_ERROR =
       "The host '{0}' specifies the role name '{1}' which does not resolve to an existing role in the app catalog";

    public const string METABASE_HOST_MISSING_OS_ATTR_ERROR =
       "The host '{0}' does not specify its 'os' attribute";

    public const string METABASE_BAD_HOST_APP_ERROR =
       "App name '{0}' does not resolve to an existing app in the app catalog";

    public const string METABASE_HOST_ROLE_APP_MISMATCH_ERROR =
       "App name '{0}' is not a part of sky role '{1}' that this host has";

    public const string METABASE_VALIDATION_ROLE_APP_ERROR =
       "Role '{0}' declares app '{1}' which was not found in app catalog";

    public const string METABASE_VALIDATION_ROLE_APP_EXE_MISSING_ERROR =
       "Role '{0}' declares app '{1}' which does not specify any executable command on either application or role level. Add '{2}' attribute to app or role level";

    public const string METABASE_NOC_DEFAULT_GEO_CENTER_WARNING =
       "NOC '{0}' uses the default geo center: '{1}'";

    public const string METABASE_EFFECTIVE_APP_CONFIG_OVERRIDE_ERROR =
       "Override error at '{0}': {1}";

    public const string METABASE_EFFECTIVE_APP_CONFIG_ERROR =
       "Error calculating the effective config for app name '{0}' at host '{1}': {2}";

    public const string METABASE_APP_PACKAGES_ERROR =
       "Error calculating packages for app name '{0}' at host '{1}': {2}";

    public const string METABASE_VALIDATION_APP_CONFIG_ROOT_MISMTACH_ERROR =
       "App config root name of  '{0}' does not match the very metabase root";

    public const string METABASE_PLATFORM_OS_RESERVED_NAME_ERROR =
       "Metabase platform file declares a '{0}' operating system which is a reserved name";

    public const string METABASE_NAME_ATTR_UNDEFINED_ERROR =
       "Metabase entity must have a name attribute with a non-blank value: ";

    public const string METABASE_ENTITY_NAME_INVALID_ERROR =
       "Metabase entity {0} has an invalid name '{1}'. See .IsValidName() func";

    public const string METABASE_PLATFORM_NAME_DUPLICATION_ERROR =
       "Metabase platforms file declares some platform(s) more than once";

    public const string METABASE_NETWORK_NAME_DUPLICATION_ERROR =
       "Metabase networks file declares some network(s) more than once";

    public const string METABASE_NETWORK_GET_BINDING_NODE_ERROR =
       "Metabase could not get binding conf node for network '{0}' service '{1}' binding '{2}'. Service has to have at least one binding. Error: {3}";

    public const string METABASE_NETWORK_SVC_NAME_DUPLICATION_ERROR =
       "Metabase networks file declares a network '{0}' that has some service(s) listed more than once";

    public const string METABASE_NETWORK_GRP_NAME_DUPLICATION_ERROR =
       "Metabase networks file declares a network '{0}' that has some group(s) listed more than once";

    public const string METABASE_NETWORK_NO_SVC_ERROR =
       "Metabase networks file declares a network '{0}' that has no services listed";

    public const string METABASE_NETWORK_NO_SVC_BINDINGS_ERROR =
       "Metabase networks file declares a network '{0}' service'{1}' that has no bindings listed";

    public const string METABASE_NETWORK_SVC_BINDING_NAME_DUPLICATION_ERROR =
       "Metabase networks file declares a network '{0}' service '{1}' that has some binding(s) listed more than once";

    public const string METABASE_NETWORK_CONFIG_ERROR =
       "Metabase networks file contains errors:";

    public const string METABASE_NETWORK_SVC_RESOLVE_ERROR =
       "Can not resolve host '{0}' network '{1}' service '{2}'. Error: {3}";

    public const string METABASE_NETWORK_REGION_ROUTING_EMPTY_ROUTE_ASSIGNMENT_ERROR =
       "Region level config at '{0}' defines a networking route without any meaningful 'to-address/port/group' assignments";

    public const string METABASE_NETWORK_REGION_ROUTING_ATTR_UNRECOGNIZED_WARNING =
       "Region level config at '{0}' defines a networking route with an unknown attribute '{1}'. It is neither a route pattern filter nor a 'to-*' resolver attribute";

    public const string METABASE_NETWORK_REGION_ROUTING_FROM_PATH_ERROR =
       "Region level config at '{0}' defines a netwotrking route with filter 'from='{1}'' that does not resolve to any entity in region catalog";

    public const string METABASE_NAMED_NETWORK_NOT_FOUND_ERROR = "Metabase networks file does not define network '{0}'";

    public const string METABASE_NAMED_NETWORK_SVC_NOT_FOUND_ERROR = "Metabase networks file does not define network '{0}' service '{1}'";

    public const string METABASE_NAMED_NETWORK_GRP_NOT_FOUND_ERROR = "Metabase networks file does not define network '{0}' group '{1}'";

    public const string METABASE_NETWORK_SVC_DEFAULT_BINDING_NAME_ERROR =
       "Metabase networks file declares a network '{0}' service '{1}' that references default binding '{2}' that is not declared";

    public const string METABASE_BIN_PACKAGE_INVALID_PLATFORM_ERROR =
       "Bin package '{0}' references platform that is not known to the metabase";

    public const string METABASE_BIN_PACKAGE_INVALID_OS_ERROR =
       "Bin package '{0}' references operating system that is not known to the metabase";

    public const string METABASE_BIN_PACKAGE_MISSING_MANIFEST_ERROR =
       "Bin package '{0}' is missing a manifest file '{1}'";

    public const string METABASE_BIN_PACKAGE_OUTDATED_MANIFEST_ERROR =
       "Bin package '{0}' contains an outdated manifest file '{1}'. Regenerate package manifest using AMM tool with '/gbm' switch";

    public const string METABASE_REG_CATALOG_NAV_ERROR =
       "Error navigating region catalog. Operation: '{0}'. Path: '{1}' does not resolve to expected target";

    public const string METABASE_REG_NOC_PARENT_NOC_ZONE_ERROR =
       "NOC '{0}' specifies ParentNOCZonePath of '{1}' which does not resolve to existing zone";

    public const string METABASE_REG_NOC_PARENT_NOC_ZONE_NO_ROOT_ERROR =
       "NOC '{0}' specifies ParentNOCZonePath of '{1}' which has no common root with this NOC";

    public const string METABASE_REG_NOC_PARENT_NOC_ZONE_LEVEL_ERROR =
       "NOC '{0}' specifies ParentNOCZonePath of '{1}' which must be higher in sky hierarchy than this NOC, but it is not";

    public const string METABASE_REG_GEO_CENTER_ERROR =
       "Error in region catalog section {0}('{1}').$'geo-center' = '{2}'. Error: {3}";

    public const string METABASE_NET_SVC_RESOLVER_TARGET_NOC_INACCESSIBLE_ERROR =
       "Can not resolve target service '{0}' from host '{1}' to host '{2}' on network '{3}' scope '{4}'. Destination inaccessible because parties are in different NOCs";

    public const string METABASE_NET_SVC_RESOLVER_TARGET_GROUP_INACCESSIBLE_ERROR =
       "Can not resolve target service '{0}' from host '{1}' to host '{2}' on network '{3}' scope '{4}'. Destination inaccessible because parties are in different groups";

    public const string METABASE_NET_SVC_RESOLVER_DYN_HOST_UNKNOWN_ERROR =
       "Can not resolve target service '{0}' from host '{1}' to host '{2}' on network '{3}'. Dynamic destination host is not known";

    public const string METABASE_NET_SVC_RESOLVER_DYN_HOST_NO_ADDR_MATCH_ERROR =
       "Can not resolve target service '{0}' from host '{1}' to host '{2}' on network '{3}'. No dynamic host adapter address matches required pattern: '{4}'";

    public const string METABASE_GDID_AUTHORITIES_NONE_DEFINED_WARNING =
       "Metabase root level does not define any GDID authorities with valid host|name attributes";

    public const string METABASE_GDID_AUTHORITIES_DUPLICATION_ERROR =
       "Metabase root level defines some GDID authority/ies more than once";

    public const string METABASE_GDID_AUTHORITY_BAD_HOST_ERROR =
       "Metabase root level defines GDID authority host '{0}' that does not resolve in regional catalog";

    public const string METABASE_GDID_AUTHORITY_BAD_NETWORK_ERROR =
       "Metabase root level defines GDID authority host '{0}' that references network '{1}' that is not known";

    public const string METABASE_GDID_AUTHORITY_HOST_NOT_AGDIDA_ERROR =
       "Metabase root level defines GDID authority host '{0}' that does not have AGDIDA application in its role '{1}'";

    public const string METABASE_GDID_AUTHORITY_SVC_RESOLUTION_ERROR =
       "Metabase root level defines GDID authority host '{0}' which causes service resolution error: {1}";

    public const string METABASE_VALIDATION_WRONG_HOST_ERROR =
       "Metabase validation is to be performed as if from host '{0}' which does not resolve in region catalog";

    public const string METABASE_VALIDATION_WRONG_HOST_INFO =
       "If using AMM tool, you may have used wrong name under /from|/host switch or your {0} environment var is misconfigured";


    public const string METABASE_CONTRACTS_SERVICE_HUB_ERROR =
       "Metabase ServiceContractHub could not be initialized: ";


    public const string METABASE_NO_APP_PACKAGES_WARNING =
       "Metabase declares an application '{0}' with no packages";

    public const string METABASE_APP_PACKAGE_REDECLARED_ERROR =
       "Metabase declares an application '{0}' with some package/s declared more than once";

    public const string METABASE_INSTALLATION_BIN_PACKAGE_NOT_FOUND_ERROR =
       "Package installation tries to install a package '{0}' in metabase directory '{1}' which is not found in BIN catalog";

    public const string METABASE_APP_PACKAGE_BLANK_NAME_ERROR =
       "Metabase declares an application '{0}' which lists some package/s without a name";

    public const string METABASE_APP_PACKAGE_NOT_FOUND_IN_BIN_CATALOG_ERROR =
       "Metabase declares an application '{0}' with a references to the package '{1}' which is not declared for any platform/os in binary catalog";

    public const string METABASE_APP_DOES_NOT_HAVE_MATCHING_BIN_ERROR =
       "Metabase declares an application '{0}' which needs a package '{1}' having version '{2}' that does not have a matching bin resource per supplied OS '{3}'";

    public const string METABASE_PROCESSOR_SET_MISSING_ATTRIBUTE_ERROR =
      "Zone processor set host is missing '{0}'";

    public const string METABASE_PROCESSOR_SET_DUPLICATE_ATTRIBUTE_ERROR =
      "Zone processor set host is duplicate '{0}'";

    public const string METABASE_PROCESSOR_SET_HOST_IS_NOT_PROCESSOR_HOST_ERROR =
      "Zone processor set host with id '{0}' is not processor host";

    public const string METABASE_ZONE_COULD_NOT_FIND_PROCESSOR_HOST_ERROR =
      "Zone '{0}' could not find processor host by id '{1}'";

    public const string APPL_CMD_STOPPING_INFO =
       "Application container is stopping as the result of the app terminal command received from session '{0}' connected on '{1}' as '{2}'";

    public const string AHGOV_INSTANCE_ALREADY_ALLOCATED_ERROR = "HostGovernorService instance is already allocated";

    public const string AHGOV_INSTANCE_NOT_ALLOCATED_ERROR = "HostGovernorService instance is not allocated";

    public const string AHGOV_APP_PROCESS_CRASHED_AT_STARTUP_ERROR =
        "Application '{0}' process '{1}' with args '{2}' crashed while startup, see its logs";

    public const string AHGOV_APP_PROCESS_STD_OUT_NULL_ERROR =
        "Application '{0}' process '{1}' with args '{2}' standard output is null";

    public const string AHGOV_APP_PROCESS_NO_SUCCESS_AT_STARTUP_ERROR =
        "Application '{0}' process '{1}' with args '{2}' did not return success code 'OK.', see its logs";

    public const string AHGOV_ARD_UPDATE_PROBLEM_ERROR =
        "AHGOV respawned by ARD after an update problem. Check ARD logs. 'UPD' and/or 'RUN' folders may be locked by some other foreign process";

    public const string GLUE_BINDING_UNSUPPORTED_FUNCTION_ERROR = "Glue binding `{0}` supports only `{1}`. `{2}` is unsupported";
    public const string GLUE_BINDING_RESPONSE_ERROR = "Glue binding `{0}` response error: {1}";
    public const string GLUE_BINDING_REQUEST_ERROR = "Glue binding `{0}` request error: {1}";

    public const string AZGOV_INSTANCE_NOT_ALLOCATED_ERROR = "ZoneGovernorService instance is not allocated";
    public const string AZGOV_INSTANCE_ALREADY_ALLOCATED_ERROR = "ZoneGovernorService instance is already allocated";

    public const string AZGOV_REGISTER_SUBORDINATE_HOST_ERROR = "ZoneGovernorService can not register subordinate host '{0}' because of the error: {1}";
    public const string AZGOV_REGISTER_SUBORDINATE_HOST_PARENT_ERROR = "This zone governor '{0}' is not a direct or indirect parent of the host '{1}' in this NOC";

    public const string INSTR_SEND_TELEMETRY_TOP_LOST_ERROR = "Could not send instrumentation telemetry up the zone gov sky chain as the very root reached. Error: {0}";

    public const string LOG_SEND_TOP_LOST_ERROR = "Could not send log up the zone gov sky chain as the very root reached. Error: {0}";


    public const string UNIT_NAME_TIME = "times";


    public const string SKY_SVC_CLIENT_HUB_SINGLETON_CTOR_ERROR = "Error while making singleton instance of service client hub implementation '{0}'. Error: {1}";
    public const string SKY_SVC_CLIENT_HUB_MAPPING_ERROR = "Service hub error mapping a client for '{0}' service: {1}";
    public const string SKY_SVC_CLIENT_HUB_MAKE_INSTANCE_ERROR = "Service hub error making a client instance for contract mapping '{0}'. Activation error: {1}";
    public const string SKY_SVC_CLIENT_HUB_NET_RESOLVE_ERROR = "Service hub error resolving service node for contract mapping '{0}'. Resolver error: {1}";
    public const string SKY_SVC_CLIENT_HUB_CALL_RETRY_FAILED_ERROR = "Service hub error calling '{0}'  after {1} retries tried";
    public const string SKY_SVC_CLIENT_HUB_SETUP_INSTANCE_ERROR = "Service hub error from setup a client instance for contract mapping '{0}'. Setup error: {1}";
    public const string SKY_SVC_CLIENT_HUB_RETRY_CALL_HOST_ERROR = "Service hub error calling '{0}' service on '{1}'. Error: {2}";
    public const string SKY_SVC_CLIENT_MAPPING_CTOR_ERROR = "Service hub ContractMapping.ctor(' {0} ') error: {1}";


    public const string SECURITY_AUTH_TOKEN_SERIALIZATION_ERROR = "{0} can not serialize unexpected data '{1}'. Token.Data must be of a 'string' type";
    public const string SECURITY_AUTH_TOKEN_DESERIALIZATION_ERROR = "{0} could not deserialize unexpected data. Caught: {1}";


    public const string LOCK_SESSION_PATH_ERROR = "LockSession can not be created at the path '{0}'. Error: {1}";
    public const string LOCK_SESSION_ZGOV_SETUP_ERROR = "Invalid metabase setup. Locking failover host count is different from primary host count in the zone '{0}'";
    public const string LOCK_SESSION_NOT_ACTIVE_ERROR = "LockSession '{0}' / '{1}' is not present in the list of active sessions";
    public const string LOCK_SESSION_PATH_LEVEL_NO_ZGOVS_ERROR = "Lock session can not be established at the level identified by path '{0}' as there are no zgov nodes in the hierarchy at the level or above to service the request";


    public const string KDB_TABLE_IS_NULL_OR_EMPTY_ERROR = "KDB operation'{0}' error: Table name is null or white space";
    public const string KDB_TABLE_CHARACTER_ERROR = "KDB operation'{0}' error: Table name contains invalid character '{1}'";
    public const string KDB_TABLE_MAX_LEN_ERROR = "KDB operation'{0}' error: Table name of {1} characters exeeds max len of {2}";
    public const string KDB_KEY_MAX_LEN_ERROR = "KDB operation'{0}' error: Key name of {1} characters exeeds max len of {2}";
    public const string KDB_KEY_IS_NULL_OR_EMPTY_ERROR = "KDB operation'{0}' error: Key is null or empty";



    public const string MDB_AREA_CONFIG_PARTITION_NOT_FOUND_ERROR =
       "MDB area '{0}' configuration has no partition slot that fits the briefcase GDID: '{1}'";

    public const string MDB_AREA_CONFIG_NO_NAME_ERROR =
       "MDB area is defined without a name in config. Name must be explicitly defined";

    public const string MDB_AREA_CONFIG_NO_DATASTORE_ERROR =
       "MDB area '{0}' is defined without a data store";

    public const string MDB_AREA_CONFIG_INVALID_NAME_ERROR =
       "MDB area has an invalid name defined";

    public const string MDB_AREA_CONFIG_NO_PARTITIONS_ERROR =
       "MDB area '{0}' config has no partitions defined'";

    public const string MDB_AREA_CONFIG_DUPLICATE_RANGES_ERROR =
       "MDB area '{0}' config contains duplicate ranges'";

    public const string MDB_AREA_CONFIG_PARTITION_GDID_ERROR =
       "MDB area '{0}' config contains partition with wrong GDID '{1}'. Reason: {2}'";

    public const string MDB_AREA_CONFIG_NO_PARTITION_SHARDS_ERROR =
       "MDB area '{0}' config has no shards defined for partition '{1}'";

    public const string MDB_AREA_CONFIG_DUPLICATE_PARTITION_SHARDS_ERROR =
       "MDB area '{0}' config has duplicate shard order/number defined for partition '{1}'";

    public const string MDB_AREA_CONFIG_SHARD_CSTR_ERROR =
       "MDB area '{0}' config partition '{1}' shard connect string '{2}' is missing";

    public const string MDB_STORE_CONFIG_NO_TARGET_NAME_ERROR =
       "MDB store requires non-blank target name to be assigned in config";

    public const string MDB_STORE_CONFIG_NO_AREAS_ERROR =
       "MDB store config missing any areas";

    public const string MDB_STORE_CONFIG_GDID_ERROR =
       "MDB store config has problems starting with GDIDGenerator: ";

    public const string MDB_PARTITIONED_AREA_MISSING_ERROR =
       "MDB area '{0}' does not exist or is not a partitioned area";

    public const string MDB_STORE_CONFIG_MANY_CENTRAL_ERROR =
       "MDBStore config specifies more than one central area";

    public const string MDB_OBJECT_SHARDING_ID_ERROR =
   "Can not obtain sharding ID from object of type '{0}'";

    public const string PM_HOSTSET_CONFIG_MISSING_NAME_ERROR   = "ProcessManager hostset config is missing the 'name' attribute";
    public const string PM_HOSTSET_CONFIG_DUPLICATE_NAME_ERROR = "ProceeManager hostset config already contains HostSet named '{0}'";
    public const string PM_HOSTSET_CONFIG_PATH_MISSING_ERROR   = "ProceeManager hostset config 'path' is missing for HostSet named '{0}'";

    public const string CONFIGURATION_INCLUDE_PRAGMA_DEPTH_ERROR = "Include pragma recursive depths exceeded: {0}";
    public const string CONFIGURATION_INCLUDE_PRAGMA_ERROR = "Include error at '{0}': {1}";

    public const string TODO_QUEUE_NOT_FOUND_ERROR = "Todo queue '{0}' not found";
    public const string TODO_QUEUE_ENQUEUE_DIFFERENT_ERROR = "Can not enqueue todos from different queues in one enqueue call";
    public const string TODO_FRAME_SER_NOT_SUPPORTED_ERROR = "TodoFrame of '{0}' serializer not supported: {1}";
    public const string TODO_FRAME_DESER_NOT_SUPPORTED_ERROR = "TodoFrame of '{0}' deserializer not supported: {1}";
    public const string TODO_FRAME_SER_ERROR = "TodoFrame serialization error of '{0}': {1}";
    public const string TODO_FRAME_DESER_ERROR = "TodoFrame deserialization error of '{0}': {1}";

    public const string TODO_CORRELATED_MERGE_ERROR = "CorrelatedTodo '{0}'.Merge('{1}') leaked: {2}";

    public const string TODO_ENQUEUE_TX_BODY_ERROR = "Error executing enqueue in '{0}' transaction body: {1}";

    public const string LOG_ARCHIVE_PUT_TX_BODY_ERROR = "Error executing put in '{0}' transaction body: {1}";
    public const string LOG_ARCHIVE_MESSAGE_NOT_FOUND_ERROR = "Log message with id {0} not found in log archive";

    public const string TELEMETRY_ARCHIVE_PUT_TX_BODY_ERROR = "Error executing put in '{0}' transaction body: {1}";

    public const string PROCESS_FRAME_SER_NOT_SUPPORTED_ERROR = "ProcessFrame of '{0}' serializer not supported: {1}";
    public const string PROCESS_FRAME_DESER_NOT_SUPPORTED_ERROR = "ProcessFrame of '{0}' deserializer not supported: {1}";
    public const string PROCESS_FRAME_SER_ERROR = "ProcessFrame serialization error of '{0}': {1}";
    public const string PROCESS_FRAME_DESER_ERROR = "ProcessFrame deserialization error of '{0}': {1}";

    public const string SIGNAL_FRAME_SER_NOT_SUPPORTED_ERROR = "SignalFrame of '{0}' serializer not supported: {1}";
    public const string SIGNAL_FRAME_DESER_NOT_SUPPORTED_ERROR = "SignalFrame of '{0}' deserializer not supported: {1}";
    public const string SIGNAL_FRAME_SER_ERROR = "SignalFrame serialization error of '{0}': {1}";
    public const string SIGNAL_FRAME_DESER_ERROR = "SignalFrame deserialization error of '{0}': {1}";
    public const string PID_PARSE_ERROR = "String value '{0}' can not be parsed as PID";


    public const string WM_SERVICE_NO_CHANNELS_ERROR = "{0} service start error - no channels configured";
    public const string WM_SERVICE_DUPLICATE_CHANNEL_ERROR = "{0} service config error - duplicate channel name '{0}'";
  }
}
