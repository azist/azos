
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

      public const string WAVE_LOG_TOPIC = "WAVE";
      public const string NULL_STRING = "<null>";

      public const string UNSPECIFIED = "<unspecified>";

      public const string CONFIG_WAVE_SECTION = "wave";


  }
}
