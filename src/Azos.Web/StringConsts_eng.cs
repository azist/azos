/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Web
{
  public static class StringConsts
  {
    public const string UNKNOWN = "<unknown>";
    public const string ARGUMENT_ERROR = "Argument error: ";

    public const string OPERATION_NOT_SUPPORTED_ERROR = "Operation not supported: ";

    public const string WEB_CALL_RETURN_JSONMAP_ERROR = "The received content is not representable as JsonDataMap: '{0}..'";


    public const string WEB_REQUEST_ERROR = "Error while performing WebRequest: ";

    public const string FS_SVN_PARAMS_SERVER_URL_ERROR =
          "SVN connection parameters need to specify non-blank ServerURL";

    public const string FS_S3_PARAMS_SERVER_URL_ERROR =
          "S3 connection parameters need to specify non-blank Bucket and Region";

    public const string GEO_LOOKUP_SVC_RESOLUTION_ERROR =
          "GEO lookup service does not support '{0}' resolution";

    public const string GEO_LOOKUP_SVC_PATH_ERROR =
          "GEO lookup service is pointed at '{0}' path which does not exist";

    public const string GEO_LOOKUP_SVC_DATA_FILE_ERROR =
          "GEO lookup service needs data file '{0}' which was not found";

    public const string WEB_CALL_UNSUCCESSFUL_ERROR = "Web call to `..{0}` was unsuccessful: HTTP {1} - {2}";


    public const string MULTIPART_DOUBLE_EOL_ISNT_FOUND_AFTER_HEADER_ERROR = "Multipart: Double \\r\\n isn't found after header.";
    public const string MULTIPART_PARTS_COULDNT_BE_EMPTY_ERROR = "Multipart: Parts couldn't be null or empty.";
    public const string MULTIPART_PART_COULDNT_BE_EMPTY_ERROR = "Multipart: Part couldn't empty.";
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

    public const string GEO_LOOKUP_SVC_CANCELED_ERROR =
          "GEO lookup service start canceled";

    public const string MAILER_SINK_IS_NOT_SET_ERROR = "Mailer sink is not set";
    public const string MESSAGE_SINK_IS_NOT_OWNED_ERROR = "The message sink being set is not owned by this service";
    public const string SENDING_MESSAGE_HAS_NOT_SUCCEEDED = "Sending message on sink '{0}' has not succeeded";

    public const string MAILER_SINK_SMTP_IS_NOT_CONFIGURED_ERROR = "SMTP Mailer sink is not configured: ";

    public const string FS_SESSION_BAD_PARAMS_ERROR =
      "Can not create an instance of file system session '{0}'. Make sure that suitable derivative of FileSystemSessionConnectParams is passed for the particular file system";

    public const string DELETE_MODIFY_ERROR = "Can not modify field value while in deleting operation. Field <{0}>";

    public const string HTTP_OPERATION_ERROR = "HTTP[S] error: ";

  }
}
