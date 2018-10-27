/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.IO.FileSystem.S3.V4
{
  public class S3V4FileSystemCapabilities : IFileSystemCapabilities
  {
    #region Static /.ctor

      private static readonly char[] PATH_SEPARATORS = new char[]{'/'};

      private static S3V4FileSystemCapabilities s_Instance = new S3V4FileSystemCapabilities();

      public static S3V4FileSystemCapabilities Instance { get { return s_Instance;} }

    #endregion

    #region ctor

      private S3V4FileSystemCapabilities() {}

    #endregion

    #region IFileSystemCapabilities

      public bool SupportsVersioning
      {
        get { return false; }
      }

      public bool SupportsTransactions
      {
        get { return false; }
      }

      public int MaxFilePathLength
      {
        get { return 2048; }
      }

      public int MaxFileNameLength
      {
        get { return 1024; }
      }

      public int MaxDirectoryNameLength
      {
        get { return 256; }
      }

      public ulong MaxFileSize
      {
        get { return 104857600; }
      }

      public char[] PathSeparatorCharacters
      {
        get { return PATH_SEPARATORS; }
      }

      public bool IsReadonly
      {
        get { return false; }
      }

      public bool SupportsSecurity
      {
        get { return true; }
      }

      public bool SupportsCustomMetadata
      {
        get { return true; }
      }

      public bool SupportsDirectoryRenaming
      {
        get { return false; }
      }

      public bool SupportsFileRenaming
      {
        get { return false; }
      }

      public bool SupportsStreamSeek
      {
        get { return false; }
      }

      public bool SupportsFileModification
      {
        get { return true; }
      }

      public bool SupportsCreationTimestamps
      {
        get { return false; }
      }

      public bool SupportsModificationTimestamps
      {
        get { return true; }
      }

      public bool SupportsLastAccessTimestamps
      {
        get { return false; }
      }

      public bool SupportsReadonlyDirectories
      {
        get { return false; }
      }

      public bool SupportsReadonlyFiles
      {
        get { return false; }
      }

      public bool SupportsCreationUserNames
      {
        get { return true; }
      }

      public bool SupportsModificationUserNames
      {
        get { return false; }
      }

      public bool SupportsLastAccessUserNames
      {
        get { return false; }
      }

      public bool SupportsFileSizes
      {
        get { return true; }
      }

      public bool SupportsDirectorySizes
      {
        get { return false; }
      }

      public bool SupportsAsyncronousAPI
      {
        get { return false; }
      }

    #endregion
  } //S3FileSystemCapabilities

}
