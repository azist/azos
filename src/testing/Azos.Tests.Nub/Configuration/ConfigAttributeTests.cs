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
      [Config] private int m_A;//a private field
      public int A => m_A;
      [Config] public bool B { get; set; }//a prop

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

    public class EntityC
    {
      [Config] private string m_Private;
      [Config] protected string m_Protected;
      [Config] public string m_Public;

      public string Private => m_Private;
      public string Protected => m_Protected;
      public string Public => m_Public;

      [Config] private string PrivateProp { get; set; }
      [Config] protected string ProtectedProp { get; set; }
      [Config] public string PublicProp { get; set; }

      public string GetPrivateProp() => PrivateProp;
      public string GetProtectedProp() => ProtectedProp;
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

    [Run]
    public void ApplyToFieldWithDifferentAccessModifiers()
    {
      var cfg = "private=pvt protected=prot public=pub private-prop=pvt2 protected-prop=prot2 public-prop=pub2".AsLaconicConfig();
      var obj = new EntityC();
      ConfigAttribute.Apply(obj, cfg);

      obj.See();

      Aver.AreEqual("pvt", obj.Private);
      Aver.AreEqual("prot", obj.Protected);
      Aver.AreEqual("pub", obj.Public);

      Aver.AreEqual("pvt2", obj.GetPrivateProp());
      Aver.AreEqual("prot2", obj.GetProtectedProp());
      Aver.AreEqual("pub2", obj.PublicProp);
    }

    public class EntityZ
    {
      [Config] public TZaza Zaza1;
      [Config] public TZaza Zaza2{get; set;}
      [Config] public string Another;
    }

    public struct TZaza
    {
      public TZaza(string msg) => Msg = msg;

      [ConfigCtor]  public TZaza(IConfigSectionNode cs) => Msg = cs.ValOf("msg");

      [ConfigCtor]  public TZaza(IConfigAttrNode ca) => Msg = ca.Value;

      public readonly string Msg;
    }

    [Run]
    public void CustomTypeWithConfigCtor()
    {
      var cfg = "a{zaza1='message1' Another='cool' zaza2{msg='2message'}}".AsLaconicConfig();
      var obj = new EntityZ();
      ConfigAttribute.Apply(obj, cfg);

      obj.See();

      Aver.AreEqual("message1", obj.Zaza1.Msg);
      Aver.AreEqual("cool", obj.Another);
      Aver.AreEqual("2message", obj.Zaza2.Msg);
    }


  }
}