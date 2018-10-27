/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Security;
using Azos.IO.FileSystem;

namespace Azos.IO.FileSystem.GoogleDrive.V2
{
    public class GoogleDriveSession : FileSystemSession
    {
      #region Properties

        public GoogleDriveClient Client { get; private set; }

      #endregion

      #region .ctor

        protected internal GoogleDriveSession(GoogleDriveFileSystem fs, IFileSystemHandle handle, GoogleDriveParameters cParams)
          : base(fs, handle, cParams)

        {
          var email  = (m_User.Credentials as GoogleDriveCredentials).Email;
          Client = new GoogleDriveClient(email, cParams.CertPath, cParams.TimeoutMs, cParams.Attempts);
        }

      #endregion
    }
}
