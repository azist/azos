/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Web.Cloud
{
  public abstract class CloudTemplate : Collections.INamed, IConfigurable
  {
    #region STATIC
    public static TTemplate Make<TTemplate>(CloudSystem system, IConfigSectionNode node)
      where TTemplate : CloudTemplate
    { return FactoryUtils.MakeAndConfigure<TTemplate>(node, typeof(TTemplate), args: new object[] { system }); }
    #endregion

    #region .ctor
    public CloudTemplate(CloudSystem system) { }
    public CloudTemplate(CloudSystem system, IConfigSectionNode node) { Configure(node); }
    #endregion

    #region Properties
    [Config] public string Name { get; set; }
    #endregion

    #region Public
    public virtual void Configure(IConfigSectionNode node) { ConfigAttribute.Apply(this, node); }
    #endregion
  }
}
