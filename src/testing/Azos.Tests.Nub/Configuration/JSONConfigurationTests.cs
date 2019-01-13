/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


using Azos.Conf;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Configuration
{
    [Runnable]
    public class JSONConfigurationTests
    {
        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void ReadOnlyErrorOnNodeCreate()
        {
           var conf = new Azos.Conf.JSONConfiguration();
           conf.Create();

           conf.SetReadOnly(true);

           conf.Root.AddChildNode("A", null);
        }

        [Run]
        public void NodeCreate()
        {
           var conf = new Azos.Conf.JSONConfiguration();
           conf.Create();
           conf.Root.AddChildNode("A", null);

           Aver.AreEqual("A", conf.Root["A"].Name);
        }


        [Run]
        public void EmptySectionAndAttributeNodes()
        {
           var conf = new Azos.Conf.JSONConfiguration();
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
           var conf = new Azos.Conf.JSONConfiguration();
           conf.Create();
           conf.Root.AddChildNode("A", null);
           conf.SetReadOnly(true);
           conf.Root.Name = "changed-name";
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void ReadOnlyErrorOnNodeDelete()
        {
           var conf = new Azos.Conf.JSONConfiguration();
           conf.Create();
           conf.Root.AddChildNode("A", null);
           conf.SetReadOnly(true);
           conf.Root["A"].Delete();
        }

        [Run]
        public void NodeDelete()
        {
           var conf = new Azos.Conf.JSONConfiguration();
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
           var conf = new Azos.Conf.JSONConfiguration();
           conf.Create();
           conf.Root.AddChildNode("A", null);

           Aver.AreEqual(true, conf.Root.Exists);
           conf.Root.Delete();
           Aver.AreEqual(false, conf.Root.Exists);
        }


        [Run]
        public void NodeRename()
        {
           var conf = new Azos.Conf.JSONConfiguration();
           conf.Create();
           conf.Root.AddChildNode("A", null);
           conf.Root["A"].Name = "B";
           Aver.AreEqual("B", conf.Root["B"].Name);
        }

        [Run]
        public void NavigationAndValueAccessors()
        {
           var conf = new Azos.Conf.JSONConfiguration();
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
           var json = @"
           {
            root:
            { 'kind': 'Absolute',
              a:
              {
                 b:{'cool': true, c: 75 }
              },
            'web.world': 'who knows?'
           }}";

           var conf = Azos.Conf.JSONConfiguration.CreateFromJSON(json);

           Aver.IsTrue(UriKind.Absolute == conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
           Aver.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
           Aver.AreEqual(75, conf.Root["a"]["b"].AttrByName("c").ValueAsInt());
           Aver.AreEqual(-123, conf.Root["a"]["dont exist"].AttrByName("c").ValueAsInt(-123));

           Aver.AreEqual("who knows?", conf.Root.AttrByName("web.world").ValueAsString());

           var savedJSON = conf.SaveToString(JSONWritingOptions.PrettyPrint);

           Console.WriteLine(savedJSON);

           //retest after configuration was saved and then reloaded from string
           conf =  Azos.Conf.JSONConfiguration.CreateFromJSON(savedJSON);
           Aver.IsTrue(UriKind.Absolute == conf.Root.AttrByName("kind").ValueAsEnum<UriKind>(UriKind.Relative));
           Aver.AreEqual(true, conf.Root["a"]["b"].AttrByName("cool").ValueAsBool(false));
           Aver.AreEqual(75, conf.Root["a"]["b"].AttrByName("c").ValueAsInt());
           Aver.AreEqual("who knows?", conf.Root.AttrByName("web.world").ValueAsString());
        }


        [Run]
        public void SectionValue()
        {
          var json = @"{
           root: {
                  '-section-value': 123,
                  a: {'-section-value': 237}

           }}";

           var conf = Azos.Conf.JSONConfiguration.CreateFromJSON(json);

           Console.WriteLine(conf.SaveToString(JSONWritingOptions.PrettyPrint));



           Aver.AreEqual(123, conf.Root.ValueAsInt());
           Aver.AreEqual(237, conf.Root["a"].ValueAsInt());
        }


        [Run]
        public void ModifiedFlag()
        {
          var json = @"{
           root: {

            a: {
               b: {cool: true, snake: false, c: {'-section-value': 75}}
            }

           }}";

           var conf = Azos.Conf.JSONConfiguration.CreateFromJSON(json);

           Aver.IsFalse(conf.Root.Modified);

           Aver.AreEqual("75", conf.Root.Navigate("a/b/c").Value);
           conf.Root.Navigate("a/b/c").Value = "a";
           Aver.AreEqual("a", conf.Root.Navigate("a/b/c").Value);

           Aver.IsTrue(conf.Root.Modified);
           conf.Root.ResetModified();
           Aver.IsFalse(conf.Root.Modified);

           conf.Root.NavigateSection("a/b").AddAttributeNode("a", true);
           Aver.IsTrue(conf.Root.Modified);
           conf.Root.Navigate("a/b").ResetModified();
           Aver.IsFalse(conf.Root.Modified);
        }

        [Run]
        public void LaconicJSONRoundtrip()
        {
          var conf1 = @"
            tezt-root='Hahaha\nha!'
            {
              types
              {
                _override=stop
                t1='Type1'
                t2='Type2'
              }
              providers
              {
                provider1
                {
                  type=$(/types/$t1)
                  name='Zhaba'
                }
                provider2
                {
                  type=$(/types/$t2)
                  name='Koshka'

                  //notice sub-sections with the same name
                  set{ a=123 b=true c=11{da=net}}
                  set{ a=5623 b=false}
                  set{ a=78 b=true}
                }
              }
            }".AsLaconicConfig();

           assert(conf1);
           Console.WriteLine(conf1.ToLaconicString());

           var map = conf1.Configuration.ToConfigurationJSONDataMap();
           var json = map.ToJSON();

           var cjson = JSONConfiguration.CreateFromJSON(json);
           assert(cjson.Root);

           json = cjson.SaveToString(JSONWritingOptions.PrettyPrint);
           Console.WriteLine(json);
           cjson = JSONConfiguration.CreateFromJSON(json);
           assert(cjson.Root);
        }

        private void assert(ConfigSectionNode root)
        {
          Aver.AreEqual("tezt-root", root.Name);
          Aver.AreEqual("Hahaha\nha!", root.Value);

          Aver.AreEqual("Type1", root.Navigate("types/$t1").Value);
          Aver.AreEqual("Type2", root.Navigate("types/$t2").Value);

          Aver.AreEqual("Type1", root.Navigate("providers/provider1/$type").Value);
          Aver.AreEqual("Type2", root.Navigate("providers/provider2/$type").Value);
          Aver.IsFalse(root.Navigate("providers/provider3").Exists);

          Aver.AreEqual(3, root["providers"]["provider2"].ChildCount);
          Aver.AreEqual("Koshka", root.Navigate("providers/provider2/$name").Value);

          Aver.AreEqual("net", root.Navigate("providers/provider2/[0]/c/$da").Value);
          Aver.AreEqual(5623, root.Navigate("providers/provider2/[1]/$a").ValueAsInt());
          Aver.IsFalse(root.Navigate("providers/provider2/[1]/$b").ValueAsBool());
          Aver.AreEqual(78, root.Navigate("providers/provider2/[2]/$a").ValueAsInt());
          Aver.IsTrue(root.Navigate("providers/provider2/[2]/$b").ValueAsBool());
        }


    }
}
