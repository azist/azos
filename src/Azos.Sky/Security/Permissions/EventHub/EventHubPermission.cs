/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;

namespace Azos.Sky.Security.Permissions.EventHub
{
  /// <summary>
  /// Controls whether users can produce event hub events
  /// </summary>
  public sealed class EventProducerPermission : TypedPermission
  {
    public EventProducerPermission() : base(AccessLevel.VIEW) { }
    public override string Description => StringConsts.PERMISSION_DESCRIPTION_EventProducer;
  }

  /// <summary>
  /// Controls whether users can consume(subscribe to) event hub event feed
  /// </summary>
  public sealed class EventConsumerPermission : TypedPermission
  {
    public EventConsumerPermission() : base(AccessLevel.VIEW) { }
    public override string Description => StringConsts.PERMISSION_DESCRIPTION_EventConsumer;
  }
}
