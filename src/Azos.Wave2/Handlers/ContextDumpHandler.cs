/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Web;
using Azos.Conf;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Dumps WorkContext status - used for debugging purposes
  /// </summary>
  public class ContextDumpHandler : WorkHandler
  {
     protected ContextDumpHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match){}
     protected ContextDumpHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode){}

     protected override void DoHandleWork(WorkContext work)
     {
        var dump = new{
          About = work.About,

          Server = new
          {
             Name = work.Server.Name,
             Type = work.Server.GetType().FullName,
             LocalTime = work.Server.LocalizedTime,
             Environment = work.Server.EnvironmentName,
             KernelHttpQueueLimit = work.Server.KernelHttpQueueLimit,
             ParallelAccepts = work.Server.ParallelAccepts,
             ParallelWorks = work.Server.ParallelWorks,
             Prefixes = work.Server.Prefixes,
             Dispatcher = work.Server.Dispatcher.Name,
             ShowDumpMatches = work.Server.ShowDumpMatches.OrderedValues.Select(m=>m.Name).ToList(),
             LogMatches = work.Server.LogMatches.OrderedValues.Select(m=>m.Name).ToList(),
          },//server
          Request = new
          {
             AcceptTypes = work.Request.AcceptTypes,
             ContentEncoding = work.Request.ContentEncoding!= null?work.Request.ContentEncoding.EncodingName : SysConsts.NULL_STRING,
             ContentLength = work.Request.ContentLength64,
             ContentType  = work.Request.ContentType,
             Cookies = work.Request.Cookies.Count,
             HasEntityBody = work.Request.HasEntityBody,
             Headers = collToList( work.Request.Headers),
             Method = work.Request.HttpMethod,
             IsAuthenticated = work.Request.IsAuthenticated,
             IsLocal = work.Request.IsLocal,
             IsSecure = work.Request.IsSecureConnection,
             KeepAlive = work.Request.KeepAlive,
             LocalEndPoint = work.Request.LocalEndPoint.ToString(),
             ProtoVersion = work.Request.ProtocolVersion.ToString(),
             Query = collToList(work.Request.QueryString),
             RawURL = work.Request.RawUrl,
             RemoteEndPoint = work.Request.RemoteEndPoint.ToString(),
             EffectiveCallerRemoteEndPoint = work.EffectiveCallerIPEndPoint.ToString(),
             UserAgent = work.Request.UserAgent,
             UserHostAddress = work.Request.UserHostAddress,
             UserHostName = work.Request.UserHostName,
             UserLanguages = work.Request.UserLanguages,
          },
          Dispatcher = new
          {
             Filters = work.Server.Dispatcher.Filters.Select(f=>f.ToString()),
             Handlers = work.Server.Dispatcher.Handlers.Select(h=>h.ToString()),

          },

          Session = work.Session==null ? SysConsts.NULL_STRING : work.Session.GetType().FullName,

          Handled = work.Handled,
          Aborted = work.Aborted,
          NoDefaultAutoClose = work.NoDefaultAutoClose,
          Items = work.Items.Select(kvp=>kvp.Key),
          GeoEntity = work.GeoEntity!=null?work.GeoEntity.ToString() : SysConsts.NULL_STRING
        };



        work.Response.ContentType = ContentType.JSON;
        work.Response.WriteJSON(dump, Serialization.JSON.JsonWritingOptions.PrettyPrint);
     }

     private List<string> collToList(System.Collections.Specialized.NameValueCollection collection)
     {
        var result = new List<string>();
        for(var i=0; i<collection.Count; i++)
          result.Add("{0} = {1}".Args(collection.GetKey(i), collection[i]));

        return result;
     }

  }
}
