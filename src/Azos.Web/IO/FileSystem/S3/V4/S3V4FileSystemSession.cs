
using Azos.Conf;
using Azos.Security;

using WSC = Azos.Web.StringConsts;

namespace Azos.IO.FileSystem.S3.V4
{
  /// <summary>
  /// Provides connection parameters specific for Amazon S3 V4
  /// </summary>
  public class S3V4FileSystemSessionConnectParams: FileSystemSessionConnectParams
  {
    public const string CONFIG_BUCKET_ATTR = "bucket";
    public const string CONFIG_REGION_ATTR = "region";

    public const string CONFIG_ACCESSKEY_ATTR = "access-key";
    public const string CONFIG_SECRETKEY_ATTR = "secret-key";

    public const string CONFIG_TIMEOUT_MS_ATTR = "timeout-ms";

    public S3V4FileSystemSessionConnectParams(): base() {}
    public S3V4FileSystemSessionConnectParams(IConfigSectionNode node): base(node) {}
    public S3V4FileSystemSessionConnectParams(string connectStr, string format = Configuration.CONFIG_LACONIC_FORMAT): base(connectStr, format) {}

    private int m_TimeoutMs;

    /// <summary>
    /// AWS S3 bucket (i.e. bucket01)
    /// </summary>
    [Config] public string Bucket { get; set; }

    /// <summary>
    /// AWS S3 bucket region (i.e. us-west-2)
    /// </summary>
    [Config] public string Region { get; set; }

    /// <summary>
    /// Request timeout milliseconds
    /// </summary>
    [Config] public int TimeoutMs
    {
      get { return m_TimeoutMs; }
      set { m_TimeoutMs = value < 0 ? 0 : value; }
    }

    public override void Configure(IConfigSectionNode node)
    {
 	    base.Configure(node);
      var accessKey = node.AttrByName(CONFIG_ACCESSKEY_ATTR).Value;
      var secretKey = node.AttrByName(CONFIG_SECRETKEY_ATTR).Value;

      if (accessKey.IsNotNullOrWhiteSpace())
	    {
        var cred = new S3Credentials(accessKey, secretKey);
        var at = new AuthenticationToken(Bucket, accessKey);
        User = new User(cred, at, UserStatus.User, accessKey, accessKey, Rights.None);
	    }
    }
  }

  /// <summary>
  /// Provides S3 (v4) file system connection session
  /// </summary>
  public class S3V4FileSystemSession : FileSystemSession
  {
    protected internal S3V4FileSystemSession(S3V4FileSystem fs, IFileSystemHandle handle, S3V4FileSystemSessionConnectParams cParams)
                      : base(fs, handle, cParams)
    {
      m_Bucket = cParams.Bucket;
      m_Region = cParams.Region;
      m_TimeoutMs = cParams.TimeoutMs;
    }

    private string m_Bucket;
    private string m_Region;
    private int m_TimeoutMs;

    /// <summary>
    /// Amazon S3 bucket (i.e. bucket01)
    /// </summary>
    public string Bucket { get { return m_Bucket; }}

    /// <summary>
    /// Amazon S3 region (i.e. us-west-2)
    /// </summary>
    public string Region { get { return m_Region; }}

    /// <summary>
    /// Request timeout in milliseconds
    /// </summary>
    public int TimeoutMs { get { return m_TimeoutMs; } }

    /// <summary>
    /// Extracts AccessKey from User
    /// </summary>
    public string AccessKey
    {
      get
      {
        if (m_User == null || m_User==User.Fake) return string.Empty;
        var cred = m_User.Credentials as S3Credentials;
        if (cred.AccessKey.IsNullOrWhiteSpace()) return string.Empty;
        return cred.AccessKey;
      }
    }

    /// <summary>
    /// Extracts SecretKey from User
    /// </summary>
    public string SecretKey
    {
      get
      {
        if (m_User == null || m_User==User.Fake) return string.Empty;
        var cred = m_User.Credentials as S3Credentials;
        if (cred.SecretKey.IsNullOrWhiteSpace()) return string.Empty;
        return cred.SecretKey;
      }
    }

    protected override void ValidateConnectParams(FileSystemSessionConnectParams cParams)
    {
      var s3cp = cParams as S3V4FileSystemSessionConnectParams;

      if (s3cp.Bucket.IsNullOrWhiteSpace() || s3cp.Region.IsNullOrWhiteSpace())
        throw new NFXIOException(WSC.FS_S3_PARAMS_SERVER_URL_ERROR);
    }
  } //S3V4FileSystemSession

}
