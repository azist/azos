/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos
{
  /// <summary>
  /// A dictionary of framework text messages.
  /// Localization may be done in this class in future
  /// </summary>
  internal static class StringConsts
  {
    public const string SECURITY_NON_AUTHENTICATED = "<non-authenticated>";

    public const string IMMUTABLE_SYS_INSTANCE = "System instance of `{0}` is immutable";

    public const string FLOW_NO_REF_CYCLES_VIOLATION_ERROR = "State machine '{0}' caused violation of NoRefCycles constraint. Most likely the object graph has cycles which are either not supported or the operation body failed to check for presence of the reference in the set";

    public const string APP_SET_MEMORY_MODEL_ERROR =
      "The App.SetMemoryModel() method must be called at process entry point before the app container allocation";

        public const string OBJECT_DISPOSED_ERROR =
            "Object '{0}' instance was already disposed";

        public const string OBJECT_WAS_NOT_DETERMINISTICALLY_DISPOSED_ERROR =
            "Object '{0}' instance was not deterministically disposed";

        public const string INTERNAL_SYSTEM_ERROR =
            "Internal system error: ";

        public const string OPERATION_NOT_SUPPORTED_ERROR =
            "Operation not supported: ";

        public const string LOGSVC_NOSINKS_ERROR =
            "No log sinks registered. Log daemon `{0}` could not start";

        public const string DAEMON_INVALID_STATE =
            "Daemon is in inappropriate state for requested operation: ";

        public const string DAEMON_COMPOSITE_CHILD_START_ABORT_ERROR =
            "Composite daemon start aborted due to exception from child service '{0}' start: {1} ";

        public const string ARGUMENT_ERROR = "Argument error: ";

        public const string APP_CONTAINER_NESTING_ERROR =
@"Catastrophic error trying to nest app container of type '{0}' over an existing app '{1}' which does not have AllowNesting flag set.
 Revise app container allocation logic which is usually at the app entry point.
 This error may also be caused by unit tests hosted in the container which does not allow nesting";

        public const string PAL_ALREADY_SET_ERROR = "Platform abstraction layer is already set. It can only be set once per process at the very entry point";

        public const string PAL_ABSTRACTION_IS_NOT_PROVIDED_ERROR = "Platform abstraction layer for '{0}' is not provided";

        public const string CANNOT_RETURN_NULL_ERROR = "'next' function cannot return null. ";

        public const string HTTP_OPERATION_ERROR = "HTTP[S] error: ";

        public const string FILE_NOT_FOUND_ERROR = "File not found: ";

        public const string LOGDAEMON_SINK_START_ERROR =
            "Log daemon '{0}' could not start sink '{1}'. Sink.TestOnStart = {2}. Sink exception:\n   {3}";

        public const string LOGSVC_SINK_EXCEEDS_MAX_PROCESSING_TIME_ERROR =
            "Log sink '{0}' exceeded allowed max processing time. Allowed {1} ms, but took {2} ms";

        public const string LOGSVC_FILE_SINK_FILENAME_ERROR =
            "Log sink '{0}'.'file-name' pattern '{1}' is incorrect: {2}";

        public const string LOGSVC_FILE_SINK_PATH_ERROR =
            "Log sink '{0}'.'path' pattern '{1}' is incorrect: {2}";

        public const string LOGSVC_SINK_IS_OFFLINE_ERROR =
            "Log sink '{0}' is offline due to prior failure and RestartProcessingAfterMs timespan has not expired yet";

        public const string LOGSVC_FAILOVER_MSG_TEXT =
            "Log message {0} delivery failed over from sink '{1}' to '{2}'. Average processing latency of failed sink is {3} ms";

        public const string DI_ATTRIBUTE_TYPE_INCOMPATIBILITY_ERROR =
        "Incompatible injection types. Injection type expectation of '{0}' is not assignable into the field: {1} {2}";

        public const string DI_ATTRIBUTE_APPLY_ERROR =
        "Error while applying attribute [{0}] on class '{1}' instance field '{2}': {3}";

        public const string DI_UNSATISIFED_INJECTION_ERROR =
@"Dependency injection on class '{0}' instance field '{1}' is not marked as `optional` and could not be satisfied using: [{2}].
The injected value has to be present in app chassis having its type assignment-compatible with the target field or 'Type' constraint;
and the name of module or INamed entity must match if the 'Name' constraint was specified in the attribute";

        public const string SHARDING_OBJECT_ID_ERROR =
          "Can not obtain sharding ID from object of type '{0}'";

        public const string AVER_THROWS_NOT_THROWN_ERROR = "Method '{0}' is decorated with {1} averment, but nothing was thrown";
        public const string AVER_THROWS_TYPE_MISMATCH_ERROR = "Method '{0}' averment expects exception of type '{1}' to be thrown, but '{2}' was thrown instead";
        public const string AVER_THROWS_MSG_MISMATCH_ERROR = "Method '{0}' exception averment mismatch. Expected {1} '{2}' but got '{3}'";
        public const string AVER_TIME_MIN_ERROR_ERROR = "Method '{0}' avers to be completed in at least {1:n0} ms but completed in {2:n0} ms instead";
        public const string AVER_TIME_MAX_ERROR_ERROR = "Method '{0}' avers to be completed in at most {1:n0} ms but completed in {2:n0} ms instead";

        public const string RUN_ATTR_BAD_CONFIG_ERROR = "RunAttribute specifies bad config syntax: ";
        public const string RUN_BINDER_METHOD_ARGS_MISSING_ERROR = "Method '{0}' arguments are not supplied by the Run() attribute";
        public const string RUN_BINDER_ARG_MISSING_ERROR = "Method '{0}' argument '{1}' must be supplied as it does not have a default value";
        public const string RUN_BINDER_SECTION_MISSING_ERROR = "Method '{0}' argument '{1}' must be config section, not an attribute";
        public const string RUN_BINDER_BINDING_ERROR = "Runner can not bind {0}.{1}({2}): {3} \n {4}";

        public const string RUN_RUNNER_ALL_METHODS_LEAKED1_ERROR =
          "Runner.RunAllMethods() leaked error which was not handled by epilogue: {0}";

        public const string RUN_RUNNER_ALL_METHODS_LEAKED2_ERROR =
          "Runner.RunAllMethods() leaked error: {0} \n then Runner epilogue threw: {1}";

        public const string RUN_RUNNER_RUN_LEAKED2_ERROR =
          "Runner.SafeRunMethod() leaked error: {0} \n then Run epilogue threw: {1}";

        public const string RUN_ASYNC_VOID_NOT_SUPPORTED_METHOD_ERROR =
          "Runnable async method '{0}' must either return a Task or Task<T>; async void is not runnable";



        public const string CONFIGURATION_FILE_UNKNOWN_ERROR = "Configuration file is unknown/not set ";

        public const string CONFIGURATION_EMPTY_NODE_MODIFY_ERROR = "Empty configuration node may not be modified";

        public const string CONFIGURATION_READONLY_ERROR = "Readonly configuration can not be modified";

        public const string CONFIGURATION_TYPE_CREATION_ERROR = "Type could not be created from node path '{0}'. Error: {1}";

        public const string CONFIGURATION_TYPE_ASSIGNABILITY_ERROR = "Instance of type '{0}' is not assignable to '{1}'";

        public const string CONFIGURATION_MAKE_USING_CTOR_ERROR = "MakeUsingCtor for type '{0}' failed with error: {1}";

        public const string CONFIGURATION_TYPE_NOT_SUPPLIED_ERROR = "Type not supplied either as 'type' attribute or default";

        public const string CONFIGURATION_TYPE_RESOLVE_ERROR =
         "Type name '{0}' could not be resolved into a type object. If you do not use fully-qualified type names, then ensure that parent node config chain defines an '{1}' attribute which contains a semicolon-delimited list of type search locations for partial type names used under that config node level";

        public const string CONFIGURATION_CLONE_EMPTY_NODE_ERROR = "Empty sentinel nodes can not be cloned";

        public const string CONFIGURATION_ENTITY_NAME_ERROR = "The name is invalid for this configuration type. Name: ";

        public const string CONFIGURATION_NODE_NAME_ERROR = "The supplied node name '{0}' is not supported by this configuration when StrictNames is set to true";

        public const string CONFIGURATION_ATTRIBUTE_MEMBER_READONLY_ERROR = "Instance of '{0}' could not be configured using ConfigAttribute because its member '{1}' is readonly ";

        public const string CONFIGURATION_NAVIGATION_REQUIRED_ERROR = "Bad navigation: path '{0}' requires node but did not land at an existing node";

        public const string CONFIGURATION_PATH_SEGMENT_NOT_SECTION_ERROR = "Bad navigation: path segment '{0}' in path '{1}' can not be navigated to because its parent is not a section node ";

        public const string CONFIGURATION_PATH_INDEXER_ERROR = "Bad navigation: path '{0}' contains bad indexer specification '{1}'";

        public const string CONFIGURATION_PATH_INDEXER_SYNTAX_ERROR = "Bad navigation: path '{0}' contains bad indexer syntax";

        public const string CONFIGURATION_PATH_VALUE_INDEXER_SYNTAX_ERROR = "Bad navigation: path '{0}' contains bad value indexer syntax";

        public const string CONFIGURATION_PATH_VALUE_INDEXER_CAN_NOT_USE_WITH_ATTRS_ERROR = "Bad navigation: cannot use value indexer with attributes, path '{0}'";

        public const string CONFIGURATION_NODE_DOES_NOT_BELONG_TO_THIS_CONFIGURATION_ERROR = "Node '{0}' does not belong to configuration";

        public const string CONFIGURATION_NODE_MUST_NOT_BELONG_TO_THIS_CONFIGURATION_ERROR = "Node '{0}' must not belong to configuration";

        public const string CONFIGURATION_CAN_NOT_INCLUDE_INSTEAD_OF_ROOT_ERROR = "Can not include '{0}' instead of this root node";

        public const string CONFIGURATION_NAVIGATION_SECTION_REQUIRED_ERROR = "Path '{0}' requires section node but did not land at an existing section";

        public const string CONFIGURATION_NAVIGATION_BAD_PATH_ERROR = "Bad navigation path: ";

        public const string CONFIGURATION_SECTION_INDEXER_EMPTY_ERROR = "Section indexer is an empty array";

        public const string CONFIGURATION_VALUE_COULD_NOT_BE_GOTTEN_AS_TYPE_ERROR = "Value from '{0}' could not be gotten as type '{1}' ";

        public const string CONFIGURATION_OVERRIDE_PROHOBITED_ERROR = "Section override failed because it is prohibited by base node: ";


        public const string CONFIGURATION_PATH_ICONFIGURABLE_SECTION_ERROR = "Instance of '{0}' could not be configured using ConfigAttribute because its member '{1}' is IConfigurable, however config path did not yield a IConfigSectionNode instance";

        public const string CONFIGURATION_PATH_ICONFIGSECTION_SECTION_ERROR = "Instance of '{0}' could not be configured using ConfigAttribute because its member '{1}' is IConfigSection, however config path did not yield a IConfigSectionNode instance";


        public const string CONFIGURATION_ATTR_APPLY_VALUE_ERROR = "Error applying config attribute to property/field '{0}' for the instance of '{1}'. Exception: {2}";


        public const string CONFIGURATION_OVERRIDE_SPEC_ERROR = "Configuration node override specification was not understood per supplied NodeOverrideRules: ";

        public const string CONFIGURATION_SCRIPT_EXECUTION_ERROR = "Configuration script execution error: ";

        public const string CONFIGURATION_SCRIPT_TARGET_CONFIGURATION_MUST_BE_EMPTY_ERROR = "Target configuration must be empty";

        public const string CONFIGURATION_SCRIPT_EXPRESSION_EVAL_ERROR = "Configuration script expression '{0}' evaluation at node '{1}'. Error: {2}";

        public const string CONFIGURATION_SCRIPT_SYNTAX_ERROR = "Script syntax error: ";

        public const string CONFIGURATION_SCRIPT_ELSE_NOT_AFTER_IF_ERROR = CONFIGURATION_SCRIPT_SYNTAX_ERROR + "ELSE clause '{0}' is not after IF clause";

        public const string CONFIGURATION_SCRIPT_SET_PATH_ERROR = "Configuration script SET instruction '{0}' references path '{1}' which does not exist";

        public const string CONFIGURATION_SCRIPT_CALL_TARGET_PATH_ERROR = "Configuration script CALL instruction '{0}' references invocation target path '{1}' which does not exist";


        public const string CONFIGURATION_SCRIPT_TIMEOUT_ERROR = "Configuration script exceeded allowed timeout of {0}ms while executing '{1}' statement";


        public const string CONFIGURATION_INCLUDE_PRAGMA_ERROR = "Config error processing include pragma '{0}': {1}";

        public const string BUILD_INFO_READ_ERROR =
            "Error reading BUILD_INFO resource: ";

        public const string INVALID_IPSTRING_ERROR =
            "Invalid IP:PORT string: ";

         public const string INVALID_EPOINT_ERROR =
            "Invalid endpoint IP|HOST:PORT string '{0}'. Error: {1}";


        public const string INVALID_TIMESPEC_ERROR =
            "Invalid time specification";

        public const string STRING_VALUE_COULD_NOT_BE_GOTTEN_AS_TYPE_ERROR = "String value '{0}' could not be gotten as type '{1}' ";


    public const string COLLECTION_CAPPED_QUEUE_LIMIT_ERROR =
      "{0} limit is reached and can not enqueue more data. The Handling is `{1}`. Try changing Handling or increase the limits";

    public const string SLIM_STREAM_CORRUPTED_ERROR = "Slim data stream is corrupted: ";

    public const string SECDB_STREAM_CORRUPTED_ERROR = "SecDB data stream is corrupted: ";


    public const string BINLOG_STREAM_NULL_ERROR = "BinLog stream is null: ";
    public const string BINLOG_STREAM_CANT_SEEK_WRITE_ERROR = "BinLog stream can not seek or can not write: ";
    public const string BINLOG_STREAM_CORRUPTED_ERROR = "BinLog data stream is corrupted: ";
    public const string BINLOG_READER_FACTORY_ERROR = "BinLog reader factory error: ";
    public const string BINLOG_READER_TYPE_MISMATCH_ERROR = "BinLog reader type mismatched. Class: '{0}' Stream: '{1}'";
    public const string BINLOG_BAD_READER_TYPE_ERROR = "BinLog header contains reader type which could not be loaded or is not a valid LogReader derivative: ";

    public const string SECURITY_AUTHROIZATION_ERROR =
        "Authorization to '{0}' failed from '{1}'";

    public const string SECURITY_IDP_UPSTREAM_CALL_ERROR =
         "IDP upstream server call failure: {0}";

    public const string SECURITY_IDP_PROTOCOL_ERROR =
         "IDP upstream server protocol error: {0}";

    public const string SECURITY_REPRESENT_CREDENTIALS_FORGOTTEN =
        "Credentials can not be represented as they are forgotten";

    public const string UNKNOWN_STRING = "unknown";


    public const string APP_MODULE_DUPLICATE_CHILD_ERROR =
        "Application module duplicate name: '{0}' module already contains a child module named: '{1}'";

    public const string APP_MODULE_GET_BY_TYPE_ERROR =
        "Application could not get the requested module of type `{0}`";

    public const string APP_MODULE_GET_BY_NAME_ERROR =
        "Application could not get the requested named module `{0}` of type `{1}`";


    public const string INVALID_OPERATION_ERROR =
        "Invalid operation: ";

    public const string READONLY_COLLECTION_MUTATION_ERROR =
        "Can not mutate read-only collection: ";

    public const string CHECK_LIST_ALREADY_RUN_ERROR =
        "Checklist has already been run";

    public const string OBJSTORESVC_PROVIDER_CONFIG_ERROR =
            "ObjectStoreService could not configure provider: ";

    public const string INSTRUMENTATIONSVC_PROVIDER_CONFIG_ERROR =
            "InstrumentationService could not configure provider: ";

    public const string CAN_NOT_CREATE_MORE_SCOPE_EXPRESSIONS_ERROR =
        "Can not create more expression instances in this scope as expression evaluation assembly is already built: ";

    public const string CAN_NOT_ADD_FAILED_SCOPE_COMPILE_ERROR =
        "Can not create more expression instances in this scope as expression evaluation assembly compilation failed: ";

    public const string EXPRESSION_SCOPE_COMPILE_ERROR =
        "Expression scope compilation error: ";



    public const string MODEL_METHOD_NOT_FOUND_ERROR =
     "Model could not find a public callable method to invoke. Method Name: '{0}'";

    public const string MODEL_METHOD_ERROR =
     "Model callable method error: ";

    public const string FIELD_COMPARISON_NOT_IMPLEMENTED_ERROR =
     "Comparison is not implemented at Field abstract level";






    public const string FIELD_ATTRIBUTES_DEFS_ERROR =
       "Error while applying field definitions BuildAndDefineFields('{0}'). Check attributes";

    public const string FIELD_TYPE_MAP_ERROR =
       "Error mapping type '{0}' to record model field. No mapping exists";


    public const string APP_INJECTED_CONFIG_FILE_NOT_FOUND_ERROR =
         "Specified application configuration file `{0}` does not exist. Revise command line arguments for `-{1}` switch";

    public const string APP_CONFIG_SETTING_NOT_FOUND_ERROR =
        "Application configuration setting \"{0}\" not found.";

    public const string CONFIG_RECURSIVE_VARS_ERROR =
        "Configuration line contains recursive vars that can not be resolved: \"{0}\"";

    public const string CONFIG_INFINITE_VARS_ERROR =
        "Error evaluating value \"{0}\" as maximum variable evaluation iteration limit of {1} was exceeded. "+
        "This can happen if external variable providers keep on yielding resolved values with new variables which never resolve within the allowed limit of {1} iterations";

    public const string CONFIG_NO_PROVIDER_LOAD_FILE_ERROR =
        "No configuration provider can load content from file name: ";

    public const string CONFIG_NO_PROVIDER_HANDLE_FILE_ERROR =
        "No configuration provider can handle file name: ";

    public const string CONFIG_NO_PROVIDER_LOAD_FORMAT_ERROR =
        "No configuration provider can open content supplied in this format: ";

    public const string CONFIG_VARS_EVAL_ERROR =
        "Configuration variable '{0}' evaluation error: {1}";

    public const string CONFIG_INCLUDE_PRAGMA_DEPTH_ERROR = "Include pragma recursive depth exceeded: {0}";

    public const string CONFIG_BEHAVIOR_APPLY_ERROR =
        "Error while applying behavior to {0}. Error: {1}";

    public const string CONFIG_JSON_MAP_ERROR =
        "JSONConfig must be represented by a valid JSON map(hash) with a single root key, not array or multi-key map";

    public const string CONFIG_JSON_STRUCTURE_ERROR =
        "JSONConfig was supplied content with invalid logical structure, all members of an array must be non-null maps that represent config sub-sections or scalar attribute values";

    public const string WORK_ITEM_NOT_AGGREGATABLE_ERROR =
        "Work item must implement IAggregatableWorkItem interface to be posted in this queue";


    public const string APP_CLEANUP_COMPONENT_ERROR = "App {0} component cleanup error: {1}";



        public const string APP_LOG_INIT_ERROR =
              "App log initApplication error: ";

        public const string APP_MODULE_INIT_ERROR =
              "App root module initApplication error: ";

        public const string APP_TIMESOURCE_INIT_ERROR =
              "App time source initApplication error: ";

        public const string APP_EVENT_TIMER_INIT_ERROR =
              "App event timer initApplication error: ";

        public const string APP_INSTRUMENTATION_INIT_ERROR =
              "App instrumentation initApplication error: ";

        public const string APP_THROTTLING_INIT_ERROR =
              "App throttling initApplication error: ";

        public const string APP_DATA_STORE_INIT_ERROR =
              "App data store initApplication error: ";

        public const string APP_ASSEMBLY_PRELOAD_ERROR =
              "App assembly preload from '{0}' error: {1}";

        public const string APP_OBJECT_STORE_INIT_ERROR =
              "App object store initApplication error: ";

        public const string APP_GLUE_INIT_ERROR =
              "App glue initApplication error: ";

        public const string APP_DI_INIT_ERROR =
              "App DI initApplication error: ";

       public const string APP_SECURITY_MANAGER_INIT_ERROR =
              "App security manager initApplication error: ";

        public const string APP_APPLY_BEHAVIORS_ERROR =
              "App apply behaviors initApplication error: ";

        public const string APP_FINISH_NOTIFIABLES_ERROR =
              "Application finish notifiables threw exceptions: ";


        public const string APP_STARTER_BEFORE_ERROR =
              "Error calling Starter.ApplicationStartBeforeInit() '{0}'. Exception: {1}";

        public const string APP_STARTER_AFTER_ERROR =
              "Error calling Starter.ApplicationStartAfterInit() '{0}'. Exception: {1}";


        public const string APP_FINISH_NOTIFIABLE_BEFORE_ERROR =
              "Error calling notifiable.ApplicationFinishBeforeCleanup() '{0}'. Exception: {1}";

        public const string APP_FINISH_NOTIFIABLE_AFTER_ERROR =
             "Error calling notifiable.ApplicationFinishAfterCleanup() '{0}'. Exception: {1}";



   public const string TEMPLATE_COMPILER_ALREADY_COMPILED_ERROR =
        "Operation not applicable as target TemplateCompiler instance has already been compiled. Compiler class: ";


    public const string TEMPLATE_CS_COMPILER_UNMATCHED_SPAN_ERROR =
        "Span started on line {0} and is unmatched up until line {1}";

    public const string TEMPLATE_CS_COMPILER_EMPTY_EXPRESSION_ERROR =
        "Empty expression on line {0}";

    public const string TEMPLATE_CS_COMPILER_CONFIG_CLOSE_TAG_ERROR =
        "Missing config close tag";

    public const string TEMPLATE_CS_COMPILER_CONFIG_ERROR =
        "Configuration segment error: ";

    public const string TEMPLATE_LJS_FRAGMENT_TRANSPILER_ERROR =
        "LJS fragment around {0} transpiler error: {1}";

    public const string TEMPLATE_JS_COMPILER_CONFIG_ERROR =
        "Configuration error: ";

    public const string TEMPLATE_JS_COMPILER_DUPLICATION_ID =
      "Duplication of id: {0}";

    public const string TEMPLATE_JS_COMPILER_INCLUDE_ERROR =
      "Cannot include path: '{0}'. Exception: {1}";


    public const string GUARDED_ACTION_SCOPE_ERROR =
        "Guarded action {0} threw: {1}";

    public const string GUARDED_CLAUSE_MAY_NOT_BE_NULL_ERROR =
        "Guarded method '{0}' clause '{1}' may not be null";

    public const string GUARDED_CLAUSE_OFTYPE_ERROR =
         "Guarded method '{0}' type clause '{1}' may not be null and must be of '{2}' type or its descendants";

    public const string GUARDED_CLAUSE_VALUEOFTYPE_ERROR =
         "Guarded method '{0}' clause value '{1}' may not be null and must be of '{2}' type or its descendants";

    public const string GUARDED_CLAUSE_TYPECAST_ERROR =
         "Guarded method '{0}' clause '{1}' may not be type cast to '{2}'";

    public const string GUARDED_CONFIG_NODE_CLAUSE_MAY_NOT_BE_EMPTY_ERROR =
        "Guarded method '{0}' config node clause '{1}' may not be null or empty";

    public const string GUARDED_STRING_CLAUSE_MAY_NOT_BE_BLANK_ERROR =
        "Guarded method '{0}' string clause '{1}' may not be null or blank/whitespace";

    public const string GUARDED_STRING_CLAUSE_MAY_NOT_EXCEED_MAX_LEN_ERROR =
        "Guarded method '{0}' string clause '{1}' = '{2}' length of {3} exceeds the max length of {4}";

    public const string GUARDED_STRING_CLAUSE_MAY_NOT_BE_LESS_MIN_LEN_ERROR =
        "Guarded method '{0}' string clause '{1}' = '{2}' length of {3} is less than the min length of {4}";

    public const string GUARDED_STRING_CLAUSE_MUST_BE_BETWEEN_MIN_MAX_LEN_ERROR =
        "Guarded method '{0}' string clause '{1}' = '{2}' length is {3} but must be between {4} and {5}";

    public const string GUARDED_CLAUSE_CONDITION_ERROR =
        "Guarded method '{0}' clause '{1}' failed condition check";

    public const string STREAM_READ_EOF_ERROR =
        "Stream EOF before operation could complete: ";


    public const string SLIM_READ_X_ARRAY_MAX_SIZE_ERROR =
        "Slim reader could not read requested array of {0} {1} as it exceeds the maximum limit of {2} bytes'";

    public const string SLIM_WRITE_X_ARRAY_MAX_SIZE_ERROR =
        "Slim writer could not write requested array of {0} {1} as it exceeds the maximum limit of {2} bytes'";



    public const string SLIM_SERIALIZATION_EXCEPTION_ERROR =
        "Exception in SlimSerializer.Serialize():  ";

    public const string SLIM_DESERIALIZATION_EXCEPTION_ERROR =
        "Exception in SlimSerializer.Deserialize():  ";


    public const string SLIM_DESERIALIZE_CALLBACK_ERROR =
        "Exception leaked from OnDeserializationCallback() invoked by SlimSerializer. Error:  ";

    public const string SLIM_ISERIALIZABLE_MISSING_CTOR_ERROR =
        "ISerializable object does not implement .ctor(SerializationInfo, StreamingContext): ";

    public const string SLIM_BAD_HEADER_ERROR =
        "Bad SLIM format header";

    public const string SLIM_TREG_COUNT_ERROR =
        "Slim type registry count mismatch";

    public const string SLIM_TREG_CSUM_ERROR =
        "Slim type registry CSUM mismatch";


    public const string SLIM_HNDLTOREF_MISSING_TYPE_NAME_ERROR =
        "HandleToReference(). Missing type name: ";


    public const string SLIM_ARRAYS_TYPE_NOT_ARRAY_ERROR =
        "DescriptorToArray(). Type is not array : ";

    public const string SLIM_ARRAYS_MISSING_ARRAY_DIMS_ERROR =
        "DescriptorToArray(). Missing array dimensions: ";

    public const string SLIM_ARRAYS_OVER_MAX_DIMS_ERROR =
        "Slim does not support an array with {0} dimensions. Only up to {1} maximum array dimensions supported";

     public const string SLIM_ARRAYS_OVER_MAX_ELM_ERROR =
        "Slim does not support an array with {0} elements. Only up to {1} maximum array elements supported";

    public const string SLIM_ARRAYS_WRONG_ARRAY_DIMS_ERROR =
        "DescriptorToArray(). Wrong array dimensions: ";

    public const string SLIM_ARRAYS_ARRAY_INSTANCE_ERROR =
        "DescriptorToArray(). Error instantiating array '";

    public const string SLIM_SER_PROHIBIT_ERROR =
        "Slim can not process type '{0}' as it is marked with [{1}] attribute";


    public const string POD_ISERIALIZABLE_MISSING_CTOR_ERROR =
        "ISerializable object does not implement .ctor(SerializationInfo, StreamingContext): ";

    public const string POD_DONT_KNOW_HOWTO_DESERIALIZE_FROM_CUSTOM_DATA =
        "PortableObjectDocument can not deserialize an instance of '{0}' as the type foes not provide neither PortableObjectDocumentDeserializationTransform nor ISerializable implementation";


    public const string IO_FS_ITEM_IS_READONLY_ERROR =
        "FileSystem '{0}' item '{1}' is read only";


    public const string GLUE_SHUTTING_DOWN_REPORT =
        "Component {0} is shutting down";

    public const string GLUE_CLIENT_CONNECTION_REPORT =
        "Transport {0} {1} client connection {2}{3}";

    public const string GLUE_TRANSPORT_CONNECTED_REPORT =
        "Transport {0} connected to address {1}";

    public const string GLUE_TRANSPORT_DISCONNECT_REPORT =
        "Transport {0} disconnected from address {1}: {2}";

    public const string GLUE_NAMED_BINDING_NOT_FOUND_ERROR =
        "The binding could not be located by name: ";

    public const string GLUE_CLIENT_CALL_ERROR =
        "Client call failed. Status: ";

    public const string GLUE_SYSTEM_NOT_RUNNING_ERROR =
        "Application, Glue or its components is not running/maybe shutting down";

    public const string GLUE_CLIENT_CALL_TRANSPORT_ACQUISITION_TIMEOUT_ERROR =
        "Binding '{0}' could not acquire client transport for making a call after waiting {1} ms. Revise limits in client transport binding config";

    public const string GLUE_CLIENT_CALL_NO_BINDING_ERROR =
        "'{0}' client call failed because there is no binding available";

    public const string GLUE_BINDING_GLUE_MISMATCH_ERROR =
        "Binding '{0}' does not work under this Glue: '{1}'";

    public const string GLUE_CALL_SERVICED_BY_DIFFERENT_REACTOR_ERROR =
        "The call is already serviced by different reactor instance";

    public const string GLUE_NO_INPROC_MATCHING_SERVER_ENDPOINT_ERROR =
        "No matching inrpoc server endpoint found for: ";

    public const string GLUE_MSG_DUMP_PATH_INVALID_ERROR =
        "Msg dump path is invalid: ";

    public const string GLUE_MSG_DUMP_FAILURE_ERROR =
        "Msg dump failure: ";

    public const string GLUE_DUPLICATE_NAMED_INSTANCE_ERROR =
        "Duplicate named instance: ";

    public const string GLUE_ONE_WAY_RESPONSE_ERROR =
        "Attempting to obtain response for a [OneWay] call";

    public const string GLUE_POOL_ITEM_ALLOCATION_ERROR =
        "Pool item allocation error: ";

    public const string GLUE_SERVER_ENDPOINT_CONTRACT_SERVER_TYPE_ERROR =
        "Server endpoint contract server name could not be resolved to type: ";

    public const string GLUE_TYPE_SPEC_ERROR =
        "The type specification could not be resolved to actual type. Make sure that contract assembly is present on both client and server: ";

    public const string GLUE_METHOD_SPEC_UNSUPPORTED_ERROR =
        "Method '{0}'.'{1}' is unsupported by Glue as it contains '{2}' parameter which is either generic, OUT or REF";

    public const string GLUE_METHOD_ARGS_MARSHAL_LAMBDA_ERROR =
        "Could not compile dynamic lambda for args marshaling for method '{0}'.'{1}'. Exception: {2}";


    public const string GLUE_NO_SERVER_INSTANCE_ERROR =
        "Attempt to invoke instance method but no server instance identified/allocated yet. Could not invoke server contract method: {0}.{1}";


    public const string GLUE_NO_ARGS_MARSHAL_LAMBDA_ERROR =
        "The server received dynamic RequestMsg-derivative for '{0}.{1}' however no dynamic lambda was compiled. This may be due to the mismatch between [ArgsMarshalling] on server contract method and the client";


    public const string GLUE_SERVER_CONTRACT_METHOD_INVOCATION_ERROR =
        "Contract method '{0}.{1}' threw exception: {2}";

    public const string GLUE_AMBIGUOUS_CTOR_DCTOR_DEFINITION_ERROR =
        "Ambiguous constructor/destructor definition. Could not invoke server contract method: {0}.{1}";

    public const string GLUE_ARGS_MARSHALLING_INVALID_REQUEST_MSG_TYPE_ERROR =
        "Invalid type supplied into ArgsMarshalling attribute. The request message type must be RequestMsg-derived and not be RequestAnyMsg";

    public const string GLUE_NATIVE_SERIALIZATION_WRONG_ROOT_ERROR =
        "Wrong root type passed to native MsgSerializer";

    public const string GLUE_NATIVE_SERIALIZATION_WRONG_ROOT_TOKEN_IN_STREAM_ERROR =
        "Wrong root token in stream read by MsgSerializer";

    public const string GLUE_ENDPOINT_CONTRACT_NOT_IMPLEMENTED_ERROR =
        "Server endpoint '{0}' does not implement contract: {1}";

    public const string GLUE_ENDPOINT_CONTRACT_INTF_MAPPING_ERROR =
        "Server endpoint '{0}' contract '{1}' method '{2}' could not be mapped to method in implementor '{3}'";

     public const string GLUE_ENDPOINT_MSPEC_NOT_FOUND_ERROR =
        "Server endpoint '{0}' contract '{1}' method spec '{2}' not found";


    public const string GLUE_ENDPOINT_CONTRACT_MANY_SERVERS_WARNING =
        "More than one server classes implement requested contract {0} at server endpoint '{1}'";

    public const string GLUE_SERVER_ONE_WAY_CALL_ERROR =
        "Server-side error while processing OneWay call: ";

    public const string GLUE_SERVER_HANDLER_ERROR =
        "Server-side error in handler: ";

    public const string GLUE_SERVER_INSTANCE_ACTIVATION_ERROR =
        "Server instance activation error for: ";

    public const string GLUE_STATEFUL_SERVER_INSTANCE_DOES_NOT_EXIST_ERROR =
        "Stateful server instance does not exist/may have expired or object store is not configured: ";

    public const string GLUE_STATEFUL_SERVER_INSTANCE_LOCK_TIMEOUT_ERROR =
        "Stateful server instance is not marked as [ThreadSafe] and could not be locked before timeout has passed: ";

    public const string GLUE_CLIENT_INSPECTORS_THREW_ERROR =
        "Client-side inspectors threw error upon arrived response message processing: ";

    public const string GLUE_CLIENT_CONNECT_ERROR =
        "Error connecting client {0} to address {1}: ";

    public const string GLUE_LISTENER_EXCEPTION_ERROR =
        "Listener threw exception: ";

    public const string GLUE_CLIENT_THREAD_ERROR =
        "Exception in client processing thread: ";

    public const string GLUE_CLIENT_THREAD_COMMUNICATION_ERROR =
        "Communication exception in client processing thread. Channel will be closed: ";

    public const string GLUE_UNEXPECTED_MSG_ERROR =
        "Received unexpected message. Expected: ";

    public const string GLUE_BAD_PROTOCOL_FRAME_ERROR =
        "Bad protocol transport frame: ";

     public const string GLUE_BAD_PROTOCOL_CLIENT_SITE_ERROR =
        "Bad protocol transport client site frame: ";

    public const string GLUE_MAX_MSG_SIZE_EXCEEDED_ERROR =
        "Message size of {0} bytes exceeds limit of {1} bytes. Operation: '{2}'";

    public const string GLUE_MPX_SOCKET_SEND_CLOSED_ERROR =
        "Glue mpx socket is closed and can not send data";

    public const string GLUE_MPX_SOCKET_SEND_CHUNK_ALREADY_GOTTEN_ERROR =
        "Glue mpx socket send chunk is already reserved/gotten and not released yet. Keep in mind that MpxSocket is NOT THREAD SAFE for sends";

    public const string GLUE_MPX_SOCKET_RECEIVE_ACTION_ERROR =
        "Glue mpx socket receive action leaked: ";


    public const string ASSERTION_ERROR =
        "Assertion failure";


    public const string AST_UNSUPPORTED_UNARY_OPERATOR_ERROR = "Unsupported unary operator: '{0}'";
    public const string AST_UNSUPPORTED_BINARY_OPERATOR_ERROR = "Unsupported binary operator: '{0}'";
    public const string AST_BAD_IDENTIFIER_ERROR = "Bad identifier: '{0}'";


    public const string CA_PROCESSOR_EXCEPTION_ERROR =
        "{0} processor {1} error: {2}";


    public const string SCHEMA_INCLUDE_FILE_DOSNT_EXIST_ERROR =
        "Relational schema include file does not exist: ";

    public const string SCHEMA_INCLUDE_FILE_REFERENCED_MORE_THAN_ONCE_ERROR =
        "Relational schema include file is referenced more than once. Cycle possible. File: ";

    public const string SCHEMA_HAS_ALREADY_COMPILED_OPTION_CHANGE_ERROR =
        "Can not change options on '{0}' after compilation happened";

    public const string RELATIONAL_COMPILER_OUTPUT_PATH_DOSNT_EXIST_ERROR =
        "Relational schema compiler output path does not exist: ";


    public const string RELATIONAL_COMPILER_INCLUDE_SCRIPT_NOT_FOUND_ERROR =
        "Relational schema compiler include script '{0}' at node '{1}' not found";


    public const string JSON_SERIALIZATION_MAX_NESTING_EXCEEDED_ERROR =
        "JSONWriter can not serialize object graph as it exceeds max nesting level of {0}. Graph may contain reference cycles";


    public const string JSON_DESERIALIZATION_ABSTRACT_TYPE_ERROR =
        "Type `{0}` is abstract. Did you forget to decorate the field or the type with custom {1} derivative to allow for object polymorphism on deserialization?";


    public const string CRUD_CURSOR_ALREADY_ENUMERATED_ERROR = "CRUD Cursor was already enumerated";


    public const string CRUD_FIELDDEF_TARGET_DERIVATION_ERROR =
      "Fieldset `{0}` contains bad `deriveFromTargetName` references to targets that are either not found in any [Field] attribute instance on that field, contain reference cycles, or missing the [Field] declarations without derivation dependencies. The field dependency graph could not be resolved";

    public const string CRUD_FIELDDEF_ATTR_MISSING_ERROR = "CRUD FieldDef must be constructed using at least one [Field] attribute. Name: '{0}'";

    public const string CRUD_FIELD_VALUE_REQUIRED_ERROR = "Field value is required";

    public const string CRUD_FIELD_VALUE_IS_NOT_IN_LIST_ERROR = "Field value `{0}` is not in the list of allowed values";

    public const string CRUD_FIELD_VALUE_MIN_LENGTH_ERROR = "Field value is shorter than min length of {0}";

    public const string CRUD_FIELD_VALUE_MAX_LENGTH_ERROR = "Field value exceeds max length of {0}";

    public const string CRUD_FIELD_VALUE_SCREEN_NAME_ERROR = "Field value is not a valid screen name ID";

    public const string CRUD_FIELD_VALUE_EMAIL_ERROR = "Field value is not a valid EMail";

    public const string CRUD_FIELD_VALUE_PHONE_ERROR = "Field value is not a valid Telephone";

    public const string CRUD_FIELD_VALUE_URI_ERROR = "Field value is not a valid Uri";

    public const string CRUD_FIELD_VALUE_REGEXP_ERROR = "Field value is not valid per defined format: {0}";

    public const string CRUD_FIELD_VALUE_MIN_BOUND_ERROR = "Field value is below the permitted min bound";

    public const string CRUD_FIELD_VALUE_MAX_BOUND_ERROR = "Field value is above the permitted max bound";

    public const string CRUD_FIELD_VALUE_GET_ERROR = "CRUD field '{0}' value get error: {1}";

    public const string CRUD_FIELD_VALUE_SET_ERROR = "CRUD field '{0}' value set error: {1}";

    public const string CRUD_GDID_ERA_CONVERSION_ERROR = "CRUD field '{0}' value conversion: GDID value with Era!=0 can not be converted to type '{1}'";

    public const string CRUD_FIELD_NOT_FOUND_ERROR = "CRUD field '{0}' not found in schema '{1}'";

    public const string CRUD_QUERY_RESOLVER_ALREADY_STARTED_ERROR = "CRUD QueryResolver already started to be used and can not be configured";

    public const string CRUD_METADATA_PARSE_ERROR = "Metadata parse at '{0}' exception: '{1}'. Metadata: {2}";

    public const string CRUD_FIELD_ATTR_PROTOTYPE_CTOR_ERROR = "Field attribute construction from prototype error: {0}";

    public const string CRUD_FIELD_VALUE_LIST_DUP_ERROR = "Value list `{0}..` contains a duplicate key `{1}`";

    public const string CRUD_TYPED_DOC_RECURSIVE_FIELD_DEFINITION_ERROR = "TypedDoc '{0}' recursive field definition. Check for [Field(prototype..)] cycle";

    public const string CRUD_TYPED_DOC_SINGLE_CLONED_FIELD_ERROR = "TypedDoc '{0}' defines field clone via [Field(....)]]'{1}' in which case only a single [Field(....)] decoration is allowed";

    public const string CRUD_TYPED_DOC_CLONED_FIELD_NOTEXISTS_ERROR = "TypedDoc '{0}' defines field clone via [Field(....)]]'{1}' but there is no field with such name in the cloned-from type";

    public const string CRUD_TYPE_IS_NOT_DERIVED_FROM_ROW_ERROR = "CRUD supplied type of '{0}' is not a Row-derivative";

    public const string CRUD_TYPE_IS_NOT_DERIVED_FROM_TYPED_DOC_ERROR = "CRUD supplied type of '{0}' is not a TypedDoc-derivative";

    public const string CRUD_FIND_BY_KEY_LENGTH_ERROR = "CRUD table FindByKey/KeyRowFromValues was supplied wrong number of key field values";

    public const string CRUD_ROW_UPGRADE_KEY_MUTATION_ERROR = "Upgraded row key has changed";

    public const string CRUD_ROWSET_OPERATION_ROW_IS_NULL_OR_SCHEMA_MISMATCH_ERROR = "CRUD rowset was supplied either a null row or a row with a different schema";

    public const string CRUD_TRANSACTION_IS_NOT_OPEN_ERROR = "CRUD transaction is not open for requested operation '{0}'. Current transaction status: '{0}'";

    public const string CRUD_OPERATION_NOT_SUPPORTED_ERROR = "CRUD operation not supported: ";

    public const string CRUD_QUERY_SOURCE_PRAGMA_ERROR = "CRUD query source pragma error in line number {0}: '{1}'. Error: {2}";

    public const string CRUD_QUERY_SOURCE_PRAGMA_LINE_ERROR = "PRAGMA line error Name: '{0}' Value: '{1}'";

    public const string CRUD_QUERY_RESOLUTION_ERROR = "CRUD query '{0}' could not be resolved: {1}";

    public const string CRUD_QUERY_RESOLUTION_NO_HANDLER_ERROR = "no handler matches query name";

    public const string CRUD_CONFIG_EMPTY_LOCATIONS_WARNING = "CRUD configuration contains empty location entries which are ignored";

    public const string CRUD_OPERATION_CALL_CONTEXT_SCOPE_MISMATCH_ERROR = "CRUDOperationCallContext scope mismatch error";

    public const string DISTRIBUTED_DATA_GDID_CTOR_ERROR = "GDID can not be created from the supplied: 'authority={0}>{1},counter={2}>{3}'";

    public const string DISTRIBUTED_DATA_GDID_PARSE_ERROR = "String value '{0}' can not be parsed as GDID";

    public const string DISTRIBUTED_DATA_PARCEL_INVALID_OPERATION_ERROR = "Error performing '{0}' operation on parcel '{1}' because parcel is in '{2}' state";

    public const string DISTRIBUTED_DATA_PARCEL_MERGE_NOT_IMPLEMENTED_ERROR = "Error performing Merge() operation on parcel '{0}' because DoMerge() is not implemented. Either 'parcel.MetadataAttribute.SupportsMerge' was not checked before making this call, or forgot to implement DoMerge()";

    public const string DISTRIBUTED_DATA_PARCEL_SEAL_VALIDATION_ERROR = "Error sealing parcel '{0}' due to validation errors: ";

    public const string DISTRIBUTED_DATA_PARCEL_UNWRAP_FORMAT_ERROR = "Parcel '{0}' can not unwrap the payload as its format '{1}' is not handled";
    public const string DISTRIBUTED_DATA_PARCEL_UNWRAP_DESER_ERROR = "Parcel '{0}' could not unwrap the payload due to deserialization exception: {1}";


    public const string DISTRIBUTED_DATA_PARCEL_MISSING_ATTRIBUTE_ERROR = "Parcel '{0}' does not specify the required [DataParcel(...)] attribute in its declaration";

    public const string ELINK_CHAR_COMBINATION_ERROR = "ELink '{0}' could not be read as it contains an invalid combination '{1}'";
    public const string ELINK_CHAR_LENGTH_LIMIT_ERROR = "ELink '{0}...' could not be encoded/decoded as it exceeds maximum permissible length";
    public const string ELINK_SEGMENT_LENGTH_ERROR = "ELink '{0}' could not be read as it contains an invalid segment data length";
    public const string ELINK_CHAR_LENGTH_ERROR = "ELink '{0}' could not be read as it contains an invalid character data length";
    public const string ELINK_CSUM_MISMATCH_ERROR = "ELink '{0}' could not be read as its checksum does not match";

    public const string CACHE_VALUE_FACTORY_ERROR = "Cache value factory func threw error from {0}. Exception: {1}";
    public const string CACHE_RECORD_ITEM_DISPOSE_ERROR = "Cache value threw error while trying to be disposed from {0}. Exception: {1}";

    public const string CACHE_TABLE_CTOR_SIZES_WARNING =
                      "Cache.Table.ctor(bucketCount==recPerPage), two parameters may not be equal because they will cause hash clustering. The 'recPerPage' has been increased by the system";


    public const string STANDARDS_DISTANCE_UNIT_TYPE_ERROR = "Unsupported distance unit type: {0}";
    public const string STANDARDS_WEIGHT_UNIT_TYPE_ERROR = "Unsupported weight unit type: {0}";

    public const string LOCAL_INSTALL_ROOT_PATH_NOT_FOUND_ERROR = "Local installation root path '{0}' does not exist";

    public const string LOCAL_INSTALL_NOT_STARTED_INSTALL_PACKAGE_ERROR = "Local installation not started but InstallPackage() was called";

    public const string LOCAL_INSTALL_PACKAGES_MANIFEST_FILE_NAME_COLLISION_ERROR =
      "Packages manifest file '{0}' could not be saved locally as some packages contain files that collide with the name";

    public const string LOCAL_INSTALL_LOCAL_MANIFEST_READ_ERROR =
      "Local installation can not open local manifest file '{0}'. Exception: {1}";

    public const string LOCAL_INSTALL_INSTALL_SET_PACKAGE_MANIFEST_READ_ERROR =
      "Local installation can not install a package '{0}' from the install set. The package manifest could not be read with exception: {1}";

    public const string LOCAL_INSTALL_INSTALL_SET_PACKAGE_WITHOUT_MANIFEST_ERROR =
      "Local installation can not install a package '{0}' from the install set. The package does not contain a manifest file '{1}' in its root";


    public const string FS_DUPLICATE_NAME_ERROR = "Can not have file system instance of type '{0}' with the name '{1}' as this name is already registered. ";

    public const string FS_SESSION_BAD_PARAMS_ERROR =
      "Can not create an instance of file system session '{0}'. Make sure that suitable derivative of FileSystemSessionConnectParams is passed for the particular file system";

    public const string NETGATE_CONFIG_DUPLICATE_ENTITY_ERROR =
              "NetGate configuration specifies duplicate name '{0}' for '{1}'";

    public const string NETGATE_VARDEF_NAME_EMPTY_CTOR_ERROR =
              "NetGate.VarDef must have a valid name in config or passed to .ctor";


    public const string FINANCIAL_AMOUNT_DIFFERENT_CURRENCIES_ERROR =
            "Financial operation '{0}' could not proceed. Amounts '{1}' and '{2}' are in different currencies";

    public const string FINANCIAL_AMOUNT_PARSE_ERROR =
            "Could not parse amount '{0}'";

    public const string PILE_PTR_VALIDATION_ERROR = "Pile pointer `{0}` value is invalid for '{1}'";

    public const string PILE_AV_BAD_PILEID_ERROR =  "Pile access violation. Piled '{0}' does not resolve to IPile instance by PileID";

    public const string PILE_AV_BAD_SEGMENT_ERROR =  "Pile access violation. Bad segment: ";

    public const string PILE_AV_BAD_ADDR_EOF_ERROR =  "Pile access violation. Bad address points beyond buf length: ";

    public const string PILE_AV_BAD_ADDR_CHUNK_FLAG_ERROR =  "Pile access violation. Does not point to a valid used object chunk: ";

    public const string PILE_AV_BAD_ADDR_PAYLOAD_SIZE_ERROR =  "Pile access violation. Pointer '{0}' point to wrong chunk payload size '{1}";

    public const string PILE_CHUNK_SZ_ERROR =  "Pile free chunk sizes are not properly configured. Must be consequently increasing {0} sizes starting at {1} bytes at minimum";

    public const string PILE_CONFIG_PROPERTY_ERROR =  "Pile configuration error in property '{0}'. Error: {1}";

    public const string PILE_DUPLICATE_ID_ERROR =  "Pile '{0}' duplicate identity '{1}'";

    public const string PILE_CRAWL_INTERNAL_SEGMENT_CORRUPTION_ERROR =  "Pile segment crawl internal error: chunk flag corrupted at address {0}";

    public const string PILE_OUT_OF_SPACE_ERROR =  "Pile is out of allowed space of {0:n} max bytes, {1} max segments @ {2:n} bytes/segment";

    public const string PILE_OBJECT_LARGER_SEGMENT_ERROR =  "Pile could not put object of {0:n0} bytes as this size exceeds the size of a segment of {1:n0} bytes";

    public const string PILE_MMF_NO_DATA_DIRECTORY_ROOT_ERROR =  "MMFPile data directory root '{0}' does not exist";


    public const string IO_STREAM_POSITION_ERROR =  "Stream position of {0} is beyond the length of stream {1}";

    public const string IO_STREAM_NOT_SUPPORTED_ERROR =  "Stream {0} does not support '{1}'";


    public const string METADATA_GENERATION_SCHEMA_FIELD_ERROR = "Error generating metadata for Schema `{0}` field `{1}`: {2}";


    public const string OVERPUNCH_TO_NUMBER_ERROR = "Signed overpunch value of `{0}` is not parsable as a number";

    public const string PILE_CACHE_SCV_START_PILE_NULL_ERROR =
      "Pile cache service can not start because pile is null";


    public const string PILE_CACHE_SCV_START_PILE_NOT_STARTED_ERROR =
      "Pile cache service can not start as it is injected a pile instance which is not managed by cache but has not been started externally";


    public const string PILE_CACHE_TBL_KEYTYPE_MISMATCH_ERROR =
      "Key type mismatch for pile cache table '{0}'. Existing: '{1}' Requested: '{2}'";

    public const string PILE_CACHE_TBL_DOES_NOT_EXIST_ERROR =
      "Pile cache table '{0}' does not exist in the cache";

    public const string PILE_CACHE_TBL_KEYCOMPARER_MISMATCH_ERROR =
      "Key comparer mismatch for pile cache table '{0}'. Existing: '{1}' Requested: '{2}'";


    public const string BSON_DOCUMENT_SIZE_EXCEEDED_ERROR =
        "Size '{0}' exceeds BSON default document size {1}";

    public const string BSON_ELEMENT_OBJECT_VALUE_SET_ERROR =
        "Can not set the '{0}' value of BSON element '{1}'. Error: {2}";

    public const string BSON_READ_PREMATURE_EOF_ERROR =
        "Premature EOF while doing '{0}' over BSON stream";

    public const string BSON_NAMED_ELEMENT_ADDED_ERROR =
        "The element with name '{0}' has already been added";

    public const string BSON_TYPE_NOT_SUPORTED_ERROR =
        "BSON type '{0}' is not supported. BSON source is likely corrupted";

    public const string BSON_DOCUMENT_RECURSION_ERROR =
        "BSONDocument recursion detected";

    public const string BSON_ARRAY_ELM_NAME_ERROR =
        "BSONElement.Name can not be accessed because it is an array element. Check IsArrayElement";

    public const string BSON_TEMPLATE_COMPILE_ERROR =
        "BSONDocument template compilation error: {0}. Template source: ' {1} '";

    public const string BSON_TEMPLATE_ARG_DEPTH_EXCEEDED =
        "BSONDocument template arg binding error: depth exceeded (possible recursion)";


    public const string BSON_ARRAY_ELM_DOC_ERROR =
        "BSONElement '{0}' is an array element and can not be used in document directly";

    public const string BSON_OBJECTID_LENGTH_ERROR =
        "Byte array length must be equal to 12";

    public const string BSON_THREE_BYTES_UINT_ERROR =
        "The value of {0} should be less than 2^24 to be correctly encoded as 3-bytes";

    public const string BSON_UNEXPECTED_END_OF_STRING_ERROR =
        "Unexpected end of string. Expected: 0x00";

    public const string BSON_INCORRECT_STRING_LENGTH_ERROR =
        "BSON source is corrupted. String length '{0}' should be positive integer";

    public const string BSON_READ_BOOLEAN_ERROR =
        "BSON source is corrupted. Unexpected boolean value '{0}'";

    public const string BSON_EOD_ERROR =
        "BSON source is corrupted. Incorrect end of document/array";

    public const string CLR_BSON_CONVERSION_TYPE_NOT_SUPPORTED_ERROR =
        "CLR type '{0}' conversion into BSON is not supported";

    public const string CLR_BSON_CONVERSION_REFERENCE_CYCLE_ERROR =
        "CLR value of type '{0}' could not be converted into BSON as there is a reference cycle";

    public const string DECIMAL_OUT_OF_RANGE_ERROR =
        "Decimal value of {0} is outside of to-int64 convertible range of {1}..{2}";

    public const string BUFFER_LONGER_THAN_ALLOWED_ERROR =
        "Byte[] buffer has a length of {0} bytes which is over the allowed maximum of {1} bytes";

    public const string BSON_DECIMAL_INT32_INT64_CONVERTION_ERROR =
        "Either BSONInt32 or BSONInt64 required for conversion to decimal";

    public const string BSON_DESERIALIZER_DOC_MISSING_TID_ERROR =
        "BSONSerializer.Deserialize() document missing the '{0}' type id string field";

    public const string BSON_DESERIALIZER_DOC_TID_GUID_ERROR =
        "BSONSerializer.Deserialize() document '{0}' field contain an invalid GUID string: '{1}'";

    public const string BSON_DESERIALIZER_MAKE_TYPE_ERROR =
        "BSONSerializer.Deserialize() error making type '{0}': {1}";

    public const string BSON_DESERIALIZER_DFB_ERROR = "BSONSerializer DeserializeFromBSON() leaked: {0}";
    public const string BSON_SERIALIZER_STB_ERROR =   "BSONSerializer SerializeToBSON() leaked: {0}";

    public const string BSON_GDID_BUFFER_ERROR =
        "Error converting GDID data buffer: {0}";

    public const string BSON_GUID_BUFFER_ERROR =
        "Error converting GUID data buffer: {0}";

    public const string SECDB_FILE_HEADER_ERROR = "Error while parsing the SecDB file header: ";

    public const string SECDB_FS_SEEK_STREAM_ERROR =
       "SecDB requires a file system that supports random file access with content stream seek. Passed '{0}' does not";

    public const string SECDB_FILE_NOT_FOUND_ERROR =
       "SecDB file '{0}' could not be read as it was not found by the file system '{1}'";

    public const string SEALED_STRING_OUT_OF_SPACE_ERROR =  "SealedString has allocated a maximum of {0} segments";
    public const string SEALED_STRING_TOO_BIG_ERROR =  "SealedString value of {0} bytes is too big";


    public const string DIRECTORY_COLLECTION_IS_NULL_OR_EMPTY_ERROR = "Directory operation'{0}' error: Item collection name is null or white space";
    public const string DIRECTORY_COLLECTION_CHARACTER_ERROR = "Directory operation'{0}' error: Item collection name contains an invalid character '{1}'. Collection names may only contain base ASCII/Latin characters or digits 'A'..'Z'/'0'..'9' digits or '_'";
    public const string DIRECTORY_COLLECTION_MAX_LEN_ERROR = "Directory operation'{0}' error: Item collection name of {1} characters exceeds max len of {2}";


    public const string CLIENT_WRONG_ENDPOINT_SERVICE_ERROR = "The supplied endpoint does not belong to this service: '{0}'";

    public const string CLIENT_WRONG_TRANSPORT_TYPE_ERROR = "Service '{0}' can not release transport '{1}'";

    public const string HTTP_CLIENT_CALL_FAILED = "Call to {0} on `{1}` eventually failed; {2} endpoints tried; See .InnerException";

    public const string HTTP_CLIENT_CALL_ASSIGMENT_ERROR = "HttpService .Call() can not be made due to invalid endpoint assignments: {0}";

    public const string WEB_CALL_RETURN_JSONMAP_ERROR = "The received content is not representable as JsonDataMap: '{0}..'";
    public const string WEB_CALL_UNSUCCESSFUL_ERROR = "Web call to `...{0}` was unsuccessful: HTTP {1} - {2}";


    public const string AROW_SATELLITE_ASSEMBLY_NAME_ERROR = "Could not find serialization satellite for assembly `{0}` as the name pattern does not match. The source assembly file name must end with `*.dll` by convention";
    public const string AROW_SATELLITE_ASSEMBLY_LOAD_ERROR = "Satellite assembly `{0}` load failure: {1}";
    public const string AROW_TYPE_NOT_SUPPORTED_ERROR =  "ArowSerializer does not have a ITypeSerializationCore registered for type '{0}'";
    public const string AROW_MEMBER_TYPE_NOT_SUPPORTED_ERROR =  "Members of type '{0}' are not supported";
    public const string AROW_GENERATOR_PATH_DOESNOT_EXIST_ERROR =  "Arow code generator: path '{0}' does not exist";

    public const string AROW_MAX_ARRAY_LEN_ERROR =  "Array max length of {0} exceeded by Arow deserialization";
    public const string AROW_DESER_CORRUPT_ERROR =  "Arow deserialization is corrupt";
    public const string AROW_HEADER_CORRUPT_ERROR =  "Arow header is corrupted";

    public const string LOCAL_GDID_GEN_SYSTEM_TIME_CHANGE_ERROR =
      "Local GDID generation failed because wait period for the next time slot was exceeded due to a significant clock drift. Did system time change? This generator needs an accurate time source synchronized down to 1 second";

    public const string TIME_HOURLIST_BAD_SPEC = "Bad hour list syntax, out of order, or span overlap in value `{0}`";
    public const string TIME_HOURLIST_BAD_SPEC_FOR = "Bad hour list syntax, out of order, or span overlap in value `{0}` specified for '{1}'";


    public const string MULTIPART_DOUBLE_EOL_ISNT_FOUND_AFTER_HEADER_ERROR = "Multipart: Double \\r\\n isn't found after header.";
    public const string MULTIPART_PARTS_COULDNT_BE_EMPTY_ERROR = "Multipart: Parts can not be null or empty.";
    public const string MULTIPART_PART_COULDNT_BE_EMPTY_ERROR = "Multipart: Part can not be empty";
    public const string MULTIPART_PART_MUST_BE_ENDED_WITH_EOL_ERROR = "Multipart: Part must be ended with EOL.";
    public const string MULTIPART_PART_IS_ALREADY_REGISTERED_ERROR = "Multipart: Part with the name {0} is already registered.";
    public const string MULTIPART_PART_EMPTY_NAME_ERROR = "Multipart: Name of part couldn't be null or empty.";
    public const string MULTIPART_NO_LF_NOR_CRLF_ISNT_FOUND_ERROR = "Neither \\r\\n nor \\n are found. Invalid multipart stream.";
    public const string MULTIPART_BOUNDARY_MISMATCH_ERROR = "Multipart: Boundary from stream doesn't match expected boundary.\r\nExpected=\"{0}\".\r\nActual=\"{1}\".";
    public const string MULTIPART_BOUNDARY_COULDNT_BE_SHORTER_3_ERROR = "Multipart: Boundary couldn't be shorter than 3 chars.";
    public const string MULTIPART_BOUNDARY_IS_TOO_SHORT = "Multipart: Full boundary is too short.";
    public const string MULTIPART_BOUNDARY_SHOULD_START_WITH_HYPHENS = "Multipart: Boundary should start with double hyphen.";
    public const string MULTIPART_TERMINATOR_ISNT_FOUND_ERROR = "Multipart: Terminator isn't found.";
    public const string MULTIPART_PART_SEGMENT_ISNT_TERMINATED_CORRECTLY_ERROR = "Multipart isn't terminated with {0}.";
    public const string MULTIPART_STREAM_NOT_NULL_MUST_SUPPORT_READ_ERROR = "Multipart: Stream couldn't be null and must support read operation.";
    public const string MULTIPART_STREAM_NOT_NULL_MUST_SUPPORT_WRITE_ERROR = "Multipart: Stream couldn't be null and must support write operation.";
    public const string MULTIPART_SPECIFIED_BOUNDARY_ISNT_FOUND_ERROR = "Specified boundary not found. Invalid multipart stream.";


    public const string GUID_TYPE_RESOLVER_NO_TYPES_ERROR = "{0} has no guid-decorated types registered";
    public const string GUID_TYPE_RESOLVER_ERROR = "Type id '{0}' does not map to any {1} type";
    public const string GUID_TYPE_RESOLVER_DUPLICATE_ATTRIBUTE_ERROR = "Type '{0}' specifies duplicate Guid '{1}' already used by '{2}'";
    public const string GUID_TYPE_RESOLVER_MISSING_ATTRIBUTE_ERROR = "Type '{0}' does not specify the required [{1}(...)] attribute in its declaration";


    public const string BIX_STREAM_CORRUPTED_ERROR = "Bix data stream is corrupted: ";
    public const string BIX_SATELLITE_ASSEMBLY_NAME_ERROR = "Could not find Bix serialization satellite for assembly `{0}` as the name pattern does not match. The source assembly file name must end with `*.dll` by convention";
    public const string BIX_CONFIGURED_ASSEMBLY_LOAD_ERROR = "Bix configuration includes assembly `{0}` which failed to load with: {1}.\n You may need to revise the `{2}` app config section";
    public const string BIX_SATELLITE_ASSEMBLY_LOAD_ERROR = "Bix satellite assembly `{0}` load failure: {1}";
    public const string BIX_TYPE_NOT_SUPPORTED_ERROR = "Bixer does not have a matching BixCore<`{0}`>";
    public const string BIX_MEMBER_TYPE_NOT_SUPPORTED_ERROR = "Bix members of type '{0}' are not supported";
    public const string BIX_GENERATOR_PATH_DOESNOT_EXIST_ERROR = "Bix code generator: path '{0}' does not exist";

    public const string BIX_JSON_HANDLER_UNRESOLVED_TYPE_ID_ERROR =
      "Bix type id `{0}` could not be resolved by BixJsonHandler. Did you register type resolver assemblies?";

    public const string BIX_MAX_SERIALIZATION_DEPTH_ERROR = "Bix max serialization depth of {0} was exceeded. Bix only supports DAGs and does not support cyclical references.";
    public const string BIX_MAX_ARRAY_LEN_ERROR = "Array max length of {0} exceeded by Bix deserialization";
    public const string BIX_DESER_CORRUPT_ERROR = "Bix deserialization is corrupt";
    public const string BIX_HEADER_CORRUPT_ERROR = "Bix header is corrupt";


    public const string BIX_WRITE_X_ARRAY_MAX_SIZE_ERROR = "Attempt to write an array [{0}] of {1} is over the allowed limit of {2}";
    public const string BIX_WRITE_X_COLLECTION_MAX_SIZE_ERROR = "Attempt to write a collection [{0}] of {1} is over the allowed limit of {2}";
    public const string BIX_READ_X_ARRAY_MAX_SIZE_ERROR = "Attempt to read an array [{0}] of {1} is over the allowed limit of {2}";


    public const string METADATA_CTOR_CONTENT_ERROR = "Metadata specification error. `{0}`.ctor(`{1}` content is bad). Revise attribute declaration. Cause: {2}";


    public const string STRAT_BINDING_NOTHING_REGISTERED_ERROR =
      "The `{0}` is not configured with any strategies. Revise assembly bindings";

    public const string STRAT_BINDING_UNRESOLVED_ERROR =
      "Strategy binding error: contract `{0}` could not be resolved into any actual implementing type. Revise assembly bindings";

    public const string STRAT_BINDING_MATCH_ERROR =
      "Strategy binding error: contract `{0}` failed to match context `{1}`. Revise assembly bindings and strategy traits/pattern matching decorations";

  }
}
