/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Text;
using System.IO;

using Azos.Apps;
using Azos.IO.FileSystem.S3.V4;
using Azos.Scripting;

namespace Azos.Tests.Integration.IO.FileSystem.S3.V4
{
  [Runnable]
  public class S3V4Tests: ExternalCfg
  {
    [Run]
    public void PutFolder()
    {
      using(new AzosApplication(null, LACONF.AsLaconicConfig()))
      {
        string fullFolderName = S3_DXW_ROOT;
        S3V4URI folderUri = S3V4URI.CreateFolder(fullFolderName);

        if (S3V4.FolderExists(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0))
          S3V4.RemoveFolder(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0);

        S3V4.PutFolder(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0);

        PutFile();

        S3V4.RemoveFolder(folderUri, S3_ACCESSKEY, S3_SECRETKEY, 0);
      }
    }

    public void PutFile()
    {
      string fullFileName = S3_DXW_ROOT + "/" + S3_FN1;
      S3V4URI fileUri = new S3V4URI(fullFileName);

      if (S3V4.FileExists(fileUri, S3_ACCESSKEY, S3_SECRETKEY, 0))
        S3V4.RemoveFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, 0);

      S3V4.PutFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, S3_CONTENTSTREAM1, 0);

      using (MemoryStream ms = new MemoryStream())
      {
        S3V4.GetFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, ms, 0);

        byte[] s3FileContentBytes = ms.GetBuffer();

        string s3FileContentStr = Encoding.UTF8.GetString(s3FileContentBytes, 0, (int)ms.Length);

        Aver.AreEqual(S3_CONTENTSTR1, s3FileContentStr);
      }

      S3V4.RemoveFile(fileUri, S3_ACCESSKEY, S3_SECRETKEY, 0);
    }
  }
}
