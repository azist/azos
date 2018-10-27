/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Azos.Serialization.JSON;
using Azos.IO.FileSystem.GoogleDrive.V2;

namespace Azos.IO.FileSystem.GoogleDrive.Auth
{

  #warning This class must be moved out into Azos.Security
  /// <summary>
  /// JWT (https://developers.google.com/identity/protocols/OAuth2ServiceAccount).
  ///
  /// A JWT is composed of three parts: a header, a claim set, and a signature.
  /// The header and claim set are JSON objects. These JSON objects are serialized to UTF-8 bytes,
  /// then encoded using the Base64url encoding. This encoding provides resilience against encoding
  /// changes due to repeated encoding operations. The header, claim set, and signature are concatenated
  /// together with a period (.) character.
  ///
  /// {Base64url encoded header}.{Base64url encoded claim set}.{Base64url encoded signature}
  /// </summary>
  public class JsonWebToken
  {
    #region CONST

      private const string TYPE = "JWT";
      private const string SHA256 = "SHA256";
      private const string RS256 = "RS256";
      private const string DELIMETER = ".";
      private const string PASSWORD = "notasecret";
      private const string SCOPE = "https://www.googleapis.com/auth/drive";
      private const string AUD = "https://accounts.google.com/o/oauth2/token";

      public static readonly string GRANT_TYPE = "urn:ietf:params:oauth:grant-type:jwt-bearer";

    #endregion

    #region Inner Types

      /// <summary>
      /// JWT Header
      /// </summary>
      private class Header
      {
        public string alg { get; set; }
        public string typ { get; set; }
      }

      /// <summary>
      /// JWT Claim set
      /// </summary>
      private class Claimset
      {
        public string iss { get; set; }
        public string scope { get; set; }
        public string aud { get; set; }
        public long exp { get; set; }
        public long iat { get; set; }
      }

    #endregion

    #region Private Fields

      private RSACryptoServiceProvider m_Key;
      private Header m_Header = new Header() { alg = RS256, typ = TYPE };
      private Claimset m_Claimset;

    #endregion

    #region .ctor

      public JsonWebToken(string email, string certPath)
      {
        Ensure.NotNull(email, "email");
        Ensure.NotNull(certPath, "certPath");

        var certificate = new X509Certificate2(certPath, PASSWORD,  X509KeyStorageFlags.Exportable);

        m_Key = FromCertificate(certificate);

        m_Claimset = new Claimset
        {
            iss = email,
            scope = SCOPE,
            aud = AUD
        };

        Refresh();
      }

    #endregion

    #region Public

      public void Refresh()
      {
        if (m_Claimset == null)
          return;

        var now = DateTime.UtcNow;
        m_Claimset.iat = now.ToSecondsSinceUnixEpochStart();
        m_Claimset.exp = now.AddHours(1).ToSecondsSinceUnixEpochStart();
      }

      public override string ToString()
      {
        var jwt = new StringBuilder();

        jwt.Append(Base64UrlEncode(m_Header));
        jwt.Append(DELIMETER);
        jwt.Append(Base64UrlEncode(m_Claimset));

        var signature = Sign(jwt.ToString());
        jwt.Append(DELIMETER).Append(signature);

        return jwt.ToString();
      }

    #endregion

    #region Private

      private string Sign(string data)
      {
        var signature = m_Key.SignData(Encoding.UTF8.GetBytes(data), SHA256);
        return Base64UrlEncode(signature);
      }

      /// <summary>
      /// Workaround to correctly cast the private key as a RSACryptoServiceProvider type 24.
      /// </summary>
      private static RSACryptoServiceProvider FromCertificate(X509Certificate2 certificate)
      {
        var rsa = (RSACryptoServiceProvider)certificate.PrivateKey;
        var privateKeyBlob = rsa.ExportCspBlob(true);
        var key = new RSACryptoServiceProvider();
        key.ImportCspBlob(privateKeyBlob);
        return key;
      }

      private static string Base64UrlEncode(object input)
      {
        var json = input.ToJSON();
        var buffer = Encoding.UTF8.GetBytes(json);

        return Base64UrlEncode(buffer);
      }

      private static string Base64UrlEncode(byte[] input)
      {
        return Convert.ToBase64String(input).Replace("=", String.Empty).Replace('+', '-').Replace('/', '_');
      }

    #endregion
  }
}
