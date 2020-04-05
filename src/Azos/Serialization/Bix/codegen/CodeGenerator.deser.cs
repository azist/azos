/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using Azos.Conf;
using Azos.Data;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Generates code for serializer and deserializer
  /// </summary>
  partial class CodeGenerator
  {

    protected virtual void EmitDeserialize(StringBuilder source, Schema schema, string targetName)
    {
      source.AppendLine("    void ITypeSerializationCore.Deserialize(TypedDoc aDoc, ReadingStreamer streamer)");
      source.AppendLine("    {");
      source.AppendLine("      var doc = ({0})aDoc;".Args(schema.TypedDocType.FullName));
         EmitDeserializeBody(source, schema, targetName);
      source.AppendLine("    }");
    }

    protected virtual void EmitDeserializeBody(StringBuilder source, Schema schema, string targetName)
    {
      source.AppendLine("      while(true)");
      source.AppendLine("      {");

      source.AppendLine("         var name = Reader.ReadName(streamer);");
      source.AppendLine("         if (name==0) break;//EODoc");
      source.AppendLine("         var dt = Reader.ReadDataType(streamer);");
      source.AppendLine("         DataType? atp = null;");
      source.AppendLine("         switch(name)");
      source.AppendLine("         {");
      foreach(var def in schema)
      {
        var fatr = def.Attrs.FirstOrDefault( a => a.IsArow);
        if (fatr==null) continue;
        var name = GetName(fatr.BackendName);
        if (name==0) continue;//no name specified

        source.AppendLine("           case {0}: {{ // '{1}'".Args(name, fatr.BackendName));

        EmitDeserializeField(source, def, targetName);

        source.AppendLine("                     }");//case label
      }

      //source.AppendLine("             default: break;");
      source.AppendLine("         }");
      source.AppendLine("         Reader.ConsumeUnmatched(doc, streamer, CodeGenerator.GetName(name), dt, atp);");
      source.AppendLine("      }");

    }

    protected virtual void EmitDeserializeField(StringBuilder source, Schema.FieldDef fdef, string targetName)
    {
      var t = fdef.Type;
      var isNullable = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
      var isValueType = t.IsValueType;
      var isArray = t.IsArray;
      var isList = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
      var prop = fdef.MemberInfo.Name;

      var tcName = t.FullNameWithExpandedGenericArgs().Replace('+','.');

      //Specialized handling via dict
      string code = null;
  //    if (!Reader.DESER_TYPE_MAP.TryGetValue(t, out code)) //we go by the model that we have now, if what was ser does not match put it in amorphous
      {
        if (isNullable || !isValueType)
          source.AppendLine("           if (dt==DataType.Null) {{ doc.{0} = null; continue;}} ".Args(prop));

        if (t.IsEnum)
        {
          source.AppendLine("           if (dt!=DataType.Int32) break;");
          source.AppendLine("           var ev = ({0})Reader.ReadInt32(streamer);".Args(tcName));
          source.AppendLine("           doc.{0} = ev;".Args(prop));
          source.AppendLine("           continue;");
          return;
        } else if (typeof(TypedDoc).IsAssignableFrom(t))
        {
          source.AppendLine("           if (dt!=DataType.Doc) break;");
          source.AppendLine("           var vdoc = new {0}();".Args(t.FullName));
          source.AppendLine("           if (Reader.TryReadRow(doc, vdoc, streamer, CodeGenerator.GetName(name)))");
          source.AppendLine("             doc.{0} = vdoc;".Args(prop));
          source.AppendLine("           continue;");
          return;
        } else if (isArray)
        {
          var et = t.GetElementType();
          if(typeof(TypedDoc).IsAssignableFrom(et))
          {
            source.AppendLine("           if (dt!=DataType.Array) break;");
            source.AppendLine("           atp = Reader.ReadDataType(streamer);");
            source.AppendLine("           if (atp!=DataType.Doc) break;");
            source.AppendLine("           doc.{0} = Reader.ReadRowArray<{1}>(doc, streamer, CodeGenerator.GetName(name));".Args(prop, et.FullName));
            source.AppendLine("           continue;");
            return;
          }
        } else if (isList)
        {
          var gat = t.GetGenericArguments()[0];
          if(typeof(TypedDoc).IsAssignableFrom(gat))
          {
            source.AppendLine("           if (dt!=DataType.Array) break;");
            source.AppendLine("           atp = Reader.ReadDataType(streamer);");
            source.AppendLine("           if (atp!=DataType.Doc) break;");
            source.AppendLine("           doc.{0} = Reader.ReadRowList<{1}>(doc, streamer, CodeGenerator.GetName(name));".Args(prop, gat.FullName));
            source.AppendLine("           continue;");
            return;
          }
        }

      }

      if (code.IsNotNullOrWhiteSpace())
      {
         source.AppendLine( code.Args(fdef.MemberInfo.Name) );
         return;
      }

      //Generate
      if (isArray)
      {
        var et = t.GetElementType();
        source.AppendLine("           if (dt==DataType.Null) doc.{0} = null;".Args(prop));
        source.AppendLine("           else if (dt!=DataType.Array) break;");
        source.AppendLine("           else");
        source.AppendLine("           {");
        source.AppendLine("              atp = Reader.ReadDataType(streamer);");
        source.AppendLine("              if (atp!=DataType.{0}) break;".Args(et.Name));
        source.AppendLine("              var len = Reader.ReadArrayLength(streamer);");
        source.AppendLine("              var arr = new {0}[len];".Args(et.FullName));
        source.AppendLine("              for(var i=0; i<len; i++) arr[i] = Reader.Read{0}(streamer);".Args(et.Name));
        source.AppendLine("              doc.{0} = arr;".Args(prop));
        source.AppendLine("           }");
        source.AppendLine("           continue;");
      } else if (isList)
      {
        var gat = t.GetGenericArguments()[0];
        var tn = gat.Name;
        source.AppendLine("           if (dt==DataType.Null) doc.{0} = null;".Args(prop));
        source.AppendLine("           if (dt!=DataType.Array) break;");
        source.AppendLine("           else");
        source.AppendLine("           {");
        source.AppendLine("              atp = Reader.ReadDataType(streamer);");
        source.AppendLine("              if (atp!=DataType.{0}) break;".Args(tn));
        source.AppendLine("              var len = Reader.ReadArrayLength(streamer);");
        source.AppendLine("              var lst = new List<{0}>(len);".Args(gat.FullName));
        source.AppendLine("              for(var i=0; i<len; i++) lst.Add( Reader.Read{0}(streamer) );".Args(tn));
        source.AppendLine("              doc.{0} = lst;".Args(prop));
        source.AppendLine("           }");
        source.AppendLine("           continue;");
      } else if (isNullable || t==typeof(string))
      {
        var tn = fdef.NonNullableType.Name;
        source.AppendLine("           if (dt==DataType.Null) doc.{0} = null;".Args(prop));
        source.AppendLine("           else if (dt==DataType.{1}) doc.{0} = Reader.Read{1}(streamer);".Args(prop, tn));
        source.AppendLine("           else break;");
        source.AppendLine("           continue;");
      }
      else //regular
      {
        var tn = fdef.NonNullableType.Name;
        source.AppendLine("           if (dt==DataType.{1}) doc.{0} = Reader.Read{1}(streamer);".Args(prop, tn));
        source.AppendLine("           else break;");
        source.AppendLine("           continue;");
      }
    }


  }
}
