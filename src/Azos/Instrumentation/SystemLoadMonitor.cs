/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Platform;

namespace Azos.Instrumentation
{
  /// <summary>
  /// A single system load measurement sample, you can extend this class to add additional monitored values
  /// </summary>
  public class SysLoadSample : IAtomNamed
  {
    public static readonly Atom CURRENT = Atom.Encode("current");
    public const string CONFIG_AVERAGE_SECTION = "average";

    public static readonly Atom DEFAULT_AVERAGE_NAME = Atom.Encode("DEFAULT");
    public const double DEFAULT_AVERAGE_FACTOR = 0.02d;

    public SysLoadSample(Atom name, double emaFactor)
    {
      m_Name = name.IsValidNonZero(nameof(name));
      m_EmaFactor = emaFactor.AtMinimum(0d);
    }

    private readonly Atom m_Name;
    private readonly double m_EmaFactor;


    public Atom Name => m_Name;
    public double EmaFactor => m_EmaFactor;
    public bool IsAverage => m_EmaFactor > 0d;

    public double CpuLoadPercent   { get; internal set; }
    public double RamLoadPercent   { get; internal set; }
    public long   AvailableRamMb   { get; internal set; }


    /// <summary>
    /// Calculates a vectorized average for the class as a whole (field by field)
    /// </summary>
    public virtual bool AddSample(SysLoadSample sample)
    {
      if (!IsAverage) return false;
      CpuLoadPercent = CpuLoadPercent.Ema(sample.CpuLoadPercent, EmaFactor);
      RamLoadPercent = RamLoadPercent.Ema(sample.RamLoadPercent, EmaFactor);
      AvailableRamMb = AvailableRamMb.Ema(sample.AvailableRamMb, EmaFactor);

      return true;
    }
  }


  public interface ISystemLoadMonitor<TSample> : IModule where TSample : SysLoadSample
  {
    /// <summary>
    /// Returns load sampling averages
    /// </summary>
    IAtomRegistry<TSample> Averages { get; }

    /// <summary>
    /// Returns load monitoring sample averaged using default settings
    /// </summary>
    TSample DefaultAverage { get; }

    /// <summary>
    /// Returns the most current system load sample without averaging
    /// </summary>
    TSample CurrentSample { get; }
  }

  public abstract class SystemLoadMonitor<TSample> : ModuleBase, ISystemLoadMonitor<TSample> where TSample : SysLoadSample
  {
    public SystemLoadMonitor(IApplication app) : base(app)
    {
      m_Current = MakeSample(SysLoadSample.CURRENT, 0d);
      m_Averages = new AtomRegistry<TSample>();
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;

    private TSample m_Current;


    private Task m_Scheduled;
    private AtomRegistry<TSample> m_Averages;



    public TSample CurrentSample => m_Current;
    public IAtomRegistry<TSample> Averages => m_Averages;
    public TSample DefaultAverage => m_Averages[SysLoadSample.DEFAULT_AVERAGE_NAME];

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node != null)
      {
        foreach(var navg in node.ChildrenNamed(SysLoadSample.CONFIG_AVERAGE_SECTION))
        {
          var avg = MakeSample(navg.Of("name").ValueAsAtom(Atom.ZERO), navg.Of("factor").ValueAsDouble(0d));
          m_Averages.Register(avg).IsTrue("Unique avg name  `{0}`".Args(avg.Name));
        }
      }
    }

    protected override bool DoApplicationAfterInit()
    {
      var sysDefault = MakeSample(SysLoadSample.DEFAULT_AVERAGE_NAME, SysLoadSample.DEFAULT_AVERAGE_FACTOR);
      m_Averages.RegisterOrReplace(sysDefault);
      m_Scheduled = Task.Factory.StartNew(() => takeScheduledMeasureAsync());
      return base.DoApplicationAfterInit();
    }

    private async Task takeScheduledMeasureAsync()
    {
      try
      {
        m_Current = await MeasureCurrentAsync().ConfigureAwait(false);
        m_Averages.ForEach(one => one.AddSample(m_Current));
      }
      catch(Exception error)
      {
        WriteLog(Log.MessageType.CatastrophicError, nameof(takeScheduledMeasureAsync), error.ToMessageWithType(), error);
      }
      finally
      {
        if (App.Active)
        {
          m_Scheduled = Task.Delay(500).ContinueWith(_ => takeScheduledMeasureAsync());
        }
      }
    }


    protected abstract TSample MakeSample(Atom name, double emaFactor);

    protected virtual async Task<TSample> MeasureCurrentAsync()
    {
      var sample = MakeSample(SysLoadSample.CURRENT, 0d);
      //cpu and ream
      await MeasureCpuAsync(sample).ConfigureAwait(false);
      await MeasureRamAsync(sample).ConfigureAwait(false);

      return sample;
    }

    protected virtual Task MeasureCpuAsync(TSample sample)
    {
      sample.CpuLoadPercent = Computer.CurrentProcessorUsagePct / 100d;
      return Task.CompletedTask;
    }

    protected virtual Task MeasureRamAsync(TSample sample)
    {
      var ram = Computer.GetMemoryStatus();
      sample.RamLoadPercent = ram.LoadPct / 100d;
      //...virt memory
      sample.AvailableRamMb = Computer.CurrentAvailableMemoryMb;
      return Task.CompletedTask;
    }

  }


  public class DefaultSystemLoadMonitor : SystemLoadMonitor<SysLoadSample>
  {
    public DefaultSystemLoadMonitor(IApplication app) : base(app){ }

    protected override SysLoadSample MakeSample(Atom name, double emaFactor) => new SysLoadSample(name, emaFactor);

  }
}
