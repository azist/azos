/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

using Azos.Collections;
using Azos.Platform;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  public partial class Schema
  {
    /// <summary>
    /// Provides a definition for a single field of a data document
    /// </summary>
    [Serializable]
    public sealed class FieldDef : INamed, IOrdered, ISerializable, IJsonWritable
    {

      public FieldDef(string name, Type type, FieldAttribute attr)
       => ctor(name, 0, type, new[]{attr}, null);

      public FieldDef(string name, Type type, IEnumerable<FieldAttribute> attrs)
       => ctor(name, 0, type, attrs, null);

      public FieldDef(string name, Type type, Access.QuerySource.ColumnDef columnDef)
      {
        FieldAttribute attr;
        if (columnDef!=null)
            attr =  new FieldAttribute(targetName: TargetedAttribute.ANY_TARGET,
                                      storeFlag: columnDef.StoreFlag,
                                      required: columnDef.Required,
                                      visible: columnDef.Visible,
                                      key: columnDef.Key,
                                      backendName: columnDef.BackendName,
                                      description: columnDef.Description);
        else
            attr =  new FieldAttribute(targetName: TargetedAttribute.ANY_TARGET);

        var attrs = new FieldAttribute[1] { attr };
        ctor(name, 0, type, attrs, null);
      }

      internal FieldDef(string name, int order, Type type, IEnumerable<FieldAttribute> attrs, PropertyInfo memberInfo = null)
       => ctor(name, order, type, attrs, memberInfo);

      private void ctor(string name, int order, Type type, IEnumerable<FieldAttribute> attrs, PropertyInfo memberInfo = null)
      {
        if (name.IsNullOrWhiteSpace() || type==null || attrs==null)
            throw new DataException(StringConsts.ARGUMENT_ERROR + "FieldDef.ctor(..null..)");

        m_Name = name;
        m_Order = order;
        m_Type = type;
        m_Attrs = new List<FieldAttribute>(attrs);
        m_TargetAttrsCache = new ConstrainedSetLookup<string, FieldAttribute>(findFieldAttributeFor, StringComparer.InvariantCultureIgnoreCase);

        if (m_Attrs.Count<1)
          throw new DataException(StringConsts.CRUD_FIELDDEF_ATTR_MISSING_ERROR.Args(name));

        //add ANY_TARGET attribute
        if (!m_Attrs.Any(a => a.TargetName == TargetedAttribute.ANY_TARGET))
        {
          var isAnyKey = m_Attrs.Any( a => a.Key );
          var ata = new FieldAttribute(FieldAttribute.ANY_TARGET, key: isAnyKey);
          m_Attrs.Add( ata );
        }

        m_Attrs.ForEach( a => a.Seal() );

        //Set and compiler setter
        if (memberInfo != null)
        {
          m_MemberInfo = memberInfo;
          m_MemberSet = makeSetterLambda(memberInfo);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            m_NonNullableType = type.GetGenericArguments()[0];
        else
            m_NonNullableType = type;

        m_AnyTargetKey = this[null].Key;
      }

      //20200305 DKh
      private static Action<TypedDoc, object> makeSetterLambda(PropertyInfo pi)
      {
        var self = Expression.Parameter(typeof(object), "self");
        var val  = Expression.Parameter(typeof(object), "val");
        var prop = Expression.Property(Expression.TypeAs(self, pi.DeclaringType), pi);

        Expression set;
        if (pi.PropertyType.IsValueType)
        {
          set = Expression.Condition(Expression.Equal(val, Expression.Constant(null)),
                 Expression.Assign(prop, Expression.Default(pi.PropertyType)),
                 Expression.Assign(prop, Expression.Unbox(val, pi.PropertyType)));
        }
        else
         set = Expression.Assign(prop, Expression.Convert(val, pi.PropertyType));

        return Expression.Lambda<Action<TypedDoc, object>>(set, self, val).Compile();//System.Runtime.CompilerServices.DebugInfoGenerator.CreatePdbGenerator());
      }

      private FieldDef(SerializationInfo info, StreamingContext context)
      {
        m_Name = info.GetString("nm");
        m_Order = info.GetInt32("o");
        m_Type = Type.GetType( info.GetString("t"), true);
        m_NonNullableType = Type.GetType( info.GetString("nnt"), true);
        m_Attrs = info.GetValue("attrs", typeof(List<FieldAttribute>)) as List<FieldAttribute>;
        m_AnyTargetKey = info.GetBoolean("atk");
        m_TargetAttrsCache = new ConstrainedSetLookup<string, FieldAttribute>(findFieldAttributeFor, StringComparer.InvariantCultureIgnoreCase);

        var mtp = info.GetString("mtp");
        if (mtp!=null)
        {
            var tp = Type.GetType( mtp, true);

            var mn = info.GetString("mn");
            if (mn!=null)
            {
                m_MemberInfo = tp.GetProperty(mn);
                m_MemberSet = makeSetterLambda(m_MemberInfo);
            }
        }
      }

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        info.AddValue("nm", m_Name);
        info.AddValue("o", m_Order);
        info.AddValue("t", m_Type.AssemblyQualifiedName);
        info.AddValue("nnt", m_NonNullableType.AssemblyQualifiedName);
        info.AddValue("attrs", m_Attrs);
        info.AddValue("atk", m_AnyTargetKey);

        if (m_MemberInfo==null)
        {
            info.AddValue("mtp", null);
            info.AddValue("mn", null);
        }
        else
        {
            info.AddValue("mtp", m_MemberInfo.DeclaringType.AssemblyQualifiedName);
            info.AddValue("mn", m_MemberInfo.Name);
        }
      }


      private string m_Name;
      internal int m_Order;
      private Type m_Type;
      private Type m_NonNullableType;
      private List<FieldAttribute> m_Attrs;
      private PropertyInfo m_MemberInfo;
      private Action<TypedDoc, object> m_MemberSet;
      private bool m_AnyTargetKey;

      /// <summary>
      /// Returns the name of the field
      /// </summary>
      public string                        Name  { get { return m_Name;}}

      /// <summary>
      /// Returns the field type
      /// </summary>
      public Type                          Type  { get { return m_Type;}}

      /// <summary>
      /// For nullable value types returns the field type regardless of nullability, it is the type argument of Nullable struct;
      /// For reference types returns the same type as Type property
      /// </summary>
      public Type                          NonNullableType  { get { return m_NonNullableType;}}

      /// <summary>
      /// Returns field attributes
      /// </summary>
      public IEnumerable<FieldAttribute>   Attrs { get { return m_Attrs;}}

      /// <summary>
      /// Gets absolute field order index in a data document/row of data
      /// </summary>
      public int                           Order { get {return m_Order;} }

      /// <summary>
      /// For TypedRow-descendants returns a PropertyInfo object for the underlying property
      /// </summary>
      public PropertyInfo                  MemberInfo { get {return m_MemberInfo;} }

      /// <summary>
      /// Returns true when this field is attributed as being a key field in an attribute that targets ANY_TARGET
      /// </summary>
      public bool                          AnyTargetKey { get {return m_AnyTargetKey;} }


      /// <summary>
      /// Returns true when this field is attributed as being a visible in any of the targeted attribute
      /// </summary>
      public bool AnyVisible { get { return m_Attrs.Any(a=>a.Visible);} }


      /// <summary>
      /// Returns description from field attribute or parses it from field name
      /// </summary>
      public string Description
      {
        get
        {
          var attr = this[null];
          var result = attr!=null ? attr.Description : "";

          if (result.IsNullOrWhiteSpace())
            result = Azos.Text.Utils.ParseFieldNameToDescription(Name, true);

          return result;
        }
      }


      /// <summary>
      /// Sets direct property value using pre-compiled code. This method is much faster than reflection (8-10x times).
      /// The implementation does not do any null checking for speed
      /// </summary>
      public void SetPropertyValue(TypedDoc doc, object value) => m_MemberSet(doc, value);

      /// <summary>
      /// For fields with ValueList returns value's description per specified schema
      /// </summary>
      public string ValueDescription(object fieldValue, string target = null, bool caseSensitiveKeys = false)
      {
        var sv = fieldValue.AsString();
        if (sv.IsNullOrWhiteSpace()) return string.Empty;
        var atr = this[target];
        if (atr==null) return fieldValue.AsString(string.Empty);
        var vl = atr.ParseValueList(caseSensitiveKeys);

        return vl[sv].AsString(string.Empty);
      }


      /// <summary>
      /// Returns true when at least one attribute was marked as NonUI - meaning that this field must not be serialized-to/deserialized-from client UI
      /// </summary>
      public bool  NonUI
      {
        get { return m_Attrs.Any(a=>a.NonUI);}
      }


      private Azos.Platform.ConstrainedSetLookup<string, FieldAttribute> m_TargetAttrsCache;
      private FieldAttribute findFieldAttributeFor(string target)
      {
        FieldAttribute result = null;

        if (target != FieldAttribute.ANY_TARGET)
        {
          result = m_Attrs.FirstOrDefault(a => target.EqualsIgnoreCase(a.TargetName));
        }

        if (result == null)
          result = m_Attrs.FirstOrDefault(a => TargetedAttribute.ANY_TARGET.EqualsIgnoreCase(a.TargetName));

        return result;
      }

      /// <summary>
      /// Returns a FieldAttribute that matches the supplied targetName, or if one was not defined then
      ///  returns FieldAttribute which matches any target or null
      /// </summary>
      public FieldAttribute this[string targetName]
      {
        get
        {
          if (targetName.IsNullOrWhiteSpace())
            targetName = FieldAttribute.ANY_TARGET;

          return m_TargetAttrsCache[targetName];
        }
      }

      /// <summary>
      /// Returns the name of the field in backend that was possibly overridden for a particular target
      /// </summary>
      public string GetBackendNameForTarget(string targetName)
      {
          FieldAttribute attr;
          return GetBackendNameForTarget(targetName, out attr);
      }

      /// <summary>
      /// Returns the name of the field in backend that was possibly overridden for a particular target
      /// along with store flag
      /// </summary>
      public string GetBackendNameForTarget(string targetName, out FieldAttribute attr)
      {
          var result = m_Name;
          var fattr = this[targetName];
          attr = fattr;
          if (fattr!=null)
          {
            if (fattr.BackendName.IsNotNullOrWhiteSpace()) result = fattr.BackendName;
          }
          return result;
      }

      public override string ToString()
      {
          return "FieldDef(Name: '{0}', Type: '{1}', Order: {2})".Args(m_Name, m_Type.FullName, m_Order);
      }

      public override int GetHashCode()
      {
          return m_Name.GetHashCodeOrdIgnoreCase() ^ m_Order;
      }

      public override bool Equals(object obj)
      {
          var other = obj as FieldDef;
          if (other==null) return false;
          return
              this.m_Name.EqualsOrdIgnoreCase(other.m_Name) &&
              this.m_Order==other.m_Order &&
              this.m_Type == other.m_Type &&
              this.m_Attrs.SequenceEqual(other.m_Attrs);
      }


      /// <summary>
      /// Writes fielddef as JSON. Do not call this method directly, instead call rowset.ToJSON() or use JSONWriter class
      /// </summary>
      void IJsonWritable.WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options)
      {
          var attr = this[null];

          if (attr != null && attr.NonUI)
          {
            wri.Write("{}");
            return;//nothing to write for NONUI
          }

          bool typeIsNullable;
          string tp = JSONMappings.MapCLRTypeToJSON(m_Type, out typeIsNullable);

          var map = new Dictionary<string, object>
          {
            {"Name",  m_Name},
            {"Order", m_Order},
            {"Type",  tp},
            {"Nullable", typeIsNullable}
          };

          //20190322 DKh inner schema
          if (typeof(Doc).IsAssignableFrom(this.NonNullableType))
          {
            map["IsDataDoc"] = true;
            map["IsAmorphous"] = typeof(IAmorphousData).IsAssignableFrom(this.NonNullableType);
            map["IsForm"] = typeof(Form).IsAssignableFrom(this.NonNullableType);

            if (typeof(TypedDoc).IsAssignableFrom(this.NonNullableType))
            {
              var innerSchema = Schema.GetForTypedDoc(this.NonNullableType);
              if (innerSchema.Any(fd => typeof(TypedDoc).IsAssignableFrom(fd.Type)))
                map["DataDocSchema"] = "@complex";
              else
                map["DataDocSchema"] = innerSchema;
            }
          }

          if (attr!=null)
          {
              map.Add("IsKey", attr.Key);
              map.Add("IsRequired", attr.Required);
              map.Add("Visible", attr.Visible);
              if (attr.Default!=null)map.Add("Default", attr.Default);
              if (attr.CharCase!=CharCase.AsIs) map.Add("CharCase", attr.CharCase);
              if (attr.Kind!=DataKind.Text) map.Add("Kind", attr.Kind);
              if (attr.MinLength!=0) map.Add("MinLen", attr.MinLength);
              if (attr.MaxLength!=0) map.Add("MaxLen", attr.MaxLength);
              if (attr.Min!=null) map.Add("Min", attr.Min);
              if (attr.Max!=null) map.Add("Max", attr.Max);
              if (attr.ValueList!=null) map.Add("ValueList", attr.ValueList);
              if (attr.Description!=null)map.Add("Description", attr.Description);
              //metadata content is in the internal format and not dumped
          }

          JsonWriter.WriteMap(wri, map, nestingLevel, options);
      }


    }


  }
}
