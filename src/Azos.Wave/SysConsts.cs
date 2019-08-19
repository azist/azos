/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Wave
{
  /// <summary>
  /// Non-localizable constants
  /// </summary>
  public static class SysConsts
  {
      /// <summary>
      /// Returns object {OK = true}
      /// </summary>
      public static readonly object JSON_RESULT_OK = new {OK = true};

      /// <summary>
      /// Returns object {OK = false}
      /// </summary>
      public static readonly object JSON_RESULT_ERROR = new {OK = false};


      public const string HEADER_IF_MODIFIED_SINCE = "If-Modified-Since";
      public const string HEADER_API_VERSION = "wv-api-ver";
      public const string HEADER_API_SESSION = "wv-api-session";
      public const string HEADER_DATA_CONTEXT = "wv-data-ctx";

      public const string WAVE_LOG_TOPIC = "WAVE";
      public const string NULL_STRING = "<null>";

      public const string UNSPECIFIED = "<unspecified>";

      public const string CONFIG_WAVE_SECTION = "wave";


  }
}
