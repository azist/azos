/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Data
{
  /// <summary>
  /// Represents a tuple of Type with string TargetName which is needed in many cases working with
  /// providers targeted by name
  /// </summary>
  public struct TargetedType : IEquatable<TargetedType>, IRequiredCheck
  {
    public TargetedType(string target, Type type)
    {
      Type = type.NonNull(nameof(type));
      TargetName = target.IsNullOrWhiteSpace() ? FieldAttribute.ANY_TARGET : target;
    }

    public readonly Type Type;
    public readonly string TargetName;

    /// <summary> True if this struct is assigned - does not represent a default instance </summary>
    public bool IsAssigned => Type != null;

    public bool CheckRequired(string targetName) => IsAssigned;

    public bool Equals(TargetedType other)
     => Type == other.Type && TargetName.EqualsOrdSenseCase(other.TargetName);

    public override int GetHashCode() => Type.GetHashCode();

    public override bool Equals(object obj) => obj is TargetedType ttp && Equals(ttp);

    public override string ToString() => $"{TargetName}::{Type?.FullName}";

    public static bool operator ==(TargetedType left, TargetedType right) =>  left.Equals(right);

    public static bool operator !=(TargetedType left, TargetedType right) => !left.Equals(right);
  }
}
