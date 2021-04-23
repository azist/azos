/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class XMLConfigurationTests
  {
    [Run]
    [Aver.Throws(typeof(ConfigException))]
    public void BadXMLName()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("# bad name", null);
    }

    [Run]
    public void StrictNamesFalse()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.StrictNames = false;
      var node = conf.Root.AddChildNode("bad name", null);
      Aver.AreEqual("bad-name", node.Name);
    }



    [Run]
    [Aver.Throws(typeof(ConfigException))]
    public void ReadOnlyErrorOnNodeCreate()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();

      conf.SetReadOnly(true);

      conf.Root.AddChildNode("A", null);
    }

    [Run]
    public void NodeCreate()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", null);

      Aver.AreEqual("A", conf.Root["A"].Name);
    }


    [Run]
    public void EmptySectionAndAttributeNodes()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", null).AddChildNode("A.A", "haha!").AddAttributeNode("good", true);

      Aver.AreEqual("haha!", conf.Root["A"]["A.A"].Value);
      Aver.AreEqual(true, conf.Root["A"]["A.A"].Exists);
      Aver.AreEqual(true, conf.Root["A"]["A.A"].AttrByName("good").Exists);
      Aver.AreEqual(true, conf.Root["A"]["A.A"].AttrByIndex(0).Exists);

      Aver.AreEqual(false, conf.Root["A1"]["A.A"].Exists);
      Aver.AreEqual(false, conf.Root["A"]["A.A1"].Exists);

      Aver.AreEqual(false, conf.Root["A"]["A.A"].AttrByName("bad").Exists);
      Aver.AreEqual(false, conf.Root["A"]["A.A"].AttrByIndex(100).Exists);
    }


    [Run]
    [Aver.Throws(typeof(ConfigException))]
    public void ReadOnlyErrorOnNodeRename()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", null);
      conf.SetReadOnly(true);
      conf.Root.Name = "changed-name";
    }

    [Run]
    [Aver.Throws(typeof(ConfigException))]
    public void ReadOnlyErrorOnNodeDelete()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", null);
      conf.SetReadOnly(true);
      conf.Root["A"].Delete();
    }

    [Run]
    public void NodeDelete()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", null);
      conf.Root.AddChildNode("B", null).AddChildNode("B1");
      conf.Root["A"].Delete();
      Aver.AreEqual(false, conf.Root["A"].Exists);
      Aver.AreEqual(true, conf.Root["B"].Exists);

      conf.Root.ResetModified();
      Aver.AreEqual(false, conf.Root["B"].Modified);
      conf.Root["B"]["B1"].Delete();
      Aver.AreEqual(true, conf.Root["B"].Modified);
    }

    [Run]
    public void RootDelete()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", null);

      Aver.AreEqual(true, conf.Root.Exists);
      conf.Root.Delete();
      Aver.AreEqual(false, conf.Root.Exists);
    }


    [Run]
    public void NodeRename()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", null);
      conf.Root["A"].Name = "B";
      Aver.AreEqual("B", conf.Root["B"].Name);
    }

    [Run]
    public void NavigationAndValueAccessors()
    {
      var conf = new Azos.Conf.XMLConfiguration();
      conf.Create();
      conf.Root.AddChildNode("A", 10).AddChildNode("A.A", 20);
      conf.Root.AddChildNode("B", 789);
      conf.Root.AddChildNode("URI", UriKind.RelativeOrAbsolute);

      conf.Root["A"]["A.A"].AddChildNode("MARS", -1000);
      Aver.AreEqual(-1000, conf.Root["A"]["A.A"]["MARS"].ValueAsInt());
      Aver.AreEqual(-1000, conf.Root[0][0][0].ValueAsInt());
      Aver.AreEqual(789, conf.Root[1].ValueAsInt());
      Aver.IsTrue(UriKind.RelativeOrAbsolute == conf.Root["URI"].ValueAsEnum<UriKind>());
      Aver.IsTrue(UriKind.RelativeOrAbsolute == conf.Root["URI"].ValueAsEnum<UriKind>(UriKind.Absolute));
      Aver.IsTrue(UriKind.RelativeOrAbsolute == conf.Root["NONENTITY"].ValueAsEnum<UriKind>(UriKind.RelativeOrAbsolute));
    }


    [Run]
    public void LoadReadSaveReadAgainCompound()
    {
      var xml = @"<root kind=""Absolute"">
           <!-- comment -->
            <a>
              <b cool=""true""> <c> 75 </c> </b>
            </a>
            <web.world>who knows?</web.world>
           </root>";

      var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

      Aver.IsTrue(UriKind.Absolute == conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
      Aver.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
      Aver.AreEqual(75, conf.Root["a"]["b"]["c"].ValueAsInt());
      Aver.AreEqual(-123, conf.Root["a"]["dont exist"]["c"].ValueAsInt(-123));

      Aver.AreEqual("who knows?", conf.Root["web.world"].ValueAsString());

      var savedXml = conf.ToString();
      //retest after configuration was saved and then reloaded from string
      conf = Azos.Conf.XMLConfiguration.CreateFromXML(savedXml);
      Aver.IsTrue(UriKind.Absolute == conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
      Aver.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
      Aver.AreEqual(75, conf.Root["a"]["b"]["c"].ValueAsInt());
      Aver.AreEqual("who knows?", conf.Root["web.world"].ValueAsString());
    }


    [Run]
    public void PathNavigation()
    {
      var xml = @"<root kind=""Absolute"">
           <!-- comment -->
            <a>
              <b cool=""true"" snake=""false""> <c> 75 </c> </b>
            </a>
            <web.world>who knows?</web.world>
           </root>";

      var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

      Aver.IsTrue(UriKind.Absolute == conf.Root.Navigate("$kind").ValueAsEnum<UriKind>(UriKind.Relative));
      Aver.IsTrue(UriKind.Absolute == conf.Root.Navigate("!$kind").ValueAsEnum<UriKind>(UriKind.Relative));

      Aver.AreEqual(75, conf.Root.Navigate("a/b/c").ValueAsInt());

      Aver.AreEqual(true, conf.Root.Navigate("a/b/c").Exists);
      Aver.AreEqual(false, conf.Root.Navigate("a/b/c/d").Exists);
      Aver.AreEqual(false, conf.Root.Navigate("GGG/b/c").Exists); //non existing section is in the beginning
      Aver.AreEqual(false, conf.Root.Navigate("a/GGG/c").Exists); //non existing section is in the middle

      Aver.AreEqual(true, conf.Root.Navigate("/a/b/$cool").ValueAsBool());
      Aver.AreEqual(false, conf.Root.Navigate("/a/b/$snake").ValueAsBool());
      Aver.AreEqual("b", conf.Root.Navigate("/a/b").Name);
      Aver.AreObjectsEqual(conf.Root, (conf.Root.Navigate("a/b") as ConfigSectionNode).Navigate("../.."));

    }

    [Run]
    [Aver.Throws(typeof(ConfigException))]
    public void PathNavigationWithRequiredNodes()
    {
      var xml = @"<root>

            <a>
              <b cool=""true"" snake=""false""> <c> 75 </c> </b>
            </a>

           </root>";

      var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

      Aver.AreEqual(75, conf.Root.Navigate("a/b/c").ValueAsInt());

      //this should throw
      Aver.AreEqual(false, conf.Root.Navigate("!a/b/c/d").Exists);

    }

    [Run]
    public void ModifiedFlag()
    {
      var xml = @"<root>

            <a>
              <b cool=""true"" snake=""false""> <c> 75 </c> </b>
            </a>

           </root>";

      var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

      Aver.IsFalse(conf.Root.Modified);

      conf.Root.Navigate("a/b/c").Value = "a";

      Aver.IsTrue(conf.Root.Modified);
      conf.Root.ResetModified();
      Aver.IsFalse(conf.Root.Modified);

      conf.Root.NavigateSection("a/b").AddAttributeNode("a", true);
      Aver.IsTrue(conf.Root.Modified);
      conf.Root.Navigate("a/b").ResetModified();
      Aver.IsFalse(conf.Root.Modified);

    }

    [Run]
    public void LoadMixedContent()
    {
      var xml = @"<root>GAGARIN<a> <!-- comment -->
              <b cool=""true""> <c> 75 </c> </b>
            </a>
            <web.world>who knows?</web.world>
           </root>";

      var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

      Aver.AreEqual("GAGARIN", conf.Root.Value);
      Aver.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
      Aver.AreEqual(75, conf.Root["a"]["b"]["c"].ValueAsInt());
      Aver.AreEqual(-123, conf.Root["a"]["dont exist"]["c"].ValueAsInt(-123));

      Aver.AreEqual("who knows?", conf.Root["web.world"].ValueAsString());
    }



  }
}
