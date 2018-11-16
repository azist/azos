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
    public NOPMessenger() : base(){}

    public void SendMsg(Message msg)
    {

    }

    void IApplicationFinishNotifiable.ApplicationFinishBeforeCleanup(IApplication application)
    {

    }

    void IApplicationFinishNotifiable.ApplicationFinishAfterCleanup(IApplication application)
    {

    }
  }
}
