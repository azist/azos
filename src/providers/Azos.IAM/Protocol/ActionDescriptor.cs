using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Describes Actor/User who is doing a change in the system
  /// </summary>
  public sealed class ActionDescriptor : TypedDoc
  {

    [Field] public GDID G_Actor { get; set; }
    [Field] public string ActorUserAgent { get; set; }
    [Field] public string ActorHost { get; set; }
    [Field] public string ActionNote { get; set; }
  }
}
