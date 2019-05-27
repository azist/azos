/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Represents a globally-unique process identifier. The pids are allocated of SkySystem.ProcessManager
  /// </summary>
  [Serializable]
  public struct PID : IEquatable<PID>
  {
    public static readonly PID Zero = new PID(null, -1, null, false);

    public PID(string zone, int processorID, string id, bool isUnique) { Zone = zone; ProcessorID = processorID; ID = id; IsUnique = isUnique; }

    public PID(JsonDataMap dataMap)
    {
      Zone        = dataMap[nameof(Zone)]       .AsString();
      ProcessorID = dataMap[nameof(ProcessorID)].AsInt();
      ID          = dataMap[nameof(ID)]         .AsString();
      IsUnique    = dataMap[nameof(IsUnique)]   .AsBool();
    }

    public readonly string Zone;
    public readonly int ProcessorID;
    public readonly string ID;
    public readonly bool IsUnique;

    public override string ToString() { return "PID[{0}:{1}:{2}]{3}".Args(Zone, ProcessorID, ID, IsUnique ? string.Empty : "*"); }
    public override int GetHashCode() { return ID.GetHashCode(); }

    public override bool Equals(object obj)
    {
      if (!(obj is PID)) return false;
      return this.Equals((PID)obj);
    }

    public bool Equals(PID other)
    {
      return this.Zone.EqualsOrdIgnoreCase(other.Zone) &&
             this.ProcessorID == other.ProcessorID &&
             this.ID.EqualsOrdIgnoreCase(other.ID) &&
             this.IsUnique == other.IsUnique;
    }

    public static PID Parse(string str)
    {
      PID parsed;
      if (!TryParse(str, out parsed))
        throw new WorkersException(StringConsts.PID_PARSE_ERROR.Args(str));
      return parsed;
    }

    public static bool TryParse(string str, out PID? pid)
    {
      PID parsed;
      if (TryParse(str, out parsed))
      {
        pid = parsed;
        return true;
      }

      pid = null;
      return false;
    }

    public static bool TryParse(string str, out PID pid)
    {
      pid = Zero;
      var afterZone = str.IndexOf(':');
      var zone = str.Substring(4, afterZone - 4);
      var afterProcessorID = str.IndexOf(':', ++afterZone);
      int processorID;
      if (!int.TryParse(str.Substring(afterZone, afterProcessorID - afterZone), out processorID)) return false;
      var afterID = str.IndexOf(']', ++afterProcessorID);
      var id = str.Substring(afterProcessorID, afterID - afterProcessorID);
      afterID++;
      var isUnique = afterID == str.Length || str[afterID] != '*';
      pid = new PID(zone, processorID, id, isUnique);
      return true;
    }
  }
}
