/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class LoadAndNavigationTests
  {

    [Run]
    public void Memory_Create_Modify()
    {
      var cfg = new MemoryConfiguration();
      Aver.IsFalse(cfg.Root.Exists, "root does not exist on newly create conf");
      Aver.IsFalse(cfg.IsReadOnly, "newly created config is not read only");

      cfg.Create("meduza");
      Aver.IsTrue(cfg.Root.Exists, "root.Exists after create");
      Aver.AreEqual("meduza", cfg.Root.Name, "root name is set per create");

      Aver.IsNull(cfg.Root.Value, "empty node value is null");
      cfg.Root.Value = "abc";
      Aver.AreEqual("abc", cfg.Root.Value, "value not is gotten");

      Aver.IsTrue(cfg.Root.Modified, "root is modified after value set");
      cfg.Root.ResetModified();
      Aver.IsFalse(cfg.Root.Modified, "root is not modified after resetModified()");
    }

    [Run]
    public void Memory_Set_Get_Values()
    {
      var cfg = new MemoryConfiguration();

      cfg.Create();
      cfg.Root.AddAttributeNode("a", "1");
      cfg.Root.AddAttributeNode("b").Value = "2";

      Aver.AreEqual("1", cfg.Root.AttrByName("a").Value);
      Aver.AreEqual("1", cfg.Root.AttrByName("a").VerbatimValue);

      Aver.AreEqual(1, cfg.Root.AttrByName("a").ValueAsInt());
      Aver.AreEqual(true, cfg.Root.AttrByName("a").ValueAsBool());

      Aver.AreEqual(1f, cfg.Root.AttrByName("a").ValueAsFloat());
      Aver.AreEqual(1d, cfg.Root.AttrByName("a").ValueAsDouble());
      Aver.AreEqual(1m, cfg.Root.AttrByName("a").ValueAsDecimal());
      Aver.AreEqual((byte)1, cfg.Root.AttrByName("a").ValueAsByte());
      Aver.AreEqual((sbyte)1, cfg.Root.AttrByName("a").ValueAsSByte());
      Aver.AreEqual(1L, cfg.Root.AttrByName("a").ValueAsLong());
      Aver.AreEqual(1UL, cfg.Root.AttrByName("a").ValueAsULong());
      Aver.AreEqual((short)1, cfg.Root.AttrByName("a").ValueAsShort());
      Aver.AreEqual((ushort)1, cfg.Root.AttrByName("a").ValueAsUShort());

      Aver.AreEqual(0, cfg.Root.AttrByName("dont exist").ValueAsInt());
      Aver.AreEqual(-100, cfg.Root.AttrByName("dont exist").ValueAsInt(-100));

      Aver.AreEqual("2", cfg.Root.AttrByName("b").Value);
      Aver.AreEqual("2", cfg.Root.AttrByName("b").VerbatimValue);

      Aver.AreEqual(null, cfg.Root.AttrByName("c").Value);
      Aver.AreEqual(null, cfg.Root.AttrByName("c").VerbatimValue);

      cfg.Root.AttrByName("c", autoCreate: true).Value = "$(/$b)-$(/$a)";//<-- auto create attribute
      Aver.AreEqual("2-1", cfg.Root.AttrByName("c").Value);
      Aver.AreEqual("$(/$b)-$(/$a)", cfg.Root.AttrByName("c").VerbatimValue);
    }

    [Run]
    public void Laconic_Navigation()
    {
      var cfg = @"root{
a=1
b=2
 sub-1{
   a=100
   b=true
   sub-1-1=12345{ a=1{v=1} a=2{v=2} a=3{v=3} a=800{message=kuku}}
 }
}".AsLaconicConfig();

      Aver.AreEqual("root", cfg.Name);
      Aver.IsTrue(cfg["sub-1"].Exists);
      Aver.IsFalse(cfg["absent"].Exists);
      Aver.IsTrue(cfg["absent", "sub-1"].Exists);
      Aver.AreEqual("sub-1", cfg["sub-1"].Name);
      Aver.AreEqual("sub-1", cfg["pizza", "absent", "sub-1"].Name);
      Aver.AreEqual("sub-1", cfg["pizza", "sub-1", "absent"].Name);

      Aver.AreEqual("sub-1-1", cfg["pizza", "sub-1", "absent"]["none", "sub-1-1"].Name);

      Aver.IsFalse(cfg.Navigate("/absent").Exists);
      Aver.Throws<ConfigException>(() => cfg.Navigate("!/absent"));//! denotes requires path
      Aver.IsTrue(cfg.Navigate("/").Exists);
      Aver.AreEqual("root", cfg.Navigate("/").Name);

      Aver.AreEqual("root", cfg.Navigate("/").Name);

      Aver.IsTrue(cfg.Navigate("/absent;/").Exists);// /absent OR /
      Aver.IsTrue(cfg.Navigate("/absent|/").Exists);// /absent OR /

      Aver.IsTrue(cfg.Navigate("/absent ; /").Exists);// /absent OR /
      Aver.IsTrue(cfg.Navigate("/absent | /").Exists);// /absent OR /


      Aver.AreEqual("sub-1", cfg.Navigate("/sub-1").Name);
      Aver.AreEqual("sub-1", cfg.Navigate(" /sub-1").Name);
      Aver.AreEqual("sub-1", cfg.Navigate("/          sub-1").Name);
      Aver.IsFalse(cfg.Navigate("/   sub-        1").Exists);


      Aver.AreEqual("a", cfg.Navigate("/sub-1/sub-1-1/a").Name);
      Aver.AreEqual("1", cfg.Navigate("/sub-1/sub-1-1/a").Value);
      Aver.AreEqual("a", cfg.Navigate("/sub-1/sub-1-1/[1]").Name);
      Aver.AreEqual("2", cfg.Navigate("/sub-1/sub-1-1/[1]").Value);
      Aver.AreEqual("a", cfg.Navigate("/sub-1/sub-1-1/[2]").Name);
      Aver.AreEqual("3", cfg.Navigate("/sub-1/sub-1-1/[2]").Value);

      Aver.AreEqual("3", cfg.Navigate("/sub-1/sub-1-1/a[3]").Value);//a section with value 3
      Aver.AreEqual("800", cfg.Navigate("/sub-1/sub-1-1/a[message=kuku]").Value);//a section with attribute message="kuku"

      Aver.AreEqual("12345", cfg.Navigate("/sub-z;/sub-1/hhh;/sub-1/sub-1-1;/").Value);//coalescing


      Aver.AreEqual("2", cfg.Navigate("/sub-1/sub-1-1/a/../ .. /../$b").Value);//../$attribute
    }

    [Run]
    public void Laconic_Vars()
    {
      var cfg = @"root
{
v1=$(sub-1/$b)
v2=$(sub-1/$c)
v3=$(sub-1/[0]/a[800]/$message)

v4=$($v44)
v44=$(sub-1/$bad)

v5=$(/$absent|$none|$v3|$never-existed)

a=1
b=2
 sub-1{
   a=100
   b=$(/$a) //1
   c=$($a)$($b) //1001
   sub-1-1=12345{ a=1{v=1} a=2{v=2} a=3{v=3} a=800{message=kuku}}

   bad=$(/$v44)
 }
}".AsLaconicConfig();

      Aver.AreEqual("1", cfg.AttrByName("v1").Value);
      Aver.AreEqual("1001", cfg.AttrByName("v2").Value);
      Aver.AreEqual("kuku", cfg.AttrByName("v3").Value);

      Aver.Throws<ConfigException>(() => { var hang = cfg.AttrByName("v4").Value; });
      Aver.AreEqual("$($v44)", cfg.AttrByName("v4").VerbatimValue);//does not throw

      Aver.AreEqual("kuku", cfg.AttrByName("v5").Value);

    }
  }
}