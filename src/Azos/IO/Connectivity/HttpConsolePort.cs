using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Conf;
using Azos.Web;

namespace Azos.IO.Connectivity
{
  /// <summary>
  /// Provides tele (remote) console port implementation based on Http/s
  /// </summary>
  public abstract class HttpConsolePort : TeleConsolePort, IConsolePort
  {

    public HttpConsolePort(IApplication app, string name = null) : base(app, name)
    {
    }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Http);
      base.Destructor();
    }

    private HttpClient m_Http;

    protected override void SendMsgBatchOnce(TeleConsoleMsgBatch batch)
    {
      var response = m_Http.PostAndGetJsonMapAsync("tele-con", batch).GetAwaiter().GetResult();
      response.UnwrapPayloadMap();
      return;
    }
  }

}
