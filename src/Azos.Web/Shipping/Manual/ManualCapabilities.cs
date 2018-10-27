/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Web.Shipping.Manual
{
  /// <summary>
  /// Denotes capabilities of the Manual shipping system
  /// </summary>
  public class ManualCapabilities : IShippingSystemCapabilities
  {
    #region Static

      private static ManualCapabilities s_Instance = new ManualCapabilities();

      public static ManualCapabilities Instance { get { return s_Instance; } }

    #endregion

    #region .ctor

      private ManualCapabilities() {}

    #endregion

    #region IShippingSystemCapabilities

      public bool SupportsAddressValidation      { get { return false; } }
      public bool SupportsCarrierServices        { get { return true;  } }
      public bool SupportsLabelCreation          { get { return false; } }
      public bool SupportsShipmentTracking       { get { return true;  } }
      public bool SupportsShippingCostEstimation { get { return false; } }

    #endregion

  }
}
