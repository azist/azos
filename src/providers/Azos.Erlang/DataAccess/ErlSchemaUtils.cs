
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Collections;
using Azos.Erlang;

namespace Azos.Data.Access.Erlang
{
  public static class ErlSchemaUtils
  {
    /// <summary>
    /// Convert Row to a hierarchical term that corresponds to the row's schema.
    /// The Erlang term is in the form:
    /// <code>{SchemaName::atom(), [{FieldName::atom(), Value}]}.</code>
    /// </summary>
    /// <param name="doc">Doc to convert to Erlang object</param>
    /// <param name="targetName">Target to use to lookup field attributes in the row</param>
    /// <param name="schemaName">Alternative schema name to use for the outermost name of the schema in the output.</param>
    /// <param name="outputDefVals">When false no field values are included in output if they are equal to default values</param>
    public static IErlObject ToErlObject(this Doc doc, string targetName = null, string schemaName = null, bool outputDefVals = true)
    {
      var list = new ErlList();
      for (int i = 0; i < doc.Count(); ++i)
      {
        var fld    = doc.Schema[i];
        var atr    = fld[targetName];
        var clrVal = doc[i];
        IErlObject erlVal;

        // Primitive types are converted using the schema type mapping functions
        if (clrVal is Doc)
        {
          // If the row is nested, traverse the hierarchy recursively:
          var child = (Doc)clrVal;

          if (ts_VisitedDocs == null)
            ts_VisitedDocs = new HashSet<Doc>(Collections.ReferenceEqualityComparer<Doc>.Instance);

          if (ts_VisitedDocs.Contains(child))
            throw new ErlDataAccessException(StringConsts.ERL_CANNOT_CONVERT_TYPES_CYCLE_ERROR
                                             .Args(doc.Schema.Name, fld.Name));
          ts_VisitedDocs.Add(child);

          try { erlVal = child.ToErlObject(targetName, null, outputDefVals); }
          finally { ts_VisitedDocs.Remove(child); }

          list.Add(erlVal);
        }
        // Skip default values if instructed to do so with outputDefVals
        else if (clrVal != null && (outputDefVals || atr.Default == null || !clrVal.Equals(atr.Default)))
        {
          erlVal = SchemaMap.ClrToErlValue(atr.BackendType, clrVal);
          list.Add(new ErlTuple(new ErlAtom(fld.Name), erlVal));
        }
        else if (clrVal == null && atr.Default != null)
        {
          erlVal = SchemaMap.ClrToErlValue(atr.BackendType, atr.Default);
          list.Add(new ErlTuple(new ErlAtom(fld.Name), erlVal));
        }
      }
      return new ErlTuple(new ErlAtom(schemaName ?? doc.Schema.Name), list);
    }

