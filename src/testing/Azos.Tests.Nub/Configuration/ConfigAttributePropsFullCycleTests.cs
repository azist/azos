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
  public class ConfigAttributePropsFullCycleTests
  {
    class _person : IConfigurable, IConfigurationPersistent
    {
      [Config] public string   Name { get; set; }
      [Config] public string   Description { get; set; }
      [Config] public DateTime  BirthUtc { get; set; }
      [Config(NoIso8601 = true)] public DateTime BirthRegular { get; set; }
      [Config] public DateTime? RegistrationUtc { get; set; }
      [Config] public decimal Income { get; set; }
      [Config] public decimal? AnotherIncome { get; set; }
      public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);
      public ConfigSectionNode PersistConfiguration(ConfigSectionNode parentNode, string name) => ConfigAttribute.BuildNode(this, parentNode, name);
    }

    [Run]
    public void PopulateSaveLoadCheck()
    {
      var dobUtc = new DateTime(1980, 07, 18, 14, 23, 07, DateTimeKind.Utc);
      var dobLcl = dobUtc.ToLocalTime();

      var person = new _person()
      {
        Name = "Zorro Morra",
        Description   = "Regular turtle",
        BirthUtc        = dobUtc,
        BirthRegular    = dobLcl,
        RegistrationUtc = new DateTime(1990, 08, 18, 14, 23, 07, DateTimeKind.Utc),
        Income = 110_000M,
        AnotherIncome = null
      };

      var cfg = Azos.Conf.Configuration.NewEmptyRoot();
      var got = person.PersistConfiguration(cfg, "person");
      cfg.See();

      var person2 = FactoryUtils.MakeAndConfigure<_person>(got, typeof(_person));
      person2.See();
      Aver.AreEqual(1980, person2.BirthUtc.Year);
      Aver.AreEqual(07, person2.BirthUtc.Month);
      Aver.AreEqual(18, person2.BirthUtc.Day);
      Aver.AreEqual(14, person2.BirthUtc.Hour);
      Aver.AreEqual(23, person2.BirthUtc.Minute);
      Aver.IsTrue(person2.BirthUtc.Kind == DateTimeKind.Utc);

      var lcl = DateTime.SpecifyKind(person2.BirthRegular, DateTimeKind.Local).ToUniversalTime();
      lcl.See();

      Aver.AreEqual(1980, lcl.Year);
      Aver.AreEqual(07,   lcl.Month);
      Aver.AreEqual(18,   lcl.Day);
      Aver.AreEqual(14,   lcl.Hour);
      Aver.AreEqual(23,   lcl.Minute);
      Aver.IsTrue(lcl.Kind == DateTimeKind.Utc);
    }


  }
}
