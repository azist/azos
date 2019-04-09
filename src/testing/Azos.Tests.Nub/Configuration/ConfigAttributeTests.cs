/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Scripting;
using System;

namespace Azos.Tests.Nub.Configuration
{
#pragma warning disable 649

  [Runnable]
  public class ConfigAttributesTests
  {
    public static readonly IConfigSectionNode CONF = @"

root
{
  entity-a{type='Azos.Tests.Nub.Configuration.ConfigAttributesTests+EntityA, Azos.Tests.Nub' a=1 b=true}
  entity-b{a=2 b=false} //without type spec

  sub
  {
    section
    {
      a = 890
      b = -800.80123
      d = ""March 15, 1980""
      client
      {
        name='client1'
      }
      another{
        server
        {
          name='server1'
        }
      }
    }
  }
}
      ".AsLaconicConfig();

    public class EntityA : IConfigurable
    {
      [Config]private int m_A;//a private field
      public int A => m_A;
      [Config]public bool B{ get; set;}//a prop

      public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);
    }

    public class EntityB : IConfigurable
    {
      [Config("client")] IConfigSectionNode m_Client;
      public IConfigSectionNode Client => m_Client;
      [Config("another/server")] public IConfigSectionNode Server { get; set; }
      [Config] public int A { get; set; }
      [Config] public float B { get; set; }
      [Config] public DateTime D { get; set; }

      public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);
    }


    [Run]
    public void CreateWithType()
    {
      var entity = FactoryUtils.MakeAndConfigure<EntityA>(CONF["entity-a"]);
      Aver.AreEqual(1, entity.A);
      Aver.AreEqual(true, entity.B);
    }

    [Run]
    public void CreateWithoutType()
    {
      var entity = FactoryUtils.MakeAndConfigure<EntityA>(CONF["entity-b"], typeof(EntityA));
      Aver.AreEqual(2, entity.A);
      Aver.AreEqual(false, entity.B);
    }

    [Run]
    public void CreateComplex()
    {
      var entity = FactoryUtils.MakeAndConfigure<EntityB>(CONF["sub"]["section"], typeof(EntityB));
      Aver.AreEqual(890, entity.A);
      Aver.AreEqual(-800.80123f, entity.B);
      Aver.AreEqual(15, entity.D.Day);
      Aver.AreEqual(3, entity.D.Month);
      Aver.AreEqual(1980, entity.D.Year);

      Aver.IsNotNull(entity.Client);
      Aver.IsNotNull(entity.Server);

      Aver.AreEqual("client1", entity.Client.AttrByName("name").Value);
      Aver.AreEqual("server1", entity.Server.AttrByName("name").Value);
    }
  }
#pragma warning restore 649
}