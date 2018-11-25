/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading;

using Azos.Glue.Protocol;

namespace Azos.Glue
{
  /// <summary>
  /// In the server side code, provides access to server call ambient context. Use to access Headers
  /// </summary>
  public static class ServerCall
  {
    private static AsyncLocal<IGlue> ats_Glue = new AsyncLocal<IGlue>();
    private static AsyncLocal<RequestMsg> ats_Request = new AsyncLocal<RequestMsg>();
    private static AsyncLocal<Headers>    ats_ResponseHeaders = new AsyncLocal<Headers>();


    /// <summary>
    /// Returns app chassis reference that the call is serviced by
    /// </summary>
    public static IApplication App => ats_Glue.Value?.App;

    /// <summary>
    /// Returns Glue reference that the call is serviced by
    /// </summary>
    public static IGlue Glue => ats_Glue.Value;

    /// <summary>
    /// Returns RequestMsg which is being processed. Access incoming headers through Request.Headers
    /// </summary>
    public static RequestMsg Request => ats_Request.Value;

    /// <summary>
    /// Returns Headers instance that will be appended to response.
    /// If headers are null, they will get created
    /// </summary>
    public static Headers ResponseHeaders
    {
      get
      {
        var result = ats_ResponseHeaders.Value;
        if (result==null)
        {
          result = new Headers();
          ats_ResponseHeaders.Value = result;
        }
        return result;
      }
    }

    /// <summary>
    /// Returns response headers if allocated or null
    /// </summary>
    public static Headers GetResponseHeadersOrNull()
    {
      return ats_ResponseHeaders.Value;
    }

    /// <summary>
    /// Internal framework-only method to bind thread-level context.
    /// Normally this would never be called but in some unit tests
    /// </summary>
    public static void __SetThreadLevelContext(IGlue glue, RequestMsg request)
    {
      ats_Glue.Value = glue;
      ats_Request.Value = request;
    }

    /// <summary>
    /// Internal framework-only method to clear thread-level context
    /// </summary>
    public static void __ResetThreadLevelContext()
    {
      ats_Glue.Value = null;
      ats_Request.Value = null;
      ats_ResponseHeaders.Value = null;
    }
  }

}
