/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using System.Security.Cryptography;
using Azos.Web.IO;

namespace Azos.Web.Social
{
  public partial class Twitter : SocialNetwork
  {

    #region Inner Types

      public class TwitterCryptor
      {
        private HMACSHA1 m_Crypto;

        public TwitterCryptor(string consumerSecret, string oauthTokenSecret = null)
        {
          string signingKey = RFC3986.Encode(consumerSecret) + '&' + RFC3986.Encode(oauthTokenSecret);
          m_Crypto = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey));
          m_Crypto.Initialize();
        }

        public string GetHashString(string src)
        {
          byte[] srcBytes = Encoding.UTF8.GetBytes(src);
          byte[] hashBytes = m_Crypto.ComputeHash(srcBytes);
          string hashStr = Convert.ToBase64String(hashBytes);
          return hashStr;
        }
      }

    #endregion

  } //TwitterCryptor

}
