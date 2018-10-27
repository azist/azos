/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Log
{
  /// <summary>
  /// Stipulates general logging message types like: Info/Warning/Error etc...
  /// </summary>
  public enum MessageType
  {
    /// <summary>
    /// Used in debugging temp code
    /// </summary>
    Debug = 0,

    DebugA,
    DebugB,
    DebugC,
    DebugD,

    /// <summary>
    /// Emitted by DataStore implementations
    /// </summary>
    DebugSQL,

    /// <summary>
    /// Emitted by Glue/Net code
    /// </summary>
    DebugGlue,

    /// <summary>
    /// Last debug-related message type for use in debug-related max-level config setting
    /// </summary>
    DebugZ,

    /// <summary>
    /// Tracing, no danger to system operation
    /// </summary>
    Trace = 50,

    TraceA,
    TraceB,
    TraceC,
    TraceD,

    /// <summary>
    /// Emitted by DataStore implementations
    /// </summary>
    TraceSQL,

    /// <summary>
    /// Emitted by Glue/Net code
    /// </summary>
    TraceGlue,

    /// <summary>
    /// Emitted by Erlang code
    /// </summary>
    TraceErl,

    /// <summary>
    /// Last trace-related message type for use in trace-related max-level config setting
    /// </summary>
    TraceZ,

    /// <summary>
    /// Performance/Instrumentation-related message
    /// </summary>
    PerformanceInstrumentation = 90,

    /// <summary>
    /// Informational message, no danger to system operation
    /// </summary>
    Info = 100,

    InfoA,
    InfoB,
    InfoC,
    InfoD,

    /// <summary>
    /// Last info-related message type for use in info-related max-level config setting
    /// </summary>
    InfoZ,

    /// <summary>
    /// The message instance represents many others - probably aggregated with time multiple records into one
    /// </summary>
    Aggregate,

    /// <summary>
    /// Permission audit, usualy a result of client user action, no danger to system operation
    /// </summary>
    SecurityAudit = 200,

    /// <summary>
    /// SYSLOG.Notice Events that are unusual but not error conditions - might be summarized in an email to developers or admins to spot potential problems - no immediate action required.
    /// </summary>
    Notice = 300,

    /// <summary>
    /// Caution - inspect and take action.
    /// SYSLOG.Warning, not an error, but indication that an error will occur if action is not taken, e.g. file system 85% full - each item must be resolved within a given time.
    /// </summary>
    Warning = 400,

    /// <summary>
    /// Recoverable error, system will most-likely continue working normally.
    /// SYSLOG.Error Non-urgent failures, these should be relayed to developers or admins; each item must be resolved within a given time.
    /// </summary>
    Error = 500,

    /// <summary>
    /// SYSLOG.Critical Should be corrected immediately, but indicates failure in a primary system, an example is a loss of a backup ISP connection.
    /// </summary>
    Critical = 600,

    /// <summary>
    /// SYSLOG.Alert Should be corrected immediately, therefore notify staff who can fix the problem. An example would be the loss of a primary ISP connection.
    /// </summary>
    CriticalAlert = 700,

    /// <summary>
    /// Unrecoverable error, system  will most-likely experience major operation disruption.
    /// SYSLOG.Emergency - A "panic" condition usually affecting multiple apps/servers/sites. At this level it would usually notify all tech staff on call.
    /// </summary>
    Emergency = 1000,

    /// <summary>
    /// Unrecoverable error, system  will experience major operation disruption.
    /// SYSLOG.Emergency - A "panic" condition usually affecting multiple apps/servers/sites. At this level it would usually notify all tech staff on call.
    /// </summary>
    CatastrophicError = 2000
  }
}