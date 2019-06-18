using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.Security.Services
{
  /// <summary> Common base for all tokens stored in a token ring </summary>
  public abstract class RingToken : TypedDoc
  {
    protected RingToken(string issuer, int expireInSeconds)
    {
      var len = TokenByteStrength;

      var guid = Guid.NewGuid();//guid is needed to prevent possible random token key collision with existing one
      var guidpad = guid.ToNetworkByteOrder();//16 bytes

      //The KeyRing implementation ensures strong Cryptographic entropy on process-wide random by design
      var rnd = Ambient.Random.NextRandomBytes(len.min, len.max);
      var btoken = guidpad.Concat(rnd).ToArray();
      Value = Convert.ToBase64String(btoken, Base64FormattingOptions.None);

      IssuedBy = issuer.NonBlank(issuer);
      IssueUtc = Ambient.UTCNow;
      ExpireUtc = IssueUtc.Value.AddSeconds(expireInSeconds);
    }

    [Field(required: true, description: "Represent the value of the token")]
    public string Value{ get; set; }

    [Field(required: true, description: "Name of party issuing token")]
    public string IssuedBy { get; set; }

    [Field(required: true, description: "When was the token issued")]
    public DateTime? IssueUtc{  get; set; }

    [Field(required: true, description: "When the token gets invalid, as-if non existing")]
    public DateTime? ExpireUtc { get; set; }

    public abstract (int min,int max) TokenByteStrength { get; }

    protected override Exception CheckValueLength(string targetName, Schema.FieldDef fdef, FieldAttribute atr, object value)
    {
      if (fdef.Name==nameof(Value))
      {
        var btoken = Convert.FromBase64String((string)value);
        var l = btoken.Length - 16;//size of Guid
        var req = TokenByteStrength;
        if (l<req.min || l>req.max)
          return new FieldValidationException(this, nameof(Value), "Token Value byte length is {0} but must be between {1} and {2}".Args(l, req.min, req.max));
      }

      return base.CheckValueLength(targetName, fdef, atr, value);
    }
  }
}
