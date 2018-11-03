using System;

using Azos.Web.Messaging;
using Azos.Serialization.Arow;
using Azos.Data;

namespace Azos.Sky.WebMessaging
{
  /// <summary>
  /// Represents messages stored in Azos Sky system
  /// </summary>
  [Serializable, Arow]
  public class SkyWebMessage : Message
  {
    protected SkyWebMessage() { }
    public SkyWebMessage(GDID? gdid = null, Guid? id = null, DateTime? utcCreateDate = null) : base(id, utcCreateDate)
    {
      GDID = gdid ?? SkySystem.GdidProvider.GenerateOneGdid(SysConsts.GDID_NS_MESSAGING, SysConsts.GDID_NAME_MESSAGING_MESSAGE);
    }

    /// <summary>
    /// Represents a unique ID assigned to the message in the distributed system
    /// </summary>
    [Field(backendName: "gdid", isArow: true)]
    public GDID GDID { get; private set; }

    /// <summary>
    /// Represents a status that the message is in
    /// </summary>
    [Field(backendName: "stat", isArow: true)]
    public MsgStatus Status { get; set; }

    /// <summary>
    /// Represents a public status that the message is in
    /// </summary>
    [Field(backendName: "pstat", isArow: true)]
    public MsgPubStatus PubStatus { get; set; }

    /// <summary>
    /// Provides a public status timestamp
    /// </summary>
    [Field(backendName: "pstts", isArow: true)]
    public DateTime? PubStatusTimestamp { get; set; }

    /// <summary>
    /// Provides a public status operator - who changed the status
    /// </summary>
    [Field(backendName: "pstop", isArow: true)]
    public string PubStatusOperator { get; set; }

    /// <summary>
    /// Provides a public status description
    /// </summary>
    [Field(backendName: "pstd", isArow: true)]
    public string  PubStatusDescription { get; set; }

    /// <summary>
    /// Provides a comma-separated list of "folders" that the message is in.
    /// When a UI agent gets the msg feed, it puts the messages in corresponding folders per user
    /// or if such folder does not exist, puts it in "Other" folder
    /// </summary>
    [Field(backendName: "fld", isArow: true)]
    public string Folders { get; set; }

    /// <summary>
    /// Provides a comma-separated list of "adornments" that the message is embellished with.
    /// Example: "star,heart,triangle"
    /// </summary>
    [Field(backendName: "adrn", isArow: true)]
    public string Adornments { get; set; }
  }

}
