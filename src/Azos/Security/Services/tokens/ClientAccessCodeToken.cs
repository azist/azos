using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Serialization.Arow;

namespace Azos.Security.Services
{
  [Arow]
  public sealed class ClientAccessCodeToken : RingToken
  {
    public override (int min, int max) TokenByteStrength => (4, 16);

    public ClientAccessCodeToken(string issuer, int expireInSeconds) : base(issuer, expireInSeconds)
    {
    }

    [Field(description: "The original Id of the client which this access code was issued for")]
    public string ClientId{ get; set;}

  }
}
