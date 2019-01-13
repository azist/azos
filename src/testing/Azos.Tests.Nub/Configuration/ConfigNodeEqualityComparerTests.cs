/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;

using Azos.Conf;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class ConfigNodeEqualityComparerTests
  {
    [Run]
    public void JustRoot()
    {
      var root1 = Azos.Conf.LaconicConfiguration.CreateFromString("root{}").Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(null, null));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(null, root1));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, null));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));

      var root2 = Azos.Conf.LaconicConfiguration.CreateFromString("rOOt { }").Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      root2.Name = "new_root";
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));
    }

    [Run]
    public void RootWithValue()
    {
      var root1 = Azos.Conf.LaconicConfiguration.CreateFromString("root='people' {}").Root;
      var root2 = Azos.Conf.LaconicConfiguration.CreateFromString("root=people {}").Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      var root3 = Azos.Conf.LaconicConfiguration.CreateFromString("root='PeoPle' {}").Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root3));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root3, root1));
    }

    [Run]
    public void WithoutName()
    {
      var root1 = Azos.Conf.LaconicConfiguration.CreateFromString("root{}").Root;
      var root2 = Azos.Conf.LaconicConfiguration.CreateFromString("root{}").Root;

      var n1 = root1.AddChildNode("");
      n1.AddChildNode("details");
      var n2 = root2.AddChildNode("");
      n2.AddChildNode("details");
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
    }

    [Run]
    public void DifferentTypes()
    {
      var root1 = Azos.Conf.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root as IConfigNode;
      var root2 = Azos.Conf.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root as IConfigSectionNode;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      var root3 = Azos.Conf.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root as IConfigSectionNode;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root3));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root2));
    }

    [Run]
    public void Exists()
    {
      var root1 = Azos.Conf.LaconicConfiguration.CreateFromString("details { name = 'John Smith'}").Root;
      var root2 = Azos.Conf.LaconicConfiguration.CreateFromString("details {}").Root;

      var node1 = root1.AttrByName("name");
      var node2 = root2.AttrByName("name");
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node1, node1));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node2, node2));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(node1, node2));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(node2, node1));

      var node3 = root2.AttrByName("name");
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node3, node3));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node2, node3));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(node3, node2));
    }

    [Run]
    public void AttributesCountAndOrder()
    {
      string conf1 =
        @"root
          {
            type = 'person'
            has_drive_license = 0
            details {}
          }";
      string conf2 =
        @"root
          {
            type = 'person'
            details {}
            has_drive_license = 0
          }";
      string conf3 =
        @"root
          {
            type = 'person'
            details {}
          }";
      string conf4 =
        @"root
          {
            has_drive_license = 0
            type = 'person'
            details {}
          }";

      var root1 = Azos.Conf.LaconicConfiguration.CreateFromString(conf1).Root;
      var root2 = Azos.Conf.LaconicConfiguration.CreateFromString(conf2).Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      // count
      var root3 = Azos.Conf.LaconicConfiguration.CreateFromString(conf3).Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root3));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root3, root1));

      // order
      var root4 = Azos.Conf.LaconicConfiguration.CreateFromString(conf4).Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root4, root4));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root4));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root4, root1));
    }

    [Run]
    public void SectionsCountAndOrder()
    {
      string conf1 =
        @"root
          {
            type = 'person'
            details {}
            contacts {}
          }";
      string conf2 =
        @"root
          {
            details {}
            type = 'person'
            contacts {}
          }";
      string conf3 =
        @"root
          {
            details {}
            type = 'person'
          }";
      string conf4 =
        @"root
          {
            contacts {}
            type = 'person'
            details {}
          }";

      var root1 = Azos.Conf.LaconicConfiguration.CreateFromString(conf1).Root;
      var root2 = Azos.Conf.LaconicConfiguration.CreateFromString(conf2).Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));

      // count
      var root3 = Azos.Conf.LaconicConfiguration.CreateFromString(conf3).Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root3, root3));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root3));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root3, root1));

      // order
      var root4 = Azos.Conf.LaconicConfiguration.CreateFromString(conf4).Root;
      Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root4, root4));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root1, root4));
      Aver.IsFalse(ConfigNodeEqualityComparer.Instance.Equals(root4, root1));
    }

    [Run]
    public void Overall()
    {
      string conf1 =
        @"root
        {
          type = person
          details
          {
            name = 'John Smith'
            age = 22
          }
          contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          has-drive-license = 1
        }";
      string conf2 =
        @"Root
        {
          Details
          {
            name = 'John Smith'
            AGE = 22
          }
          Contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          Type = person
          has-drive-license = 1
        }";
        var root1 = Azos.Conf.LaconicConfiguration.CreateFromString(conf1).Root;
        var root2 = Azos.Conf.LaconicConfiguration.CreateFromString(conf2).Root;
        Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root1));
        Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root2));
        Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root1, root2));
        Aver.IsTrue(ConfigNodeEqualityComparer.Instance.Equals(root2, root1));
    }

    [Run]
    public void HashCode()
    {
      string conf1 =
        @"root
        {
          type = person
          details
          {
            name = 'John Smith'
            age = 22
          }
          contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          has-drive-license = 1
        }";
      string conf2 =
        @"Root
        {
          Details
          {
            name = 'John Smith'
            AGE = '22'
          }
          Contacts
          {
            phone=1111 { mobile='' }
            address {}
          }
          Type = person
          has-drive-license = 1
        }";
        var root1 = Azos.Conf.LaconicConfiguration.CreateFromString(conf1).Root;
        var root2 = Azos.Conf.LaconicConfiguration.CreateFromString(conf2).Root;
        var hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(root1);
        var hash2 = ConfigNodeEqualityComparer.Instance.GetHashCode(root2);
        Aver.AreEqual(hash1, hash2);

        var hashOfNull = ConfigNodeEqualityComparer.Instance.GetHashCode(null);
        Aver.AreEqual(hashOfNull, hashOfNull);

        root1 = Azos.Conf.LaconicConfiguration.CreateFromString("root { }").Root;
        root2 = Azos.Conf.LaconicConfiguration.CreateFromString("rOOt { }").Root;
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(root1);
        hash2 = ConfigNodeEqualityComparer.Instance.GetHashCode(root2);
        Aver.AreEqual(hash1, hash2);

        root2 = Azos.Conf.LaconicConfiguration.CreateFromString("root=people { }").Root;
        hash2 = ConfigNodeEqualityComparer.Instance.GetHashCode(root2);
        Aver.AreNotEqual(hash1, hash2);

        var sectionNode = root1["details"];
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(sectionNode);
        Aver.AreEqual(hash1, hashOfNull);

        var attrNode = root1.AttrByIndex(0);
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(attrNode);
        Aver.AreEqual(hash1, hashOfNull);

        attrNode = root1.AttrByName("type");
        hash1 = ConfigNodeEqualityComparer.Instance.GetHashCode(attrNode);
        Aver.AreEqual(hash1, hashOfNull);
    }
  }
}
