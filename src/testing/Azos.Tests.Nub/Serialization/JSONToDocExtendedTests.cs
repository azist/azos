/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;

using Azos.Scripting;

using Azos.Serialization.JSON;
using Azos.Collections;
using System.IO;
using Azos.Data;
using Azos.Time;
using Azos.Financial;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JSONToDocExtendedTests
  {

    public struct CustomStructType : IJSONWritable, IJSONReadable
    {
      public CustomStructType(string text)
      {
        Text = text;
        Length = text==null? 0: text.Length;
      }

      public string Text;
      public int Length;

      public bool ReadAsJSON(object data, bool fromUI, JSONReader.NameBinding? nameBinding)
      {
        if (data==null) return false;

        var str = data as string;
        if (str==null) str = data.ToString();

        Text = str;
        Length = str.Length;
        return true;
      }

      public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
      {
        JSONWriter.EncodeString(wri, Text, options);
      }
    }

    public class CustomDoc : TypedDoc
    {
      [Field] public string ID{ get;set;}
      [Field] public CustomStructType Data{ get;set;}
    }


    [Run]
    public void CustomWritableReadable()
    {
      var d1 = new CustomDoc{ ID = "sss", Data =  new CustomStructType("Custom string 1") };

      var json = d1.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap);
      Console.WriteLine(json);
      var jsonMap = json.JSONToDataObject() as JSONDataMap;

      var d2 = new CustomDoc();
      JSONReader.ToDoc(d2, jsonMap);

      Aver.AreEqual(d1.Data.Text, d2.Data.Text);
      Aver.AreEqual(d1.Data.Length, d2.Data.Length);
    }

    public class WithVariousStructsDoc : TypedDoc
    {
      [Field] public GDID       Gdid{ get;set;}
      [Field] public GDIDSymbol GdidSymbol { get; set; }
      [Field] public Guid       Guid { get; set; }
      [Field] public Atom       Atom { get; set; }
      [Field] public TimeSpan   Timespan { get; set; }
      [Field] public DateTime   DateTime { get; set; }
      [Field] public NLSMap     Nls { get; set; }
      [Field] public DateRange  DateRange { get; set; }
      [Field] public Amount     Amount { get; set; }
      [Field] public StringMap  StringMap { get; set; }
    }



  }
}
