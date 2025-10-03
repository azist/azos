/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Azos.Wave.Kestrel
{
  /// <summary>
  /// Provides middleware that routes incoming traffic into <see cref="WaveServer"/> for processing.
  /// If no WaveServer instance claims the traffic, it is passed down the middleware chain.
  /// You would typically add this in app builder: <code>app.UseMiddleware&lt;AzosWaveMiddleware&gt;(app);</code>
  /// </summary>
  public class AzosWaveMiddleware
  {
    private readonly RequestDelegate m_Next;
    private readonly IApplication m_App;

    public AzosWaveMiddleware(RequestDelegate next, IApplication app)
    {
      m_Next = next.NonNull(nameof(next));
      m_App = app.NonNull(nameof(app));
    }

    public async Task InvokeAsync(HttpContext context)
    {
      //app.UseMiddleware<AzosWaveMiddleware>(app);

      var pool = WaveServer.Pool.Get(m_App);
      var server = await pool.DispatchAsync(context).ConfigureAwait(false);

      if (server == null)
        await m_Next(context).ConfigureAwait(false);
    }

  }
}
