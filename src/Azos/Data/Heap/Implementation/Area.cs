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
using Azos.Instrumentation;

namespace Azos.Data.Heap.Implementation
{
  public sealed class Area : ApplicationComponent<HeapLogic>, IArea, IInstrumentable
  {
    internal Area(HeapLogic director) : base(director){ }

    public IHeap Heap => ComponentDirector;

    public IEnumerable<Type> ObjectTypes => throw new NotImplementedException();

    public IEnumerable<Type> QueryTypes => throw new NotImplementedException();

    public Router Sharding => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public override string ComponentLogTopic => throw new NotImplementedException();

    public bool InstrumentationEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters => throw new NotImplementedException();

    public Task<object> ExecuteQueryAsync(AreaQuery query)
    {
      throw new NotImplementedException();
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      throw new NotImplementedException();
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      throw new NotImplementedException();
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      throw new NotImplementedException();
    }

    public IHeapCollection GetCollection(Type tObject)
    {
      throw new NotImplementedException();
    }

    public IHeapCollection<T> GetCollection<T>() where T : HeapObject
    {
      throw new NotImplementedException();
    }
  }
}
