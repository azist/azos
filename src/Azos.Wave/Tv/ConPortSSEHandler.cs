using System;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.Wave.Handlers;

namespace Azos.Wave.Tv
{
  /// <summary>
  /// Handles SSE stream for console events
  /// </summary>
  public class ConPortSSEHandler : SSEMailboxHandler
  {
    public ConPortSSEHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                       : base(dispatcher, name, order, match){ }

    public ConPortSSEHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode)
                       : base(dispatcher, confNode) { }

    protected override (bool isNew, Mailbox mbox) ConnectMailbox(WorkContext work)
    {
      return base.ConnectMailbox(work);
    }

  }
}
