/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Sky.Messaging
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

    public IMessenger Messenger => ComponentDirector;

    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled{ get; set;}

    public abstract MsgChannels SupportedChannels { get; }

    /// <summary>
    /// Returns names of supported channels. A sink may support more than one channel, however
    /// a typical sink supports only one channel, hence the default implementation which returns this sink name
    /// </summary>
    public virtual IEnumerable<string> SupportedChannelNames { get { yield return Name; } }


    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING)]
    public SendMessageErrorHandling ErrorHandlingMode{ get ; set; }

    /// <summary>
    /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread.
    /// The message has to pass Filter().
    /// Returns true if message passed filter and was handled by the underlying implementation, false when it either did not pass filter
    /// or could not be handled by the underlying provider (e.g. contains no usable address etc..)
    /// </summary>
    public bool SendMsg(Message msg)
    {
      if (msg == null) throw new SkyException(StringConsts.ARGUMENT_ERROR + "MessageSink.SendMsg(msg==null)");

      if (!Running) return false;
      if (!Filter(msg)) return false;

      var sent = DoSendMsg(msg);
      if (!sent && ErrorHandlingMode == SendMessageErrorHandling.Throw)
        throw new SkyException(StringConsts.SENDING_MESSAGE_HAS_NOT_SUCCEEDED.Args(Name));

      return sent;
    }

    /// <summary>
    /// Override to perform message filtering. This is called by `SendMsg(msg)`.
    /// Return true if this sink can process the message in principle.
    /// The default implementation matches sink on the channel specified in "TO" address
    /// </summary>
    protected virtual bool Filter(Message msg)
    {
      return msg.AddressToBuilder.MatchNamedChannel(this.SupportedChannelNames); //why does this not match on CC or BCC??
    }

    /// <summary>
    /// Performs actual sending of msg. This method does not have to be thread-safe as it is called by a single thread
    /// </summary>
    protected abstract bool DoSendMsg(Message msg);

  }
}
