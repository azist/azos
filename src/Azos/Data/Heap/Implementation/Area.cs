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
using Azos.Client;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Heap.Implementation
{
  public sealed class Area : ApplicationComponent<Heap>, IArea, IInstrumentable
  {
    public const string CONFIG_AREA_SECTION = "area";

    internal Area(Heap director, IConfigSectionNode cfg) : base(director)
    {
      //build
      //check Name for atom compliance
    }

    public IHeap Heap => ComponentDirector;


    public IService ServiceClient => throw new NotImplementedException();

    public IEnumerable<Type> ObjectTypes => throw new NotImplementedException();

    public IEnumerable<Type> QueryTypes => throw new NotImplementedException();

    public Router Sharding => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public override string ComponentLogTopic => throw new NotImplementedException();

    public bool InstrumentationEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters => throw new NotImplementedException();

    public IEnumerable<INode> Nodes => throw new NotImplementedException();

    public INodeSelector NodeSelector => throw new NotImplementedException();

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

    public ISpace GetSpace(Type tObject)
    {
      throw new NotImplementedException();
    }

    public ISpace<T> GetSpace<T>() where T : HeapObject
    {
      throw new NotImplementedException();
    }
  }
}
