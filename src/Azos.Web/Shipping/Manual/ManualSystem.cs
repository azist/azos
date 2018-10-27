/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Web.Shipping.Manual
{
  public class ManualSystem : ShippingSystem
  {
    #region CONSTS

      public const string MANUAL_REALM = "manual-system";

    #endregion

    #region .ctor

      public ManualSystem(string name, IConfigSectionNode node) : base(name, node)
      {
      }

      public ManualSystem(string name, IConfigSectionNode node, object director) : base(name, node, director)
      {
      }

    #endregion

    #region IShippingSystem impl

      public override IShippingSystemCapabilities Capabilities
      {
        get { return ManualCapabilities.Instance; }
      }

      protected override ShippingSession DoStartSession(ShippingConnectionParameters cParams = null)
      {
        cParams = cParams ?? DefaultSessionConnectParams;
        return new ManualSession(this, (ManualConnectionParameters)cParams);
      }

      protected override ShippingConnectionParameters MakeDefaultSessionConnectParams(IConfigSectionNode paramsSection)
      {
        return ShippingConnectionParameters.Make<ManualConnectionParameters>(paramsSection);
      }

      public override Label CreateLabel(ShippingSession session, IShippingContext context, Shipment shipment)
      {
        throw new ShippingException("Not supported");
      }

      public override Address ValidateAddress(ShippingSession session, IShippingContext context, Address address, out ValidateShippingAddressException error)
      {
        throw new ShippingException("Not supported");
      }

      public override ShippingRate EstimateShippingCost(ShippingSession session, IShippingContext context, Shipment shipment)
      {
        throw new ShippingException("Not supported");
      }

    #endregion

    #region .pvt

    #endregion
  }
}
