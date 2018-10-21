
using System;
using System.Text;

using Azos.Security;

namespace Azos.Web.Pay.PayPal
{
  /// <summary>
  /// Represents basic PayPal credentials for registered application
  /// which include Business account email, client ID and client secret.
  /// </summary>
  public class PayPalCredentials : Credentials
  {
    private const string BASIC_AUTH = "Basic {0}";
    private const string BASIC_AUTH_FORMAT = "{0}:{1}";

    public PayPalCredentials(string clientID, string clientSecret)
    {
      ClientID = clientID;
      ClientSecret = clientSecret;
    }

    public readonly string ClientID;
    public readonly string ClientSecret;

    public string AuthorizationHeader
    { get { return BASIC_AUTH.Args(Convert.ToBase64String(Encoding.UTF8.GetBytes(BASIC_AUTH_FORMAT.Args(ClientID, ClientSecret)))); } }
  }
}
