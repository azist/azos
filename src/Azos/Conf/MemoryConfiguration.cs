/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Conf
{
  /// <summary>
  /// Implements configuration that can not be persisted/loaded anywhere - just stored in memory
  /// </summary>
  [Serializable]
  public sealed class MemoryConfiguration : Configuration
  {
    /// <summary>
    /// Creates an instance of a new configuration in memory
    /// </summary>
    public MemoryConfiguration() : base() { }

    private bool m_IsReadOnly;

    /// <inheritdoc/>
    public override bool IsReadOnly => m_IsReadOnly;

    /// <summary>
    /// Sets the configuration read only backing property
    /// </summary>
    public void SetReadOnly(bool val)
    {
      m_IsReadOnly = val;
    }
  }
}
