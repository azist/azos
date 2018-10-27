/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Environment;
using Azos.Web.Shipping;

namespace Azos.Tests.Integration.Web.Shipping
{
  public class FakeShippingSystemHost : ShippingSystemHost
  {
    #region .ctor

      protected FakeShippingSystemHost(string name, IConfigSectionNode node)
        : base(name, node)
      {
      }

      protected FakeShippingSystemHost(string name, IConfigSectionNode node, object director)
        : base(name, node, director)
      {
      }

    #endregion
  }
}
