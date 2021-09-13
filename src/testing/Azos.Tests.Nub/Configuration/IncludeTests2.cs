/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class IncludeTests2
  {
    [Run]
    public void Include()
    {
      var c1 = @"root{ a{} b{} }".AsLaconicConfig().Configuration;
      var c2 = @"root2{ z=900 zuza{ looza{x=100}} }".AsLaconicConfig().Configuration;


      c1.Include(c1.Root["b"], c2.Root);

      c1.Root.ToLaconicString().See();

      Aver.AreEqual(900, c1.Root.AttrByName("z").ValueAsInt());//got merged from root2
      Aver.IsTrue(c1.Root["a"].Exists);
      Aver.IsFalse(c1.Root["b"].Exists);
      Aver.IsTrue(c1.Root["zuza"].Exists);
      Aver.AreEqual(100, c1.Root.Navigate("zuza/looza/$x").ValueAsInt());
    }

    [Run]
    public void ProcessAppIncludes()
    {
      var cfg = @"app
      {
        process-includes=--include
        --include
        {
          name=SectionA
          provider{ type='Azos.Tests.Nub.Configuration.IncludeTests2+CustomProvider, Azos.Tests.Nub'}
        }

        --include
        {
          name=SectionB
          provider{ type='Azos.Tests.Nub.Configuration.IncludeTests2+CustomProvider, Azos.Tests.Nub'}
        }
      }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      //When injecting config from the .ctor the process includes is not applied automatically
      CommonApplicationLogic.ProcessAllExistingConfigurationIncludes(cfg);

      cfg.ToLaconicString().See();

      Aver.IsTrue(cfg["SectionA"].Exists);
      Aver.IsTrue(cfg["SectionB"].Exists);
      Aver.AreEqual(1, cfg["SectionA"].ValOf("a").AsInt());
      Aver.AreEqual(-2, cfg["SectionA"].ValOf("b").AsInt());

      Aver.AreEqual(1, cfg["SectionB"].ValOf("a").AsInt());
      Aver.AreEqual(-2, cfg["SectionB"].ValOf("b").AsInt());
    }

    public class CustomProvider : IConfigNodeProvider
    {
      public void Configure(IConfigSectionNode node) { }

      public ConfigSectionNode ProvideConfigNode(object context = null)
      {
        return @"root{ a=1 b=-2 }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      }

    }

    [Run]
    public void ProcessAppIncludeCopies()
    {
      var cfg = @"app
      {
        process-includes=--include

        sectionA
        {
          a=1
          b=2
        }

        --include
        {
          name=SectionZ
          copy=/sectionA
        }

        --include
        {
          copy=/sectionA
        }

        --include
        {
          copy=/dontexist-notrequired-soitworks
        }
      }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      //When injecting config from the .ctor the process includes is not applied automatically
      CommonApplicationLogic.ProcessAllExistingConfigurationIncludes(cfg);

      cfg.ToLaconicString().See();

      Aver.AreEqual(2, cfg.ChildCount);//sectionA nad SectionZ the next anonymous include goes under very root

      //copied over sectionA to root by the last un-named include
      Aver.AreEqual(3, cfg.AttrCount);
      Aver.AreEqual(1, cfg.ValOf("a").AsInt());
      Aver.AreEqual(2, cfg.ValOf("b").AsInt());

      Aver.AreEqual(2, cfg["SectionA"].AttrCount);
      Aver.AreEqual(1, cfg["SectionA"].ValOf("a").AsInt());
      Aver.AreEqual(2, cfg["SectionA"].ValOf("b").AsInt());

      Aver.AreEqual(2, cfg["SectionZ"].AttrCount);
      Aver.AreEqual(1, cfg["SectionZ"].ValOf("a").AsInt());
      Aver.AreEqual(2, cfg["SectionZ"].ValOf("b").AsInt());
    }


    [Run, Aver.Throws(typeof(ConfigException), "requires node but")]
    public void ProcessAppIncludeCopies_ReferenceRequired()
    {
      var cfg = @"app
      {
        process-includes=--include

        sectionA
        {
          a=1
          b=2
        }

        --include
        {
          name=SectionZ
          copy=!/sectionIsNotThere
        }
      }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      //When injecting config from the .ctor the process includes is not applied automatically
      CommonApplicationLogic.ProcessAllExistingConfigurationIncludes(cfg);
    }

    [Run, Aver.Throws(typeof(ConfigException), "recursive depth exceeded")]
    public void ProcessAppIncludeCopies_Recursion()
    {
      var cfg = @"app
      {
        process-includes=include

        include
        {
          name=include
          copy=!/include
        }
      }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      CommonApplicationLogic.ProcessAllExistingConfigurationIncludes(cfg);
    }

    [Run]
    public void ProcessAppIncludeCopies_Optional()
    {
      var cfg = @"app
      {
        process-includes=include

        include
        {
          name=a
          copy=/not-there
        }
      }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      CommonApplicationLogic.ProcessAllExistingConfigurationIncludes(cfg);

      cfg.ToLaconicString().See();

      Aver.AreEqual(0, cfg.ChildCount);
    }

    [Run, Aver.Throws(typeof(ConfigException), "did not land at an existing node")]
    public void ProcessAppIncludeCopies_Required()
    {
      var cfg = @"app
      {
        process-includes=include

        include
        {
          name=a
          copy=!/not-there
        }
      }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      CommonApplicationLogic.ProcessAllExistingConfigurationIncludes(cfg);
    }

  }
}