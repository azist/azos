/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azos.Data;
using Azos.Data.Business;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DeconstructionTests
  {
    [Run]
    public void Test01()
    {
      var json = new
      {
        __deconstruct = "'April 1 1980' Joseph 5552921030"
      }.ToJson();

      var filter = JsonReader.ToDoc<PersonFilter>(json);

      filter.See();
      Aver.AreEqual("(555) 292-1030", filter.Phone);
      Aver.AreEqual(1980, filter.DOB.Year);
      Aver.AreEqual("Joseph", filter.Name);
    }

    [Run]
    public void Test02()
    {
      var json = new
      {
        __deconstruct = "  'Joseph Murray' 555-292.1040 'April 1 1985'"
      }.ToJson();

      var filter = JsonReader.ToDoc<PersonFilter>(json);

      filter.See();
      Aver.AreEqual("(555) 292-1040", filter.Phone);
      Aver.AreEqual(1985, filter.DOB.Year);
      Aver.AreEqual("Joseph Murray", filter.Name);
    }

    [Run]
    public void Test03()
    {
      var json = new
      {
        Name = "Sokoloff",
        __deconstruct = "  'Joseph Murray' 555-292.1041 'April 1 1985'"
      }.ToJson();

      var filter = JsonReader.ToDoc<PersonFilter>(json);

      filter.See();
      Aver.AreEqual("(555) 292-1041", filter.Phone);
      Aver.AreEqual(1985, filter.DOB.Year);
      Aver.AreEqual("Sokoloff", filter.Name);
    }

    [Run]
    public void Test04()
    {
      var json = new
      {
        Name = "Sokoloff",
        __deconstruct = (string)null
      }.ToJson();

      var filter = JsonReader.ToDoc<PersonFilter>(json);

      filter.See();
      Aver.IsNull(filter.Phone);
      Aver.AreEqual(default(DateTime), filter.DOB);
      Aver.AreEqual("Sokoloff", filter.Name);
    }

    [Run]
    public void Test05()
    {
      var json = new
      {
        Name = "Sokoloff",
        __deconstruct = "                                     "
      }.ToJson();

      var filter = JsonReader.ToDoc<PersonFilter>(json);

      filter.See();
      Aver.IsNull(filter.Phone);
      Aver.AreEqual(default(DateTime), filter.DOB);
      Aver.AreEqual("Sokoloff", filter.Name);
    }



    public class PersonFilter : FilterModel<object>
    {
      public override bool AmorphousDataEnabled => true;

      [Field]
      public string Name { get; set; }

      [Field]
      public string Phone { get; set; }

      [Field]
      public DateTime DOB { get; set; }

      protected override void DoAmorphousDataAfterLoad(string targetName)
      {
        base.DoAmorphousDataAfterLoad(targetName);
        this.DeconstructAmorphousStringData((doc, tokens) =>
        {
          foreach (var token in tokens) {
            if (Phone.IsNullOrWhiteSpace() && token.IsPhone()) Phone = token.NormalizePhone();
            else if (DOB == default && token.IsDate()) DOB = token.NormalizeDate();
            else if (Name.IsNullOrWhiteSpace()) Name = token.Value.AsString();
          }
          return true;
        });
      }

      protected override Task<SaveResult<object>> DoSaveAsync() => Task.FromResult(new SaveResult<object>(123));
    }


  }
}
