/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Client;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Sky.FileGateway.Server
{
  /// <summary>
  /// Represents server gateway system implementation
  /// </summary>
  public class GatewaySystem : ApplicationComponent<DefaultFileGatewayLogic>, IAtomNamed
  {
    public const string CONFIG_SYSTEM_SECTION = "system";

    internal GatewaySystem(DefaultFileGatewayLogic director, IConfigSectionNode conf) : base(director)
    {
      ConfigAttribute.Apply(this, conf.NonEmpty(nameof(conf)));
      m_Name.HasRequiredValue(nameof(Name));

      m_Volumes = new AtomRegistry<Volume>();
      foreach(var nVolume in conf.ChildrenNamed(Volume.CONFIG_VOLUME_SECTION))
      {
        var one = FactoryUtils.MakeDirectedComponent<Volume>(this, nVolume, null, new[]{ nVolume });
        m_Volumes.Register(one).IsTrue($"Unique {one.GetType().DisplayNameWithExpandedGenericArgs()}(`{one.Name}`)");
      }

      (m_Volumes.Count > 0).IsTrue("Configured volumes");
    }

    protected override void Destructor()
    {
      cleanup();
      base.Destructor();
    }

    private void cleanup()
    {
      var was = m_Volumes.ToArray();
      m_Volumes = new AtomRegistry<Volume>();
      was.ForEach(one => this.DontLeak(() => one.Dispose(), errorFrom: nameof(cleanup)));
    }

    [Config]
    private Atom m_Name;
    private AtomRegistry<Volume> m_Volumes;

    public Atom Name => m_Name;
    public IAtomRegistry<Volume> Volumes => m_Volumes;

    public override string ComponentLogTopic => ComponentDirector.ComponentLogTopic;

    /// <summary>
    /// Returns volume by name or throws if not found
    /// </summary>
    public Volume this[Atom volume] => m_Volumes[volume.IsValidNonZero(nameof(volume))] ?? throw $"Gateway system `{Name}`, volume `{volume}`".IsNotFound();
  }
}
