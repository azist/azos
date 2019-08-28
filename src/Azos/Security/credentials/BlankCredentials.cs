/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Security
{
  /// <summary>
  /// Represents an absence of credentials. This is a singleton class
  /// </summary>
  [Serializable]
  public sealed class BlankCredentials : Credentials
  {
    private BlankCredentials(){ }

    private static BlankCredentials m_Instance;

    /// <summary>
    /// Singleton instance of blank credentials
    /// </summary>
    public static BlankCredentials Instance
    {
      get
      {
        if (m_Instance==null)
          m_Instance = new BlankCredentials();
        return m_Instance;
      }
    }
  }
}
