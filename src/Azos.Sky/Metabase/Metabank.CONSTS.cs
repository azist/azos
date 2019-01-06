/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.Metabase
{
  partial class Metabank
  {
    public const string CONFIG_APP_CONFIG_INCLUDE_PRAGMA_ATTR = "app-config-include-pragma";
    public const string CONFIG_APP_CONFIG_INCLUDE_PRAGMAS_DISABLED_ATTR = "app-config-include-pragmas-disabled";

    public const string CONFIG_COMMON_FILE = "$.common";
    public const string CONFIG_SECTION_LEVEL_FILE = "$";
    public const string CONFIG_SECTION_LEVEL_ANY_APP_FILE = "$.app";
    public const string CONFIG_PLATFORMS_FILE = "platforms";
    public const string CONFIG_NETWORKS_FILE  = "networks";
    public const string CONFIG_CONTRACTS_FILE = "contracts";

    public const string CONFIG_SECTION_LEVEL_APP_FILE_PREFIX = "$.";
    public const string CONFIG_SECTION_LEVEL_APP_FILE_SUFFIX = ".app.";


    public const string APP_CATALOG = "app";
    public const string BIN_CATALOG = "bin";
    public const string REG_CATALOG = "reg";
    public const string SEC_CATALOG = "sec";

    public const string CONFIG_NAME_ATTR = "name";
    public const string CONFIG_DESCRIPTION_ATTR = "description";
    public const string CONFIG_OFFLINE_ATTR = "offline";
    public const string CONFIG_ROLE_ATTR = "role";
    public const string CONFIG_PATH_ATTR = "path";
    public const string CONFIG_GEO_CENTER_ATTR = "geo-center";
    public const string CONFIG_VERSION_ATTR = "version";
    public const string CONFIG_AUTO_RUN_ATTR = "auto-run";
    public const string CONFIG_EXE_FILE_ATTR = "exe-file";
    public const string CONFIG_EXE_ARGS_ATTR = "exe-args";

    public const string CONFIG_APP_CONFIG_SECTION = "app-config";
    public const string CONFIG_OS_APP_CONFIG_INCLUDE_SECTION = "include-os-app-config";

    public const string CONFIG_OS_ATTR = "os";

    public const string CONFIG_PLATFORM_SECTION = "platform";
    public const string CONFIG_OS_SECTION = "os";

    public const string CONFIG_PACKAGES_SECTION = "packages";
    public const string CONFIG_PACKAGE_SECTION = "package";

    public const string DEFAULT_PACKAGE_VERSION = "head";

    public const string CONFIG_GDID_SECTION  = "gdid";

    public const string CONFIG_NETWORK_SECTION  = "network";

    public const string CONFIG_NETWORK_ROUTING_SECTION = "network-routing";
    public const string CONFIG_NETWORK_ROUTING_ROUTE_SECTION    = "route";
    public const string CONFIG_NETWORK_ROUTING_NETWORK_ATTR  = "network";
    public const string CONFIG_NETWORK_ROUTING_FROM_ATTR     = "from";
    public const string CONFIG_NETWORK_ROUTING_SERVICE_ATTR  = "service";
    public const string CONFIG_NETWORK_ROUTING_BINDING_ATTR  = "binding";
    public const string CONFIG_NETWORK_ROUTING_TO_ADDRESS_ATTR  = "to-address";
    public const string CONFIG_NETWORK_ROUTING_TO_PORT_ATTR     = "to-port";
    public const string CONFIG_NETWORK_ROUTING_TO_GROUP_ATTR    = "to-group";
    public const string CONFIG_NETWORK_ROUTING_HOST_ATTR        = "host";

    public const char HOST_DYNAMIC_SUFFIX_SEPARATOR = '~';//cross-check with IsValidName() which should include this value from valid names

    public const string CONFIG_HOST_DYNAMIC_ATTR = "dynamic";

    public const string CONFIG_HOST_ZGOV_LOCK_FAILOVER_ATTR = "zgov-lock-failover";
    public const string CONFIG_HOST_PROCESS_HOST_ATTR = "process-host";


    public const string CONFIG_SERVICE_SECTION  = "service";
    public const string CONFIG_GROUP_SECTION    = "group";
    public const string CONFIG_DEFAULT_BINDING_ATTR = "default-binding";
    public const string CONFIG_BINDINGS_SECTION  = "bindings";
    public const string CONFIG_SCOPE_ATTR = "scope";
    public const string CONFIG_ADDRESS_ATTR = "address";
    public const string CONFIG_PORT_ATTR = "port";

    public const string CONFIG_TARGET_SUFFIX_ATTR = "target-suffix";

    public const string CONFIG_PARENT_NOC_ZONE_ATTR = "parent-noc-zone";


    public const string CONFIG_HOST_SET_BUILDER_SECTION = "host-set-builder";
    public const string CONFIG_HOST_SET_SECTION = "host-set";
    public const string CONFIG_HOST_SET_HOST_SECTION = "host";

    public const string CONFIG_PROCESSOR_SET_SECTION = "processor-set";
    public const string CONFIG_PROCESSOR_HOST_SECTION = "host";
    public const string CONFIG_PROCESSOR_HOST_ID_ATTR = "id";
    public const string CONFIG_PROCESSOR_HOST_PRIMARY_PATH_ATTR = "primary-path";
    public const string CONFIG_PROCESSOR_HOST_SECONDARY_PATH_ATTR = "secondary-path";

    public const int DEFAULT_RESOLVE_DYNAMIC_HOST_NET_SVC_TIMEOUT_MS = 2000;
    public const int MIN_RESOLVE_DYNAMIC_HOST_NET_SVC_TIMEOUT_MS = 1000;
  }
}
