using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Relays matching traffic to the external party for execution passing through request/response pair back and forth
  /// </summary>
  public class RelayHandler : WorkHandler
  {
    private string m_RemoteUri;

    //temp code until async pipeline
    protected override void DoHandleWork(WorkContext work) => DoHandleWorkAsync(work).GetAwaiter().GetResult();

    protected virtual async Task DoHandleWorkAsync(WorkContext work)
    {
      var uri = CalculateRemoteUri(work);
      var client = GetClient();
      await RelayRequest(work, client);
      await RelayResponse(work, client);
    }

    /// <summary>
    /// Appends request absolute path to RemoteUri. Override to do custom addressing
    /// </summary>
    protected virtual string CalculateRemoteUri(WorkContext work)
    {
      //dont forget query
      var query = work.Request.Url.Query;
      return null;
    }

    protected virtual HttpClient GetClient()
    {
      return null;
    }

    protected virtual async Task RelayRequest(WorkContext work, HttpClient client, string clientUri)
    {
      var method = new HttpMethod(work.Request.HttpMethod);
      HttpContent content = null;
      using (var request = new HttpRequestMessage(method, clientUri))
      {
        request.Content = content;
        using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
        {
          work.Response.StatusCode = (int)response.StatusCode;
          work.Response.StatusDescription = response.ReasonPhrase;
          //work.Response.ContentType = response.Content.Headers.ContentType.ToString();
          response.Headers.ForEach(rh => work.Response.Headers.Add(rh.ToString()));

          //todo Add FORWARDED for Header
          await response.Content.CopyToAsync(work.Response.GetDirectOutputStreamForWriting());
        }

      }
    }

    protected virtual Task RelayResponse(WorkContext work, HttpClient client)
    {
      return Task.CompletedTask;
    }

  }
}
