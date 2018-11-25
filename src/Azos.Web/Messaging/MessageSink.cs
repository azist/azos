/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Web.Messaging
{

  /// <summary>
  /// Marker interface for injection into MessageDaemon.Sink property from code
  /// </summary>
  public interface IMessageSink
  {
    IMessenger Messenger {  get; }
  }

  /// <summary>
  /// Base for ALL implementations that work under MailerService
  /// </summary>
  public abstract class MessageSink : DaemonWithInstrumentation<MessageDaemon>, IMessageSink, IConfigurable
  {

    protected MessageSink(MessageDaemon director) : base(director)
    {

    }

    public override string ComponentLogTopic => CoreConsts.WEBMSG_TOPIC;

    public IMessenger Messenger=> ComponentDirector;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled{ get; set;}

    public abstract MsgChannels SupportedChannels { get; }

    public virtual IEnumerable<string> SupportedChannelNames { get { yield return Name; } }


    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING)]
    public SendMessageErrorHandling ErrorHandlingMode{ get ; set; }

    /// <summary>
    /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread
    /// </summary>
    public bool SendMsg(Message msg)
    {
      if (msg == null) throw new WebException(StringConsts.ARGUMENT_ERROR + "MessageSink.SendMsg(msg==null)");

      if (!Running) return false;
      if (!Filter(msg)) return false;

      var sent = DoSendMsg(msg);
      if (!sent && ErrorHandlingMode == SendMessageErrorHandling.Throw)
        throw new WebException(StringConsts.SENDING_MESSAGE_HAS_NOT_SUCCEEDED.Args(Name));

      return sent;
    }

    protected virtual bool Filter(Message msg)
    {
      return msg.AddressToBuilder.MatchNamedChannel(this.SupportedChannelNames);
    }

    /// <summary>
    /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread
    /// </summary>
    protected abstract bool DoSendMsg(Message msg);

  }
}
