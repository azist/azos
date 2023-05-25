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
using Azos.Apps.Injection;
using Azos.Client;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Instrumentation;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle.Feed
{
  /// <summary>
  /// Pulls chronicle data feed from X number of channels into local receptacle
  /// </summary>
  public sealed class Channel : ApplicationComponent<ChronicleFeedPullAgent>, IAtomNamed
  {
    public const string CONFIG_CHANNEL_SECTION = "channel";

    public Channel(ChronicleFeedPullAgent parent, IConfigSectionNode cfg) : base(parent)
    {
      RestIntervalSec = 0;
      FetchBy = 0;
      ConfigAttribute.Apply(this, cfg);

      m_Name.HasRequiredValue(nameof(Name));
    }

    protected override void Destructor()
    {
      base.Destructor();
    }


    [Config] private Atom m_Name;
    private int m_RestIntervalSec;

    private int m_FetchBy;


    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int RestIntervalSec
    {
      get => m_RestIntervalSec;
      set => m_RestIntervalSec.KeepBetween(1, 10 * 60);
    }

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public int FetchBy
    {
      get => m_FetchBy;
      set => m_FetchBy.KeepBetween(10, 500);
    }

    public Atom Name => m_Name;

  }
}
