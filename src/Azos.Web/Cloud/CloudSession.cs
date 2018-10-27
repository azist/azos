/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Conf;

namespace Azos.Web.Cloud
{
  public class CloudSession : DisposableObject, Collections.INamed
  {
    #region .ctor
    protected CloudSession(CloudSystem cloudSystem, CloudConnectionParameters cParams)
    {
      if (cloudSystem == null || cParams == null)
        throw new CloudException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor((cloudSystem|cParams)=null)");

      CloudSystem = cloudSystem;
      ConnectionParameters = cParams;

      lock (CloudSystem.Sessions)
        CloudSystem.Sessions.Add(this);
    }

    protected override void Destructor()
    {
      if (CloudSystem != null)
        lock (CloudSystem.Sessions)
          CloudSystem.Sessions.Remove(this);

      base.Destructor();
    }
    #endregion

    #region Fields
    public readonly CloudSystem CloudSystem;
    protected readonly CloudConnectionParameters ConnectionParameters;
    #endregion

    #region Properties
    public string Name { get { return ConnectionParameters.Name; } }
    #endregion

    #region Public
    public void Deploy(string id, string templateName, IConfigSectionNode node = null)
    {
      var template = CloudSystem.Templates[templateName];
      if (template == null)
        throw new CloudException("TODO");
      Deploy(id, template, node);
    }

    public void Deploy(string id, IConfigSectionNode conf, IConfigSectionNode node = null)
    {
      var template = CloudSystem.MakeTemplate(conf);
      Deploy(id, template, node);
    }

    public void Deploy(string id, CloudTemplate template, IConfigSectionNode node = null)
    {
      CloudSystem.Deploy(this, id, template, node);
    }
    #endregion
  }
}
