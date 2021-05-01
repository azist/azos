/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Data.Modeling.DataTypes
{
  /// <summary>
  /// Represents a domain - a named type
  /// </summary>
  public abstract class Domain : Collections.INamed, IConfigurable
  {

    #region Properties

    /// <summary>
    /// Returns the name of this domain, i.e. 'THumanAge', 'TSalary'
    /// </summary>
    public virtual string Name => GetType().Name;

    #endregion

    public virtual void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }
}
