

namespace Azos.Web.Shipping.Shippo
{
  public class ShippoSession : ShippingSession
  {
    public ShippoSession(ShippingSystem shipSystem, ShippoConnectionParameters cParams)
      : base(shipSystem, cParams)
    {
      m_ConnectionParams = cParams;
    }

    private readonly ShippoConnectionParameters m_ConnectionParams;

    public ShippoConnectionParameters ConnectionParams { get { return m_ConnectionParams; } }
  }
}
