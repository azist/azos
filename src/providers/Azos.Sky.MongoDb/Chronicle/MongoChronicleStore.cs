/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle.Server
{
  /// <summary>
  /// Implements ILogChronicleStoreLogic and IInstrumentationChronicleStoreLogic using MongoDb
  /// </summary>
  public sealed class MongoChronicleStoreLogic : Daemon, ILogChronicleStoreLogicImplementation, IInstrumentationChronicleStoreLogicImplementation
  {
    public MongoChronicleStoreLogic(IApplication application) : base(application) { }
    public MongoChronicleStoreLogic(IModule parent) : base(parent) { }

    public override string ComponentLogTopic => throw new NotImplementedException();

    public Task<IEnumerable<Message>> GetAsync(LogChronicleFilter filter)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<JsonDataMap>> GetAsync(InstrumentationChronicleFilter filter)
    {
      throw new NotImplementedException();
    }

    public Task WriteAsync(LogBatch data)
    {
      throw new NotImplementedException();
    }

    public Task WriteAsync(InstrumentationBatch data)
    {
      throw new NotImplementedException();
    }
  }
}
