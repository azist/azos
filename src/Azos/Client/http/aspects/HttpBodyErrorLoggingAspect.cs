/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Log;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Client
{
  /// <summary>
  /// Reacts to HTTP request body errors detected by server.
  /// WAVE server uses a special response header `wv-body-error` by default
  /// which communicates a problem back to the calling client.
  /// A typical use case is to log the client body which was sent to server but the server was
  /// not able to parse it, responding with 400 and additional details in the aforementioned header
  /// </summary>
  public sealed class HttpBodyErrorLoggingAspect : AspectBase, IHttpBodyErrorAspect
  {
    public HttpBodyErrorLoggingAspect(IApplicationComponent director, IConfigSectionNode config) : base(director, config)
    {
    }

    public override string ComponentLogTopic => CoreConsts.CLIENT_TOPIC;

    /// <inheritdoc/>
    [Config]
    public string BodyErrorHeader { get;  set; }

    /// <summary>
    /// When set instructs the handler to dump http content object as byte[] locally at the specified path.
    /// The file name has a GUID matching the log message
    /// </summary>
    [Config]
    public string ContentBinDumpPath { get; set; }


    /// <inheritdoc/>
    public async Task ProcessBodyErrorAsync(string uri,
                      HttpMethod method,
                      object body,
                      string contentType,
                      JsonWritingOptions options,
                      HttpRequestMessage request,
                      HttpResponseMessage response,
                      bool isSuccess,
                      string rawResponseContent,
                      IEnumerable<string> bodyErrorValues)
     {
       var guid = Guid.NewGuid();

       var pars = new
       {
         uri = uri,
         mtd = method.Method,
         ctp = contentType,
         body = body == null ? null : new {
           tp = body.GetType().DisplayNameWithExpandedGenericArgs(),
         },

         success = isSuccess,
         response = response == null ? null : new {
           code = response.StatusCode,
           reason = response.ReasonPhrase,
           contentSnippet = rawResponseContent.TakeFirstChars(0xff, " ...more")
         },
         errors = bodyErrorValues
       }.ToJson(JsonWritingOptions.CompactRowsAsMap);


       var msg = new Message()
       {
         Guid = guid,
         Topic = CoreConsts.CLIENT_TOPIC,
         From = nameof(HttpBodyErrorLoggingAspect),
         Type = MessageType.Error,
         Text = "Body error",
         RelatedTo = Ambient.CurrentCallFlow?.ID ?? Guid.Empty,
         Parameters = pars
       };


       var dumpPath = ContentBinDumpPath;
       if (dumpPath.IsNotNullOrWhiteSpace())
       {
         await this.DontLeakAsync(async () =>
         {
           var fn = System.IO.Path.Combine(dumpPath, "{0}.content.bin".Args(guid));
           var rawcontent = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
           await System.IO.File.WriteAllBytesAsync(fn, rawcontent).ConfigureAwait(false);
         }, $"Dumping content at `{dumpPath}` failed: ").ConfigureAwait(false);
       }

       //The asynchronous log api has synchronous intf
       ComponentDirector.App.Log.Write(msg);
     }
  }
}
