/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data;


namespace Azos.Glue
{
  /// <summary>
  /// Represents a network node. It is a binding, logical address of a host and a service that host provides
  /// Nodes are not contract-dependent. The components of address are not case-sensitive.
  /// The form of the address is: <code>binding://host:service</code>. The "host" and "service" segment syntaxes depend on binding and may not contain the ':' char.
  /// An example of some 'mytest' binding: 'mytest://adr=1.1.1.1,nic=eth001:job,chat,backup'
  /// </summary>
  [Serializable]
  public struct Node : Collections.INamed, IEquatable<Node>, IRequired
  {
    public const string BINDING_SEPARATOR = "://";
    public const string SERVICE_SEPARATOR = ":";


    //the string is stored as a whole for serialization efficiency
    private string m_ConnectString;

    /// <summary>
    /// Inits a node struct. It is a binding, logical address of a host and a service that host provides
    /// Nodes are not contract-dependent. The componets of address are not case-sensitive.
    /// The form of the address is: <code>binding://host:service</code>. The "host" and "service" segment syntaxes depend on binding and may not contain the ':' char.
    /// An example of some 'mytest' binding: 'mytest://adr=1.1.1.1,nic=eth001:job,chat,backup'
    /// </summary>
    public Node(string connectString)
    {
      m_ConnectString = connectString.NonBlank(connectString);
    }


    /// <summary>
    /// Gets a connection string - a structured URL-like connection descriptor that identifies a host
    ///  along with binding and service. The components of address are not case-sensitive.
    /// The form of the address is: <code>binding://host:service</code>. The "host" and "service" segment syntaxes depend on binding and may not contain the ':' char.
    /// An example of some 'mytest' binding: 'mytest://adr=1.1.1.1,nic=eth001:job,chat,backup'
    /// </summary>
    public string ConnectString => m_ConnectString ?? string.Empty;


    /// <summary>
    /// INamed shortcut to ConnectString
    /// </summary>
    public string Name => ConnectString;

    /// <summary>
    /// Returns true when struct has some data assigned i.e. connect string is specified
    /// </summary>
    public bool Assigned => m_ConnectString.IsNotNullOrWhiteSpace();


    public bool CheckRequired(string targetName) => Assigned;

    /// <summary>
    /// Gets binding portion of ConnectString. This value selects binding adapter
    /// </summary>
    public string Binding
    {
      get
      {
        if (m_ConnectString.IsNullOrWhiteSpace()) return string.Empty;
        var i = m_ConnectString.IndexOf(BINDING_SEPARATOR);
        if (i<=0) return string.Empty;
        return m_ConnectString.Substring(0, i);
      }
    }

    /// <summary>
    /// Gets host portion of ConnectString. This value may have a structure of its own which is understood by binding adapter
    /// </summary>
    public string Host
    {
      get
      {
        if (m_ConnectString.IsNullOrWhiteSpace()) return string.Empty;
        var i = m_ConnectString.IndexOf(BINDING_SEPARATOR);
        if (i<0)//binding spec missing
        {
          i = m_ConnectString.IndexOf(SERVICE_SEPARATOR);
          if (i<=0) return m_ConnectString;
          return m_ConnectString.Substring(0, i);
        }
        else
        {
          i+=BINDING_SEPARATOR.Length;
          var j= m_ConnectString.IndexOf(SERVICE_SEPARATOR, i);
          if (j<0) return m_ConnectString.Substring(i);
          return m_ConnectString.Substring(i, j-i);
        }
      }
    }

    /// <summary>
    /// Gets service/port portion of ConnectString. This value may have a structure of its own which is understood by binding adapter
    /// </summary>
    public string Service
    {
      get
      {
        if (m_ConnectString.IsNullOrWhiteSpace()) return string.Empty;
        var i = m_ConnectString.IndexOf(BINDING_SEPARATOR);
        var j = m_ConnectString.LastIndexOf(SERVICE_SEPARATOR);

        if (j<0 || j<=i || j+1>=m_ConnectString.Length) return string.Empty;

        return m_ConnectString.Substring(j+1);

      }
    }

    public override string ToString() => Assigned ? ConnectString : CoreConsts.UNKNOWN;
    public override int GetHashCode() => m_ConnectString==null ? 0 : this.m_ConnectString.GetHashCodeOrdIgnoreCase();
    public bool Equals(Node other) => m_ConnectString.EqualsOrdIgnoreCase(other.m_ConnectString);
    public override bool Equals(object obj) => obj is Node n ? this.Equals(n) : false;

    public static bool operator ==(Node a, Node b) =>  a.Equals(b);
    public static bool operator !=(Node a, Node b) => !a.Equals(b);
  }

}
