/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
