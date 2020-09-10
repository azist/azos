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
    /// An attribute name used for FieldDescriptor deterministic ordering. Some
    /// algorithms, such as positional serialization rely on specific ordering.
    /// The attribute gets included in FieldDef's metadata property
    /// </summary>
    public const string META_FIELD_ORDER_ALT = "order";

    /// <summary>
    /// Field Descriptor provides pre-calculated targeted data about a field in a Data document
    /// </summary>
    public struct FieldDescriptor
    {
      internal FieldDescriptor(string name, Schema.FieldDef def, FieldAttribute atr)
      {
        TargetFieldName = name;
        FieldDef = def;
        Attr = atr;
      }

      /// <summary>
      /// The name of the field for the target - taken from BackendName
      /// </summary>
      public readonly string TargetFieldName;

      /// <summary>
      /// Original schema field definition object
      /// </summary>
      public readonly Schema.FieldDef FieldDef;

      /// <summary>
      /// Attribute that matches the target
      /// </summary>
      public readonly FieldAttribute Attr;

      /// <summary>
      /// Metadata associated with the field or null
      /// </summary>
      public IConfigSectionNode Meta => Attr?.Metadata;

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
        m_DataList = new List<FieldDescriptor>();

        foreach (var fd in source)
        {
          m_Data[fd.TargetFieldName] = fd;
          m_DataList.Add(fd);
        }
      }

      private readonly Type m_DocType;
      private readonly Schema m_Schema;
      private readonly string m_TargetName;
      private readonly Dictionary<string, FieldDescriptor> m_Data;
      private readonly List<FieldDescriptor> m_DataList;

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
      /// Accesses FieldDescriptor by its sequence/index as defined by `ord` metadata attribute
      /// </summary>
      public FieldDescriptor this[int i]
        => (i >=0 && i < m_DataList.Count) ? m_DataList[i] : new FieldDescriptor();

      /// <summary>
      /// Total count in the set
      /// </summary>
      public int Count => m_Data.Count;

      public IEnumerator<FieldDescriptor> GetEnumerator() => m_Data.Values.GetEnumerator();
      IEnumerator IEnumerable.GetEnumerator() => m_Data.Values.GetEnumerator();
    }


    private static FiniteSetLookup<Type, FiniteSetLookup<string, FieldDescriptors>> s_Cache =
        new FiniteSetLookup<Type, FiniteSetLookup<string, FieldDescriptors>>( t =>
          new FiniteSetLookup<string, FieldDescriptors>( targetName => {

            var schema = Schema.GetForTypedDoc(t);
            var fields = schema
                  .Select(fd => (fd: fd, attr: fd[targetName]))
                  .Where(item => item.attr!=null)
                  .Select(item => new FieldDescriptor(item.fd.GetBackendNameForTarget(targetName), item.fd, item.attr))
                  .OrderBy(fd => fd.Meta != null ? fd.Meta.Of(META_FIELD_ORDER, META_FIELD_ORDER_ALT).ValueAsInt() : fd.FieldDef.Order);

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


    private static FiniteSetLookup<Type, FiniteSetLookup<string, FieldDescriptors>> s_ExactCache =
        new FiniteSetLookup<Type, FiniteSetLookup<string, FieldDescriptors>>(t =>
         new FiniteSetLookup<string, FieldDescriptors>(targetName => {

           var schema = Schema.GetForTypedDoc(t);
           var fields = schema
                 .Select(fd => (fd: fd, attr: fd[targetName]))
                 .Where(item => item.attr != null && item.attr.TargetName.EqualsIgnoreCase(targetName))
                 .Select(item => new FieldDescriptor(item.fd.GetBackendNameForTarget(targetName), item.fd, item.attr))
                 .OrderBy(fd => fd.Meta != null ? fd.Meta.Of(META_FIELD_ORDER, META_FIELD_ORDER_ALT).ValueAsInt() : fd.FieldDef.Order);

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
