/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Shipping
{
  /// <summary>
  /// Represents a process-global host for a shipping systems
  /// </summary>
  public class ShippingSystemHost : DaemonWithInstrumentation<object>, IShippingSystemHostImplementation
  {
    #region .ctor

      protected ShippingSystemHost(string name, IConfigSectionNode node) : this(name, node, null) { }

      protected ShippingSystemHost(string name, IConfigSectionNode node, object director) : base(director)
      {
        if (node != null) Configure(node);
        if (name.IsNotNullOrWhiteSpace()) this.Name = name;
      }

    #endregion

    #region Properties

      public override bool InstrumentationEnabled
      {
        get { return false; }
        set { }
      }

    #endregion
  }
}
