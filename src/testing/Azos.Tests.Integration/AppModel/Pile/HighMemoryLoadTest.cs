/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.ApplicationModel.Pile;
using Azos.Environment;
using Azos.Scripting;

namespace Azos.Tests.Integration.AppModel.Pile
{
    /// <summary>
    /// Base for all high-load tests
    /// </summary>
    public abstract class HighMemoryLoadTest : IRunnableHook
    {
        public abstract ulong MinRAM { get; }

        void IRunnableHook.Prologue(Runner runner, FID id)
        {
            System.GC.Collect();
            var ms = Azos.OS.Computer.GetMemoryStatus();

            var has = ms.TotalPhysicalBytes;
            if (has < MinRAM)
                Aver.Fail("The machine has to have at least {0:n0} bytes of ram for this test, but it only has {1:n0} bytes".Args(MinRAM, has));
        }

        bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
        {
           System.GC.Collect();
           return false;
        }
    }

    /// <summary>
    /// Base for all high-load tests for PCs with 32 GB RAM at least
    /// </summary>
    public class HighMemoryLoadTest32RAM : HighMemoryLoadTest
    {
        public override ulong MinRAM { get { return 32ul * 1000ul * 1000ul * 1000ul; } }
    }

    /// <summary>
    /// Base for all high-load tests for PCs with 64 GB RAM at least
    /// </summary>
    public class HighMemoryLoadTest64RAM : HighMemoryLoadTest
    {
        public override ulong MinRAM { get { return 64ul * 1000ul * 1000ul * 1000ul; } }
    }
}
