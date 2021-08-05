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
using Azos.Conf;
using Azos.Data;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Glue;
using Azos.Platform;
using Azos.Wave;

namespace Azos.Sky.EventHub.Server
{
  /// <summary>
  /// Provides server implementation for IEventHubServerNodeLogic based on Mongo Db
  /// </summary>
  public sealed class MongoEventHubServerNode : ModuleBase, IEventHubServerNodeLogic
  {
    public const int CONSUMER_ID_MAX_LEN = 255;
    public const string DB_PREFIX = "sky_evt_";

    public MongoEventHubServerNode(IApplication application) : base(application) { }
    public MongoEventHubServerNode(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      base.Destructor();
    }

    private Node m_ServerNode;
    private MongoClient m_Client;
    private FiniteSetLookup<Atom, Database> m_DbMap;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.QUEUE_TOPIC;

    /// <summary>
    /// Mongo server node connector, e.g. "appliance://" or "mongo://127.0.0.1:27017"
    /// </summary>
    [Config]
    public string ServerNode { get; set; }


    #region Protected plumbing
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node == null) return;
    }

    protected override bool DoApplicationAfterInit()
    {
      m_ServerNode = new Node(ServerNode.NonBlank(nameof(ServerNode)))
                         .IsTrue( v => v.Binding.IsOneOf(MongoClient.MONGO_BINDING, MongoClient.APPLIANCE_BINDING), nameof(ServerNode));

      m_Client = App.GetDefaultMongoClient();

      m_DbMap = new FiniteSetLookup<Atom, Database>(ns => {
        var server = m_Client[m_ServerNode];
        var db = server[DB_PREFIX + ns];
        return db;
      });

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      return base.DoApplicationBeforeCleanup();
    }
    #endregion

    #region IEventHubServerLogic
    public Task<ChangeResult> WriteAsync(Atom ns, Atom queue, Event evt)
    {
      const int MONGO_KEY_VIOLATION = 11000;

      checkRoute(ns, queue);
      var ve = evt.NonNull(nameof(Event)).Validate();
      if (ve != null) throw ve;

      //Checkpoint is forced as the time of writing to disk at the client
      var bson = BsonConvert.ToBson(evt);
      var collection = getQueueCollection(ns, queue);//ensures indexes

      var crud = collection.Insert(bson);

      //need to detect key violation as it is a sign of idempotency retry which should be treated as success
      var success = (crud.WriteErrors == null) ||
                    (crud.WriteErrors.Length > 0  &&  crud.WriteErrors[0].Code == MONGO_KEY_VIOLATION);

      var result = success ? new ChangeResult(ChangeResult.ChangeType.Inserted, 1, "Affected: " + crud.TotalDocumentsAffected, crud.WriteErrors?.Select(e => new {e.Code, e.Message}))
                           : new ChangeResult(ChangeResult.ChangeType.Undefined, 0, "Error", crud.WriteErrors?.Select(e => new { e.Code, e.Message }));

      return Task.FromResult(result);
    }

    public Task<IEnumerable<Event>> FetchAsync(Atom ns, Atom queue, ulong checkpoint, int skip, int count, bool onlyid)
    {
      checkRoute(ns, queue);

      var collection = getQueueCollection(ns, queue);

      var qry = BsonConvert.GetFetchQuery(checkpoint);
      var selector = BsonConvert.GetFetchSelector(onlyid);

      IEnumerable<Event> result = null;

      if (skip < 0) skip = 0;
      count = count.KeepBetween(1, EventHubClientLogic.FETCH_BY_MAX);
      var fetchBy = Math.Min(count, EventHubClientLogic.FETCH_BY_MAX);

      using(var cursor = collection.Find(qry, skip, fetchBy, selector))
      {
        result = cursor.Take(count)
                       .Select( doc => BsonConvert.FromBson(doc) ).ToArray();
      }

      return Task.FromResult(result);
    }

    public Task<ulong> GetCheckpointAsync(Atom ns, Atom queue, string consumer)
    {
      checkRoute(ns, queue);
      consumer.NonBlankMax(CONSUMER_ID_MAX_LEN, nameof(consumer));
      throw new NotImplementedException();
    }

    public Task SetCheckpointAsync(Atom ns, Atom queue, string consumer, ulong checkpoint)
    {
      checkRoute(ns, queue);
      consumer.NonBlankMax(CONSUMER_ID_MAX_LEN, nameof(consumer));
      throw new NotImplementedException();
    }
    #endregion

    #region .pvt

    private Database getNamespaceDatabase(Atom ns) => m_DbMap[ns];

    private Collection getQueueCollection(Atom ns, Atom queue)
    {
      var db = getNamespaceDatabase(ns);
      var collection = db.GetOrRegister(queue.Value, out var wasAdded);
      if (wasAdded)
      {
        //Create index on CheckpointUtc
        this.DontLeak(
          () => db.RunCommand(BsonConvert.CreateIndex(queue.Value)),
          errorLogType: Azos.Log.MessageType.CriticalAlert
        );
      }
      return collection;
    }


    private void checkRoute(Atom ns, Atom queue)
    {
      if (ns.IsZero || !ns.IsValid) throw HTTPStatusException.BadRequest_400("invalid ns");
      if (queue.IsZero || !queue.IsValid) throw HTTPStatusException.BadRequest_400("invalid queue");
    }
    #endregion


  }
}

