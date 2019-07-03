using System;
using System.Linq;

using Azos.Data;

namespace Azos.Security.Services
{
  /// <summary> Common base for all tokens stored in a token ring </summary>
  public abstract class RingToken : TypedDoc
  {
    public const string PROTECTED_MSG_TARGET = "protected-msg";


    [Field(backendName: "id", isArow: true)]
    [Field(required: true, key: true, backendName: "_id", description: "Value/ID of the token - a unique primary key. Used only for server-side tokens")]
    [Field(targetName: PROTECTED_MSG_TARGET, storeFlag: StoreFlag.None)]
    public string ID { get; set; }

    /// <summary>
    /// Version UTC stamp, the system replicates data based on 1 sec resolution.
    /// By design more precise resolution is not needed for practical reasons.
    /// </summary>
    [Field(backendName: "_v", isArow: true)]
    [Field(required: true, backendName: "_v", description: "Version timestamp of this doc, used for replication")]
    [Field(targetName: PROTECTED_MSG_TARGET, storeFlag: StoreFlag.None)]
    public string VersionUtc { get; set; }

    /// <summary>
    /// Version Delete flag, used for CRDT idempotent flag
    /// </summary>
    [Field(backendName: "_d", isArow: true)]
    [Field(required: true, backendName: "_d", description: "Version delete flag, used for replication")]
    [Field(targetName: PROTECTED_MSG_TARGET, storeFlag: StoreFlag.None)]
    public bool VersionDeleted { get; set; }

    [Field(backendName: "b", isArow: true)]
    [Field(required: true, backendName: "b", description: "Name of party issuing token")]
    [Field(targetName: PROTECTED_MSG_TARGET, storeFlag: StoreFlag.None)]
    public string IssuedBy { get; set; }

    [Field(backendName: "i", isArow: true)]
    [Field(required: true, backendName: "i", description: "When was the token issued")]
    public DateTime? IssueUtc{  get; set; }

    [Field(backendName: "e", isArow: true)]
    [Field(required: true, backendName: "e", description: "When the token becomes invalid, as-if non existing")]
    public DateTime? ExpireUtc { get; set; }

    public abstract int TokenByteStrength { get; }
    public abstract int TokenDefaultExpirationSeconds { get; }

    protected override Exception CheckValueLength(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (fdef.Name==nameof(ID))
      {
        var l = value.ToString().Length;
        if (l<TokenByteStrength)
          return new FieldValidationException(this, nameof(ID), "Token ID byte length is {0} bytes, but must be {1} bytes".Args(l, TokenByteStrength));
      }

      return base.CheckValueLength(targetName, fdef, atr, value);
    }
  }
}
