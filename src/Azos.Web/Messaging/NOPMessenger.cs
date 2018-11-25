/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;

namespace Azos.Web.Messaging
{
  /// <summary>
  /// Implements NOP Mailer that does nothing
  /// </summary>
  public sealed class NOPMessenger : Daemon, IMessengerImplementation
  {
    public NOPMessenger(IApplication app) : base(app){}
    public NOPMessenger(IApplicationComponent dir) : base(dir) { }

    public void SendMsg(Message msg)
    {
    }

    public override string ComponentLogTopic => CoreConsts.WEBMSG_TOPIC;
  }
}
