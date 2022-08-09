/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Net;

using Azos.Templatization;

namespace Azos.Wave.Templatization
{
    /// <summary>
    /// Renders into HttpResponse object
    /// </summary>
    public class ResponseRenderingTarget : IRenderingTarget
    {
        public ResponseRenderingTarget(WorkContext work)
        {
          Context = work;
          Response = work.Response;
        }

        /// <summary>
        /// Returns HttpContext for current request
        /// </summary>
        public readonly WorkContext Context;

        /// <summary>
        /// Returns Response object for WorkContext
        /// </summary>
        public readonly Response Response;


        public object Encode(object value)
        {
            if (value==null) return string.Empty;

            return WebUtility.HtmlEncode(value.ToString());
        }

        public void Write(object value)
        {
            Response.Write(value);
        }


        public void Flush(bool lastChunk)
        {
            Response.Flush();
        }
    }
}
