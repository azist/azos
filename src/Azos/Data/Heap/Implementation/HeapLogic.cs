/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Collections;
using Azos.Instrumentation;

namespace Azos.Data.Heap.Implementation
{
  public sealed class HeapLogic : ModuleBase, IHeapLogic, IInstrumentable
  {
    public HeapLogic(IApplication application) : base(application) { }
    public HeapLogic(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    public IRegistry<IArea> Areas => throw new NotImplementedException();
  }
}
