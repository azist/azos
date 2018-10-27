/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
