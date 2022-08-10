/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Wave.Kestrel
{
  internal class AzosLogProvider : ILoggerProvider
  {
    internal AzosLogProvider(IApplication app) => App = app.NonNull(nameof(app));
    public void Dispose() { }

    public readonly IApplication App;

    public ILogger CreateLogger(string categoryName) => new AzosLogger(App, categoryName);
  }

  internal struct AzosLogger : ILogger
  {
    public static readonly Atom CHANNEL = Atom.Encode("msft");

    internal AzosLogger(IApplication app, string topic)
    {
      App = app.NonNull(nameof(app));
      Topic = topic;
    }

    public readonly IApplication App;
    public readonly string Topic;

    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      var msg = new Message
      {
        Channel = CHANNEL,
        Topic = Topic,
        Type = xlat(logLevel),
        From = $"{eventId.Id}::{eventId.Name}",
        Exception = exception,
        Text = formatter(state, exception),
        Parameters = state.ToJson()
      };

      App.Log.Write(msg);
    }

    private MessageType xlat(LogLevel lvl)
    {
      switch (lvl)
      {
        case LogLevel.Debug: return MessageType.Debug;
        case LogLevel.Trace: return MessageType.Trace;
        case LogLevel.Information : return MessageType.Info;
        case LogLevel.Warning: return MessageType.Warning;
        case LogLevel.Error: return MessageType.Error;
        case LogLevel.Critical: return MessageType.Critical;
        default: return MessageType.Info;
      }
    }
  }
}
