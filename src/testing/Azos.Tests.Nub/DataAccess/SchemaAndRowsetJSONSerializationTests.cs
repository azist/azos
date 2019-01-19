/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.Scripting;


using Azos.Data;

using Azos.Serialization.JSON;


namespace Azos.Tests.Nub.DataAccess
{
    [Runnable]
    public class SchemaAndRowsetSerializationTests
    {
      [Run("readOnly=true")]
      [Run("readOnly=false")]
      public void Schema_FromJSON(bool readOnly)
      {
        var src = new TeztRow().Schema;
        var json = src.ToJSON();

        var trg = Schema.FromJSON(json, readOnly);

        Aver.AreEqual(trg.ReadOnly, readOnly);
        schemaAssertions(trg, src);
      }

      [Run]
      public void Rowset_FromJSON_ShemaOnly()
      {
        var src = new Rowset(new TeztRow().Schema);
        var options = new Azos.Serialization.JSON.JSONWritingOptions
                                  {
                                    RowsetMetadata = true,
                                    SpaceSymbols = true,
                                    IndentWidth = 2,
                                    MemberLineBreak = true,
                                    ObjectLineBreak = true
                                  };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json, true);

        schemaAssertions(trg.Schema, src.Schema);
        Aver.AreEqual(trg.Count, 0);
      }

      [Run]
      public void Table_FromJSON_ShemaOnly()
      {
        var src = new Table(new TeztRow().Schema);
        var options = new Azos.Serialization.JSON.JSONWritingOptions
                                  {
                                    RowsetMetadata = true,
                                    SpaceSymbols = true,
                                    IndentWidth = 2,
                                    MemberLineBreak = true,
                                    ObjectLineBreak = true
                                  };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json, true);

