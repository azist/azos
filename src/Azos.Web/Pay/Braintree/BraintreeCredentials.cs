
using System;
using System.Text;

using Azos.Security;

namespace Azos.Web.Pay.Braintree
{
  public class BraintreeCredentials : Credentials
  {
    private const string BASIC_AUTH = "Basic {0}";
    private const string BASIC_AUTH_FORMAT = "{0}:{1}";

    public BraintreeCredentials(string merchantId, string publicKey, string privateKey)
    {
      MerchantID = merchantId;
      PublicKey = publicKey;
      PrivateKey = privateKey;
    }

    public readonly string MerchantID;
    public readonly string PublicKey;
    public readonly string PrivateKey;

    public override string ToString() { return "[{0} {1}]".Args(MerchantID, PublicKey); }

    public string AuthorizationHeader
    { get { return BASIC_AUTH.Args(Convert.ToBase64String(Encoding.UTF8.GetBytes(BASIC_AUTH_FORMAT.Args(PublicKey, PrivateKey)))); } }
  }
}
