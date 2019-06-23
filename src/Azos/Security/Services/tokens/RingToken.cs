using System;
using System.Linq;

using Azos.Data;

namespace Azos.Security.Services
{
  /// <summary> Common base for all tokens stored in a token ring </summary>
  public abstract class RingToken : TypedDoc
  {

    [Field(backendName: "v", isArow: true)]
    [Field(required: true, key: true, backendName: "_id", description: "Value of the token - a unique primary key")]
    public string Value { get; set; }

    /// <summary>
    /// Version UTC stamp, the system replicates data based on 1 sec resolution.
    /// By design more precise resolution is not needed for practical reasons.
    /// </summary>
    [Field(backendName: "-v", isArow: true)]
    [Field(required: true, description: "Version timestamp of this doc, used for replication")]
    public string VersionUtc { get; set; }

    /// <summary>
    /// Version Delete flag, used for CRDT idempotent flag
    /// </summary>
    [Field(backendName: "-d", isArow: true)]
    [Field(required: true, description: "Version delete flag, used for replication")]
    public bool VersionDeleted { get; set; }

    [Field(backendName: "b", isArow: true)]
    [Field(required: true, description: "Name of party issuing token")]
    public string IssuedBy { get; set; }

    [Field(backendName: "i", isArow: true)]
    [Field(required: true, description: "When was the token issued")]
    public DateTime? IssueUtc{  get; set; }

    [Field(backendName: "e", isArow: true)]
    [Field(required: true, description: "When the token gets invalid, as-if non existing")]
    public DateTime? ExpireUtc { get; set; }

    public abstract (int min,int max) TokenByteStrength { get; }
    public abstract int TokenDefaultExpirationSeconds { get; }

    protected override Exception CheckValueLength(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (fdef.Name==nameof(Value))
      {
        var btoken = Convert.FromBase64String((string)value);
        var l = btoken.Length - 16;//size of Guid
        var req = TokenByteStrength;
        if (l<req.min || l>req.max)
          return new FieldValidationException(this, nameof(Value), "Token Value byte length is {0} bytes, but must be between {1} and {2} bytes".Args(l, req.min, req.max));
      }

      return base.CheckValueLength(targetName, fdef, atr, value);
    }
  }
}
