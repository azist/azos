/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class CustomMetadataTests_MethodAttributes_Conf
  {

    public class Car
    {
      [CustomMetadata("a=123 b=789 score=100 description='Generic car' origin{_override=all country=world} z=0")]
      public virtual void Draw() {  }
    }

    public class AmericanCar : Car
    {
      [CustomMetadata("score=75 description='Cars built in the US' origin{_override=stop country=usa}")]
      public override void Draw() { }
    }

    public class Buick : AmericanCar
    {
      [CustomMetadata("score=90 description='Very usable and decent quality' a=-900")]
      public override void Draw() { }
    }

    public class Cadillac : AmericanCar
    {
      [CustomMetadata("score=40 description='Luxury item, but unreliable'  origin{country=XYZYZ/*this will never take effect*/}")]
      public override void Draw() { }
    }

    public class JapaneseCar : Car
    {
     [CustomMetadata("score=110 description='Cars built in Japan' origin{_override=stop country=jap} z=1")]
      public override void Draw() { }
    }

    public class Honda : JapaneseCar
    {
      [CustomMetadata("description='Honda motors'")]
      public override void Draw() { }
    }

    public class Toyota : JapaneseCar
    {
      [CustomMetadata("description='Toyota motors' b=-1 score=137 z=7")]
      public override void Draw() { }
    }

    //no attribute
    public class EuropeanCar : Car
    {
     //nothing to override
    }

    public class BMW : EuropeanCar
    {
      [CustomMetadata("description='Bavarian Motor Works' z=190")]
      public override void Draw() { }
    }



    [Run]
    public void Car_1()
    {
      var data = Conf.Configuration.NewEmptyRoot();
      CustomMetadataAttribute.Apply(typeof(Car).GetMethod("Draw"), this, data);

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
      CustomMetadataAttribute.Apply(typeof(AmericanCar).GetMethod("Draw"), this, data);

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
      CustomMetadataAttribute.Apply(typeof(Buick).GetMethod("Draw"), this, data);

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
      CustomMetadataAttribute.Apply(typeof(Cadillac).GetMethod("Draw"), this, data);

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
      CustomMetadataAttribute.Apply(typeof(Honda).GetMethod("Draw"), this, data);

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
      CustomMetadataAttribute.Apply(typeof(Toyota).GetMethod("Draw"), this, data);

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
      CustomMetadataAttribute.Apply(typeof(EuropeanCar).GetMethod("Draw"), this, data);

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
      CustomMetadataAttribute.Apply(typeof(BMW).GetMethod("Draw"), this, data);

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