/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.WebMessaging
{
  /// <summary>
  /// Denotes the status of the message
  /// </summary>
  public enum MsgStatus
  {
    /// <summary>
    /// The message is new/initial status
    /// </summary>
    Initial = 0,

    /// <summary>
    /// The message was read, but may show in the list of read messages
    /// </summary>
    Read,

    /// <summary>
    /// The message is archived - it should not appear in regular lists
    /// </summary>
    Archived,

    /// <summary>
    /// The message is marked for deletion and should be destroyed
    /// </summary>
    Deleted
  }

  /// <summary>
  /// Denotes types of message visibility/publication
  /// </summary>
  public enum MsgPubStatus
  {
    /// <summary>
    /// Message was banned and is not shown
    /// </summary>
    Banned = -1,

    /// <summary>
    /// Message is published and can be shown
    /// </summary>
    Published = 0,

    /// <summary>
    /// Message is in draft mode and not published yet ( visible only to msg author)
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Message is in preview mode for the limited audience
    /// </summary>
    Preview = 2,

    /// <summary>
    /// Message needs review by the system/moderator before it can be shown.
    /// Moderator can transition this to DRAFT, BANNED or PUBLISHED
    /// </summary>
    NeedsReview = 3
  }

  public enum MsgChannelWriteResult
  {
    /// <summary>Message was not sent now and will not be delivered in future due to sending error in channel</summary>
    ChannelError = -10,

    /// <summary>Message was not sent now and will not be delivered in future due to sending error</summary>
    PermanentFailure = -3,

    /// <summary>The channel encoutered error and will try to resend in some time</summary>
    WillRetryAfterFailure = -2,

    /// <summary>The message can not be delivered in principle (e.g. bad address)</summary>
    Undeliverable = -1,

    /// <summary>The operation finished with an undetermined state</summary>
    Undefined = 0,

    /// <summary>The message was sent</summary>
    Success = 1,

    /// <summary>The message was routed into gateway</summary>
    Gateway = 2
  }
}
