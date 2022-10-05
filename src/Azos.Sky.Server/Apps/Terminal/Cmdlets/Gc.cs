/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Diagnostics;
using System.Runtime;
using Azos.Conf;

namespace Azos.Apps.Terminal.Cmdlets
{
  public sealed class Gc : Cmdlet
  {
    public Gc(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
      var watch = Stopwatch.StartNew();

      var before = GC.GetTotalMemory(false);
      GC.Collect(GC.MaxGeneration,
                 GCCollectionMode.Forced,
                 m_Args.Of("block").ValueAsBool(true),
                 m_Args.Of("compact").ValueAsBool(true));

      if (m_Args.Of("wait").ValueAsBool())
      {
        GC.WaitForPendingFinalizers();
      }

      var after = GC.GetTotalMemory(false);
      var freed = before - after;

      var result = $"MaxGen:      {GC.MaxGeneration}\n" +
                   $"IsServer:    {GCSettings.IsServerGC}\n" +
                   $"LatencyMode: {GCSettings.LatencyMode}\n" +
                   $"LOHCMode:    {GCSettings.LargeObjectHeapCompactionMode}\n" +
                   $"Total process allocations:    {IOUtils.FormatByteSizeWithPrefix(GC.GetTotalAllocatedBytes(false), false)}\n" +
                   $"-----------------------------------------------\n" +
                   $"Elapsed:        {watch.ElapsedMilliseconds:n0} ms\n" +
                   $"Total before:   {IOUtils.FormatByteSizeWithPrefix(before, false),12}\n" +
                   $"Total after:    {IOUtils.FormatByteSizeWithPrefix(after, false),12}\n" +
                   $"Total freed:    {IOUtils.FormatByteSizeWithPrefix(freed, false),12}\n ";

      return result;
    }

    public override string GetHelp()
    {
        return @"Invokes garbage collector on all generations.
    Pass `block=false` to avoid blocking collection;
    Pass `compact=false` to avoid compaction;
    Pass `wait=true` to wait for pending finalizers";
    }
  }

}
