/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky
{
  internal static class StringConsts
  {
    public const string UNKNOWN = "<unknown>";
    public const string ARGUMENT_ERROR = "Argument error: ";

    public const string FS_SVN_PARAMS_SERVER_URL_ERROR =
          "SVN connection parameters need to specify non-blank ServerURL";

    public const string GEO_LOOKUP_SVC_RESOLUTION_ERROR =
          "GEO lookup service does not support '{0}' resolution";

    public const string GEO_LOOKUP_SVC_PATH_ERROR =
          "GEO lookup service is pointed at '{0}' path which does not exist";

    public const string GEO_LOOKUP_SVC_DATA_FILE_ERROR =
          "GEO lookup service needs data file '{0}' which was not found";

    public const string GEO_LOOKUP_SVC_CANCELED_ERROR =
          "GEO lookup service start canceled";

    public const string MAILER_SINK_IS_NOT_SET_ERROR = "Mailer sink is not set";
    public const string MESSAGE_SINK_IS_NOT_OWNED_ERROR = "The message sink being set is not owned by this service";
    public const string SENDING_MESSAGE_HAS_NOT_SUCCEEDED = "Sending message on sink '{0}' has not succeeded";

    public const string MAILER_SINK_SMTP_IS_NOT_CONFIGURED_ERROR = "SMTP Mailer sink is not configured: ";

    public const string FS_SESSION_BAD_PARAMS_ERROR =
      "Can not create an instance of file system session '{0}'. Make sure that suitable derivative of FileSystemSessionConnectParams is passed for the particular file system";

    public const string HTTP_OPERATION_ERROR = "HTTP[S] error: ";

    public const string CMS_ID_ERROR =
       "Got invalid ContentId `{0}`='{1}'. Only the following characters are permitted in CMS ContentId: 'a-z|A-Z|0-9|.|-|_' and be no longer than {2} chars. May not start with `.`";

    public const string CMS_CONTENT_ACCESS_DENIED = "Content access denied: {0}";

    public const string PERMISSION_DESCRIPTION_RemoteTerminalOperatorPermission =
      "Controls whether users can access remote terminals";

    public const string PERMISSION_DESCRIPTION_ChroniclePermission =
       "Controls whether users can access chronicles";

    public const string PERMISSION_DESCRIPTION_EventProducer =
       "Controls whether users can produce event hub events";

    public const string PERMISSION_DESCRIPTION_EventConsumer =
       "Controls whether users can consume(subscribe to) event hub event feed";

  }
}
