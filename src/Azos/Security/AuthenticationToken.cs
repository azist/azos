/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Security
{
    /// <summary>
    /// Represents security provider-internal ID that SecurityManager assigns into User object on authentication.
    /// These tokens can be used in place of Credentials to re-authenticate users or to re-query user rights (e.g. upon re/authorization).
    /// External parties should never be supplied with this struct as it is system backend internal token used inside the system
    /// </summary>
    [Serializable]
    public struct AuthenticationToken
    {
      public AuthenticationToken(string realm, object data)
      {
          m_Realm = realm;
          m_Data = data;
      }

      private string m_Realm;
      private object m_Data;

      /// <summary>
      /// Provides information about back-end security source (realm) that performed authentication, i.e. LDAP instance, Database name etc...
      /// </summary>
      public string Realm
      {
        get {return m_Realm;}
      }

      /// <summary>
      /// Provides provider-specific key/id that uniquely identifies the user in the realm
      /// </summary>
      public object Data
      {
        get { return m_Data; }
      }

      /// <summary>
      /// Returns true when the structure contains data
      /// </summary>
      public bool Assigned
      {
        get { return m_Realm.IsNotNullOrWhiteSpace() || m_Data!=null;}
      }

      public override string ToString()
      {
        return "AuthToken({0}::{1})".Args(m_Realm, m_Data);
      }
    }
}
