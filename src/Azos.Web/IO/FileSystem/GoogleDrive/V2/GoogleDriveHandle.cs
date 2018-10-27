/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.IO.FileSystem.GoogleDrive.V2
{
  /// <summary>
  /// Google Drive file handle
  /// </summary>
  public class GoogleDriveHandle : IFileSystemHandle
  {
    #region Properties

      public string Id { get; set; }
      public string Name { get; set; }
      public bool IsFolder { get; set; }
      public bool IsReadOnly { get; set; }
      public ulong Size { get; set; }
      public DateTime CreatedDate { get; set; }
      public DateTime ModifiedDate { get; set; }

    #endregion

    #region .ctor

      public GoogleDriveHandle(JSONDataMap info)
      {
        Id            = info[Metadata.ID].AsString();
        Name          = info[Metadata.TITLE].AsString();
        IsFolder      = info[Metadata.MIME_TYPE].Equals(GoogleDriveMimeType.FOLDER);
        Size          = info[Metadata.FILE_SIZE].AsULong();
        CreatedDate   = info[Metadata.CREATED_DATE].AsDateTime();
        ModifiedDate  = info[Metadata.MODIFIED_DATE].AsDateTime();
        IsReadOnly    = !info[Metadata.EDITABLE].AsBool();
      }

    #endregion
  }
}
