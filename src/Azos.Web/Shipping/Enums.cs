/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Web.Shipping
{
  /// <summary>
  /// Denotes type of shipping packages i.e. Envelope, Box etc.
  /// </summary>
  public enum PackageType
  {
    Unknown   = 0,
    Card      = 10,
    Envelope  = 20,
    Package   = 30, // thick envelope
    Box       = 40,
    Box_Small = 41,
    Box_Med   = 42,
    Box_Large = 43,
    Pak       = 50,
    Tube      = 60,

    Crate  = 70,
    Drum   = 80,
    Pallet = 90
  }

  /// <summary>
  /// Denotes price category: cheap, expedited etc.
  /// </summary>
  public enum PriceCategory
  {
    Unknown = 0,
    Saver = 1,
    Standard = 2,
    Expedited = 3
  }

  /// <summary>
  /// Denotes shipping carrier i.e. USPS,FedEx etc.
  /// </summary>
  public enum CarrierType
  {
    Unknown = 0,
    USPS = 1,
    FedEx = 2,
    UPS = 3,
    DHLExpress = 4,
    Custom = 100000
  }

  /// <summary>
  /// Shipping label print format i.e. PDF, PNG etc.
  /// </summary>
  public enum LabelFormat
  {
    PDF = 0,
    PDF_4X6 = 1,
    PNG = 2,
    ZPLII = 3
  }

  /// <summary>
  /// Tracking status
  /// </summary>
  public enum TrackStatus
  {
    Unknown = 0,
    Transit = 1,
    Delivered = 2,
    Failure = 3,
    Returned = 4,
    Cancelled = 5,
    Error = 4
  }

}
