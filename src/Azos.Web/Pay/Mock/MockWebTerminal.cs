
using System;

namespace Azos.Web.Pay.Mock
{
  public class MockWebTerminal : PayWebTerminal
  {
    public MockWebTerminal(MockSystem paySystem)
      : base(paySystem) { }

    public override object GetPayInit()
    {
      throw new NotImplementedException();
    }
  }
}
