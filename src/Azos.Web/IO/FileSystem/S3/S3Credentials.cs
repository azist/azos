
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
