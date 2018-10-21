
namespace Azos.Web.Pay.Mock
{
  public class MockSession: PaySession
  {
    public MockSession(PaySystem paySystem, MockConnectionParameters cParams, IPaySessionContext context = null)
      : base(paySystem, cParams, context) {}

    public string Email
    {
      get
      {
        if (!IsValid) return string.Empty;
        var cred = User.Credentials as MockCredentials;
        if (cred == null) return string.Empty;
        return cred.Email;
      }
    }
  }
}
