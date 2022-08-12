/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Tools.Wport
{
  public abstract class ActionBase : ApplicationComponent, IApplicationComponent, IConfigurable
  {
    protected ActionBase(IApplication application, Uri uri) : base(application)
    {
      Uri = uri;
    }

    public readonly Uri Uri;

    public override string ComponentLogTopic => CoreConsts.WEB_TOPIC;

    public virtual void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }

    public abstract void Run();

  }
}
