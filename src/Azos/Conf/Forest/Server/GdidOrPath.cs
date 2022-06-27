/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;

namespace Azos.Conf.Forest.Server
{
  /// <summary>
  /// Parses tree node entity id address into either GDID or TreePath depending on schema
  /// </summary>
  public struct GdidOrPath : IEquatable<GdidOrPath>
  {
    public static GdidOrPath OfGVersion(EntityId id)
      => new GdidOrPath(id).IsTrue(v => v.Id.IsGVersion(), $"`{Constraints.SCH_GVER}` schema");

    public static GdidOrPath OfGNode(EntityId id)
      => new GdidOrPath(id).IsTrue(v => v.Id.IsGNode(), $"`{Constraints.SCH_GNODE}` schema");

    public static GdidOrPath OfPath(EntityId id)
      => new GdidOrPath(id).IsTrue(v => v.Id.IsPath(), $"`{Constraints.SCH_PATH}` schema");

    public static GdidOrPath OfGNodeOrPath(EntityId id)
      => new GdidOrPath(id).IsTrue(v => v.Id.IsGNode() || v.Id.IsPath(), $"`{Constraints.SCH_GNODE}` or `{Constraints.SCH_PATH}` schema");


    public GdidOrPath(EntityId id)
    {
      try
      {
        (id.System.IsValid && !id.System.IsZero).IsTrue("ForestId");
        (id.Type.IsValid && !id.Type.IsZero).IsTrue("TreeId");
        id.Address.NonBlank("Address");

        Id = id = new EntityId(id.System, id.Type, id.Schema, id.Address.NonBlank("address").ToLowerInvariant());//normalize the address

        if (id.IsGNode() || id.IsGVersion())
        {
          GdidAddress = GDID.Parse(id.Address);
          PathAddress = null;
        }
        else if (id.IsPath())
        {
          GdidAddress = GDID.ZERO;
          PathAddress = new TreePath(id.Address);
        }
        else
        {
          throw new ConfigException("Unsupported tree address schema");
        }
      }
      catch(Exception cause)
      {
        throw new CallGuardException(nameof(GdidOrPath),
                                     nameof(id),
                                     "Invalid config forest tree node specification: `{0}`: {1}".Args(id.AsString.TakeFirstChars(48, ".."), cause.Message), cause)
        {
          PutDetailsInHttpStatus = true
        };
      }
    }

    public readonly EntityId Id;
    public readonly GDID GdidAddress;
    public readonly TreePath PathAddress;

    public bool IsAssigned => Id.IsAssigned;
    public TreePtr Tree => new TreePtr(Id);

    public bool Equals(GdidOrPath other) => this.Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
    public override bool Equals(object obj) => obj is GdidOrPath other ? this.Equals(other) : false;

    public override string ToString() => Id.ToString();

    public static bool operator ==(GdidOrPath a, GdidOrPath b) => a.Equals(b);
    public static bool operator !=(GdidOrPath a, GdidOrPath b) => !a.Equals(b);
  }
}
