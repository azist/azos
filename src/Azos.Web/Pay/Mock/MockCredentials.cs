
using Azos.Security;

namespace Azos.Web.Pay.Mock
{
  /// <summary>
  /// Represents mock credentials (email)
  /// </summary>
  public class MockCredentials : Credentials
  {
    public MockCredentials(string email)
    {
      m_Email = email;
    }

    private string m_Email;

    public string Email { get { return m_Email; }}

    public override string ToString()
    {
      return m_Email;
    }

  } //MockCredentials
}
