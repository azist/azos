/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azos.Wave.Kestrel
{
  /// <summary>
  /// Supresses Azos app container termination by intercepting CTRL+C combination;
  /// however .Net 6 `IWebHost.Start()` does not require it
  /// </summary>
  public sealed class AzosHostLifetime : DisposableObject, IHostLifetime
  {
    private readonly ILogger<AzosHostLifetime> m_Logger;

    public AzosHostLifetime(ILogger<AzosHostLifetime> logger)
    {
      m_Logger = logger;
    }

    protected override void Destructor()
    {
      base.Destructor();
      Console.CancelKeyPress -= cancelKeyPressed;
    }
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
      Console.CancelKeyPress += cancelKeyPressed;
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


    private void cancelKeyPressed(object sender, ConsoleCancelEventArgs e)
    {
      //////  Console.WriteLine("-------------- CTRL+C -----------------");
      m_Logger.LogInformation($"Console key combination Ctrl+C is intercepted by {nameof(AzosHostLifetime)}");
      e.Cancel = true;
    }
  }
}
