/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Security
{
  /// <summary>
  /// Represents credentials supplied from/to Social Net site (i.e. Facebook, Twitter etc.)
  /// </summary>
  [Serializable]
  public sealed class SocialNetTokenCredentials : Credentials
  {
    public SocialNetTokenCredentials(string netName, string token, string userName = null)
    {
      m_NetName = netName;
      m_Token = token;
      m_UserName = userName;
    }

    private string m_NetName;
    private string m_Token;
    private string m_UserName;

    /// <summary>
    /// Name of social network that returned the token
    /// </summary>
    public string NetName => m_NetName ?? string.Empty;

    /// <summary>
    /// Auth token returned by the network
    /// </summary>
    public string Token => m_Token ?? string.Empty;

    /// <summary>
    /// Optional user name as returned from social network (i.e. email or account screen name)
    /// </summary>
    public string UserName => m_UserName ?? string.Empty;

    public override void Forget()
    {
      m_Token = string.Empty;
      base.Forget();
    }

    public override string ToString()
    {
      return "{0}({1}:{2})".Args(GetType().Name, NetName, UserName);
    }
  }
}
