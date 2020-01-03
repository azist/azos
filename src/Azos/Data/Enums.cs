/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data
{
  /// <summary>
  /// Defines log level for DataStores
  /// </summary>
  public enum StoreLogLevel
  {
    None = 0,
    Debug,
    Trace
  }

  /// <summary>
  /// Determines whether entity should be loaded/stored from/to storage
  /// </summary>
  public enum StoreFlag
  {
     LoadAndStore = 0,
     OnlyLoad,
     OnlyStore,
     None
  }

  /// <summary>
  /// Types of char casing
  /// </summary>
  public enum CharCase
  {
    /// <summary>
    /// The string remains as-is
    /// </summary>
    AsIs = 0,

    /// <summary>
    /// The string is converted to upper case
    /// </summary>
    Upper,

    /// <summary>
    /// The string is converted to lower case
    /// </summary>
    Lower,

    /// <summary>
    /// The first and subsequent chars after space or '.' are capitalized, the rest left intact
    /// </summary>
    Caps,

    /// <summary>
    /// The first and subsequent chars after space or '.' are capitalized, the rest is lower-cased
    /// </summary>
    CapsNorm
  }

  /// <summary>
  /// Provides hint/classification for field data representation/purpose
  /// </summary>
  public enum DataKind
  {
    /// <summary>
    /// The data represents a text typed-as is, or does not have any other special representation for numbers and other types
    /// </summary>
    Text,

    /// <summary>
    /// The value must adhere to screen name validation - must not start from dot/underscore/dash/number and only contain letters and numbers.
    /// Screen names are used as publicly visible IDs such as user segment of email addresses
    /// </summary>
    ScreenName,

    /// <summary>
    /// Unified Resource Identifier
    /// </summary>
    Uri,

    /// <summary>
    /// Telephone/Text/MMS numbers, including international ones starting with +
    /// </summary>
    Telephone,
    EMail,

    /// <summary>
    /// Country name
    /// </summary>
    Country,

    /// <summary>
    /// Address detail line
    /// </summary>
    AddressLine,

    /// <summary>
    /// Address city name
    /// </summary>
    AddressCity,

    /// <summary>
    /// Address state/province
    /// </summary>
    AddressState,

    /// <summary>
    /// Address postal code
    /// </summary>
    AddresPostalCode,


    Search,

    /// <summary>
    /// Color palette selection
    /// </summary>
    Color,

    Date,
    DateTime,
    DateTimeLocal,
    Year,
    Month,
    Week,
    Day,
    YearMonth,
    TimeOfDay,
    Number,
    Percent,

    /// <summary>
    /// Monetary amount, typically with 2 or 4 digits of decimal precision
    /// </summary>
    Money,
    YearRange,
    DateRange,
    DateTimeRange,
    IntNumberRange,
    RealNumberRange,

    /// <summary>
    /// The data represents a monetary value range
    /// </summary>
    MoneyRange,

    /// <summary>
    /// The data represents a range of percentages
    /// </summary>
    PercentRange,

    /// <summary>
    /// National Identification number, such as SSN in the US.
    /// Note: the actual format depends on country, so this flag and the actual country should be used to determine the applicable format
    /// </summary>
    NationalId,

    /// <summary>
    /// National/State driver/license ID.
    /// Note: the actual format depends on country/state
    /// </summary>
    DriverId,

    /// <summary>
    /// Primary account number, such as credit card number
    /// </summary>
    PaymentPAN,

    /// <summary>
    /// Bank Routing Number
    /// </summary>
    BankRouting,

    /// <summary>
    /// Bank account number
    /// </summary>
    BankAccount
  }
}
