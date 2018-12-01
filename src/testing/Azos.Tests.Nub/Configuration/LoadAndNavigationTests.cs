/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
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
      Aver.IsFalse( cfg.Root.Exists, "root does not exist on newly create conf" );
      Aver.IsFalse( cfg.IsReadOnly, "newly created config is not read only" );

      cfg.Create("meduza");
      Aver.IsTrue( cfg.Root.Exists , "root.Exists after create");
      Aver.AreEqual("meduza", cfg.Root.Name, "root name is set per create");

      Aver.IsNull( cfg.Root.Value , "empty node value is null" );
      cfg.Root.Value = "abc";
      Aver.AreEqual("abc", cfg.Root.Value, "value not is gotten");

      Aver.IsTrue( cfg.Root.Modified, "root is modified after value set");
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



  }
}