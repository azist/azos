/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Text;

namespace Azos.Web.GeoLookup
{
  /// <summary>
  /// Provides address segment block information
  /// </summary>
  public struct IPSubnetBlock
  {
    public readonly SealedString Subnet;
    public readonly SealedString LocationID;
    public readonly SealedString RegisteredLocationID;
    public readonly SealedString RepresentedLocationID;
    public readonly bool AnonymousProxy;
    public readonly bool SatelliteProvider;
    public readonly SealedString PostalCode;
    public readonly float Lat;
    public readonly float Lng;
    public IPSubnetBlock(
      SealedString subnet,
      SealedString locationID,
      SealedString registeredLocationID,
      SealedString representedLocationID,
      bool anonymousProxy,
      bool satelliteProvider,
      SealedString postalCode,
      float lat,
      float lng)
    {
      Subnet = subnet;
      LocationID = locationID;
      RegisteredLocationID = registeredLocationID;
      RepresentedLocationID = representedLocationID;
      AnonymousProxy = anonymousProxy;
      SatelliteProvider = satelliteProvider;
      PostalCode = postalCode;
      Lat = lat;
      Lng = lng;
    }
  }

  /// <summary>
  /// Provides location information
  /// </summary>
  public struct Location
  {
    public readonly SealedString ID;
    public readonly SealedString LocaleCode;
    public readonly SealedString ContinentID;
    public readonly SealedString ContinentName;
    public readonly SealedString CountryISOName;
    public readonly SealedString CountryName;
    public readonly SealedString SubdivisionISOCode;
    public readonly SealedString SubdivisionName;
    public readonly SealedString Subdivision2ISOCode;
    public readonly SealedString Subdivision2Name;
    public readonly SealedString CityName;
    public readonly SealedString MetroCode;
    public readonly SealedString TimeZone;
    public Location(
      SealedString id,
      SealedString localeCode,
      SealedString continentID,
      SealedString continentName,
      SealedString countryISOName,
      SealedString countryName,
      SealedString subdivisionISOCode,
      SealedString subdivisionName,
      SealedString subdivision2ISOCode,
      SealedString subdivision2Name,
      SealedString cityName,
      SealedString metroCode,
      SealedString timeZone)
    {
      ID = id;
      LocaleCode = localeCode;
      ContinentID = continentID;
      ContinentName = continentName;
      CountryISOName = countryISOName;
      CountryName = countryName;
      SubdivisionISOCode = subdivisionISOCode;
      SubdivisionName = subdivisionName;
      Subdivision2ISOCode = subdivision2ISOCode;
      Subdivision2Name = subdivision2Name;
      CityName = cityName;
      MetroCode = metroCode;
      TimeZone = timeZone;
    }
  }
}
