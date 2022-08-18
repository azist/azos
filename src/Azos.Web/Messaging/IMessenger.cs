/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Conf;

namespace Azos.Sky.Messaging
{
  /// <summary>
  /// Describes an entity that can send Message(s) such as emails, text, voice calls etc.
  /// </summary>
  public interface IMessenger : IApplicationComponent, IDaemonView
  {
    void SendMsg(Message msg);
  }

  public interface IMessengerImplementation : IMessenger, IConfigurable, IDaemon
  {

  }
}
