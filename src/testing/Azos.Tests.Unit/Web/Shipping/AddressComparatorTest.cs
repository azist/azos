/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Scripting;

using Azos.Web.Shipping;

namespace Azos.Tests.Unit.Web.Shipping
{
  [Runnable]
  public class AddressComparatorTest
  {
    [Run]
    public void Validate_Line1()
    {
      var address = getDefaultAddress();

      var other1 = getDefaultAddress(line1: "587 Kiva Str.");
      var other2 = getDefaultAddress(line1: "587 Kiva St.");
      var other3 = getDefaultAddress(line1: "587 Kiva St");

      Aver.IsTrue(AddressComparator.AreSimilar(address, other1));
      Aver.IsTrue(AddressComparator.AreSimilar(address, other2));
      Aver.IsTrue(AddressComparator.AreSimilar(address, other3));
      Aver.IsTrue(AddressComparator.AreSimilar(other1, other2));
      Aver.IsTrue(AddressComparator.AreSimilar(other1, other3));
      Aver.IsTrue(AddressComparator.AreSimilar(other2, other3));
    }
    [Run]
    public void Validate_Line2()
    {
      var address = getDefaultAddress(line2: "Apt 23");

      var other1 = getDefaultAddress(line2: "apartMent 23");
      var other2 = getDefaultAddress(line2: "ap.23");
      var other3 = getDefaultAddress(line2: "#23   ");

      Aver.IsTrue(AddressComparator.AreSimilar(address, other1));
      Aver.IsTrue(AddressComparator.AreSimilar(address, other2));
      Aver.IsTrue(AddressComparator.AreSimilar(address, other3));
      Aver.IsTrue(AddressComparator.AreSimilar(other1, other2));
      Aver.IsTrue(AddressComparator.AreSimilar(other1, other3));
      Aver.IsTrue(AddressComparator.AreSimilar(other2, other3));
    }

    [Run]
    public void Validate_Country()
    {
      var address = getDefaultAddress();
      var other = getDefaultAddress(country: "US");

      Aver.IsTrue(AddressComparator.AreSimilar(address, other));
    }


    private Address getDefaultAddress(string country = null, string line1 = null, string line2 = null, string zip = null)
    {
      country = country ?? "USA";
      line1 = line1 ?? "587 Kiva Street";
      line2 = line2 ?? string.Empty;
      zip = zip ?? "87544";

      var result = new Address
      {
        PersonName = "Stan Ulam",
        Country = country,
        Region = "NM",
        City = "Los Alamos",
        EMail = "s-ulam@myime.com",
        Line1 = line1,
        Line2 = line2,
        Postal = zip,
        Phone = "(333) 777-77-77"
      };

      return result;
    }
  }
}
