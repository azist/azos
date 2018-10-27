/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using Azos.IO.FileSystem.S3.V4.S3V4Sign;
using Azos.Scripting;

namespace Azos.Tests.Unit.IO.FileSystem.S3.V4.S3V4Sign
{
  [Runnable]
  public class CanonicalRequest: Base, IRunnableHook
  {
    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      initCONSTS();
      m_S3V4 = new S3V4Signer() { AccessKey = ACCESSKEY, SecretKey = SECRETKEY, Region = REGION, Bucket = BUCKET };
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error) => false;

    [Run]
    public void ListBuckets()
    {
      Uri uri = S3V4URLHelpers.CreateURI();

      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      IDictionary<string, string> headers = new Dictionary<string, string>() {
        {"content-type", "text/plain"},
        {"Host", uri.Host},
        {S3V4Signer.X_AMZ_CONTENT_SHA256, S3V4Signer.EMPTY_BODY_SHA256},
        {S3V4Signer.X_AMZ_DATE, dateTime.S3DateTimeString()}
      };

      string r = S3V4Signer.GetCanonicalRequest("GET", uri, headers);

      string expected =
        "GET\n" +
        "/\n" +
        "\n" +
        "content-type:text/plain\n" +
        "host:s3.amazonaws.com\n" +
        "x-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n" +
        "x-amz-date:20140217T101010Z\n" +
        "\n" +
        "content-type;host;x-amz-content-sha256;x-amz-date\n" +
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      Aver.AreEqual(expected, r);
    }

    [Run]
    public void ListBucketFiles()
    {
      Uri uri = S3V4URLHelpers.CreateURI(REGION, BUCKET);

      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      IDictionary<string, string> headers = new Dictionary<string, string>() {
        {"content-type", "text/plain"},
        {"Host", uri.Host},
        {S3V4Signer.X_AMZ_CONTENT_SHA256, S3V4Signer.EMPTY_BODY_SHA256},
        {S3V4Signer.X_AMZ_DATE, dateTime.S3DateTimeString()}
      };

      string r = S3V4Signer.GetCanonicalRequest("GET", uri, headers);

      string expected =
        "GET\n" +
        "/\n" +
        "\n" +
        "content-type:text/plain\n" +
        "host:dxw.s3-us-west-2.amazonaws.com\n" +
        "x-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n" +
        "x-amz-date:20140217T101010Z\n" +
        "\n" +
        "content-type;host;x-amz-content-sha256;x-amz-date\n" +
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      Aver.AreEqual( expected, r);
    }

    [Run]
    public void GetFile()
    {
      Uri uri = S3V4URLHelpers.CreateURI(REGION, BUCKET, ITEM_RELATIVE_PATH);

      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      IDictionary<string, string> headers = new Dictionary<string, string>() {
        {"content-type", "text/plain"},
        {"Host", uri.Host},
        {S3V4Signer.X_AMZ_CONTENT_SHA256, S3V4Signer.EMPTY_BODY_SHA256},
        {S3V4Signer.X_AMZ_DATE, dateTime.S3DateTimeString()}
      };

      string r = S3V4Signer.GetCanonicalRequest("GET", uri, headers);

      string expected =
        "GET\n" +
        "/Folder01/Test01.txt\n" +
        "\n" +
        "content-type:text/plain\n" +
        "host:dxw.s3-us-west-2.amazonaws.com\n" +
        "x-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n" +
        "x-amz-date:20140217T101010Z\n" +
        "\n" +
        "content-type;host;x-amz-content-sha256;x-amz-date\n" +
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      Aver.AreEqual(expected, r);
    }

