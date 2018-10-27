/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Security;

namespace Azos.IO.FileSystem.GoogleDrive
{
  public class GoogleDriveParameters : FileSystemSessionConnectParams
  {
    #region CONST

      public const string CONFIG_EMAIL_ATTR = "email";
      public const string CONFIG_CERT_PATH_ATTR = "cert-path";

    #endregion

    #region .ctor

      public GoogleDriveParameters(): base() {}
      public GoogleDriveParameters(IConfigSectionNode node): base(node) {}
      public GoogleDriveParameters(string connectStr, string format = Configuration.CONFIG_LACONIC_FORMAT) : base(connectStr, format) { }

    #endregion

    #region Properties

      [Config]
      public string CertPath { get; set; }

      [Config]
      public int TimeoutMs { get; set; }

      [Config]
      public int Attempts { get; set; }

    #endregion

    #region Public

      public override void Configure(IConfigSectionNode node)
      {
        base.Configure(node);

        var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;

        var credentials = new GoogleDriveCredentials(email);

        var authToken = new AuthenticationToken();

        User = new User(credentials, authToken, UserStatus.User, name:null, descr:null, rights:Rights.None);
      }

    #endregion
  }
}
