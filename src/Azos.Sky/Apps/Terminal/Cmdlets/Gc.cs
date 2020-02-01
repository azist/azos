/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Diagnostics;

using Azos.Conf;

namespace Azos.Apps.Terminal.Cmdlets
{
  public class Gc : Cmdlet
  {
    public Gc(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    {
    }

    public override string Execute()
    {
        var watch = Stopwatch.StartNew();
        var before = GC.GetTotalMemory(false);
        System.GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        var after = GC.GetTotalMemory(false);
        var freed = before - after;
        return "GC took {0:n0} ms. and freed {1:n0}".Args(watch.ElapsedMilliseconds, IOUtils.FormatByteSizeWithPrefix(freed, longPfx: true));
    }

    public override string GetHelp()
    {
        return "Invokes garbage collector on all generations";
    }
  }

}