    [Run]
    public void PutFile()
    {
      Uri uri = S3V4URLHelpers.CreateURI(REGION, BUCKET, ITEM_RELATIVE_PATH);

      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      string contentHash = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f";

      IDictionary<string, string> headers = new Dictionary<string, string>() {
        {"content-type", "text/plain"},
        {"content-length", 13.ToString()},
        {"Host", uri.Host},
        {S3V4Signer.X_AMZ_CONTENT_SHA256, contentHash},
        {S3V4Signer.X_AMZ_DATE, dateTime.S3DateTimeString()}
      };

      string r = S3V4Signer.GetCanonicalRequest("PUT", uri, headers, hashedPayload: contentHash);

      string expected =
        "PUT\n" +
        "/Folder01/Test01.txt\n" +
        "\n" +
        "content-length:13\n" +
        "content-type:text/plain\n" +
        "host:dxw.s3-us-west-2.amazonaws.com\n" +
        "x-amz-content-sha256:dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f\n" +
        "x-amz-date:20140217T101010Z\n" +
        "\n" +
        "content-length;content-type;host;x-amz-content-sha256;x-amz-date\n" +
        "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f";

      Aver.AreEqual(expected, r);
    }

    [Run]
    public void HeadFile()
    {
      Uri uri = S3V4URLHelpers.CreateURI(REGION, BUCKET, ITEM_RELATIVE_PATH);

      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      string contentHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      IDictionary<string, string> headers = new Dictionary<string, string>() {
        {"content-type", "text/plain"},
        {"Host", uri.Host},
        {S3V4Signer.X_AMZ_CONTENT_SHA256, contentHash},
        {S3V4Signer.X_AMZ_DATE, dateTime.S3DateTimeString()}
      };

      string r = S3V4Signer.GetCanonicalRequest("HEAD", uri, headers, hashedPayload: contentHash);

      string expected =
        "HEAD\n" +
        "/Folder01/Test01.txt\n" +
        "\n" +
        "content-type:text/plain\n" +
        "host:dxw.s3-us-west-2.amazonaws.com\n" +
        "x-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n" +
        "x-amz-date:20140217T101010Z\n" +
        "\n" +
        "content-type;host;x-amz-content-sha256;x-amz-date\n" +
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      Aver.AreEqual(expected, r);
    }

    [Run]
    public void DeleteFile()
    {
      Uri uri = S3V4URLHelpers.CreateURI(REGION, BUCKET, ITEM_RELATIVE_PATH);

      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      string contentHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      IDictionary<string, string> headers = new Dictionary<string, string>() {
        {"content-type", "text/plain"},
        {"Host", uri.Host},
        {S3V4Signer.X_AMZ_CONTENT_SHA256, contentHash},
        {S3V4Signer.X_AMZ_DATE, dateTime.S3DateTimeString()}
      };

      string r = S3V4Signer.GetCanonicalRequest("DELETE", uri, headers, hashedPayload: contentHash);

      string expected =
        "DELETE\n" +
        "/Folder01/Test01.txt\n" +
        "\n" +
        "content-type:text/plain\n" +
        "host:dxw.s3-us-west-2.amazonaws.com\n" +
        "x-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n" +
        "x-amz-date:20140217T101010Z\n" +
        "\n" +
        "content-type;host;x-amz-content-sha256;x-amz-date\n" +
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      Aver.AreEqual(expected, r);
    }

    [Run]
    public void GetQueryParameters()
    {
      Uri uri = S3V4URLHelpers.CreateURI(REGION, BUCKET);

      DateTime dateTime = new DateTime(2014, 02, 17, 10, 10, 10, DateTimeKind.Utc);

      string contentHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      IDictionary<string, string> headers = new Dictionary<string, string>() {
        {"content-type", "text/plain"},
        {"Host", uri.Host},
        {S3V4Signer.X_AMZ_CONTENT_SHA256, contentHash},
        {S3V4Signer.X_AMZ_DATE, dateTime.S3DateTimeString()}
      };

      IDictionary<string, string> queryParameters = new Dictionary<string, string>() {
        {"marker", "1"},
        {"delimiter", "/"}
      };

      string r = S3V4Signer.GetCanonicalRequest("GET", uri, headers, queryParameters, contentHash);

      string expected =
        "GET\n" +
        "/\n" +
        "delimiter=%2F&marker=1\n" +
        "content-type:text/plain\n" +
        "host:dxw.s3-us-west-2.amazonaws.com\n" +
        "x-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n" +
        "x-amz-date:20140217T101010Z\n" +
        "\n" +
        "content-type;host;x-amz-content-sha256;x-amz-date\n" +
        "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

      Aver.AreEqual(expected, r);
    }

    protected static S3V4Signer m_S3V4;
  }
}
