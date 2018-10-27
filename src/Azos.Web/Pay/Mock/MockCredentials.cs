/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
