namespace Azos.Data.Access
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
}
