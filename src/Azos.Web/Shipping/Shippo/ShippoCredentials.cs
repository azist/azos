
using Azos.Security;

namespace Azos.Web.Shipping.Shippo
{
  public class ShippoCredentials : Credentials
  {
    public ShippoCredentials(string privateToken, string publicToken)
    {
      PrivateToken = privateToken;
      PublicToken = publicToken;
    }

    public readonly string PrivateToken;
    public readonly string PublicToken;

    public override string ToString()
    {
      return "[{0} {1}]".Args(PrivateToken, PublicToken);
    }
  }
}
