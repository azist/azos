
namespace Azos.Web.Shipping.Manual
{
  public class ManualSession : ShippingSession
  {
    public ManualSession(ShippingSystem shipSystem, ManualConnectionParameters cParams)
      : base(shipSystem, cParams)
    {
      m_ConnectionParams = cParams;
    }

    private readonly ManualConnectionParameters m_ConnectionParams;

    public ManualConnectionParameters ConnectionParams { get { return m_ConnectionParams; } }
  }
}
