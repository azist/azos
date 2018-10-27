/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
