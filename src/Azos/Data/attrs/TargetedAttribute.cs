/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Platform;

namespace Azos.Data
{
  /// <summary>
  /// Provides a base for attributes which are targeted for particular technology (e.g. "ORACLE", "RIAK" etc.)
  /// </summary>
  [Serializable]
  public abstract class TargetedAttribute : Attribute
  {
    public const string ANY_TARGET = "*";

    protected TargetedAttribute(string targetName)
    {
      m_TargetName = targetName.IsNullOrWhiteSpace() ? ANY_TARGET : targetName;
      m_PropAssignmentTracking = true;
    }

    protected TargetedAttribute(string targetName, string metadata)
    {
      m_TargetName = targetName.IsNullOrWhiteSpace() ? ANY_TARGET : targetName;
      m_MetadataContent = metadata;
      m_PropAssignmentTracking = true;
    }

    private bool m_Sealed;
    private bool m_PropAssignmentTracking;
    private HashSet<string> m_AssignedPropNames;

    /// <summary>
    /// Returns true when attribute is sealed and can not change
    /// </summary>
    public bool Sealed => m_Sealed;

    /// <summary>
    /// Returns stream of properties which have been set before this instance was sealed
    /// </summary>
    public IEnumerable<string> AssignedPropNames => m_AssignedPropNames!=null ? m_AssignedPropNames : Enumerable.Empty<string>();

    /// <summary>
    /// Returns true if the specified named property was assigned
    /// </summary>
    public bool PropertyWasAssigned(string propName)
    {
      if (m_AssignedPropNames==null || propName.IsNullOrWhiteSpace()) return false;
      return m_AssignedPropNames.Contains(propName);
    }

    /// <summary>
    /// Called by derivative implementations to seal the instance, so it can not change
    /// </summary>
    protected internal void Seal() => m_Sealed = true;

    protected void CheckNotSealed()
    {
      if (Sealed) throw new DataException("{0} is sealed and can not be altered".Args(GetType().Name));
    }

    internal void StopPropAssignmentTracking() => m_PropAssignmentTracking = false;

    protected T AssignState<T>(T value, [System.Runtime.CompilerServices.CallerMemberName]string propName = null)
    {
      CheckNotSealed();

      if (m_PropAssignmentTracking)
      {
        if (m_AssignedPropNames==null) m_AssignedPropNames = new HashSet<string>();
        m_AssignedPropNames.Add(propName);
      }
      return value;
    }


    private string m_TargetName;
    /// <summary>
    /// Returns the name of target, i.e. the name of database engine i.e. "ORACLE11g" or "MySQL"
    /// </summary>
    public string TargetName
    {
      get => m_TargetName;
      set
      {
        var v = AssignState(value);
        m_TargetName = v.IsNullOrWhiteSpace() ? ANY_TARGET : v;
      }
    }

    protected string m_Description;
    /// <summary>
    /// Provides description
    /// </summary>
    public string Description
    {
      get => m_Description;
      set => m_Description = AssignState(value);
    }


    protected string m_MetadataContent;

    [NonSerialized]
    private IConfigSectionNode m_Metadata;

    /// <summary>
    /// Returns metadata content string in Laconic format or null. Root not is not specified. I.e.: 'a=1 b=true c{...}'
    /// </summary>
    public string MetadataContent
    {
      get => m_MetadataContent;
      set
      {
        m_MetadataContent = AssignState(value);
        m_Metadata = null;//delete cached version
      }
    }

    /// <summary>
    /// Returns structured metadata or null if there is no metadata defined
    /// </summary>
    public IConfigSectionNode Metadata
    {
      get
      {
        if (MetadataContent.IsNullOrWhiteSpace()) return null;
        if (m_Metadata==null)//not thread safe but its ok, in the worst case 2nd copy will be made
          m_Metadata = ParseMetadataContent(m_MetadataContent);

        return m_Metadata;
      }
    }


    public override int GetHashCode() => TargetName.GetHashCodeSenseCase();

    public override bool Equals(object obj)
    {
      var other = obj as TargetedAttribute;
      if (other == null) return false;
      return this.TargetName.EqualsSenseCase(other.TargetName) &&
             this.MetadataContent.EqualsSenseCase(other.MetadataContent) &&
             this.Description.EqualsSenseCase(other.Description);
    }

    public override string ToString() => GetType().Name;

    /// <summary>
    /// Parses content with or without root node
    /// </summary>
    public static ConfigSectionNode ParseMetadataContent(string content)
    {
      try
      {
        content = content ?? string.Empty;
        var root = ("meta{"+content+"}").AsLaconicConfig(handling: ConvertErrorHandling.Throw);

        //Unwrap extra "meta" root node like:  meta{ meta{ a=1 } } -> meta{ a=1 }
        //if someone wrote metadata with `meta` wrap
        if (!root.HasAttributes && root.ChildCount==1)
        {
          var subroot = root["meta"];
          if (subroot.Exists) return subroot;
        }

        return root;
      }
      catch(Exception error)
      {
        throw new DataException(StringConsts.CRUD_METADATA_PARSE_ERROR.Args(error.ToMessageWithType(), content.TakeFirstChars(48)), error);
      }
    }

    /// <summary>
    /// Interprets various refs to resources in various fields (e.g. Description), for example
    /// a description may contain a "./" shortcut which will direct the system to read the content from embedded Laconic resource file.
    /// This is needed not to pollute C# code with long string content
    /// </summary>
    protected internal virtual void ExpandResourceReferencesRelativeTo(Type tDoc, string entity)
    {
      if (tDoc == null) return;

      this.Description = ExpandOneResourceReferences(tDoc, entity, "description", this.Description);
    }

    protected string ExpandOneResourceReferences(Type tDoc, string entity, string name, string value)
    {
      const string PFX = "./";
      if (value.IsNullOrWhiteSpace()) return value;
      if (!value.StartsWith(PFX)) return value;
      value = value.Substring(PFX.Length);

      var res = value.IsNullOrWhiteSpace() ? "schemas.laconf" : "{0}.laconf".Args(value);

      var resContent = tDoc.NonNull(nameof(tDoc))
                            .GetText(res)
                            .NonBlank("Resource `{0}` referenced by {1}.{2}.{3}".Args(res, tDoc.Name, entity, name));

      try
      {
        var cfg = resContent.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        var path = entity.IsNotNullOrWhiteSpace() ? "!/{0}/{1}/${2}".Args(tDoc.Name, entity, name) : "!/{0}/${1}".Args(tDoc.Name, name);
        return cfg.Navigate(path).Value;
      }
      catch (Exception error)
      {
        throw new DataException("Error expanding resource reference {0}.{1}.{2} resource: {3}".Args(tDoc.Name, entity, name, error.ToMessageWithType()), error);
      }
    }

  }
}
