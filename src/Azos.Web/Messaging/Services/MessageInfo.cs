/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Business;

namespace Azos.Web.Messaging.Services
{
  /// <summary>
  /// Provides message header info suitable for message list display
  /// </summary>
  public sealed class MessageInfo : TransientModel
  {
    [Field(Description = "Id used for message archiving")]
    public string ArchiveId {  get; set; }

    [Field(Description = "Message.ID")]
    public Guid ID { get; set; }

    [Field(Description = "Message.RelatedID - used to group messages by conversations")]
    public Guid RelatedID { get; set; }

    [Field(Description = "UTC time stamp of when message was sent")]
    public DateTime SentUtc { get; set; }

    [Field(Description = "User who sent the message, may not be the same as the logical sender")]
    public string FromUser { get; set; }

    [Field(Description = "Message processing/delivery most recent status short line (e.g. 'Delivered')")]
    public string Status { get; set; }

    [Field(Description = "Importance of the message")]
    public MsgImportance Importance { get; set; }

    [Field(Description = "Delivery priority")]
    public MsgPriority Priority {  get; set; }

    [Field(Description = "Address of the logical sender")]
    public string[] AddressFrom {  get; set; }

    [Field(Description = "Address of the logical recipient")]
    public string[] AddressTo { get; set; }

    [Field(Description = "Message subject line")]
    public string[] Subject { get; set; }

    [Field(Description = "Message content snippet - a short string containing condensed message text to get a 'gist'")]
    public string[] Snippet { get; set; }

    [Field(Description = "True when message has any attachments")]
    public bool HasAttachemnts { get; set; }

    [Field(Description = "Named tag collection")]
    public string[] Tags { get; set; }
  }
}
