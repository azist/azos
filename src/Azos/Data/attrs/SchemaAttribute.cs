/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Data
{
  /// <summary>
  /// Provides information about a schema that this data document represents
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
  public sealed class SchemaAttribute : TargetedAttribute
  {
    public SchemaAttribute(string targetName = null, string name = null, bool immutable = false, string metadata = null) : base(targetName, metadata)
    {
      Name = name;
      Immutable = immutable;
    }

    private string m_Name;
    /// <summary>
    /// Returns the name of schema that decorated class represents, i.e. the name of database table i.e. "TBL_PERSON".
    /// This value is set so datastore implementation can use it instead of inferring table name from declaring class name
    /// </summary>
    public string Name { get => m_Name ?? string.Empty; set => m_Name = AssignState(value);}


    private bool m_Immutable;
    /// <summary>
    /// Indicates whether the data represented by the decorated instance can only be created (and possibly deleted) but can not change(no update).
    /// This attribute allows some backends to perform some optimizations (such as better failover data handling and caching) as any version of the data
    /// that could be found is valid and is the latest
    /// </summary>
    public bool Immutable { get => m_Immutable; set => m_Immutable = AssignState(value); }


    public override int GetHashCode() => base.GetHashCode() ^ Name.GetHashCodeSenseCase();

    public override bool Equals(object obj)
    {
        var other = obj as SchemaAttribute;
        if (other==null) return false;
        return base.Equals(other) &&
               this.Name.EqualsSenseCase(other.Name);
    }

    public override string ToString() => $"Schema(`{Name}`)";
  }


}
