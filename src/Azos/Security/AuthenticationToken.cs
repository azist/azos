/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.IO;
using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Represents security provider-internal ID that SecurityManager assigns into User object on authentication.
  /// These tokens can be used in place of Credentials to re-authenticate users or to re-query user rights (e.g. upon re/authorization).
  /// External parties should never be supplied with this struct as it is system backend internal token used inside the system
  /// </summary>
  [Serializable]
  public struct AuthenticationToken : IEquatable<AuthenticationToken>, IJsonWritable, IJsonReadable
  {
    public const string DELIMIT = "://";

    public AuthenticationToken(string realm, string data)
    {
      m_Realm = realm.NonBlank(nameof(realm));
      m_Data = data.NonBlank(nameof(data));
    }

    public AuthenticationToken(string realm, byte[] data)
    {
      m_Realm = realm.NonBlank(nameof(realm));
      m_Data = data.NonNull(nameof(data)).ToWebSafeBase64();
    }

    private string m_Realm;
    private string m_Data;

    /// <summary>
    /// Provides information about back-end security source (realm) that performed authentication, i.e. LDAP instance, Database name etc...
    /// </summary>
    public string Realm => m_Realm ?? CoreConsts.NULL_STRING;

    /// <summary>
    /// Provides provider-specific key/id that uniquely identifies the user in the realm
    /// </summary>
    public string Data => m_Data ?? CoreConsts.NULL_STRING;

    /// <summary>
    /// Returns byte[] representation of base64-encoded data or null
    /// </summary>
    public byte[] BinData => m_Data==null ? null : m_Data.FromWebSafeBase64();

    /// <summary>
    /// Returns true when the structure contains data
    /// </summary>
    public bool Assigned => m_Realm.IsNotNullOrWhiteSpace() || m_Data.IsNotNullOrWhiteSpace();

    public override string ToString()  => Realm + DELIMIT + Data;

    public override int GetHashCode() => Realm.GetHashCode() ^ Data.GetHashCode();

    public override bool Equals(object obj) => obj is AuthenticationToken other ? this.Equals(other) : false;

    public bool Equals(AuthenticationToken other) => this.m_Realm.EqualsOrdSenseCase(other.m_Realm) &&
                                                     this.m_Data.EqualsOrdSenseCase(other.m_Data);

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options) => wri.Write(ToString());

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.NameBinding? nameBinding)
    {
      if (data == null) return (true, new AuthenticationToken());

      if (data is string str && TryParse(str, out var t)) return (true, t);

      return (false, null);
    }


    public static bool operator ==(AuthenticationToken a, AuthenticationToken b) => a.Equals(b);
    public static bool operator !=(AuthenticationToken a, AuthenticationToken b) => !a.Equals(b);

    /// <summary>
    /// Tries to parse the token represented by string obtained from ToString() call.
    /// Null/Empty strings are treated as a successful conversion to unassigned
    /// </summary>
    public static bool TryParse(string token, out AuthenticationToken parsed)
    {
      parsed = new AuthenticationToken();

      if (token.IsNullOrWhiteSpace()) return true;//null

      var i = token.IndexOf(DELIMIT);
      if (i<=0 || i+DELIMIT.Length >= token.Length) return false;

      var realm = token.Substring(0, i);
      var data = token.Substring(i+DELIMIT.Length);
      parsed = new AuthenticationToken(realm, data);
      return true;
    }

    /// <summary>
    /// Parses the token from string throwing if not possible. Null/empty string result in unassigned tokens
    /// </summary>
    public static AuthenticationToken Parse(string token)
    {
      if (TryParse(token, out var t)) return t;
      throw new SecurityException("Could not .Parse(`{0}`)".Args(token.TakeFirstChars(32, "..")));
    }

  }
}