    /// <summary>
    /// Convert an Erlang hierarchical term representing a schema to a Row.
    /// </summary>
    /// <param name="doc">Doc to update</param>
    /// <param name="data">
    ///   Data to update row with.
    ///   The data must be in the form {SchemaName::atom, [{FieldName::atom(), Value}]}.
    /// </param>
    /// <param name="schema">Alternative schema to use in place of row.Schema</param>
    /// <param name="targetName">Name of the target for looking up field attributes</param>
    /// <param name="schemaName">Alternative name of the top-most 'SchemaName' atom used in the "data".</param>
    /// <param name="knownSchemas">List of known schemas to use when initializing a field a DynamicRow type.</param>
    public static void Update(this Doc doc, IErlObject data, Schema schema = null, string targetName = null,
                             string schemaName = null, Registry<Schema> knownSchemas = null)
    {
      if (schema == null)
        schema = doc.Schema;

      if (schemaName == null)
        schemaName = schema.Name;

      if (data == null)
        data = new ErlTuple(new ErlAtom(schemaName), new ErlList());

      // Input data must be in the form: {SchemaName::atom(), [{FieldName::atom(), Value}]}
      // where Value can be any primitive value or a hierarchical value with another Row type.
      if (!checkKeyValueTuple(data) || ((ErlTuple)data)[1].TypeOrder != ErlTypeOrder.ErlList)
        throw new ErlDataAccessException(
          StringConsts.ERL_DS_CRUD_RESP_SCH_MISMATCH_ERROR.Args(data.ToString(), schema.Name));

      var dataList = ((ErlTuple)data)[1] as ErlList;

      // Make sure that the first element of the tuple matches the schema name
      if (!((ErlTuple)data)[0].ValueAsString.Equals(schemaName))
        throw new ErlDataAccessException(
          StringConsts.ERL_DS_CRUD_RESP_SCH_MISMATCH_ERROR.Args(data.ToString(), schema.Name));

      // Contains a set of field names that are present in configuration
      var presentFieldNames = new HashSet<string>();
      foreach (var item in dataList.Where(checkKeyValueTuple).Cast<ErlTuple>())
        presentFieldNames.Add(item[0].ValueAsString);

      ErlList newList = null;

      foreach (var fld in schema.Where(fd => typeof(Doc).IsAssignableFrom(fd.Type)))
        if (!presentFieldNames.Contains(fld.Name))
        {
          if (newList == null)
            newList = (ErlList)dataList.Clone();
          // Add: {FieldName::atom(), []}
          newList.Add(new ErlTuple(new ErlAtom(fld.Name), new ErlList()));
        }

      // If no new items were added to the list use current list:
      if (newList == null) newList = dataList;

      foreach (var item in newList.Where(checkKeyValueTuple).Cast<ErlTuple>())
      {
        var name  = item[0].ValueAsString;
        var value = item[1];
        var fdef  = schema[name];
        var attr  = fdef[targetName];

        if (!attr.Visible || (attr.Metadata != null && attr.Metadata.Navigate("$readonly|$read-only|$read_only").ValueAsBool()))
          continue;

        // If this field is defined in the schema as a Row type, then we need to descend into the
        // value's hierarchical structure and treat it as a nested row
        if (typeof(Doc).IsAssignableFrom(fdef.Type))
        {
          // Get the row associated
          Schema chldSch;
          var chldRow = doc[fdef.Order] as Doc;

          // If the row has a field of Row type initialized, use its Schema value.
          if (chldRow != null)
            chldSch = chldRow.Schema;
          else
          {
            // Otherwise lookup the schema from the given registry
            if (!knownSchemas.TryGetValue(name, out chldSch))
              throw new ErlDataAccessException(
                StringConsts.ERL_DS_SCHEMA_NOT_KNOWN_ERROR.Args(name, data.ToString()));
            // Construct the field's value as dynmiac row of the looked up schema type
            chldRow = new DynamicDoc(chldSch);
            chldRow.ApplyDefaultFieldValues();
            doc[fdef.Order] = chldRow;
          }

          if (value.TypeOrder != ErlTypeOrder.ErlList)
            throw new ErlDataAccessException(
              StringConsts.ERL_DS_SCHEMA_INVALID_VALUE_ERROR.Args(chldSch.Name, value));

          // Recursively update the field's value from given data by using the field's schema:
          chldRow.Update(item, chldSch, targetName, knownSchemas: knownSchemas);
        }
        else
        {
          // This is a primitive value type
          var clr = SchemaMap.ErlToClrValue(value, schema, fdef, null, data);
          doc.SetFieldValue(fdef, clr);
        }
      }
    }

    [ThreadStatic]
    private static HashSet<Doc> ts_VisitedDocs;

    /// <summary>
    /// Check that value is in the form: {Name::atom(), Val}
    /// </summary>
    private static bool checkKeyValueTuple(IErlObject value)
    {
      var val = (value as ErlTuple);
      return val != null    &&
             val.Count == 2 &&
             val[0].TypeOrder == ErlTypeOrder.ErlAtom;
    }
  }
}