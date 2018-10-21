
using System;
using System.Collections.Generic;

namespace Azos.IO.FileSystem.GoogleDrive.V2
{
  /// <summary>
  /// Represents a request to Google Drive REST API
  /// </summary>
  class GoogleDriveRequestBody : Dictionary<string, object>
  {
    #region CONST

      private const string RFC3339 = "yyyy-MM-dd'T'HH:mm:ss.fffffffK";

    #endregion

    #region Public

      public void SetTitle(string title)
      {
        Set(Metadata.TITLE, title);
      }

      public void SetParent(string parentId)
      {
        var parent = new Dictionary<string, string>();
        parent[Metadata.ID] = parentId;

        var parents = new Dictionary<string, string>[] { parent };

        Set(Metadata.PARENTS, parents);
      }

      public void SetModifiedDate(DateTime date)
      {
        Set(Metadata.MODIFIED_DATE, date.ToString(RFC3339));
      }

      public void SetMimeType(string mimeType)
      {
        Set(Metadata.MIME_TYPE, mimeType);
      }

    #endregion

    #region Private

      private void Set(string key, object value)
      {
        this[key] = value;
      }

    #endregion
  }
}
