/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
/*
 * Author: Andrey Kolbasov <andrey@kolbasov.com>
 */
using System;
using Azos.IO.FileSystem;

namespace Azos.IO.FileSystem.GoogleDrive.V2
{
  public class GoogleDriveCapabilities : IFileSystemCapabilities
  {
    #region Static

      private static readonly char[] PATH_SEPARATORS = new char[] { '/' };

      private static GoogleDriveCapabilities s_Instance = new GoogleDriveCapabilities();

      public static GoogleDriveCapabilities Instance { get { return s_Instance; } }

    #endregion

    #region IFileSystemCapabilities

      public bool SupportsVersioning
      {
          get { return true; }
      }

      public bool SupportsTransactions
      {
          get { return false; }
      }

      public int MaxFilePathLength
      {
          get { return 260; }
      }

      public int MaxFileNameLength
      {
          get { return 255; }
      }

      public int MaxDirectoryNameLength
      {
          get { return 255; }
      }

      public ulong MaxFileSize
      {
          // 5120 GB
          get { return 5629499534213120; }
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
          get { return false; }
      }

      public bool SupportsCustomMetadata
      {
          get { return false; }
      }

      public bool SupportsDirectoryRenaming
      {
          get { return true; }
      }

      public bool SupportsFileRenaming
      {
          get { return true; }
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
          get { return true; }
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
  }
}
