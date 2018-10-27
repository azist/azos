/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


namespace Azos.Web.Pay.PayPal
{
  /// <summary>
  /// Represents PayPal OAuth token
  /// </summary>
  public class PayPalOAuthToken
  {
    public PayPalOAuthToken(string applicationID, int expiresInSec, string tokenType, string accessToken, string scope, string nonce, int expirationMarginSec)
    {
      ObtainTime = App.TimeSource.Now;
      ApplicationID = applicationID;
      ExpiresInSec = expiresInSec;
      TokenType = tokenType;
      AccessToken = accessToken;
      Scope = scope;
      Nonce = nonce;
      ExpirationMarginSec = expirationMarginSec;
    }

    public readonly int ExpirationMarginSec;
    public readonly DateTime ObtainTime;
    public readonly string ApplicationID;
    public readonly int ExpiresInSec;
    public readonly string TokenType;
    public readonly string AccessToken;
    public readonly string Scope;
    public readonly string Nonce;

    public string AuthorizationHeader { get { return "{0} {1}".Args(TokenType, AccessToken); } }

    public bool IsCloseToExpire() { return (App.TimeSource.Now - ObtainTime).TotalSeconds >= ExpiresInSec - ExpirationMarginSec; }

    public override string ToString() { return AccessToken; }
  }
}
