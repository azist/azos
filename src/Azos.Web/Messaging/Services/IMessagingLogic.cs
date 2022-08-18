/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data.Business;

namespace Azos.Sky.Messaging.Services
{
  /// <summary>
  /// Defines contract for sending web messages
  /// </summary>
  public interface IMessagingLogic : IModule
  {
    /// <summary>
    /// Sends one message asynchronously optionally attaching the ad hoc properties.
    /// Returns a unique message Id which can be used to query the message (if system supports it)
    /// later via IMessageQueryLogic contract, or NULL if the message storage is not supported.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The returned message id is the one assigned by doc storage service and it may or may not be the same as msg.ID.
    /// NULL is returned if the message is NOT saved anywhere.
    /// </para>
    /// <para>
    /// Throws on validation and delivery errors
    /// </para>
    /// </remarks>
    Task<string> SendAsync(Message msg, MessageProps props = null);
  }

  /// <summary>
  /// Provides functionality for message stores which provide message retrieval services
  /// </summary>
  public interface IMessageArchiveLogic : IModule
  {
    /// <summary>
    /// Retrieves a list of message infos (headers) from the server per the supplied filter
    /// </summary>
    Task<IEnumerable<MessageInfo>> GetMessageListAsync(MessageListFilter filter);

    /// <summary>
    /// Fetches a message by its storage id (as returned by SendAsync()) optionally fetching MessageProps.
    /// Returns NULL for messages which are not found
    /// </summary>
    Task<(Message msg, MessageProps props)> GetMessageAsync(string msgId, bool fetchProps = false);

    /// <summary>
    /// Fetches a specific message attachment identified by its position in the message.Attachments collection.
    /// Returns NULL for message attachments which are not found
    /// </summary>
    Task<Message.Attachment> GetMessageAttachmentAsync(string msgId, int attId);

    /// <summary>
    /// Returns message processing status log or null if requested message was not found
    /// </summary>
    Task<MessageStatusLog> GetMessageStatusLogAsync(string msgId);
  }

}
