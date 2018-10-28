using System;
using System.Diagnostics;

using Azos.Conf;

namespace Azos.Sky.Apps.Terminal.Cmdlets
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
            System.GC.Collect();
            var after = GC.GetTotalMemory(false);
            return "GC took {0} ms. and freed {1} bytes".Args(watch.ElapsedMilliseconds, before - after);
        }

        public override string GetHelp()
        {
            return "Invokes garbage collector";
        }
    }

}
