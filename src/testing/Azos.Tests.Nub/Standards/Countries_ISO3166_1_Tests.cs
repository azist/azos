/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;

using Azos.Scripting;
using Azos.Standards;

namespace Azos.Tests.Nub.Standards
{
  [Runnable]
  class Countries_ISO3166_1_Tests
  {
    [Run]
    public void ToAlpha2()
    {
      Aver.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.ToAlpha2("___")));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2("___", "US").EqualsOrdIgnoreCase("US"));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase("US"));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_RUSSIA).EqualsOrdIgnoreCase("RU"));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_GERMANY).EqualsOrdIgnoreCase("DE"));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_MEXICO).EqualsOrdIgnoreCase("MX"));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_CANADA).EqualsOrdIgnoreCase("CA"));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2(CoreConsts.ISO_COUNTRY_FRANCE).EqualsOrdIgnoreCase("FR"));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha2(null).EqualsOrdIgnoreCase(null));
    }
    [Run]
    public void ToAlpha3()
    {
      Aver.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.ToAlpha3("__")));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3("__", CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3("US").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3("RU").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_RUSSIA));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3("DE").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_GERMANY));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3("MX").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_MEXICO));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3("CA").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_CANADA));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3("FR").EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_FRANCE));
      Aver.IsTrue(Countries_ISO3166_1.ToAlpha3(null).EqualsOrdIgnoreCase(null));
    }
    [Run]
    public void Normalize2()
    {
      Aver.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.Normalize2("___")));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2("__", "US").EqualsOrdIgnoreCase("US"));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2("US").EqualsOrdIgnoreCase("US"));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2("RU").EqualsOrdIgnoreCase("RU"));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2("DE").EqualsOrdIgnoreCase("DE"));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2("MX").EqualsOrdIgnoreCase("MX"));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2("CA").EqualsOrdIgnoreCase("CA"));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2("FR").EqualsOrdIgnoreCase("FR"));
      Aver.IsTrue(Countries_ISO3166_1.Normalize2(null).EqualsOrdIgnoreCase(null));
    }
    [Run]
    public void Normalize3()
    {
      Aver.IsTrue(String.IsNullOrEmpty(Countries_ISO3166_1.Normalize3("__")));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3("___", CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_USA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_USA));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_RUSSIA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_RUSSIA));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_GERMANY).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_GERMANY));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_MEXICO).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_MEXICO));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_CANADA).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_CANADA));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3(CoreConsts.ISO_COUNTRY_FRANCE).EqualsOrdIgnoreCase(CoreConsts.ISO_COUNTRY_FRANCE));
      Aver.IsTrue(Countries_ISO3166_1.Normalize3(null).EqualsOrdIgnoreCase(null));
    }
  }
}
