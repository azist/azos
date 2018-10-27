/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using Azos.Conf;
using Azos.Scripting;
using CONN_PARAMS = Azos.IO.FileSystem.S3.V4.S3V4FileSystemSessionConnectParams;

namespace Azos.Tests.Unit.IO.FileSystem.S3.V4.S3V4Sign
{
  public class Base
  {
    public static string ACCESSKEY;
    public static string SECRETKEY;

    public const string HOST = "http://s3.amazonaws.com";
    public static string REGION;
    public static string BUCKET;
    public const string ITEM_RELATIVE_PATH = "Folder01/Test01.txt";

    public const string CONTENT = "Hello, World!";

    protected const string NFX_S3 = "NFX_S3";

    protected static void initCONSTS()
    {
      try
      {
        string envVarsStr = System.Environment.GetEnvironmentVariable(NFX_S3);

        var cfg = Configuration.ProviderLoadFromString(envVarsStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

        BUCKET = cfg.AttrByName(CONN_PARAMS.CONFIG_BUCKET_ATTR).Value;
        REGION = cfg.AttrByName(CONN_PARAMS.CONFIG_REGION_ATTR).Value;
        ACCESSKEY = cfg.AttrByName(CONN_PARAMS.CONFIG_ACCESSKEY_ATTR).Value;
        SECRETKEY = cfg.AttrByName(CONN_PARAMS.CONFIG_SECRETKEY_ATTR).Value;
      }
      catch (Exception ex)
      {
        throw new Exception( string.Format(
            "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added",
              NFX_S3,
              "s3{ bucket='bucket01' region='us-west-2' access-key='XXXXXXXXXXXXXXXXXXXX' secret-key='XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX'}"),
          ex);
      }
    }
  }
}
