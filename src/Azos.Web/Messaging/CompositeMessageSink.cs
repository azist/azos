/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Collections;
using Azos.Log;

namespace Azos.Web.Messaging
{
  /// <summary>
  /// Implements a composite sink which directs message delivery to multiple sinks
  /// </summary>
  public sealed class CompositeMessageSink : MessageSink
  {
    public CompositeMessageSink(MessageDaemon director) : base(director)
    {
    }

    private Registry<MessageSink> m_Sinks = new Registry<MessageSink>();


    public IRegistry<MessageSink> Sinks => m_Sinks;

    public override MsgChannels SupportedChannels
    {
      get
      {
        MsgChannels result = MsgChannels.Unspecified;

        //accumulate flags
        foreach (var sink in m_Sinks)
          result = result | SupportedChannels;

        return result;
      }
    }

    public override IEnumerable<string> SupportedChannelNames
      => m_Sinks.SelectMany(s => s.SupportedChannelNames).Distinct();


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      m_Sinks.Clear();
      foreach (var sect in node.Children)
      {
        var sink = FactoryUtils.MakeAndConfigure<MessageSink>(sect, args: new object[] { this.ComponentDirector });
        m_Sinks.Register(sink);
      }
    }

    protected override void DoStart()
    {
      try
      {
        if (!m_Sinks.Any())
          throw new WebException("Message sink registry is empty");

        m_Sinks.ForEach(s => s.Start());
      }
      catch (Exception error)
      {
        AbortStart();
        WriteLog(MessageType.CatastrophicError, nameof(DoStart), error.ToMessageWithType(), error);
        throw error;
      }
    }

    protected override void DoSignalStop()
    {
      try
      {
        m_Sinks.ForEach(s => s.SignalStop());
      }
      catch (Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(DoSignalStop), error.ToMessageWithType(), error);
        throw error;
      }
    }

    protected override void DoWaitForCompleteStop()
    {
      try
      {
        m_Sinks.ForEach(s => s.WaitForCompleteStop());
      }
      catch (Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(DoWaitForCompleteStop), error.ToMessageWithType(), error);
        throw error;
      }
    }

    protected override bool DoSendMsg(Message msg)
    {
      var sent = false;

      foreach (var sink in m_Sinks)
      {
        try
        {
          var wasSentNow = sink.SendMsg(msg);

          sent |= wasSentNow;
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Error, "{0}.{1}".Args(nameof(DoSendMsg), sink.Name), "Leaked: "+error.ToMessageWithType(), error);
        }
      }

      return sent;
    }

  }
}
