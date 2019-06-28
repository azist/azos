/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Security
{
  /// <summary>
  /// Denotes credentials that can be represented as a string that can be used for example in Authorization header
  /// </summary>
  public interface IStringRepresentableCredentials
  {
    string RepresentAsString();

    bool Forgotten { get; }

    void Forget();
  }

  /// <summary>
  /// User credentials base class. A credentials may be as simple as user+password, access card codes, door key, Twitter account token etc...
  /// </summary>
  [Serializable]
  public abstract class Credentials
  {

    private bool m_Forgotten;

    /// <summary>
    /// Indicates whether Forget() was called on this instance
    /// </summary>
    public bool Forgotten
    {
      get { return m_Forgotten; }
    }

    /// <summary>
    /// Deletes sensitive information (such as password).
    /// This method is mostly used on client (vs. server) to prevent process memory-inspection attack.
    /// Its is usually called right after Login() was called.
    /// Implementers may consider forcing post-factum GC.Collect() on all generations to make sure that orphaned
    /// memory buff with sensitive information, that remains in RAM even after all references are killed, gets
    /// compacted; consequently, this method may take considerable time to execute.
    /// </summary>
    public virtual void Forget()
    {
      m_Forgotten = true;
    }
  }

}
