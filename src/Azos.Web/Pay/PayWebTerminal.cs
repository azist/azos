
namespace Azos.Web.Pay
{
  public abstract class PayWebTerminal : IPayWebTerminal
  {
    protected PayWebTerminal(PaySystem paySystem)
    {
      PaySystem = paySystem;
    }
    public IPaySystem PaySystem { get; private set; }

    public abstract object GetPayInit();
  }
}
