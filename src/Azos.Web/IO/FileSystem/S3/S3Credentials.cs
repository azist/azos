/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Security;

namespace Azos.IO.FileSystem.S3
{
  /// <summary>
  /// Represents Amazon S3 credentials (access key and secret key)
  /// </summary>
  [Serializable]
  public class S3Credentials : Credentials
  {
    public S3Credentials(string accessKey, string secretKey)
    {
      m_AccessKey = accessKey;
      m_SecretKey = secretKey;
    }

    private string m_AccessKey;
    private string m_SecretKey;

    public string AccessKey { get { return m_AccessKey; }}
    public string SecretKey { get { return m_SecretKey; }}

    public override string ToString()
    {
      return m_AccessKey;
    }

  } //S3Credentials

}
