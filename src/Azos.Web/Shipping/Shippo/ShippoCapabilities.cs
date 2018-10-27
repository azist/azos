/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Web.Shipping.Shippo
{
  /// <summary>
  /// Denotes capabilities of the Shippo shipping system
  /// </summary>
  public class ShippoCapabilities : IShippingSystemCapabilities
  {
    #region Static

      private static ShippoCapabilities s_Instance = new ShippoCapabilities();

      public static ShippoCapabilities Instance { get { return s_Instance; } }

    #endregion

    #region .ctor

      private ShippoCapabilities() {}

    #endregion

    #region IShippingSystemCapabilities

      public bool SupportsAddressValidation      { get { return true; } }
      public bool SupportsCarrierServices        { get { return true; } }
      public bool SupportsLabelCreation          { get { return true; } }
      public bool SupportsShipmentTracking       { get { return true; } }
      public bool SupportsShippingCostEstimation { get { return true; } }

    #endregion

  }
}
