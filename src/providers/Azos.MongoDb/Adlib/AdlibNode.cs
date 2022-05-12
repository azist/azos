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
using Azos.Conf;
using Azos.Data.Access.MongoDb.Connector;
using Azos.Serialization.BSON;

namespace Azos.Data.Adlib.Server
{
  /// <summary>
  /// Implements Adlib server node using MongoDb
  /// </summary>
  public sealed class AdlibNode : ModuleBase, IAdlibLogic
  {
    public const string CONFIG_SPACE_SECT = "space";
    public const string CONFIG_ID_ATTR = "id";
    public const string CONFIG_CS_ATTR = "cs";

    public const int FETCH_BY_MAX = 255;
    public const int FETCH_BY_DEFAULT = 8;

    public AdlibNode(IApplication application) : base(application) { }
    public AdlibNode(IModule parent) : base(parent) { }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;
    public bool IsServerImplementation => true;

    private Dictionary<Atom, (string cs, Database db)> m_Spaces;

    public Task<IEnumerable<Atom>> GetSpaceNamesAsync() => Task.FromResult<IEnumerable<Atom>>(m_Spaces.Keys);

    public Task<IEnumerable<Atom>> GetCollectionNamesAsync(Atom space)
    {
      var db = getDb(space);
      var cnames = db.GetCollectionNames()
                     .Where(cn => cn.StartsWith(BsonConvert.ADLIB_COLLECTION_PREFIX))
                     .Select(cn => BsonConvert.MongoToCanonicalCollectionName(cn));
      return Task.FromResult(cnames);
    }

    public Task<IEnumerable<Item>> GetListAsync(ItemFilter filter)
    {
      var col = getCollection(filter.Space, filter.Collection);

      var (qry, selector) = BsonConvert.GetFilterQuery(filter);

      IEnumerable<Item> result = null;

      var skip = filter.PagingStartIndex;
      var count = filter.PagingCount;

      if (skip < 0) skip = 0;
      count = count.KeepBetween(1, FETCH_BY_MAX);
      var fetchBy = Math.Min(count, FETCH_BY_MAX);

      using (var cursor = col.Find(qry, skip, fetchBy, selector))
      {
        result = cursor.Take(count)
                       .Select(doc => BsonConvert.ItemFromBson(doc)).ToArray();
      }

      return Task.FromResult(result);
    }

    private void checkCrud(CRUDResult crud)
    {
      if (crud.WriteErrors != null)
      {
        throw new AdlibException("Save failed", new MongoDbConnectorServerException());
      }
    }

    public async Task<ChangeResult> SaveAsync(Item item)
    {
      var col = getCollection(item.Space, item.Collection);

      var bson = BsonConvert.ToBson(item);

      ChangeResult result;
      if (item.FormMode == FormMode.Insert)
      {
        var crud = col.Insert(bson);
        checkCrud(crud);
        result = new ChangeResult(ChangeResult.ChangeType.Inserted, 1, "Inserted", crud, 200);
      }
      else if(item.FormMode == FormMode.Update)
      {
        var crud = col.Save(bson);
        checkCrud(crud);
        result = new ChangeResult(ChangeResult.ChangeType.Updated, crud.TotalDocumentsAffected, "Updated", crud, 200);

      }
      else if (item.FormMode == FormMode.Delete)
      {
        return await DeleteAsync(item.Id).ConfigureAwait(false);
      }
      else throw new FieldValidationException(item, nameof(item.FormMode), "Invalid form mode: "+item.FormMode);

      return result;
    }

    public Task<ChangeResult> DeleteAsync(EntityId id, string shardTopic = null)
    {
      var idt = Constraints.DecodeItemId(id);

      var col = getCollection(idt.space, idt.collection);

      var what = Query.ID_EQ_GDID(idt.gdid);

      var crud = col.DeleteOne(what);
      checkCrud(crud);
      var result = new ChangeResult(ChangeResult.ChangeType.Deleted, crud.TotalDocumentsAffected, "Deleted", crud, 200);

      return Task.FromResult(result);
    }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      node.NonNull(nameof(node));

      base.DoConfigure(node);

      m_Spaces = new Dictionary<Atom, (string cs, Database db)>();

      foreach(var nspace in node.ChildrenNamed(CONFIG_SPACE_SECT))
      {
        var space = nspace.Of(CONFIG_ID_ATTR).ValueAsAtom(Atom.ZERO);

        if (!space.IsValid || space.IsZero)
        {
          throw new AdlibException("Invalid space id `${0}`".Args(CONFIG_ID_ATTR));
        }
        var cs = nspace.ValOf(CONFIG_CS_ATTR);
        if (cs.IsNullOrWhiteSpace())
        {
          throw new AdlibException("Missing required space connect string `{0}`".Args(cs));
        }

        if (m_Spaces.ContainsKey(space))
        {
          throw new AdlibException("Duplicate space(`{0}`) declaration".Args(space));
        }
        m_Spaces.Add(space, (cs, null));
      }//foreach
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Spaces.NonNull("configures spaces");
      //resolve DB cs
      foreach(var kvp in m_Spaces)
      {
        var db = App.GetMongoDatabaseFromConnectString(kvp.Value.cs);
        m_Spaces[kvp.Key] = (kvp.Value.cs, db);
      }
      return base.DoApplicationAfterInit();
    }

    private Database getDb(Atom space)
    {
      if (!m_Spaces.TryGetValue(space, out var mapping))
      {
        "Space(`{0}`)".Args(space).IsNotFound();
      }
      return mapping.db;
    }

    private Collection getCollection(Atom space, Atom collection)
    {
      var db = getDb(space);
      var result = db.GetOrRegister(BsonConvert.CanonicalCollectionNameToMongo(collection), out var wasAdded);
      if (wasAdded)
      {
        //Create index on tags
        this.DontLeak(
          () => db.RunCommand(BsonConvert.CreateIndex(collection)),
          errorLogType: Log.MessageType.CriticalAlert
        );
      }
      return result;
    }
  }
}
