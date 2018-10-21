

using Azos.Security;

namespace Azos.Web.Pay.Stripe
{
  /// <summary>
  /// Represents stripe credentials (test and publihsable keys)
  /// </summary>
  public class StripeCredentials : Credentials
  {
    public StripeCredentials(string email, string secretKey, string publishableKey)
    {
      m_Email = email;
      m_SecretKey = secretKey;
      m_PublishableKey = publishableKey;
    }

    private string m_Email;
    private string m_SecretKey;
    private string m_PublishableKey;

    public string Email { get { return m_Email; }}
    public string SecretKey { get { return m_SecretKey; }}
    public string PublishableKey { get { return m_PublishableKey; }}

    public override string ToString()
    {
      return m_Email;
    }

  } //StripeCredentials
}
