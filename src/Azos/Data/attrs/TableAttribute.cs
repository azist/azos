/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Data
{

  /// <summary>
  /// Provides information about table schema that this row is a part of
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
  public sealed class TableAttribute : TargetedAttribute
  {
      public TableAttribute(string targetName = null, string name = null, bool immutable = false, string metadata = null) : base(targetName, metadata)
      {
          Name = name ?? string.Empty;
          Immutable = immutable;
      }

      /// <summary>
      /// Returns the name of schema that decorated class represents, i.e. the name of database table i.e. "TBL_PERSON".
      /// This value is set so datastore implementation can use it instead of inferring table name from declaring class name
      /// </summary>
      public readonly string Name;

      /// <summary>
      /// Indicates whether the data represented by the decorated instance can only be created (and possibly deleted) but can not change(no update).
      /// This attribute allows some backends to perform some optimizations (such as better failover data handling and caching) as any version of the data
      /// that could be found is valid and the latest
      /// </summary>
      public readonly bool Immutable;


      public override int GetHashCode()
      {
          return TargetName.GetHashCodeSenseCase() + Name.GetHashCodeSenseCase();
      }

      public override bool Equals(object obj)
      {
          var other = obj as TableAttribute;
          if (other==null) return false;
          return this.TargetName.EqualsSenseCase(other.TargetName) && this.Name.EqualsSenseCase(other.Name) && this.MetadataContent.EqualsSenseCase(other.MetadataContent);
      }
  }


}
