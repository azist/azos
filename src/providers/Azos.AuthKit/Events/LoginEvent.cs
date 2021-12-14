/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Sky.EventHub;

namespace Azos.AuthKit.Events
{
  /// <summary>
  /// Triggered by login activity such as an attempt to perform user log-on, bad login attempt,
  /// password reset etc...
  /// </summary>
  [Bix("ada5df48-c892-4043-9d7c-3990fed8b548")]
  [Event(Constraints.EVT_NS_AUTHKIT, Constraints.EVT_QUEUE_LOGIN, DataLossMode.Default)]
  public sealed class LoginEvent : EventDocument
  {
    /// <summary>
    /// The password was successfully set
    /// </summary>
    public const string TP_OK_SET_PASSWORD = "ok-set-pwd";

    /// <summary>
    /// The logon succeeded
    /// </summary>
    public const string TP_OK_LOGON = "ok-logon";


    /// <summary>
    /// Bad password. Most likely reaction: bump the invalid count until account gets locked
    /// </summary>
    public const string TP_FAIL_PASSWORD = "fail-pwd";

    /// <summary>
    /// Account is locked and login is not possible at this time
    /// </summary>
    public const string TP_FAIL_LOCKED = "fail-locked";

    /// <summary>
    /// Login could have succeeded but the moment of login is outside of  start/end date validity spans
    /// </summary>
    public const string TP_FAIL_DATES  = "fail-dates";


    public override ShardKey GetEventPartition() => G_User.IsZero ? new ShardKey(CallFlowId) : new ShardKey(G_User);

    [Field(required: false, Description = "User account GDID PK, if it is known")]
    public GDID G_User { get; set; }

    [Field(required: false, Description = "Login GDID PK, if it is known")]
    public GDID G_Login { get; set; }

    [Field(required: true, Description = "Describes what happened, see TP_* constants")]
    public string EventType { get; set; }

    [Field(required: true, Description = "Gets a unique CallFlow identifier, you can use it for things like log correlation id")]
    public Guid CallFlowId { get; set; }

    [Field(required: true, Description = "UTC timestamp when event happened")]
    public DateTime Utc { get; set; }

    [Field(required: true, Description = "What origin (data center) has the change originated from")]
    public Atom Origin { get; set; }

    [Field(required: true, Description = "Host where event was generated")]
    public string Host { get; set; }

    [Field(required: true, Description = "Application id which generated the event")]
    public Atom App { get; set; }

    [Field(required: true, Description = "Address of the calling party (e.g. client IP)")]
    public string CallerAddress { get; set; }

    [Field(required: true, Description = "User agent (e.g. from browser) of the calling party")]
    public string CallerAgent { get; set; }

    [Field(required: true, Description = "Describes the port/entry point through which the caller made the call," +
                                         " e.g. this may be set to a web Uri that initiated the call")]
    public string CallerPort { get; set; }

    [Field(required: false, Description = "Call flow graph, if available, which generated the event")]
    public JsonDataMap CallFlow { get; set; }
  }
}
