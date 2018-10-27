/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azos.IO.FileSystem.S3.V4.S3V4Sign;
using Azos.Scripting;

namespace Azos.Tests.Unit.IO.FileSystem.S3.V4.S3V4Sign
{
  [Runnable]
  public class AuthorizationStringTest : Base, IRunnableHook
  {
    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      initCONSTS();
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error) => false;


    [Run]
    public void AutorizationGetQueryParameters()
    {
      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      IDictionary<string, string> queryParameters = new Dictionary<string, string>() {
        {"marker", "1"},
        {"delimiter", "/"}
      };

      S3V4Signer s3v4 = new S3V4Signer() {
        AccessKey = ACCESSKEY,
        SecretKey = SECRETKEY,
        Bucket = BUCKET,
        Region = REGION,
        Method = "GET",
        RequestDateTime = dateTime,
        QueryParams = queryParameters
      };


      string expected = "AWS4-HMAC-SHA256 Credential={0}/20140217/us-west-2/s3/aws4_request, SignedHeaders=content-type;host;x-amz-content-sha256;x-amz-date, ".Args(ACCESSKEY) +
        "Signature=9cda8f26243f1874d921938037a3fc6839f3067092bc305dc4b41dc810a98edb";

      Aver.AreEqual(5, s3v4.Headers.Count);
      Aver.AreEqual(expected, s3v4.Headers["Authorization"]);
    }

    [Run]
    public void AutorizationDeleteFile()
    {
      DateTime dateTime = new DateTime(2014, 02, 17, 11, 11, 11, DateTimeKind.Utc);

      S3V4Signer s3v4 = new S3V4Signer()
      {
        AccessKey = ACCESSKEY,
        SecretKey = SECRETKEY,
        Bucket = BUCKET,
        Region = REGION,
        ItemLocalPath = ITEM_RELATIVE_PATH,
        Method = "DELETE",
        RequestDateTime = dateTime
      };

      string expected = "AWS4-HMAC-SHA256 Credential={0}/20140217/us-west-2/s3/aws4_request, SignedHeaders=content-type;host;x-amz-content-sha256;x-amz-date, ".Args(ACCESSKEY) +
        "Signature=b9d74d6372c55ee54b89c9f44c866edd2577aca878d05a8eccf0cec4a921e07a";

      Aver.AreEqual(5, s3v4.Headers.Count);
      Aver.AreEqual(expected, s3v4.Headers["Authorization"]);
    }

    [Run]
    public void AutorizationPutFile()
    {
      DateTime dateTime = new DateTime(2015, 02, 17, 11, 11, 11, DateTimeKind.Utc);

      MemoryStream contentStream = new MemoryStream(Encoding.UTF8.GetBytes(CONTENT));

      S3V4Signer s3v4 = new S3V4Signer()
      {
        AccessKey = ACCESSKEY,
        SecretKey = SECRETKEY,
        Bucket = BUCKET,
        Region = REGION,
        ItemLocalPath = ITEM_RELATIVE_PATH,
        Method = "PUT",
        RequestDateTime = dateTime,
        ContentStream = contentStream
      };

      string expected = "AWS4-HMAC-SHA256 Credential={0}/20150217/us-west-2/s3/aws4_request, SignedHeaders=content-length;content-type;host;x-amz-content-sha256;x-amz-date, ".Args(ACCESSKEY) +
        "Signature=b0226cf0a900e16489d7f1feaace3f1c6d9e9f516fac2ebd61177b1f3d9b98ea";

      Aver.AreEqual(6, s3v4.Headers.Count);
      Aver.AreEqual(expected, s3v4.Headers["Authorization"]);
    }
  }
}
