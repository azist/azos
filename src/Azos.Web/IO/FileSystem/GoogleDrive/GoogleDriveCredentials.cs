
using Azos.Security;

namespace Azos.IO.FileSystem.GoogleDrive
{
  /// <summary>
  /// Google Drive credentials
  /// </summary>
  public class GoogleDriveCredentials : Credentials
  {
    #region Properties

      public string Email { get; set; }

    #endregion

    #region .ctor

      public GoogleDriveCredentials(string email)
      {
        Email = email;
      }

    #endregion
  }
}
