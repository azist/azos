/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Scripting;


using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Wave;
using Azos.Wave.Client;
using static Azos.Aver.ThrowsAttribute;

namespace Azos.Tests.Unit.Web.Client
{
  [Runnable(TRUN.BASE, 6)]
  public class RecordTest
  {
    [Run]
    public void Record_ValidInitJSON()
    {
      var json =
@"{
  'OK': true,
  'ID': '39a833dd-b48f-46c1-83a6-96603cc962a6',
  'ISOLang': 'eng',
  '__FormMode': 'Edit',
  '__CSRFToken': '1kk_qzXPLyScAa2A5y5GLTo9IlCqjuP',
  '__Roundtrip': '{\'GDID\':\'0:1:5\'}',
  'fields': [
      {
        'def': {
          'Name': 'Mnemonic',
          'Type': 'string',
          'Description': 'Mnemonic',
          'Required': true,
          'MinSize': 1,
          'Size': 25,
          'Placeholder': 'Mnemonic',
          'Stored': false
        },
        'val': 'Dns'
      },
      {
        'def': {
          'Name': 'Vertical_ID',
          'Type': 'string',
          'Description': 'Vertical',
          'Required': false,
          'Visible': false,
          'Size': 15,
          'DefaultValue': 'HAB',
          'Key': true,
          'Case': 'Upper',
          'LookupDict': {
            'HAB': 'HAB.rs Health and Beauty',
            'READRS': 'READ.rs Book Business',
            'SLRS': 'SL.RS General Business'
          }
        },
        'val': 'HAB'
      },
      {
        'def': {
          'Name': 'Table_ID',
          'Type': 'int',
          'Key': true,
          'Description': 'Table',
          'Required': true,
          'Visible': false,
          'MinValue': 1,
          'MaxValue': 123,
          'DefaultValue': 15,
          'Kind': 'Number',
          'Stored': true
        },
        'val': 2
      }
    ]
}";

      var init = JsonReader.DeserializeDataObject(json) as JsonDataMap;
      var rec = new Record(init);

      Aver.AreEqual(0, rec.ServerErrors.Count());

      Aver.AreEqual(true, rec.OK);
      Aver.AreObjectsEqual("eng", rec.ISOLang);
      Aver.AreEqual(new Guid("39a833dd-b48f-46c1-83a6-96603cc962a6"), rec.ID);
      Aver.AreObjectsEqual("Edit", rec.FormMode);
      Aver.AreObjectsEqual("1kk_qzXPLyScAa2A5y5GLTo9IlCqjuP", rec.CSRFToken);

      var roundtrip = rec.Roundtrip;
      Aver.IsNotNull(roundtrip);
      Aver.AreEqual(roundtrip.Count, 1);
      Aver.AreObjectsEqual("0:1:5", roundtrip["GDID"]);

      Aver.AreEqual(3, rec.Schema.FieldCount);

