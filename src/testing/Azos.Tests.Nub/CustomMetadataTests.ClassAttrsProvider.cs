/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Reflection;
using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class CustomMetadataTests_ClassAttributes_MetadataProvider
  {

    [CustomMetadata(typeof(CarMetadataProvider))]
    public class Car { }
    public class CarMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "a=123 b=789 score=100 description='Generic car' origin{_override=all country=world} z=0".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }

    [CustomMetadata(typeof(AmericanCarMetadataProvider))]
    public class AmericanCar : Car { }
    public class AmericanCarMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "score=75 description='Cars built in the US' origin{_override=stop country=usa}".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }

    [CustomMetadata(typeof(BuickMetadataProvider))]
    public class Buick : AmericanCar { }
    public class BuickMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "score=90 description='Very usable and decent quality' a=-900".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }

    [CustomMetadata(typeof(CadillacMetadataProvider))]
    public class Cadillac : AmericanCar { }
    public class CadillacMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "score=40 description='Luxury item, but unreliable'  origin{country=XYZYZ/*this will never take effect*/}".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }

    [CustomMetadata(typeof(JapaneseCarMetadataProvider))]
    public class JapaneseCar : Car { }
    public class JapaneseCarMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "score=110 description='Cars built in Japan' origin{_override=stop country=jap} z=1".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }

    [CustomMetadata(typeof(HondaMetadataProvider))]
    public class Honda : JapaneseCar { }
    public class HondaMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "description='Honda motors'".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }

    [CustomMetadata(typeof(ToyotaMetadataProvider))]
    public class Toyota : JapaneseCar { }
    public class ToyotaMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "description='Toyota motors' b=-1 score=137 z=7".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }

    //no attribute
    public class EuropeanCar : Car { }


    [CustomMetadata(typeof(BMWMetadataProvider))]
    public class BMW : EuropeanCar { }
    public class BMWMetadataProvider : CustomMetadataProvider
    {
      public override ConfigSectionNode ProvideMetadata(MemberInfo member, object instance, object context, ConfigSectionNode dataRoot, NodeOverrideRules overrideRules = null)
      {
        var data = "description='Bavarian Motor Works' z=190".AsLaconicConfig();
        dataRoot.MergeAttributes(data);
        dataRoot.MergeSections(data);
        return dataRoot;
      }
    }



    [Run]
    public void Car_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(Car), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(123, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(789, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(100, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(0, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Generic car", data.AttrByName("description").Value);
      Aver.AreEqual("world", data.Navigate("origin/$country").Value);
    }

    [Run]
    public void AmericanCar_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(AmericanCar), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(123, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(789, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(75, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(0, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Cars built in the US", data.AttrByName("description").Value);
      Aver.AreEqual("usa", data.Navigate("origin/$country").Value);
    }

    [Run]
    public void Buick_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(Buick), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(-900, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(789, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(90, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(0, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Very usable and decent quality", data.AttrByName("description").Value);
      Aver.AreEqual("usa", data.Navigate("origin/$country").Value);
    }

    [Run]
    public void Cadillac_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(Cadillac), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(123, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(789, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(40, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(0, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Luxury item, but unreliable", data.AttrByName("description").Value);
      Aver.AreEqual("usa", data.Navigate("origin/$country").Value); //in spite of XYZ set in config, the American-level override pragma set to stop
    }

    [Run]
    public void Honda_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(Honda), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(123, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(789, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(110, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(1, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Honda motors", data.AttrByName("description").Value);
      Aver.AreEqual("jap", data.Navigate("origin/$country").Value);
    }

    [Run]
    public void Toyota_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(Toyota), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(123, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(-1, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(137, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(7, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Toyota motors", data.AttrByName("description").Value);
      Aver.AreEqual("jap", data.Navigate("origin/$country").Value);
    }


    [Run]
    public void EuropeanCar_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(EuropeanCar), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(123, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(789, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(100, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(0, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Generic car", data.AttrByName("description").Value);
      Aver.AreEqual("world", data.Navigate("origin/$country").Value); //was not set on purpose, cloned from Car
    }

    [Run]
    public void BMW_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(BMW), null, this, data);

      Console.WriteLine(data.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint));

      Aver.AreEqual(123, data.AttrByName("a").ValueAsInt());
      Aver.AreEqual(789, data.AttrByName("b").ValueAsInt());
      Aver.AreEqual(100, data.AttrByName("score").ValueAsInt());
      Aver.AreEqual(190, data.AttrByName("z").ValueAsInt());
      Aver.AreEqual("Bavarian Motor Works", data.AttrByName("description").Value);
      Aver.AreEqual("world", data.Navigate("origin/$country").Value); //was not set on purpose, cloned from EuropeanCar/Car
    }

  }
}