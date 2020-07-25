/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Log;
using Azos.Serialization.JSON;


namespace Azos.Sky.Chronicle.Server
{
  /// <summary>
  /// Provides server implementation for ILogChronicle and  IInstrumentationChronicle
  /// </summary>
  public sealed class ChronicleServerLogic : ModuleBase, ILogChronicleLogic, IInstrumentationChronicleLogic
  {
    public const string CONFIG_LOG_STORE_SECTION = "log-store";
    public const string CONFIG_INSTRUMENTATION_STORE_SECTION = "instrumentation-store";

    public ChronicleServerLogic(IApplication application) : base(application) { }
    public ChronicleServerLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Log);
      DisposeAndNull(ref m_Instrumentation);
      base.Destructor();
    }

    private ILogChronicleStoreLogicImplementation m_Log;
    private IInstrumentationChronicleStoreLogicImplementation m_Instrumentation;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;



    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Log);
      DisposeAndNull(ref m_Instrumentation);

      if (node == null) return;

      m_Log = FactoryUtils.MakeAndConfigureDirectedComponent<ILogChronicleStoreLogicImplementation>(this,
                                 node[CONFIG_LOG_STORE_SECTION].NonEmpty(CONFIG_LOG_STORE_SECTION));

      m_Instrumentation = FactoryUtils.MakeAndConfigureDirectedComponent<IInstrumentationChronicleStoreLogicImplementation>(this,
                                 node[CONFIG_INSTRUMENTATION_STORE_SECTION].NonEmpty(CONFIG_INSTRUMENTATION_STORE_SECTION));
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Log.NonNull($"Configured {nameof(m_Log)}").Start();
      m_Instrumentation.NonNull($"Configured {nameof(m_Instrumentation)}").Start();
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      m_Log.WaitForCompleteStop();
      m_Instrumentation.WaitForCompleteStop();
      return base.DoApplicationBeforeCleanup();
    }

    public async Task WriteAsync(LogBatch data)
      => await m_Log.NonNull().WriteAsync(data);

    public async Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter)
      => await m_Log.NonNull().GetAsync(filter);

    public async Task WriteAsync(InstrumentationBatch data)
      => await m_Instrumentation.NonNull().WriteAsync(data);

    public async Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter)
      => await m_Instrumentation.NonNull().GetAsync(filter);

  }
}
