/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Pay
{
  /// <summary>
  /// Represents a starter that launches payment systems on startup
  /// </summary>
  public sealed class PaySystemStarter : IApplicationStarter
  {
    [Config]
    public string Name { get; set; }

    [Config]
    public bool ApplicationStartBreakOnException { get; set; }

    public void ApplicationStartBeforeInit(IApplication application) { }

    public void ApplicationStartAfterInit(IApplication application)
    {
      PaySystem.AutoStartSystems();
    }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }

}
