/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.JSON;

namespace Azos.Sky.Messaging.Services
{
  /// <summary>
  /// Provides message header info suitable for message list display
  /// </summary>
  public sealed class MessageStatusLog : TransientModel
  {
    public enum Status
    {
      Failure = -1,
      Undefined = 0,
      Success = 1
    }

    public sealed class Line : TransientModel
    {
      [Field(Description = "UTC of the log line")]
      public DateTime Utc { get; set; }

      [Field(Description = "Channel name which reports the status")]
      public string Channel { get ; set; }

      [Field(Description = "Short line explaining the status")]
      public Status State { get; set; }

      [Field(Description = "Short line explaining the status")]
      public string Text { get; set; }

      [Field(Description = "Processor-specific content reported by channel handler")]
      public JsonDataMap ProcessorResult { get; set; }
    }

    [Field(Description = "Message Id used for message archiving which this status is describing")]
    public string ArchiveId {  get; set; }

    [Field(Description = "Message.ID")]
    public Line[] Log { get; set; }
  }
}
