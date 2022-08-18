/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Security.EventHub
{
  /// <summary>
  /// Controls whether users can produce event hub events
  /// </summary>
  public sealed class EventProducerPermission : TypedPermission
  {
    public static readonly EventProducerPermission Instance = new EventProducerPermission();

    public EventProducerPermission() : base(AccessLevel.VIEW) { }
    public override string Description => Azos.Sky.StringConsts.PERMISSION_DESCRIPTION_EventProducer;
  }

  /// <summary>
  /// Controls whether users can consume(subscribe to) event hub event feed
  /// </summary>
  public sealed class EventConsumerPermission : TypedPermission
  {
    public static readonly EventConsumerPermission Instance = new EventConsumerPermission();

    public EventConsumerPermission() : base(AccessLevel.VIEW) { }
    public override string Description => Azos.Sky.StringConsts.PERMISSION_DESCRIPTION_EventConsumer;
  }
}
