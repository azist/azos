namespace Azos.Web.Messaging
{
  public sealed class NOPMessageSink : MessageSink
  {
    public NOPMessageSink(MessageService director)
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
