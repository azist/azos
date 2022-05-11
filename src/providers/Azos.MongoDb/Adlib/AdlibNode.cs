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
      throw new NotImplementedException();
    }

    public Task<ChangeResult> SaveAsync(Item item)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> DeleteAsync(EntityId id, string shardTopic = null)
    {
      throw new NotImplementedException();
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
  }
}
