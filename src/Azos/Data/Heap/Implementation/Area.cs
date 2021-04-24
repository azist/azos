/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
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
      //check Name for Domains.id compliance
    }

    protected override void Destructor()
    {
      base.Destructor();
    }

    private string m_Name;
    private IHttpService m_ServiceClient;
    private List<INode> m_Nodes;
    private INodeSelector m_NodeSelector;
    private Dictionary<Type, ISpace> m_Spaces;


    public string Name => m_Name;
    public IHeap  Heap => ComponentDirector;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;
    public IHttpService ServiceClient => m_ServiceClient;

    public IEnumerable<Type> ObjectTypes => throw new NotImplementedException();
    public IEnumerable<Type> QueryTypes => throw new NotImplementedException();

    public IEnumerable<INode> Nodes => m_Nodes;
    public INodeSelector NodeSelector => m_NodeSelector;


    public ISpace GetSpace(Type tObject)
     => m_Spaces.TryGetValue(tObject.IsOfType<HeapObject>(nameof(tObject)), out var space)
                   ? space
                   : throw $"GetSpace(`{tObject.GetType().DisplayNameWithExpandedGenericArgs()})`".IsNotFound();

    public ISpace<T> GetSpace<T>() where T : HeapObject
     => m_Spaces.TryGetValue(typeof(T), out var space)
                   ? space.CastTo<ISpace<T>>()
                   : throw $"GetSpace(`{typeof(T).GetType().DisplayNameWithExpandedGenericArgs()})`".IsNotFound();

    public Task<SaveResult<object>> ExecuteAsync(HeapQuery query, Guid idempotencyToken = default(Guid), INode node = null)
    {
      query.NonNull(nameof(query));
      if (node==null) node = NodeSelector.ForLocal.First();

      //...
      return null;
    }


    #region Instrumentation
    public bool InstrumentationEnabled {get; set;}

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters => ExternalParameterAttribute.GetParameters(this);

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
     => ExternalParameterAttribute.GetParameter(App, this, name, out value, groups);

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
     => ExternalParameterAttribute.GetParameters(this, groups);

    public bool ExternalSetParameter(string name, object value, params string[] groups)
     =>  ExternalParameterAttribute.SetParameter(App, this, name, value, groups);
    #endregion

  }
}
