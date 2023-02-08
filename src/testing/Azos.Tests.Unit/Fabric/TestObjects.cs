/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Sky.Fabric;
using Azos.Sky.Fabric.Server;

namespace Azos.Tests.Unit.Fabric
{
  public sealed class TeztParams : FiberParameters
  {
    [Field] public int Int1 { get; set; }
    [Field] public bool Bool1 { get; set; }
    [Field] public string String1 { get; set; }
  }

  [Bix("55322a1b-65cf-47eb-b73b-ba41caae4b7f")]
  public sealed class TeztResult : FiberResult
  {
    [Field] public int Int1 { get; set; }
    [Field] public string String1 { get; set; }
  }


  public sealed class TeztState : FiberState
  {
    public static readonly Atom SLOT_DEMOGRAPHICS = Atom.Encode("d");
    public static readonly Atom SLOT_ATTACHMENT = Atom.Encode("a");
    [Bix("5ea4d9a4-01e5-4cd0-a38a-0ae9588a5047")]
    internal sealed class DemographicsSlot : Slot
    {
      [Field] public string FirstName { get; set; }
      [Field] public string LastName { get; set; }
      [Field] public DateTime DOB { get; set; }
      [Field] public ulong AccountNumber { get; set; }
    }

    [Bix("91bc6ce6-dcf6-4d16-942a-4acd42588118")]
    internal sealed class AttachmentSlot : Slot
    {
      [Field] public string AttachmentName { get; set; }
      [Field] public byte[] AttachContent { get; set; }
    }

    public string FirstName
    {
      get => Get<DemographicsSlot>(SLOT_DEMOGRAPHICS).FirstName;
      set => Set<DemographicsSlot>(SLOT_DEMOGRAPHICS, s => s.FirstName = value);
    }

    public string LastName
    {
      get => Get<DemographicsSlot>(SLOT_DEMOGRAPHICS).LastName;
      set => Set<DemographicsSlot>(SLOT_DEMOGRAPHICS, s => s.LastName = value);
    }

    public DateTime DOB
    {
      get => Get<DemographicsSlot>(SLOT_DEMOGRAPHICS).DOB;
      set => Set<DemographicsSlot>(SLOT_DEMOGRAPHICS, s => s.DOB = value);
    }

    public ulong AccountNumber
    {
      get => Get<DemographicsSlot>(SLOT_DEMOGRAPHICS).AccountNumber;
      set => Set<DemographicsSlot>(SLOT_DEMOGRAPHICS, s => s.AccountNumber = value);
    }

    public string AttachmentName => Get<AttachmentSlot>(SLOT_ATTACHMENT).AttachmentName;
    public byte[] AttachmentContent => Get<AttachmentSlot>(SLOT_ATTACHMENT).AttachContent;

    public void SetAttachment(string name, byte[] content) => Set<AttachmentSlot>(SLOT_ATTACHMENT, s =>
    {
      s.AttachmentName = name;
      s.AttachContent = content;
    });
  }

  [FiberImage("c3aed76d-c313-4c1b-b89c-b3ef94d47e53")]
  public sealed class TeztFiber : Fiber<TeztParams, TeztState>
  {
    public override Task<FiberSignalResult> ApplySignalAsync(FiberSignal signal)
    {
      return base.ApplySignalAsync(signal);
    }

    public override Task<FiberStep> ExecuteSliceAsync()
    {
      return base.ExecuteSliceAsync();
    }
  }

}
