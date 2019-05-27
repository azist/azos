/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class ConfAndJSONTests
  {
    [Run]
    public void ConfigSectionNode_2_JSONDataMap()
    {
      var node = @"opt
                  {
                    detailed-instrumentation=true
                    tables
                    {
                      master { name='tfactory' fields-qty=14}
                      slave { name='tdoor' fields-qty=20 important=true}
                    }
                  }".AsLaconicConfig();
      var map = node.ToJSONDataMap();

      Aver.AreEqual(2, map.Count);
      Aver.IsTrue(map["detailed-instrumentation"].AsString() == "true");

      var tablesMap = (JsonDataMap)map["tables"];

      var master = (JsonDataMap)tablesMap["master"];
      Aver.IsTrue(master["name"].AsString() == "tfactory");
      Aver.IsTrue(master["fields-qty"].AsString() == "14");

      var slave = (JsonDataMap)tablesMap["slave"];
      Aver.IsTrue(slave["name"].AsString() == "tdoor");
      Aver.IsTrue(slave["fields-qty"].AsString() == "20");
      Aver.IsTrue(slave["important"].AsString() == "true");
    }

    [Run]
    public void JSONDataMap_2_ConfigSectionNode()
    {
      var map = (JsonDataMap)@" {
                                  'detailed-instrumentation': true,
                                  tables:
                                  {
                                    master: { name: 'tfactory', 'fields-qty': 14},
                                    slave: { name: 'tdoor', 'fields-qty': 20, important: true}
                                  }
                                }".JsonToDataObject();

      var cfg = map.ToConfigNode();

      Aver.AreEqual(1, cfg.Attributes.Count());
      Aver.AreEqual(1, cfg.Children.Count());

      Aver.IsTrue(cfg.AttrByName("detailed-instrumentation").ValueAsBool());

      var tablesNode = cfg.Children.Single(ch => ch.Name == "tables");

      var master = cfg.NavigateSection("tables/master");
      Aver.AreEqual(2, master.Attributes.Count());
      Aver.IsTrue(master.AttrByName("name").ValueAsString() == "tfactory");
      Aver.IsTrue(master.AttrByName("fields-qty").ValueAsInt() == 14);

      var slave = cfg.NavigateSection("tables/slave");
      Aver.AreEqual(3, slave.Attributes.Count());
      Aver.IsTrue(slave.AttrByName("name").ValueAsString() == "tdoor");
      Aver.IsTrue(slave.AttrByName("fields-qty").ValueAsInt() == 20);
      Aver.IsTrue(slave.AttrByName("important").ValueAsBool());
    }

    [Run]
    public void JSONtoLaconicToJSON()//20170414
    {
       var config = "{r:{}}".AsJSONConfig();
       Console.WriteLine(config.ToLaconicString());
       var json = config.ToJSONString();
       Console.WriteLine(json);
       Aver.AreEqual("{\"r\":{}}", json);
    }
  }
}
