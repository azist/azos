/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data;

namespace Azos.Serialization.Bix
{
  partial class CodeGenerator
  {
    protected virtual void EmitSerialize(StringBuilder source, Schema schema, string targetName)
    {
      source.AppendLine("    public override void SerializeCore(BixWriter writer, {0} doc, BixContext ctx)".Args(schema.TypedDocType.FullNameWithExpandedGenericArgs(verbatimPrefix: true)));
      source.AppendLine("    {");
        EmitSerializeBody(source, schema, targetName);
      source.AppendLine("    }");
    }

    protected virtual void EmitSerializeBody(StringBuilder source, Schema schema, string targetName)
    {
      foreach(var def in schema)
        EmitSerializeFieldLine(source, def, targetName);
    }

    protected virtual void EmitSerializeFieldLine(StringBuilder source, Schema.FieldDef fdef, string targetName)
    {
      var fatr = fdef.Attrs.FirstOrDefault( a => a.IsArow && a.TargetName.EqualsOrdIgnoreCase(targetName));
      if (fatr==null) return;//do not serialize this field as it is not a part of this target
      if (fatr.StoreFlag!=StoreFlag.OnlyStore && fatr.StoreFlag!=StoreFlag.LoadAndStore) return;//this field should not be stored(written)
      var name = GetName(fatr.BackendName);
      if (name==0) return;//no name specified

      var prop = fdef.MemberInfo.Name;
      var isNullable = fdef.Type != fdef.NonNullableType;

      source.AppendLine("      // '{0}' = {1}".Args(fatr.BackendName, name));

      //if direct write is supported
      if (Writer.IsWriteSupported(fdef.Type))
      {
        source.AppendLine("      BWR.WriteField(writer, {0}, doc.{1});".Args(name, prop));
        return;
      }

      //has an interface<T> which supports direct write(intf<T>)
      foreach (var ti in fdef.Type.GetInterfaces().Where(ti => ti.GetGenericTypeDefinition() == typeof(ICollection<>)))
      {
        if (Writer.IsWriteSupported(ti))
        {
          source.AppendLine("      BWR.WriteField(writer, {0}, doc.{1});".Args(name, prop));
          return;
        }
      }

      //is Enum type - encoded as long or long?
      if (fdef.NonNullableType.IsEnum)
      {
        if (isNullable)
          source.AppendLine("      BWR.WriteField(writer, {0}, (long?)doc.{1});".Args(name, prop));
        else
          source.AppendLine("      BWR.WriteField(writer, {0}, (long)doc.{1});".Args(name, prop));
        return;
      }

      //is data document
      if (typeof(TypedDoc).IsAssignableFrom(fdef.Type))
      {
        source.AppendLine("      BWR.WriteDocField(writer, {0}, doc.{1}, ctx);".Args(name, prop));
        return;
      }

      //is IEnumerable of TypedDoc and use C# co-variance feature passing IEnumerable<MyDoc> into IEnumerable<Doc>
      if (fdef.Type
              .GetInterfaces()
              .Any(ti => ti.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                          typeof(TypedDoc).IsAssignableFrom(ti.GetGenericArguments()[0])
                  ))
      {
        source.AppendLine("      BWR.WriteDocSequenceField(writer, {0}, doc.{1}, ctx);".Args(name, prop));
        return;
      }

      //worst case - write polymorphically anything (slowest)
      source.AppendLine("      BWR.WriteAnyField(writer, {0}, doc.{1}, ctx);".Args(name, prop));
    }

  }
}
