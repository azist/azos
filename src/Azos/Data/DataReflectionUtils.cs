/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.Platform;

namespace Azos.Data
{
  /// <summary>
  /// Provides various helper methods for obtaining/using Data Doc metadata, such as
  /// FieldDescriptors used for various tasks such as serialization/formatting tasks.
  /// FieldDescriptors represent some pre-computed metadata results which are cached and improve
  /// the runtime performance
  /// </summary>
  public static class DataReflectionUtils
  {
    /// <summary>
    /// An attribute name used for FieldDescriptor deterministic ordering. Some
    /// algorithms, such as positional serialization rely on specific ordering.
    /// The attribute gets included in FieldDef's metadata property
    /// </summary>
    public const string META_FIELD_ORDER = "ord";

    /// <summary>
    /// Field Descriptor provides pre-calculated targeted data about a field in a Data document
    /// </summary>
    public struct FieldDescriptor
    {
      internal FieldDescriptor(string name, Schema.FieldDef def, FieldAttribute atr, IConfigSectionNode meta)
      {
        TargetFieldName = name;
        FieldDef = def;
        Attr = atr;
        Meta = meta;
      }

      public readonly string TargetFieldName;
      public readonly Schema.FieldDef FieldDef;
      public readonly FieldAttribute Attr;
      public readonly IConfigSectionNode Meta;

      /// <summary>
      /// Returns true if this instance represents an assigned value
      /// </summary>
      public bool Assigned => TargetFieldName.IsNotNullOrWhiteSpace();
    }

    /// <summary>
    /// Facilitates access to field descriptors set for the specified document type and target name
    /// </summary>
    public sealed class FieldDescriptors : IEnumerable<FieldDescriptor>
    {
      internal FieldDescriptors(Type t, Schema schema, string targetName, IEnumerable<FieldDescriptor> source)
      {
        m_DocType = t;
        m_Schema = schema;
        m_TargetName = targetName;
        m_Data = new Dictionary<string, FieldDescriptor>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var fd in source)
          m_Data[fd.TargetFieldName] = fd;
      }

      private readonly Type m_DocType;
      private readonly Schema m_Schema;
      private readonly string m_TargetName;
      private readonly Dictionary<string, FieldDescriptor> m_Data;

      /// <summary>
      /// A type of data document that this set is for
      /// </summary>
      private Type DocType => m_DocType;

      /// <summary>
      /// Schema for the document being described
      /// </summary>
      private Schema Schema => m_Schema;

      /// <summary>
      /// Name of data target that this set represents, that is - this set contains FieldDescriptor instances
      /// targeted for this specific target name
      /// </summary>
      private string TargetName => m_TargetName;

      /// <summary>
      /// Accesses FieldDescriptor by its targeted name; if not found returns !FieldDescriptor.Assigned
      /// </summary>
      public FieldDescriptor this[string name]
        => m_Data.TryGetValue(name.NonNull(nameof(name)), out var existing) ? existing : default(FieldDescriptor);

      /// <summary>
      /// Total count in the set
      /// </summary>
      public int Count => m_Data.Count;

      public IEnumerator<FieldDescriptor> GetEnumerator() => m_Data.Values.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => m_Data.Values.GetEnumerator();
    }


    private static ConstrainedSetLookup<Type, ConstrainedSetLookup<string, FieldDescriptors>> s_Cache =
        new ConstrainedSetLookup<Type, ConstrainedSetLookup<string, FieldDescriptors>>( t =>
          new ConstrainedSetLookup<string, FieldDescriptors>( targetName => {

            var schema = Schema.GetForTypedDoc(t);
            var fields = schema
                  .Select(fd => new FieldDescriptor(fd.GetBackendNameForTarget(targetName), fd, fd[targetName], meta: fd[targetName].Metadata))
                  .OrderBy(fd => fd.Meta != null ? fd.Meta.AttrByName(META_FIELD_ORDER).ValueAsInt() : 0);

            return new FieldDescriptors(t, schema, targetName, fields);
          })
        );

    /// <summary>
    /// Returns FieldDescriptors set for the specified docType and target name
    /// </summary>
    public static FieldDescriptors GetFieldDescriptorsFor(Type docType, string targetName)
    {
      docType.IsOfType<TypedDoc>(nameof(docType));

      if (targetName.IsNullOrWhiteSpace()) targetName = TargetedAttribute.ANY_TARGET;

      return s_Cache[docType][targetName];
    }


    private static ConstrainedSetLookup<Type, ConstrainedSetLookup<string, FieldDescriptors>> s_ExactCache =
        new ConstrainedSetLookup<Type, ConstrainedSetLookup<string, FieldDescriptors>>(t =>
         new ConstrainedSetLookup<string, FieldDescriptors>(targetName => {

           var schema = Schema.GetForTypedDoc(t);
           var fields = schema
                 .Select(fd => (fd: fd, attr: fd[targetName]))
                 .Where(item => item.attr.TargetName.EqualsIgnoreCase(targetName))
                 .Select(item => new FieldDescriptor(item.fd.GetBackendNameForTarget(targetName), item.fd, item.attr, meta: item.attr.Metadata))
                 .OrderBy(fd => fd.Meta != null ? fd.Meta.AttrByName(META_FIELD_ORDER).ValueAsInt() : 0);

           return new FieldDescriptors(t, schema, targetName, fields);
         })
        );

    /// <summary>
    /// Returns FieldDescriptors set for the specified docType and exactly for the specified target name -
    /// if a [Field] does not match the target then that field is skipped altogether
    /// </summary>
    public static FieldDescriptors GetFieldDescriptorsExactlyFor(Type docType, string targetName)
    {
      docType.IsOfType<TypedDoc>(nameof(docType));

      if (targetName.IsNullOrWhiteSpace()) targetName = TargetedAttribute.ANY_TARGET;

      return s_ExactCache[docType][targetName];
    }

  }
}
