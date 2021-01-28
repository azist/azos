/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
namespace Azos.Web.Messaging.Sinks
{
  public sealed class NOPMessageSink : MessageSink
  {
    public NOPMessageSink(MessageDaemon director)
      : base(director)
    {
    }

    public override MsgChannels SupportedChannels
    {
      get
      {
        return MsgChannels.Unspecified;
      }
    }

    protected override bool DoSendMsg(Message msg)
    {
      return true;
    }
  }
}
