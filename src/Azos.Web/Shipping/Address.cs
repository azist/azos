
using System;


using Azos.Text;

namespace Azos.Web.Shipping
{
  public class Address
  {
    public string PersonName { get; set; }
    public string Company    { get; set; }
    public string Line1      { get; set; }
    public string Line2      { get; set; }
    public string Country    { get; set; }
    public string Region     { get; set; }
    public string City       { get; set; }
    public string Postal     { get; set; }
    public string Phone      { get; set; }
    public string EMail      { get; set; }

    public override string ToString()
    {
      return "[name: {0}, country: {1}, state: {2}, city: {3}, street1: {4}, zip: {5}]"
              .Args(PersonName ?? "-",
                    Country ?? "-",
                    Region ?? "-",
                    City ?? "-",
                    Line1 ?? "-",
                    Postal ?? "-");
    }
  }

  public static class AddressComparator
  {
    public static bool AreSimilar(Address first, Address second)
    {
      if (first==null && second==null) return true;
      if (first==null && second!=null) return false;
      if (first!=null && second==null) return false;

      if (!string.Equals(first.PersonName, second.PersonName, StringComparison.InvariantCultureIgnoreCase)) return false;
      if (!string.Equals(first.Company ?? string.Empty, second.Company ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)) return false;
      if (!string.Equals(DataEntryUtils.NormalizeUSPhone(first.Phone ?? string.Empty),
                         DataEntryUtils.NormalizeUSPhone(second.Phone ?? string.Empty),
                         StringComparison.InvariantCultureIgnoreCase))
        return false;

      if (!string.Equals(first.EMail, second.EMail)) return false;
      if (!string.Equals(GetPostalMainPart(first.Postal), GetPostalMainPart(second.Postal), StringComparison.InvariantCultureIgnoreCase)) return false;
      if (!string.Equals(first.City, second.City, StringComparison.InvariantCultureIgnoreCase)) return false;
      if (!string.Equals(first.Region, second.Region, StringComparison.InvariantCultureIgnoreCase)) return false;
      if (!string.Equals(Azos.Standards.Countries_ISO3166_1.Normalize3(first.Country ?? string.Empty),
                         Azos.Standards.Countries_ISO3166_1.Normalize3(second.Country ?? string.Empty),
                         StringComparison.InvariantCultureIgnoreCase))
        return false;

      if (!AddressLinesAreSimilar(first.Line1, second.Line1)) return false;
      if (!AddressLinesAreSimilar(first.Line2, second.Line2)) return false;

      return true;
    }

    public static bool RegionAndPostalAreSimilar(Address first, Address second)
    {
      if (first==null && second==null) return true;
      if (first==null && second!=null) return false;
      if (first!=null && second==null) return false;

      if (!string.Equals(GetPostalMainPart(first.Postal), GetPostalMainPart(second.Postal), StringComparison.InvariantCultureIgnoreCase)) return false;
      if (!string.Equals(first.Region, second.Region, StringComparison.InvariantCultureIgnoreCase)) return false;
      if (!string.Equals(Standards.Countries_ISO3166_1.Normalize3(first.Country ?? string.Empty),
                         Standards.Countries_ISO3166_1.Normalize3(second.Country ?? string.Empty),
                         StringComparison.InvariantCultureIgnoreCase))
        return false;

      return true;
    }

    public static string GetPostalMainPart(string postal)
    {
      if (postal.IsNullOrWhiteSpace()) return string.Empty;

      var main = postal.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
      if (main.Length < 1) return string.Empty;

      return main[0].Trim();
    }

    public static bool AddressLinesAreSimilar(string fAddr, string sAddr)
    {
      if (fAddr.IsNullOrWhiteSpace() && sAddr.IsNullOrWhiteSpace()) return true;
      if (fAddr.IsNullOrWhiteSpace() && sAddr.IsNotNullOrWhiteSpace()) return false;
      if (fAddr.IsNotNullOrWhiteSpace() && sAddr.IsNullOrWhiteSpace()) return false;

      fAddr = fAddr.ToLowerInvariant();
      sAddr = sAddr.ToLowerInvariant();

      if (string.Equals(fAddr, sAddr, StringComparison.InvariantCultureIgnoreCase)) return true;

      fAddr = normalizeAddress(fAddr).Trim();
      sAddr = normalizeAddress(sAddr).Trim();

      return string.Equals(fAddr, sAddr, StringComparison.InvariantCultureIgnoreCase);
    }

    private static string normalizeAddress(string address)
    {
      if (address.IsNullOrWhiteSpace()) return string.Empty;

      address = address.Replace('.', ' ').Replace(',', ' ');

      return System.Text.RegularExpressions.Regex.Replace(address, @"\s+", " ")
                                                 .Replace("street", "st")
                                                 .Replace("str", "st")
                                                 .Replace("court", "ct")
                                                 .Replace("drive", "dr")
                                                 .Replace("drv", "dr")
                                                 .Replace("lane", "ln")
                                                 .Replace("road", "rd")
                                                 .Replace("apartment", "ap")
                                                 .Replace("apt", "ap")
                                                 .Replace("#", "ap ")
                                                 .Replace("boulevard", "blvd");
    }
  }
}
