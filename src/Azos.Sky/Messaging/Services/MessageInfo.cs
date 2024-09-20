/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Adlib;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.Sky.Messaging.Services
{
  /// <summary>
  /// Provides message header info suitable for message list display
  /// </summary>
  [Bix("a616654a-0015-48a1-900b-44033780aed1")]
  [Schema(Description = "Provides message header info suitable for message list display")]
  public sealed class MessageInfo : TransientModel
  {
    /// <summary>
    /// Id used for message archiving
    /// </summary>
    [Field(Description = "Id used for message archiving")]
    public string ArchiveId { get; set; }

    /// <summary>
    /// Message.ID uniquely identifies every message
    /// </summary>
    [Field(Description = "Message.ID")]
    public Guid ID { get; set; }

    /// <summary>
    /// Message.RelatedID - used to group messages by conversations
    /// </summary>
    [Field(Description = "Message.RelatedID - used to group messages by conversations")]
    public Guid RelatedID { get; set; }

    /// <summary>
    /// UTC time stamp of when message was sent
    /// </summary>
    [Field(Description = "UTC time stamp of when message was sent")]
    public DateTime SentUtc { get; set; }

    /// <summary>
    /// User who sent the message, may not be the same as the logical sender
    /// </summary>
    [Field(Description = "User who sent the message, may not be the same as the logical sender")]
    public string FromUser { get; set; }

    /// <summary>
    /// Message processing/delivery most recent status short line (e.g. 'Delivered')
    /// </summary>
    [Field(Description = "Message processing/delivery most recent status short line (e.g. 'Delivered')")]
    public string Status { get; set; }

    /// <summary>
    /// Importance of the message
    /// </summary>
    [Field(Description = "Importance of the message")]
    public MsgImportance Importance { get; set; }

    /// <summary>
    /// Delivery priority
    /// </summary>
    [Field(Description = "Delivery priority")]
    public MsgPriority Priority {  get; set; }

    /// <summary>Optional header content which provide extra information about this entity such as message or an attachment</summary>
    [Field(Description = "Optional header content which provide extra information about this entity such as message or an attachment")]
    public ConfigVector Headers { get; set; }

    /// <summary>
    /// Address of the logical sender
    /// </summary>
    [Field(Description = "Address of the logical sender")]
    public string[] AddressFrom {  get; set; }

    /// <summary>
    /// Address of the logical recipient
    /// </summary>
    [Field(Description = "Address of the logical recipient")]
    public string[] AddressTo { get; set; }

    /// <summary>
    /// Message subject line
    /// </summary>
    [Field(Description = "Message subject line")]
    public string[] Subject { get; set; }

    /// <summary>
    /// Message content snippet - a short string containing condensed message text to get a 'gist'
    /// </summary>
    [Field(Description = "Message content snippet - a short string containing condensed message text to get a 'gist'")]
    public string[] Snippet { get; set; }

    /// <summary>
    /// True when message has any attachments
    /// </summary>
    [Field(Description = "True when message has any attachments")]
    public bool HasAttachemnts { get; set; }

    /// <summary>
    /// Named tag collection
    /// </summary>
    [Field(Description = "Named tag collection")]
    public string[] Tags { get; set; }

    /// <summary>
    /// Optional data tags which can be used for message archive search if supported
    /// </summary>
    [Field(Description = "Optional data tags which can be used for message archive search if supported")]
    public Tag[] DataTags { get; set; }
  }
}
