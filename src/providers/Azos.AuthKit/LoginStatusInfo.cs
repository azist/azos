/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.AuthKit
{
  /// <summary>
  /// LoginStatus keeps the last known snapshot of log-in volatile information to lessen the replication load Login:LoginStatus = 1:1 (same GDID)
  /// </summary>
  [Bix("BB679B3B-FCE5-408D-9AAE-D5BBB37E07F6")]
  [Schema(Description = "LoginStatus keeps the last known snapshot of log-in volatile information to lessen the replication load Login:LoginStatus = 1:1 (same GDID)")]
  public sealed class LoginStatusInfo : TransientModel
  {
    /// <summary>
    /// Login UK, status uses the same GDID as Login entity
    /// </summary>
    [Field(required: true, Description = "Login UK, status uses the same GDID as Login entity")]
    public GDID LoginId { get; set; }

    /// <summary>
    /// When pwd was set last time
    /// </summary>
    [Field(Description = "When pwd was set last time")]
    public DateTime? PasswordSetUtc { get; set; }

    /// <summary>
    /// When account was confirmed for the last time
    /// </summary>
    [Field(Description = "When account was confirmed for the last time")]
    public DateTime? ConfirmedUtc { get; set; }

    /// <summary>
    /// Optional properties such as confirmation attributes, etc.
    /// </summary>
    [Field(Description = "Optional properties such as confirmation attributes, etc.")]
    public ConfigVector Props { get; set; }

    /// <summary>
    /// Last correct login timestamp or null
    /// </summary>
    [Field(Description = "Last correct login timestamp or null")]
    public DateTime? OkUtc { get; set; }

    /// <summary>
    /// Last correct login address or null
    /// </summary>
    [Field(Description = "Last correct login address or null")]
    public string OkAddress { get; set; }

    /// <summary>
    /// Last correct login user agent or null
    /// </summary>
    [Field(Description = "Last correct login user agent or null")]
    public EntityId OkAgent { get; set; }

    /// <summary>
    /// Last bad login cause atom
    /// </summary>
    [Field(Description = "Last bad login cause atom")]
    public Atom BadCause { get; set; }

    /// <summary>
    /// Last correct login timestamp or null
    /// </summary>
    [Field(Description = "Last correct login timestamp or null")]
    public DateTime? BadUtc { get; set; }

    /// <summary>
    /// Last bad login address or null
    /// </summary>
    [Field(Description = "Last bad login address or null")]
    public string BadAddress { get; set; }

    /// <summary>
    /// Last bad login user agent or null
    /// </summary>
    [Field(Description = "Last bad login user agent or null")]
    public EntityId BadAgent { get; set; }

    /// <summary>
    /// Consecutive incorrect login count
    /// </summary>
    [Field(Description = "Consecutive incorrect login count")]
    public int BadCount { get; set; }
  }
}
