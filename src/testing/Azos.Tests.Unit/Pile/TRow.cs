/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;

using Azos.Data;
using Azos.Financial;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.Pile
{
  public enum Sex { Male, Female, Unspecified};

  public class CheckoutRow: TypedDoc
  {
    [Field(backendName: "_id")] public GDID ID { get; set; }

    [Field(backendName: "g_c")] public PersonRow Customer { get; set; }

    [Field(backendName: "fn")] public string FileName { get; set; }
    [Field(backendName: "dt")] public DateTime Date { get; set; }
    [Field(backendName: "of")] public ulong StartOffset { get; set; }
    [Field(backendName: "g_b")] public GDID G_Block  { get; set; }

    [Field(backendName: "a1")] public string Address1 { get; set; }
    [Field(backendName: "a2")] public string Address2 { get; set; }

    [Field(backendName: "chs")] public ChargeRow[] Charges { get; set; }

    public static CheckoutRow MakeFake(GDID gdid)
    {
      var ch = new CheckoutRow()
      {
        ID = gdid,
        Customer = PersonRow.MakeFake(gdid),
        FileName = "myface_" + gdid.ID.ToString(),
        Date = DateTime.Now,
        StartOffset = gdid.ID * 20,
        G_Block = gdid,

        Address1 = Text.NaturalTextGenerator.GenerateAddressLine(),
        Address2 = (gdid.ID % 7) == 0 ? Text.NaturalTextGenerator.GenerateAddressLine() : null
      };

      var chCnt = (int)(gdid.ID % 10);
      ch.Charges = new ChargeRow[chCnt];

      for (int i = 0; i < chCnt; i++)
        ch.Charges[i] = ChargeRow.MakeFake(gdid);

      return ch;
    }

    public override bool Equals(Doc other)
    {
      if (other==null) return false;
      return this.ToJson() == other.ToJson();
    }
  }

  public class ChargeRow: TypedDoc
  {
    [Field(backendName: "amt")] public Amount Amount { get; set; }
    [Field(backendName: "qty")] public int Qty { get; set; }
    [Field(backendName: "g_p")] public GDID G_Product  { get; set; }
    [Field(backendName: "n")] public string Notes { get; set; }

    public static ChargeRow MakeFake(GDID gdid)
    {
      var c = new ChargeRow()
      {
        Amount = new Amount("usd", gdid.ID % 1897),
        Qty = (int)(gdid.ID % 29),
        G_Product = gdid,
        Notes = new String('n', (int)(gdid.ID % 137))
      };

      return c;
    }
  }

  public class PersonRow: TypedDoc
  {
    [Field(backendName: "_id")] public GDID ID { get; set; }

    [Field(backendName: "nm")] public string Name { get; set; }
    [Field(backendName: "age")] public int Age { get; set; }
    [Field(backendName: "dob")] public DateTime DOB { get; set; }
    [Field(backendName: "sex")] public Sex Sex { get; set; }
    [Field(backendName: "inc")] public decimal Income { get; set; }
    [Field(backendName: "deb")] public decimal Debt { get; set; }
    [Field(backendName: "rat")] public double? Rating { get; set; }
    [Field(backendName: "n")] public string Notes { get; set; }
    [Field(backendName: "vot")] public bool? Voter { get; set; }
    [Field(backendName: "mil")] public bool? MilitarySvc { get; set; }

    [Field(backendName: "a1")] public string Address1 { get; set; }
    [Field(backendName: "a2")] public string Address2 { get; set; }
    [Field(backendName: "ac")] public string City { get; set; }
    [Field(backendName: "as")] public string State { get; set; }
    [Field(backendName: "az")] public string Zip { get; set; }

    [Field(backendName: "p1")] public string Phone1 { get; set; }
    [Field(backendName: "p2")] public string Phone2 { get; set; }

    [Field(backendName: "e1")] public string Email1 { get; set; }
    [Field(backendName: "e2")] public string Email2 { get; set; }

    [Field(backendName: "url")] public string URL { get; set; }

    [Field(backendName: "tg")] public string[] Tags { get; set; }

    public static PersonRow MakeFake(GDID parentGdid)
    {
      var age = (int)(parentGdid.ID % 99);

      var tags = Ambient.Random.NextRandomInteger > 0 ? new string[Ambient.Random.NextScaledRandomInteger(1, 20)] : null;

      if (tags != null)
        for (int i = 0; i < tags.Length; i++)
        {
          tags[i] = ((char)('a' + i)) + "tag";
        }

      var pers = new PersonRow()
      {
        ID = parentGdid,
        Name = Text.NaturalTextGenerator.GenerateFullName(true),
        Age = age,
        DOB = DateTime.Now.AddYears(-age),
        Sex = (parentGdid.ID % 2) == 0 ? Sex.Male : Sex.Female,
        Income = (parentGdid.ID % 79) * 1000,
        Debt = (parentGdid.ID % 11) * 1000,
        Rating = (parentGdid.ID % 2) == 0 ? (double?)null : 3.25,
        Notes = parentGdid.ToString(),
        Voter = (parentGdid.ID % 2) == 0 ? (bool?)null : true,
        MilitarySvc = (parentGdid.ID % 2) == 0 ? (bool?)null : false,
        Address1 = Text.NaturalTextGenerator.GenerateAddressLine(),
        Address2 = (parentGdid.ID % 7) == 0 ? Text.NaturalTextGenerator.GenerateAddressLine() : null,
        City = Text.NaturalTextGenerator.GenerateCityName(),
        State = "OH",
        Zip = "44000" + (parentGdid.ID % 999),
        Phone1 = "(555) 222-3222",
        Phone2 = (parentGdid.ID % 3) == 0 ? "(555) 737-9789" : null,
        Email1 = Text.NaturalTextGenerator.GenerateEMail(),
        Email2 = (parentGdid.ID % 5) == 0 ? Text.NaturalTextGenerator.GenerateEMail() : null,
        URL = (parentGdid.ID % 2) == 0 ? "https://ibm.com/products/" + parentGdid.ID : null,
        Tags = tags
      };

      return pers;
    }
  }
}
