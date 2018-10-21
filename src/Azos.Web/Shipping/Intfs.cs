
using System.Collections.Generic;

using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Web.Shipping
{
  /// <summary>
  /// Represents a process-global host for a shipping systems
  /// </summary>
  public interface IShippingSystemHost : Collections.INamed
  {
  }

  /// <summary>
  /// Denotes an implementation of IShippingSystemHost
  /// </summary>
  public interface IShippingSystemHostImplementation : IShippingSystemHost, IConfigurable
  {
  }

  /// <summary>
  /// Denotes context for shipping transactions. Can be used to provide additional information
  /// </summary>
  public interface IShippingContext { }

  /// <summary>
  /// Denotes capabilities of the shipping system
  /// </summary>
  public interface IShippingSystemCapabilities
  {
    /// <summary>
    /// Indicates whether a shipping system supports shipping label creation
    /// </summary>
    bool SupportsLabelCreation { get; }

    /// <summary>
    /// Indicates whethe a shipping system provides detailed tracking information about shipments
    /// </summary>
    bool SupportsShipmentTracking { get; }

    /// <summary>
    /// Indicates whether a shipping system supports shipping address validation
    /// </summary>
    bool SupportsAddressValidation { get; }

    /// <summary>
    /// Indicates whether a shipping system provides any services directly from some shipping carriers
    /// </summary>
    bool SupportsCarrierServices { get; }

    /// <summary>
    /// Indicates whether a shipping system provides any (at least approximate) shipping cost calculation
    /// </summary>
    bool SupportsShippingCostEstimation { get; }
  }

  /// <summary>
  /// Represents entity that can perform shipping fuctions like labels creation, tracking etc.
  /// </summary>
  public interface IShippingSystem
  {
    /// <summary>
    /// Returns capabilities for this shipping system
    /// </summary>
    IShippingSystemCapabilities Capabilities { get; }

    /// <summary>
    /// Starts shipping session with given or default connection parameters
    /// </summary>
    ShippingSession StartSession(ShippingConnectionParameters cParams = null);

    /// <summary>
    /// Creates shipping direct/return label
    /// </summary>
    Label CreateLabel(ShippingSession session, IShippingContext context, Shipment shipment);

    /// <summary>
    /// Retrieves shipment tracking info
    /// </summary>
    TrackInfo TrackShipment(ShippingSession session, IShippingContext context, string carrierID, string trackingNumber);

    /// <summary>
    /// Retrieves tracking URL by carrier and number
    /// </summary>
    string GetTrackingURL(ShippingSession session, IShippingContext context, string carrierID, string trackingNumber);

    /// <summary>
    /// Validates shipping address.
    /// Returns new Address instance which may contain corrected address fields ('New Yourk' -> 'New York')
    /// </summary>
    Address ValidateAddress(ShippingSession session, IShippingContext context, Address address, out ValidateShippingAddressException error);

    /// <summary>
    /// Returns all the carriers allowed for the system
    /// </summary>
    IEnumerable<ShippingCarrier> GetShippingCarriers(ShippingSession session, IShippingContext context);

    /// <summary>
    /// Estimates shipping label cost.
    /// </summary>
    /// <returns>Rate for original or alternative shipment</returns>
    ShippingRate EstimateShippingCost(ShippingSession session, IShippingContext context, Shipment shipment);

    // todo: RefundLabel
  }

  /// <summary>
  /// Denotes an implementation of IShippingSystem
  /// </summary>
  public interface IShippingSystemImplementation : IShippingSystem, IConfigurable, IInstrumentable
  {
    Azos.Log.MessageType LogLevel { get; set; }
  }

}