      var fdef = rec.Schema["Mnemonic"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("Mnemonic", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreEqual(false, fdef.AnyTargetKey);
      Aver.IsNotNull(fdef.Attrs);
      Aver.AreEqual(1, fdef.Attrs.Count());
      var attr = fdef.Attrs.First();
      Aver.AreObjectsEqual("Mnemonic", attr.Description);
      Aver.AreEqual(true, attr.Required);
      Aver.AreEqual(true, attr.Visible);
      Aver.IsNull(attr.Min);
      Aver.IsNull(attr.Max);
      Aver.AreEqual(1, attr.MinLength);
      Aver.AreEqual(25, attr.MaxLength);
      Aver.IsNull(attr.Default);
      Aver.AreEqual(0, attr.ParseValueList().Count);
      Aver.IsTrue(StoreFlag.OnlyLoad == attr.StoreFlag);
      Aver.AreEqual(@"''{Name=Mnemonic Type=string Description=Mnemonic Required=True MinSize=1 Size=25 Placeholder=Mnemonic Stored=False}", attr.MetadataContent);
      Aver.AreObjectsEqual("Dns", rec["Mnemonic"]);

      fdef = rec.Schema["Vertical_ID"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("Vertical_ID", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreEqual(true, fdef.AnyTargetKey);
      Aver.IsNotNull(fdef.Attrs);
      Aver.AreEqual(1, fdef.Attrs.Count());
      attr = fdef.Attrs.First();
      Aver.AreObjectsEqual("Vertical", attr.Description);
      Aver.AreEqual(false, attr.Required);
      Aver.AreEqual(false, attr.Visible);
      Aver.IsNull(attr.Min);
      Aver.IsNull(attr.Max);
      Aver.AreEqual(0, attr.MinLength);
      Aver.AreEqual(15, attr.MaxLength);
      Aver.IsTrue(CharCase.Upper == attr.CharCase);
      Aver.AreObjectsEqual("HAB", attr.Default);

      var map1 = FieldAttribute.ParseValueListString("HAB:HAB.rs Health and Beauty,READRS:READ.rs Book Business,SLRS:SL.RS General Business", true);
      var map2 = attr.ParseValueList(true);
      Aver.IsTrue(map1.SequenceEqual(map2));

      Aver.AreObjectsEqual("''{Name=Vertical_ID Type=string Description=Vertical Required=False Visible=False Size=15 DefaultValue=HAB Key=True Case=Upper LookupDict=\"{\\\"HAB\\\":\\\"HAB.rs Health and Beauty\\\",\\\"READRS\\\":\\\"READ.rs Book Business\\\",\\\"SLRS\\\":\\\"SL.RS General Business\\\"}\"}", attr.MetadataContent);
      Aver.AreObjectsEqual("HAB", rec["Vertical_ID"]);

      fdef = rec.Schema["Table_ID"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("Table_ID", fdef.Name);
      Aver.AreObjectsEqual(typeof(long), fdef.Type);
      Aver.AreEqual(true, fdef.AnyTargetKey);
      Aver.IsNotNull(fdef.Attrs);
      Aver.AreEqual(1, fdef.Attrs.Count());
      attr = fdef.Attrs.First();
      Aver.AreObjectsEqual("Table", attr.Description);
      Aver.AreEqual(true, attr.Required);
      Aver.AreEqual(false, attr.Visible);
      Aver.AreObjectsEqual(1, attr.Min);
      Aver.AreObjectsEqual(123, attr.Max);
      Aver.AreObjectsEqual(15, attr.Default);
      Aver.IsTrue(DataKind.Number == attr.Kind);
      Aver.IsTrue(StoreFlag.LoadAndStore == attr.StoreFlag);
      Aver.AreEqual(0, attr.ParseValueList(true).Count);
      Aver.AreObjectsEqual("''{Name=Table_ID Type=int Key=True Description=Table Required=True Visible=False MinValue=1 MaxValue=123 DefaultValue=15 Kind=Number Stored=True}", attr.MetadataContent);
      Aver.AreObjectsEqual((long)2, rec["Table_ID"]);
    }

    [Run]
    public void Record_InitJSONWithErrors()
    {
      var json =
@"{
  'error': 'Error message',
  'errorText': 'Error details',
  'fields': 
   [
     {
       'def': {
         'Name': 'ID',
         'Type': 'string'
       },
       'val': 'ABBA',
       'error': 'ID Error message',
       'errorText': 'ID Error details'
     },
     {
       'def': {
         'Name': 'Name',
         'Type': 'string'
       },
       'val': 'SUP',
       'error': 'Name Error message'
     },
     {
       'def': {
         'Name': 'Value',
         'Type': 'string'
       },
       'val': 'ASP',
       'errorText': 'Value Error details'
     },
     {
       'def': {
         'Name': 'Mess',
         'Type': 'string'
       },
       'val': 'NoError'
     }
   ]
}";

      var rec = new Record(json);
      var errors = rec.ServerErrors.ToList();

      Aver.AreEqual(4, errors.Count);
      Aver.IsNull(errors[0].FieldName);
      Aver.AreObjectsEqual("Error message", errors[0].Error);
      Aver.AreObjectsEqual("Error details", errors[0].Text);

      Aver.AreEqual(4, rec.Schema.FieldCount);

      var fdef = rec.Schema["ID"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("ID", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreObjectsEqual("ID", errors[1].FieldName);
      Aver.AreObjectsEqual("ID Error message", errors[1].Error);
      Aver.AreObjectsEqual("ID Error details", errors[1].Text);
      Aver.AreObjectsEqual("ABBA", rec["ID"]);

      fdef = rec.Schema["Name"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("Name", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreObjectsEqual("Name", errors[2].FieldName);
      Aver.AreObjectsEqual("Name Error message", errors[2].Error);
      Aver.IsNull(errors[2].Text);
      Aver.AreObjectsEqual("SUP", rec["Name"]);

      fdef = rec.Schema["Value"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("Value", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreObjectsEqual("Value", errors[3].FieldName);
      Aver.IsNull(errors[3].Error);
      Aver.AreObjectsEqual("Value Error details", errors[3].Text);
      Aver.AreObjectsEqual("ASP", rec["Value"]);

      fdef = rec.Schema["Mess"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("Mess", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreObjectsEqual("NoError", rec["Mess"]);
    }

    [Run]
    [Aver.Throws(typeof(WaveException), Message = "Record.ctor(init is bad)", MsgMatch = MatchType.Contains)]
    public void Record_BadInit()
    {
      var json =
@"{
  'error': 'Error message',
  'errorText': 'Error details'
  'fields': 
   [
     {
       'def': {
         'Name': 'ID',
         'Type': 'string'
       },
       'val': 'ABBA',
       'error': 'ID Error message',
       'errorText': 'ID Error details'
     }
   ]
}";
      var rec = new Record(json);
    }

    [Run]
    [Aver.Throws(typeof(WaveException), Message = "Record.ctor(init==null|empty)", MsgMatch = MatchType.Contains)]
    public void Record_EmptyInit()
    {
      var json = "";
      var rec = new Record(json);
    }

    [Run]
    public void Record_MapJSToCLRTypes()
    {
      var json =
@"{
  'fields': 
   [
     {
       'def': {
         'Name': 'STR',
         'Type': 'string'
       },
       'val': 'ABBA'
     },
     {
       'def': {
         'Name': 'INT',
         'Type': 'int'
       },
       'val': -23
     },
     {
       'def': {
         'Name': 'NUM',
         'Type': 'real'
       },
       'val': -123.456
     },
     {
       'def': {
         'Name': 'BOOL',
         'Type': 'bool'
       },
       'val': true
     },
     {
       'def': {
         'Name': 'DATE',
         'Type': 'datetime'
       },
       'val': '2016-03-23 12:23:59'
     },
     {
       'def': {
         'Name': 'OBJ',
         'Type': 'object'
       },
       'val': { 'n': 'name', 'age': 23 }
     },
     {
       'def': {
         'Name': 'DFT'
       },
       'val': 'Default type is string'
     }
   ]
}";

      var rec = new Record(json);
      var errors = rec.ServerErrors.ToList();

      Aver.AreEqual(0, errors.Count);

      Aver.AreEqual(7, rec.Schema.FieldCount);

      var fdef = rec.Schema["STR"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("STR", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreObjectsEqual("ABBA", rec["STR"]);

      fdef = rec.Schema["INT"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("INT", fdef.Name);
      Aver.AreObjectsEqual(typeof(long), fdef.Type);
      Aver.AreObjectsEqual((long)-23, rec["INT"]);

      fdef = rec.Schema["NUM"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("NUM", fdef.Name);
      Aver.AreObjectsEqual(typeof(double), fdef.Type);
      Aver.AreObjectsEqual(-123.456, rec["NUM"]);

      fdef = rec.Schema["BOOL"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("BOOL", fdef.Name);
      Aver.AreObjectsEqual(typeof(bool), fdef.Type);
      Aver.AreObjectsEqual(true, rec["BOOL"]);

      fdef = rec.Schema["DATE"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("DATE", fdef.Name);
      Aver.AreObjectsEqual(typeof(DateTime), fdef.Type);
      Aver.AreObjectsEqual("2016-03-23 12:23:59".AsDateTime(), rec["DATE"]);

      fdef = rec.Schema["OBJ"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("OBJ", fdef.Name);
      Aver.AreObjectsEqual(typeof(object), fdef.Type);
      var value = JsonReader.DeserializeDataObject("{ 'n': 'name', 'age': 23 }") as JsonDataMap;

      var got = rec["OBJ"] as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.IsTrue(value.SequenceEqual(got));

      fdef = rec.Schema["DFT"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("DFT", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      Aver.AreObjectsEqual("Default type is string", rec["DFT"]);
    }

    [Run]
    public void Record_MapJSToCLRKinds()
    {
      var json =
@"{
  'fields': 
   [
     {
       'def': {
         'Name': 'LOCAL',
         'Type': 'datetime',
         'Kind': 'datetime-local'
       },
       'val': '2016-03-23 12:23:59'
     },
     {
       'def': {
         'Name': 'TEL',
         'Type': 'string',
         'Kind': 'tel'
       },
       'val': '(111) 222-33-44'
     },
     {
       'def': {
         'Name': 'DFT'
       },
       'val': 'Default kind is Text'
     }
   ]
}";

      var rec = new Record(json);
      var errors = rec.ServerErrors.ToList();

      Aver.AreEqual(0, errors.Count);

      Aver.AreEqual(3, rec.Schema.FieldCount);

      var fdef = rec.Schema["LOCAL"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("LOCAL", fdef.Name);
      Aver.AreObjectsEqual(typeof(DateTime), fdef.Type);
      var attr = fdef.Attrs.First();
      Aver.IsTrue(DataKind.DateTimeLocal == attr.Kind);
      Aver.AreObjectsEqual("2016-03-23 12:23:59".AsDateTime(), rec["LOCAL"]);

      fdef = rec.Schema["TEL"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("TEL", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      attr = fdef.Attrs.First();
      Aver.IsTrue(DataKind.Telephone == attr.Kind);
      Aver.AreObjectsEqual("(111) 222-33-44", rec["TEL"]);

      fdef = rec.Schema["DFT"];
      Aver.IsNotNull(fdef);
      Aver.AreObjectsEqual("DFT", fdef.Name);
      Aver.AreObjectsEqual(typeof(string), fdef.Type);
      attr = fdef.Attrs.First();
      Aver.IsTrue(DataKind.Text == attr.Kind);
      Aver.AreObjectsEqual("Default kind is Text", rec["DFT"]);
    }

    [Run]
    public void Validate_NoError_ComplexValueList()
    {
      var json =
@"{
  'fields':[
  {'def':
   {'Name':'Tax_Code',
    'Type':'string',
    'Description':'Tax Code',
    'Required':true,
    'MinSize':1,
    'Size':32,
    'LookupDict':
    {'values':[
      {'id':'GNR',
       'val':'GNR',
       'children':[
       {'id':'A',
        'val':'A',
        'name':'Always Taxable',
        'descr':null},
       {'id':'N',
        'val':'N',
        'name':'Always Nontaxable',
        'descr':null}
       ]
      }]
    }}}]}";

      var rec = new Azos.Wave.Client.Record(json);
      rec["Tax_Code"] = "GNR.A";
      var error = rec.Validate();
      Aver.IsNull(error);

      rec["Tax_Code"] = "CLTH";
      error = rec.Validate();
      Aver.IsNull(error);
    }
  }
}
