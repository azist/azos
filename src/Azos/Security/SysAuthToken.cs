/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.IO;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Represents security provider-internal ID that SecurityManager assigns into User object on authentication.
  /// These tokens can be used in place of Credentials to re-authenticate users or to re-query user rights (e.g. upon re/authorization).
  /// External parties should never be supplied this struct as it is a system backend-internal token meant to be used only inside the system
  /// perimeter (e.g. corporate intranet, data center network etc.)
  /// </summary>
  [Serializable]
  public struct SysAuthToken : IEquatable<SysAuthToken>, IJsonWritable, IJsonReadable, IRequired
  {
    public const string DELIMIT = "::";

    public SysAuthToken(string realm, string data)
    {
      m_Realm = realm.NonBlank(nameof(realm));
      m_Data = data.NonBlank(nameof(data));
    }

    public SysAuthToken(string realm, byte[] data)
    {
      m_Realm = realm.NonBlank(nameof(realm));
      m_Data = data.NonNull(nameof(data))
                   .IsTrue(d => d.Length > 0, nameof(data))
                   .ToWebSafeBase64();
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
    public byte[] BinData => m_Data==null ? null : m_Data.TryFromWebSafeBase64();

    /// <summary>
    /// Returns true when the structure contains data
    /// </summary>
    public bool Assigned => m_Realm.IsNotNullOrWhiteSpace() || m_Data.IsNotNullOrWhiteSpace();

    public bool CheckRequired(string targetName) => Assigned;

    public override string ToString()  => Realm + DELIMIT + Data;

    public override int GetHashCode() => Realm.GetHashCode() ^ Data.GetHashCode();

    public override bool Equals(object obj) => obj is SysAuthToken other ? this.Equals(other) : false;

    public bool Equals(SysAuthToken other) => this.m_Realm.EqualsOrdSenseCase(other.m_Realm) &&
                                              this.m_Data.EqualsOrdSenseCase(other.m_Data);

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options) => JsonWriter.EncodeString(wri, ToString(), options);

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data == null) return (true, new SysAuthToken());

      if (data is string str && TryParse(str, out var t)) return (true, t);

      return (false, null);
    }


    public static bool operator ==(SysAuthToken a, SysAuthToken b) => a.Equals(b);
    public static bool operator !=(SysAuthToken a, SysAuthToken b) => !a.Equals(b);

    /// <summary>
    /// Tries to parse the token represented by string obtained from ToString() call.
    /// Null/Empty strings are treated as a successful conversion to an unassigned value
    /// </summary>
    public static bool TryParse(string token, out SysAuthToken parsed)
    {
      parsed = new SysAuthToken();

      if (token.IsNullOrWhiteSpace()) return true;//null

      var i = token.IndexOf(DELIMIT);
      if (i<=0 || i+DELIMIT.Length >= token.Length) return false;

      var realm = token.Substring(0, i);
      var data = token.Substring(i+DELIMIT.Length);
      if (realm.IsNullOrWhiteSpace() || data.IsNullOrWhiteSpace()) return false;
      parsed = new SysAuthToken(realm, data);
      return true;
    }

    /// <summary>
    /// Parses the token from string throwing if not possible. Null/empty string result in unassigned tokens
    /// </summary>
    public static SysAuthToken Parse(string token)
    {
      if (TryParse(token, out var t)) return t;
      throw new SecurityException("Could not .Parse(`{0}`)".Args(token.TakeFirstChars(32, "..")));
    }

  }
}
