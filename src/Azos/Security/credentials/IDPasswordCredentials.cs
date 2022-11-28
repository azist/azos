/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Text;

using Azos.Conf;

namespace Azos.Security
{
  /// <summary>
  /// Represents simple ID/password textual credentials.
  /// Note: The password is stored as plain text
  /// </summary>
  [Serializable]
  public sealed class IDPasswordCredentials : Credentials, IStringRepresentableCredentials
  {
    /// <summary>
    /// Obtains an unsecured string password as SecureBuffer.
    /// Note: The IDPasswordCredentials class is purposely designed to store password as plain text.
    /// This is needed for simple cases and HTTP application where login credentials are posted via plain text anyway
    /// </summary>
    public static SecureBuffer PlainPasswordToSecureBuffer(string password)
    {
      SecureBuffer buffer;
      if (password.IsNullOrEmpty())
        buffer = new SecureBuffer(0);
      else
      {
        var bytes = Encoding.UTF8.GetBytes(password);
        buffer = new SecureBuffer(bytes.Length);
        foreach (var b in bytes)
          buffer.Push(b);
        Array.Clear(bytes, 0, bytes.Length);
      }
      buffer.Seal();
      return buffer;
    }

    /// <summary>
    /// Creates IDPass credentials from base64 encoded auth header content as provided by RepresentAsString() method.
    /// Returns null if the content is unparsable
    /// </summary>
    public static IDPasswordCredentials FromBasicAuth(string basicAuth)
    {
      if (basicAuth.IsNullOrWhiteSpace()) return null;

      var bin = Convert.FromBase64String(basicAuth);
      var concat = Encoding.UTF8.GetString(bin).Trim();

      if (concat.IsNullOrWhiteSpace()) return null;

      var i = concat.LastIndexOf(':');//AZ #812
      if (i < 0) return new IDPasswordCredentials(concat, null);

      var id = i == 0 ? null : concat.Substring(0, i);
      var pwd = i == concat.Length - 1 ? null : concat.Substring(i + 1);

      return new IDPasswordCredentials(id, pwd);
    }

    public IDPasswordCredentials(string id, string pwd)
    {
      m_ID = id;
      m_Password = pwd;
    }

    /// <summary>
    /// Warning: storing plain credentials in config file is not secure. Use this method for the most simplistic cases
    /// like unit testing
    /// </summary>
    public IDPasswordCredentials(IConfigSectionNode cfg)
    {
      cfg.NonEmpty(nameof(cfg));
      ConfigAttribute.Apply(this, cfg);
    }

    [Config] private string m_ID;
    [Config] private string m_Password;

    public string ID => m_ID ?? string.Empty;
    public string Password => m_Password ?? string.Empty;

    /// <summary>
    /// Obtains an unsecured string password as SecureBuffer.
    /// Note: The IDPasswordCredentials class is purposely designed to store password as plain text.
    /// This is needed for simple cases and HTTP application where login credentials are posted via plain text anyway
    /// </summary>
    public SecureBuffer SecurePassword => PlainPasswordToSecureBuffer(m_Password);

    /// <summary>
    /// Deletes sensitive password information.
    /// This method is mostly used on client (vs. server) to prevent process memory-inspection attack.
    /// Its is usually called right after Login() was called.
    /// Implementers may consider forcing post-factum GC.Collect() on all generations to make sure that orphaned
    /// memory buff with sensitive information, that remains in RAM even after all references are killed, gets
    /// compacted. This class implementation DOES NOT call Gc.Collect();
    /// </summary>
    public override void Forget()
    {
      m_Password = string.Empty;
      base.Forget();
    }

    public override string ToString() => "{0}(`{1}`)".Args(GetType().Name, ID);

    /// <summary>
    /// Converts plain credentials to basic auth using base64
    /// </summary>
    public string RepresentAsString()
    {
      if (Forgotten)
        throw new SecurityException(StringConsts.SECURITY_REPRESENT_CREDENTIALS_FORGOTTEN);

      var concat = "{0}:{1}".Args(m_ID, m_Password);
      var encoded = Encoding.UTF8.GetBytes(concat);

      return Convert.ToBase64String(encoded, Base64FormattingOptions.None);
    }
  }
}
