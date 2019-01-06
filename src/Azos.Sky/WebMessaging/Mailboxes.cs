/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.WebMessaging
{

  /// <summary>
  /// Represents a read-only tuple of { Channel: string, gShard: GDID, gMailbox: GDID}.
  /// The gMailbox is a globally-unique ID however some large systems need to prepend it with
  /// gShard which allows for instant location of a concrete data store which holds gMailbox.
  /// This design allows to use the same infrastructure for product reviews, in which case, the MailboxID
  /// may represent a particular product (out of millions) that receives customer review messages
  /// </summary>
  [Serializable]
  public struct MailboxID : IEquatable<MailboxID>
  {
    public MailboxID(string channel, GDID gShard, GDID gMbox) { Channel = channel; G_Shard = gShard; G_Mailbox = gMbox; }

    /// <summary>
    /// System-dependent Channel ID where the mailbox is stored, for example: USER, COMPANY, VENDOR etc...
    /// </summary>
    public readonly string Channel;

    /// <summary>
    /// Sharding GDID used to instantly find the data store shard where data is kept
    /// </summary>
    public readonly GDID G_Shard;

    /// <summary>
    ///The global unique id of a mailbox, e.g. user GDID, company GDID, product "mailbox" = G_PRODUCT
    /// </summary>
    public readonly GDID G_Mailbox;

    public bool Equals(MailboxID other)
    {
      return string.Equals(this.Channel, other.Channel, StringComparison.Ordinal) &&
                           this.G_Shard == other.G_Shard &&
                           this.G_Mailbox == other.G_Mailbox;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is MailboxID)) return false;
      return this.Equals((MailboxID)obj);
    }

    public override int GetHashCode()
    {
      return Channel.GetHashCode() ^ G_Shard.GetHashCode() ^ G_Mailbox.GetHashCode();
    }

    public override string ToString()
    {
      return "`{0}` -> Mbx[{1}@{2}]".Args(Channel, G_Mailbox, G_Shard);
    }

    public static bool operator ==(MailboxID left, MailboxID right) =>  left.Equals(right);
    public static bool operator !=(MailboxID left, MailboxID right) => !left.Equals(right);
  }


  /// <summary>
  /// Represents a read-only tuple of { mailboxID: MailboxID, gMessage: GDID}
  /// </summary>
  [Serializable]
  public struct MailboxMsgID : IEquatable<MailboxMsgID>
  {
    public MailboxMsgID(MailboxID xid, GDID gMsg)
    {
      MailboxID = xid;
      G_Message = gMsg;
    }

    public readonly MailboxID MailboxID;
    public readonly GDID      G_Message;

    public bool Equals(MailboxMsgID other)
    {
      return this.MailboxID.Equals(other.MailboxID) && this.G_Message.Equals(other.G_Message);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is MailboxMsgID)) return false;
      return this.Equals((MailboxMsgID)obj);
    }

    public override int GetHashCode()
    {
      return MailboxID.GetHashCode() ^ G_Message.GetHashCode();
    }

    public override string ToString()
    {
      return "MbxMsg({0} in {1})".Args(G_Message, MailboxID);
    }

    public static bool operator ==(MailboxMsgID left, MailboxMsgID right) => left.Equals(right);
    public static bool operator !=(MailboxMsgID left, MailboxMsgID right) => !left.Equals(right);
  }

  /// <summary>
  /// Provides general information about the mailbox instance fetched by MailboxID.
  /// Systems derive form this class to return more details as appropriate
  /// </summary>
  [Serializable]
  public class MailboxInfo
  {
    /// <summary>
    /// The global unique ID of this mailbox
    /// </summary>
    public MailboxID ID { get; set; }

    /// <summary>
    /// Primary address of this mailbox, e.g. email address
    /// </summary>
    public string    PrimaryAddress  { get; set; }

    /// <summary>
    /// Localized name and description of the mailbox
    /// </summary>
    public NLSMap    Name            { get; set; }

    /// <summary>
    /// When was the mailbox established
    /// </summary>
    public DateTime? CreateDate      { get; set; }

    /// <summary>
    /// Optional URL of image representing the mailbox (i.e. user image, group image etc.)
    /// </summary>
    public string    ProfileImageURL { get; set; }


    /// <summary>
    /// Override to translate system byte types into textual representation per supplied language
    /// </summary>
    public virtual string GetMailboxChannelDisplayName(string isoLang)
    {
      return ID.Channel;
    }
  }

}
