/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Provides process status information snapshot
  /// </summary>
  [Serializable]
  public struct ProcessDescriptor
  {
    public ProcessDescriptor(PID pid, string description, DateTime timestamp, string about)
    {
      PID = pid;
      Description = description;
      Timestamp = timestamp;
      About = about;
      Status = ProcessStatus.Created;
      StatusDescription = description;
      StatusTimestamp = timestamp;
      StatusAbout = about;
    }

    public ProcessDescriptor(ProcessDescriptor processDescriptor, ProcessStatus status, string description, DateTime timestamp, string about)
    {
      PID = processDescriptor.PID;
      Description = processDescriptor.Description;
      Timestamp = processDescriptor.Timestamp;
      About = processDescriptor.About;
      Status = status;
      StatusDescription = description;
      StatusTimestamp = timestamp;
      StatusAbout = about;
    }

    public ProcessDescriptor(PID pid, string description, DateTime timestamp, string about, ProcessStatus status, string statusDescription, DateTime statusTimestamp, string statusAbout)
    {
      PID = pid;
      Description = description;
      Timestamp = timestamp;
      About = about;
      Status = status;
      StatusDescription = statusDescription;
      StatusTimestamp = statusTimestamp;
      StatusAbout = statusAbout;
    }

    public readonly PID PID;
    public readonly string Description; // Caller supplied
    public readonly DateTime Timestamp;
    public readonly string About;       // [UserName]@[AppName]@[HostName]
    public readonly ProcessStatus Status;
    public readonly string StatusDescription;
    public readonly DateTime StatusTimestamp;
    public readonly string StatusAbout;

    public override string ToString() { return "{0}:{1}:{2}({3}) - {4}".Args(Status, PID, About, Timestamp, Description); }
  }
}