        schemaAssertions(trg.Schema, src.Schema);
        Aver.AreEqual(trg.Count, 0);
      }

      [Run("rowsAsMap=true")]
      [Run("rowsAsMap=false")]
      public void Rowset_FromJSON(bool rowsAsMap)
      {
        var row = new TeztRow();
        var src = new Rowset(row.Schema);

        row.BoolField = true;
        row.CharField = 'a';
        row.StringField = "aaa";
        row.DateTimeField = new DateTime(2016, 1, 2);
        row.GDIDField = new GDID(1, 2, 3);

        row.ByteField = 100;
        row.ShortField = -100;
        row.IntField = -999;

        row.UIntField = 254869;
        row.LongField = -267392;

        row.FloatField = 32768.32768F;
        row.DoubleField = -1048576.1048576D;

        row.DecimalField = 1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {-1, 0, 1};
        row.ListString = new List<string> {"one", "two", "three"};
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "John", Age = 20 };

        src.Add(row);

        row.BoolField = false;
        row.CharField = 'b';
        row.StringField = "bbb";
        row.DateTimeField = new DateTime(2016, 2, 1);
        row.GDIDField = new GDID(4, 5, 6);

        row.ByteField = 101;
        row.ShortField = 100;
        row.IntField = 999;

        row.UIntField = 109876;
        row.LongField = 267392;

        row.FloatField = -32768.32768F;
        row.DoubleField = -048576.1048576D;

        row.DecimalField = -1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {1, 0, -1};
        row.ListString = new List<string> { "three","two", "one" };
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {0, "zero"},
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "Ann", Age = 19 };

        src.Add(row);

        var options = new Azos.Serialization.JSON.JSONWritingOptions
                          {
                            RowsetMetadata = true,
                            SpaceSymbols = true,
                            IndentWidth = 2,
                            MemberLineBreak = true,
                            ObjectLineBreak = true,
                            RowsAsMap = rowsAsMap
                          };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json);

        schemaAssertions(trg.Schema, src.Schema);
        rowsAssertions(src, trg, rowsAsMap);
      }

      [Run("rowsAsMap=true")]
      [Run("rowsAsMap=false")]
      public void Table_FromJSON(bool rowsAsMap)
      {
        var row = new TeztRow();
        var src = new Table(row.Schema);

        row.BoolField = true;
        row.CharField = 'a';
        row.StringField = "aaa";
        row.DateTimeField = new DateTime(2016, 1, 2);
        row.GDIDField = new GDID(1, 2, 3);

        row.ByteField = 100;
        row.ShortField = -100;
        row.IntField = -999;

        row.UIntField = 254869;
        row.LongField = -267392;

        row.FloatField = 32768.32768F;
        row.DoubleField = -1048576.1048576D;

        row.DecimalField = 1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {-1, 0, 1};
        row.ListString = new List<string> {"one", "two", "three"};
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "John", Age = 20 };

        src.Add(row);

        row.BoolField = false;
        row.CharField = 'b';
        row.StringField = "bbb";
        row.DateTimeField = new DateTime(2016, 2, 1);
        row.GDIDField = new GDID(4, 5, 6);

        row.ByteField = 101;
        row.ShortField = 100;
        row.IntField = 999;

        row.UIntField = 109876;
        row.LongField = 267392;

        row.FloatField = -32768.32768F;
        row.DoubleField = -048576.1048576D;

        row.DecimalField = -1.0529M;

        row.NullableField = null;

        row.ArrayInt = new int[] {1, 0, -1};
        row.ListString = new List<string> { "three","two", "one" };
        row.DictionaryIntStr = new Dictionary<int, string>
        {
          {0, "zero"},
          {1, "first"},
          {2, "second"}
        };

        row.RowField = new Person { Name = "Ann", Age = 19 };

        src.Add(row);

        var options = new Azos.Serialization.JSON.JSONWritingOptions
                                  {
                                    RowsetMetadata = true,
                                    SpaceSymbols = true,
                                    IndentWidth = 2,
                                    MemberLineBreak = true,
                                    ObjectLineBreak = true,
                                    RowsAsMap = rowsAsMap
                                  };
        var json = src.ToJSON(options);

        var trg = RowsetBase.FromJSON(json);

        schemaAssertions(trg.Schema, src.Schema);
        rowsAssertions(src, trg, rowsAsMap);
      }

      [Run("rowsAsMap=true")]
      [Run("rowsAsMap=false")]
      public void Rowset_FromJSON_FieldMissed(bool rowsAsMap)
      {
        var row = new Person { Name = "Henry", Age = 43 };
        var rowSet = new Rowset(row.Schema);
        rowSet.Add(row);
        var options = new Azos.Serialization.JSON.JSONWritingOptions
                          {
                            RowsetMetadata = true,
                            RowsAsMap = rowsAsMap
                          };
        var json = rowSet.ToJSON(options);
        var map = JSONReader.DeserializeDataObject( json ) as JSONDataMap;
        var rows = (map["Rows"] as IList<object>);
        if (rowsAsMap)
        {
          var pers = rows[0] as IDictionary<string, object>;
          pers.Remove("Age");
        }
        else
        {
          var pers = rows[0] as IList<object>;
          pers.RemoveAt(1);
        }

        bool allMatched;
        var trg = RowsetBase.FromJSON(map, out allMatched);

        Aver.IsFalse(allMatched);
        var trgRow = trg[0];
        Aver.AreEqual(trgRow.Schema.FieldCount, 2);
        Aver.AreObjectsEqual(trgRow["Name"], "Henry");
        Aver.IsNull(trgRow["Age"]);
      }

      [Run("rowsAsMap=true")]
      [Run("rowsAsMap=false")]
      public void Rowset_FromJSON_DefMissed(bool rowsAsMap)
      {
        var row = new Person { Name = "Henry", Age = 43 };
        var rowSet = new Rowset(row.Schema);
        rowSet.Add(row);
        var options = new Azos.Serialization.JSON.JSONWritingOptions
                          {
                            RowsetMetadata = true,
                            RowsAsMap = rowsAsMap
                          };
        var json = rowSet.ToJSON(options);
        var map = JSONReader.DeserializeDataObject( json ) as JSONDataMap;
        var schema = (map["Schema"] as IDictionary<string, object>);
        var defs = schema["FieldDefs"] as IList<object>;
        defs.RemoveAt(1);

        bool allMatched;
        var trg = RowsetBase.FromJSON(map, out allMatched);

        Aver.IsFalse(allMatched);
        var trgRow = trg[0];
        Aver.AreEqual(trgRow.Schema.FieldCount, 1);
        Aver.AreObjectsEqual(trgRow["Name"], "Henry");
      }

      private void rowsAssertions(RowsetBase src, RowsetBase trg, bool rowsAsMap)
      {
        Aver.AreEqual(trg.Count, src.Count);
        for (var j = 0; j < src.Count; j++)
        {
          var trgRow = trg[j];
          var srcRow = src[j] as TeztRow;
          Aver.AreEqual(trgRow["BoolField"].AsBool(), srcRow.BoolField);
          Aver.AreEqual(trgRow["CharField"].AsString(), srcRow.CharField.ToString());
          Aver.AreEqual(trgRow["StringField"].AsString(), srcRow.StringField);
          Aver.AreEqual(trgRow["DateTimeField"].AsDateTime(), srcRow.DateTimeField);
          Aver.AreEqual(trgRow["GDIDField"].AsGDID(), srcRow.GDIDField);

          Aver.AreEqual(trgRow["ByteField"].AsByte(), srcRow.ByteField);
          Aver.AreEqual(trgRow["ShortField"].AsShort(), srcRow.ShortField);
          Aver.AreEqual(trgRow["IntField"].AsInt(), srcRow.IntField);

          Aver.AreEqual(trgRow["UIntField"].AsUInt(), srcRow.UIntField);
          Aver.AreEqual(trgRow["LongField"].AsLong(), srcRow.LongField);

          Aver.AreEqual(trgRow["FloatField"].AsFloat(), srcRow.FloatField);
          Aver.AreEqual(trgRow["DoubleField"].AsDouble(), srcRow.DoubleField);
          Aver.AreEqual(trgRow["DecimalField"].AsDecimal(), srcRow.DecimalField);

          Aver.AreEqual(trgRow["NullableField"].AsNullableInt(), srcRow.NullableField);

          var array = trgRow["ArrayInt"] as IList<object>;
          Aver.IsNotNull(array);
          Aver.AreEqual(array.Count, srcRow.ArrayInt.Length);
          for (var i = 0; i < array.Count; i++)
            Aver.AreEqual(array[i].AsInt(), srcRow.ArrayInt[i]);

          var list = trgRow["ListString"] as IList<object>;
          Aver.IsNotNull(list);
          Aver.IsTrue(list.SequenceEqual(srcRow.ListString));

          var dict = trgRow["DictionaryIntStr"] as IDictionary<string, object>;
          Aver.IsNotNull(dict);
          Aver.AreEqual(dict.Count, srcRow.DictionaryIntStr.Count);
          foreach (var kvp in srcRow.DictionaryIntStr)
            Aver.AreEqual(dict[kvp.Key.ToString()].ToString(), kvp.Value);

          if (rowsAsMap)
          {
            var pers = trgRow["RowField"] as IDictionary<string, object>;
            Aver.IsNotNull(pers);
            Aver.AreEqual(pers.Count, 2);
            Aver.AreEqual(pers["Name"].AsString(), srcRow.RowField.Name);
            Aver.AreEqual(pers["Age"].AsInt(), srcRow.RowField.Age);
          }
          else
          {
            var pers = trgRow["RowField"] as IList<object>;
            Aver.IsNotNull(pers);
            Aver.AreEqual(pers.Count, 2);
            Aver.AreEqual(pers[0].AsString(), srcRow.RowField.Name);
            Aver.AreEqual(pers[1].AsInt(), srcRow.RowField.Age);
          }
        }
      }

      private void schemaAssertions(Schema trg, Schema src)
      {
        Aver.AreEqual(trg.FieldCount, src.FieldCount);

        Aver.AreObjectsEqual(trg["BoolField"].Type, typeof(bool));
        Aver.AreObjectsEqual(trg["CharField"].Type, typeof(string));
        Aver.AreObjectsEqual(trg["StringField"].Type, typeof(string));
        Aver.AreObjectsEqual(trg["DateTimeField"].Type, typeof(DateTime));
        Aver.AreObjectsEqual(trg["GDIDField"].Type, typeof(object));

        Aver.AreObjectsEqual(trg["ByteField"].Type, typeof(uint));
        Aver.AreObjectsEqual(trg["UShortField"].Type, typeof(uint));
        Aver.AreObjectsEqual(trg["UInt16Field"].Type, typeof(uint));
        Aver.AreObjectsEqual(trg["UIntField"].Type, typeof(uint));
        Aver.AreObjectsEqual(trg["UInt32Field"].Type, typeof(uint));

        Aver.AreObjectsEqual(trg["SByteField"].Type, typeof(int));
        Aver.AreObjectsEqual(trg["ShortField"].Type, typeof(int));
        Aver.AreObjectsEqual(trg["Int16Field"].Type, typeof(int));
        Aver.AreObjectsEqual(trg["IntField"].Type, typeof(int));
        Aver.AreObjectsEqual(trg["Int32Field"].Type, typeof(int));

        Aver.AreObjectsEqual(trg["ULongField"].Type, typeof(ulong));
        Aver.AreObjectsEqual(trg["UInt64Field"].Type, typeof(ulong));

        Aver.AreObjectsEqual(trg["LongField"].Type, typeof(long));
        Aver.AreObjectsEqual(trg["Int64Field"].Type, typeof(long));

        Aver.AreObjectsEqual(trg["FloatField"].Type, typeof(double));
        Aver.AreObjectsEqual(trg["SingleField"].Type, typeof(double));
        Aver.AreObjectsEqual(trg["DoubleField"].Type, typeof(double));

        Aver.AreObjectsEqual(trg["DecimalField"].Type, typeof(decimal));

        Aver.AreObjectsEqual(trg["NullableField"].Type, typeof(int?));

        Aver.AreObjectsEqual(trg["ArrayInt"].Type, typeof(List<object>));
        Aver.AreObjectsEqual(trg["ListString"].Type, typeof(List<object>));
        Aver.AreObjectsEqual(trg["DictionaryIntStr"].Type, typeof(Dictionary<string, object>));

        Aver.AreObjectsEqual(trg["RowField"].Type, typeof(object));

        Aver.IsTrue(trg["FieldWithAttrs1"].Attrs.SequenceEqual(src["FieldWithAttrs1"].Attrs));
        Aver.IsTrue(trg["FieldWithAttrs2"].Attrs.SequenceEqual(src["FieldWithAttrs2"].Attrs));
      }

      public class Person : TypedDoc
      {
        [Field] public string Name { get; set; }
        [Field] public int Age { get; set; }
      }

      private class TeztRow : TypedDoc
      {
        [Field] public bool BoolField { get; set;}
        [Field] public char CharField { get; set;}
        [Field] public string StringField { get; set;}
        [Field] public DateTime DateTimeField { get; set;}
        [Field] public GDID GDIDField { get; set;}

        [Field] public byte ByteField { get; set;}
        [Field] public sbyte SByteField { get; set; }
        [Field] public short ShortField { get; set; }
        [Field] public Int16 Int16Field { get; set; }
        [Field] public ushort UShortField { get; set; }
        [Field] public UInt16 UInt16Field { get; set; }
        [Field] public int IntField { get; set; }
        [Field] public Int32 Int32Field { get; set; }

        [Field] public uint UIntField { get; set; }
        [Field] public UInt32 UInt32Field { get; set; }
        [Field] public long LongField { get; set; }
        [Field] public Int64 Int64Field { get; set; }

        [Field] public ulong ULongField { get; set; }
        [Field] public UInt64 UInt64Field { get; set; }

        [Field] public float FloatField { get; set; }
        [Field] public Single SingleField { get; set; }
        [Field] public double DoubleField { get; set; }

        [Field] public decimal DecimalField { get; set; }

        [Field] public int? NullableField { get; set; }

        [Field] public int[] ArrayInt { get; set; }
        [Field] public List<string> ListString { get; set; }
        [Field] public Dictionary<int, string> DictionaryIntStr { get; set; }

        [Field] public Person RowField { get; set; }

        [Field(required:true, visible: true, key: true, description: "FieldWithAttrs1", valueList: "-1;0;1", kind: DataKind.Number)]
        public int FieldWithAttrs1 { get; set; }

        [Field(required:false, visible: false, key: false, minLength: 3, maxLength: 20, charCase: CharCase.Upper)]
        public string FieldWithAttrs2 { get; set; }
      }
    }
}
