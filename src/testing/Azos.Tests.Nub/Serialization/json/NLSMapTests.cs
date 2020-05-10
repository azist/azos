/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;

using Azos.Scripting;

using Azos.Serialization.JSON;
using Azos.Collections;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class NLSMapTests
  {
    [Run]
    public void NLSMap_Basic1_String()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}}";

      var nls = new NLSMap(content);

      Aver.IsTrue (nls["eng"].IsAssigned);
      Aver.IsFalse(nls["rus"].IsAssigned);

      Aver.AreEqual("Cucumber", nls["eng"].Name);
      Aver.AreEqual(null, nls["rus"].Name);

      Aver.AreEqual("It is green", nls["eng"].Description);
      Aver.AreEqual(null, nls["rus"].Description);

      Aver.AreEqual("Cucumber", nls[CoreConsts.ISOA_LANG_ENGLISH].Name);
      Aver.AreEqual(null, nls[CoreConsts.ISOA_LANG_RUSSIAN].Name);

      Aver.AreEqual("It is green", nls[CoreConsts.ISOA_LANG_ENGLISH].Description);
      Aver.AreEqual(null, nls[CoreConsts.ISOA_LANG_RUSSIAN].Description);
    }

    [Run]
    public void NLSMap_Basic1_Config()
    {
      var content="nls{ eng{n='Cucumber' d='It is green'}}".AsLaconicConfig();

      var nls = new NLSMap(content);

      Aver.IsTrue (nls["eng"].IsAssigned);
      Aver.IsFalse(nls["rus"].IsAssigned);

      Aver.AreEqual("Cucumber", nls["eng"].Name);
      Aver.AreEqual(null, nls["rus"].Name);

      Aver.AreEqual("It is green", nls["eng"].Description);
      Aver.AreEqual(null, nls["rus"].Description);
    }


    [Run]
    public void NLSMap_Basic2()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

      var nls = new NLSMap(content);

      Aver.IsTrue (nls["eng"].IsAssigned);
      Aver.IsTrue (nls["deu"].IsAssigned);
      Aver.IsFalse(nls["rus"].IsAssigned);

      Aver.AreEqual("Cucumber", nls["eng"].Name);
      Aver.AreEqual("Gurke", nls["deu"].Name);

      Aver.AreEqual("It is green", nls["eng"].Description);
      Aver.AreEqual("Es ist grün", nls["deu"].Description);

      Aver.AreEqual("Cucumber", nls[CoreConsts.ISOA_LANG_ENGLISH].Name);
      Aver.AreEqual("Gurke",    nls[CoreConsts.ISOA_LANG_GERMAN].Name);

      Aver.AreEqual("It is green", nls[CoreConsts.ISOA_LANG_ENGLISH].Description);
      Aver.AreEqual("Es ist grün", nls[CoreConsts.ISOA_LANG_GERMAN].Description);
    }

    [Run]
    public void NLSMap_OverrideBy()
    {
      var content1="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";
      var nls1 = new NLSMap(content1);

      var content2="{eng: {n: 'Cacamber',d: 'It is brown'}, rus: {n: 'Ogurez', d: 'On zeleniy'}}";
      var nls2 = new NLSMap(content2);

      var nls = nls1.OverrideBy(nls2);

      Aver.AreEqual(3, nls.Count);
      Aver.AreEqual("Cacamber", nls["eng"].Name);
      Aver.AreEqual("Gurke", nls["deu"].Name);
      Aver.AreEqual("Ogurez", nls["rus"].Name);
    }

    [Run]
    public void NLSMap_OverrideByEmpty1()
    {
      var nls1 = new NLSMap();

      var content2="{eng: {n: 'Cacamber',d: 'It is brown'}, rus: {n: 'Ogurez', d: 'On zeleniy'}}";
      var nls2 = new NLSMap(content2);

      var nls = nls1.OverrideBy(nls2);

      Aver.AreEqual(2, nls.Count);
      Aver.AreEqual("Cacamber", nls["eng"].Name);
      Aver.AreEqual(null, nls["deu"].Name);
      Aver.AreEqual("Ogurez", nls["rus"].Name);
    }

    [Run]
    public void NLSMap_OverrideByEmpty2()
    {
        var content1="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";
      var nls1 = new NLSMap(content1);

      var nls2 = new NLSMap();

      var nls = nls1.OverrideBy(nls2);

      Aver.AreEqual(2, nls.Count);
      Aver.AreEqual("Cucumber", nls["eng"].Name);
      Aver.AreEqual("Gurke", nls["deu"].Name);
      Aver.AreEqual(null, nls["rus"].Name);
    }


    [Run]
    public void NLSMap_SerializeAll()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

      var nls = new NLSMap(content);

      var json = nls.ToJson();
      Console.WriteLine(json);

      dynamic read = json.JsonToDynamic();
      Aver.IsNotNull(read);

      Aver.AreEqual("Cucumber", read.eng.n);
      Aver.AreEqual("Gurke", read.deu.n);
    }

    [Run]
    public void NLSMap_SerializeOnlyOneExisting()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

      var nls = new NLSMap(content);


      var options = new JsonWritingOptions{ NLSMapLanguageISO = CoreConsts.ISOA_LANG_GERMAN, Purpose = JsonSerializationPurpose.UIFeed};
      var json = nls.ToJson(options);
      Console.WriteLine(json);

      dynamic read = json.JsonToDynamic();
      Aver.IsNotNull(read);

      Aver.AreEqual("Gurke", read.n);
      Aver.AreEqual("Es ist grün", read.d);
    }

    [Run]
    public void NLSMap_SerializeOnlyOneNoneExisting()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

      var nls = new NLSMap(content);


      var options = new JsonWritingOptions{ NLSMapLanguageISO = CoreConsts.ISOA_LANG_RUSSIAN, Purpose = JsonSerializationPurpose.UIFeed};
      var json = nls.ToJson(options);
      Console.WriteLine(json);

      dynamic read = json.JsonToDynamic();
      Aver.IsNotNull(read);

      Aver.AreEqual("Cucumber", read.n);
      Aver.AreEqual("It is green", read.d);
    }


      [Run]
    public void NLSMap_JSONSerializationRoundtrip()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

      var nls = new NLSMap(content);


      var json = nls.ToJson();
      Console.WriteLine(json);

      var nls2 = new NLSMap(json);

      Aver.AreEqual(2, nls2.Count);
      Aver.AreEqual("Cucumber", nls2["eng"].Name);
      Aver.AreEqual("Gurke", nls2["deu"].Name);
      Aver.AreEqual(null, nls["rus"].Name);
    }


    [Run]
    public void NLSMap_Get_Name()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

      var nls = new NLSMap(content);

      var name = nls.Get(NLSMap.GetParts.Name);
      Aver.AreEqual("Cucumber", name);

      name = nls.Get(NLSMap.GetParts.Name, "deu");
      Aver.AreEqual("Gurke", name);

      name = nls.Get(NLSMap.GetParts.Name, "XXX");
      Aver.AreEqual("Cucumber", name);

      name = nls.Get(NLSMap.GetParts.Name, "XXX", dfltLangIso: "ZZZ");
      Aver.IsNull(name);
    }

    [Run]
    public void NLSMap_Get_Descr()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}}";

      var nls = new NLSMap(content);

      var descr = nls.Get(NLSMap.GetParts.Description);
      Aver.AreEqual("It is green", descr);

      descr = nls.Get(NLSMap.GetParts.Description, "deu");
      Aver.AreEqual("Es ist grün", descr);

      descr = nls.Get(NLSMap.GetParts.Description, "XXX");
      Aver.AreEqual("It is green", descr);

      descr = nls.Get(NLSMap.GetParts.Description, "XXX", dfltLangIso: "ZZZ");
      Aver.IsNull(descr);
    }

    [Run]
    public void NLSMap_Get_NameOrDescr()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

      var nls = new NLSMap(content);

      var nord = nls.Get(NLSMap.GetParts.NameOrDescription);
      Aver.AreEqual("Cucumber", nord);

      nord = nls.Get(NLSMap.GetParts.NameOrDescription, "deu");
      Aver.AreEqual("Gurke", nord);

      nord = nls.Get(NLSMap.GetParts.NameOrDescription, "XXX");
      Aver.AreEqual("Cucumber", nord);

      nord = nls.Get(NLSMap.GetParts.NameOrDescription, "rus");
      Aver.AreEqual("On Zeleniy", nord);

      nord = nls.Get(NLSMap.GetParts.NameOrDescription, "XXX", dfltLangIso: "ZZZ");
      Aver.IsNull(nord);
    }

    [Run]
    public void NLSMap_Get_DescrOrName()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

      var nls = new NLSMap(content);

      var dorn = nls.Get(NLSMap.GetParts.DescriptionOrName);
      Aver.AreEqual("It is green", dorn);

      dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "deu");
      Aver.AreEqual("Es ist grün", dorn);

      dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "XXX");
      Aver.AreEqual("It is green", dorn);

      dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "rus");
      Aver.AreEqual("On Zeleniy", dorn);

      dorn = nls.Get(NLSMap.GetParts.DescriptionOrName, "XXX", dfltLangIso: "ZZZ");
      Aver.IsNull(dorn);
    }


    [Run]
    public void NLSMap_Get_NameAndDescr()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

      var nls = new NLSMap(content);

      var nand = nls.Get(NLSMap.GetParts.NameAndDescription);
      Aver.AreEqual("Cucumber - It is green", nand);

      nand = nls.Get(NLSMap.GetParts.NameAndDescription, "deu");
      Aver.AreEqual("Gurke - Es ist grün", nand);

      nand = nls.Get(NLSMap.GetParts.NameAndDescription, "XXX");
      Aver.AreEqual("Cucumber - It is green", nand);

        nand = nls.Get(NLSMap.GetParts.NameAndDescription, "YYY", concat: "::");
      Aver.AreEqual("Cucumber::It is green", nand);

      nand = nls.Get(NLSMap.GetParts.NameAndDescription, "rus");
      Aver.AreEqual("On Zeleniy", nand);

      nand = nls.Get(NLSMap.GetParts.NameAndDescription, "XXX", dfltLangIso: "ZZZ");
      Aver.IsNull(nand);
    }

    [Run]
    public void NLSMap_Get_DescrAndName()
    {
      var content="{eng: {n: 'Cucumber',d: 'It is green'}, deu: {n: 'Gurke', d: 'Es ist grün'}, rus: { d: 'On Zeleniy'}}";

      var nls = new NLSMap(content);

      var dan = nls.Get(NLSMap.GetParts.DescriptionAndName);
      Aver.AreEqual("It is green - Cucumber", dan);

      dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "deu");
      Aver.AreEqual("Es ist grün - Gurke", dan);

      dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "XXX");
      Aver.AreEqual("It is green - Cucumber", dan);

        dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "YYY", concat: "::");
      Aver.AreEqual("It is green::Cucumber", dan);

      dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "rus");
      Aver.AreEqual("On Zeleniy", dan);

      dan = nls.Get(NLSMap.GetParts.DescriptionAndName, "XXX", dfltLangIso: "ZZZ");
      Aver.IsNull(dan);
    }

    [Run]
    public void Equality_1()
    {
      var nls = new NLSMap();
      var nls2 = new NLSMap();
      Aver.IsTrue(nls.Equals(nls2));
      Aver.AreEqual(nls.GetHashCode(), nls2.GetHashCode());
    }

    [Run]
    public void Equality_2()
    {
      var nls = new NLSMap("{eng: {n: 'name', d: 'descr'}}");
      var nls2 = new NLSMap();
      Aver.IsFalse(nls.Equals(nls2));
      Aver.IsFalse(nls2.Equals(nls));
      Aver.AreNotEqual(nls.GetHashCode(), nls2.GetHashCode());
    }

    [Run]
    public void Equality_3()
    {
      var nls = new NLSMap("{eng: {n: 'name', d: 'descr'}}");
      var nls2 = new NLSMap("{eng: {d: 'descr',          n: 'name' }}");
      Aver.IsTrue(nls.Equals(nls2));
      Aver.IsTrue(nls2.Equals(nls));
      Aver.AreEqual(nls.GetHashCode(), nls2.GetHashCode());
    }

    [Run]
    public void Equality_4()
    {
      var nls = new NLSMap("{eng: {n: 'namE', d: 'descr'}}");
      var nls2 = new NLSMap("{eng: {d: 'descr',          n: 'Name' }}");
      Aver.IsFalse(nls.Equals(nls2));
      Aver.IsFalse(nls2.Equals(nls));
    }

    [Run]
    public void Equality_5()
    {
      var nls = new NLSMap("{enG: {n: 'name', d: 'descr'}}");
      var nls2 = new NLSMap("{eng: {d: 'descr',          n: 'name' }}");
      Aver.IsFalse(nls.Equals(nls2));
      Aver.IsFalse(nls2.Equals(nls));
      Aver.AreNotEqual(nls.GetHashCode(), nls2.GetHashCode());
    }

    [Run]
    public void Equality_6()
    {
      var nls = new NLSMap("{eng: {n: 'name', d: 'descr'}, rus: {n: 'i', d: 'o'}}");
      var nls2 = new NLSMap("{  eng: {d: 'descr',          n: 'name' },       rus: {n: 'i',    d: 'o'} }");
      Aver.IsTrue(nls.Equals(nls2));
      Aver.IsTrue(nls2.Equals(nls));
      Aver.AreEqual(nls.GetHashCode(), nls2.GetHashCode());
    }

    [Run]
    public void Equality_7()
    {
      var nls = new NLSMap("{eng: {n: 'name', d: 'descr'}, rus: {n: 'i', d: 'o'}, chi: {n: '1', d: '2'}}");
      var nls2 = new NLSMap("{rus: {n: 'i',    d: 'o'},  eng: {d: 'descr',          n: 'name' }}");
      Aver.IsFalse(nls.Equals(nls2));
      Aver.IsFalse(nls2.Equals(nls));
      Aver.AreNotEqual(nls.GetHashCode(), nls2.GetHashCode());
    }


  }
}
