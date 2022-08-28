/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
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
    public const string CONFIG_NODE_SELECTOR_SECTION = "node-selector";
    public const string CONFIG_SERVICE_CLIENT_SECTION = "service-client";

    internal Area(Heap director, IConfigSectionNode cfg) : base(director)
    {
      cfg.NonEmpty(nameof(cfg));

      //1. Build Schema
      m_Schema = new TypeSchema(this, cfg[TypeSchema.CONFIG_SCHEMA_SECTION]);

      //2. Build Selector
      var cfgSelector = cfg[CONFIG_NODE_SELECTOR_SECTION];
      m_NodeSelector = FactoryUtils.MakeDirectedComponent<INodeSelector>(this, cfgSelector, typeof(DefaultNodeSelector), new object[]{cfgSelector});

      //3. Build Service Client
      var cfgClient = cfg[CONFIG_SERVICE_CLIENT_SECTION];
      m_ServiceClient = FactoryUtils.MakeDirectedComponent<IHttpService>(this, cfgClient, typeof(HttpService), new object[]{cfgClient});

      //4. Build Spaces
      m_Spaces = new Dictionary<Type, ISpace>();
      foreach(var tObject in m_Schema.ObjectTypes)
      {
        var tSpace = typeof(Space<>).MakeGenericType(tObject);
        var space = Activator.CreateInstance(tSpace).CastTo<ISpace>("space .ctor");
        m_Spaces.Add(tSpace, space);
      }
    }

    protected override void Destructor()
    {
      DisposeIfDisposableAndNull(ref m_ServiceClient);
      base.Destructor();
    }

    private string m_Name;
    private IHttpService m_ServiceClient;
    private TypeSchema m_Schema;
    private List<INode> m_Nodes;
    private INodeSelector m_NodeSelector;
    private Dictionary<Type, ISpace> m_Spaces;


    public string Name => m_Name;
    public IHeap  Heap => ComponentDirector;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    public IHttpService ServiceClient => m_ServiceClient.NonDisposed();

    public ITypeSchema Schema => m_Schema;

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

    public Task<SaveResult<object>> ExecuteAsync(HeapRequest request, Guid idempotencyToken = default(Guid), INode node = null)
    {
      request.NonNull(nameof(request));
      if (node==null) node = NodeSelector.ForLocal.First();

      //todo... implement using most appropriate node
      // get node
      // node has address which now can be used for Http call
      // call ServiceClient.Call(....)
      //...
      throw new NotImplementedException();
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
