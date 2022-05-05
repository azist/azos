/////////*<FILE_LICENSE>
//////// * Azos (A to Z Application Operating System) Framework
//////// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
//////// * See the LICENSE file in the project root for more information.
////////</FILE_LICENSE>*/

////////using System;
////////using System.Collections.Generic;
////////using System.Linq;
////////using System.Text;
////////using System.Threading.Tasks;
////////using Azos.Apps;
////////using Azos.Collections;
////////using Azos.Conf;

////////namespace Azos.Data.Adlib.Server
////////{
////////  public class Space : ApplicationComponent<IAdlibLogic>, INamed
////////  {
////////    protected internal Space(IAdlibLogic logic, IConfigSectionNode cfg) : base(logic)
////////    {
////////      m_Name  = cfg.NonEmpty(nameof(cfg))
////////                   .ValOf(Configuration.CONFIG_NAME_ATTR)
////////                   .NonBlankMinMax(Constraints.SPACE_NAME_MIN_LEN, Constraints.SPACE_NAME_MAX_LEN);

////////      m_Collections = new Registry<Collection>();
////////    }

////////    private string m_Name;
////////    private Registry<Collection> m_Collections;

////////    public string Name => m_Name;
////////    public IAdlibLogic Logic => this.ComponentDirector;

////////    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

////////    public Collection this[string name]
////////    {
////////      get
////////      {
////////        name.NonBlankMinMax(Constraints.SPACE_NAME_MIN_LEN, Constraints.SPACE_NAME_MAX_LEN);
////////        var result = m_Collections.GetOrRegister(name, n => MakeCollection(n));
////////        return result;
////////      }
////////    }

////////    protected virtual Collection MakeCollection(string name)
////////    {
////////      return new Collection(this, name);
////////    }

////////    public async Task<IEnumerable<string>> GetCollectionNamesAsync()
////////    {
////////      var result = await Logic.GetCollectionNamesAsync(this).ConfigureAwait(false);
////////      return result;
////////    }
////////  }

////////  public class Collection : INamed
////////  {
////////    protected internal Collection(Space space, string name)
////////    {
////////      m_Space = space.NonNull(nameof(space));
////////      m_Name = name.NonBlankMinMax(Constraints.SPACE_NAME_MIN_LEN, Constraints.SPACE_NAME_MAX_LEN);
////////    }

////////    private string m_Name;
////////    private Space m_Space;

////////    public string Name => m_Name;
////////    public IAdlibLogic Logic => m_Space.Logic;
////////    public Space Space => m_Space;

////////    public async Task<IEnumerable<Item>> GetListAsync(ItemFilter filter) => await Logic.GetListAsync(this, filter).ConfigureAwait(false);
////////    public async Task<ChangeResult> SaveAsync(Item item) => await Logic.SaveAsync(this, item).ConfigureAwait(false);
////////    public async Task<ChangeResult> DeleteAsync(GDID gItem) => await Logic.DeleteAsync(this, gItem).ConfigureAwait(false);
////////  }


////////}
