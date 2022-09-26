/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Dsl
{
  /// <summary>
  /// A base step for user filter (fetching) execution
  /// </summary>
  public class GetUserList : Step
  {
    public GetUserList(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order) { }

    [Config] public string Filter { get; set; }

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var logic = LoadModule.Get<IIdpUserAdminLogic>();

      var filter = GetFilter(state);

      var result = await logic.GetUserListAsync(filter).ConfigureAwait(false);
      Runner.SetResult(result);

      return null;
    }

    protected virtual UserListFilter GetFilter(JsonDataMap state)
    {
      var json = Eval(Filter, state);
      var filter = json.IsNullOrWhiteSpace() ? new UserListFilter() : JsonReader.ToDoc<UserListFilter>(json);
      return filter;
    }
  }


  /// <summary>
  /// Builds a UserListFilter from simple property values
  /// </summary>
  public class GetUserListBy : GetUserList
  {
    public GetUserListBy(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order) { }

    [Config] public string UserName { get; set; }
    [Config] public string LoginId  { get; set; }
    [Config] public string Gdid     { get; set; }
    [Config] public string Guid     { get; set; }
    [Config] public string Level    { get; set; }
    [Config] public string OrgUnit  { get; set; }
    [Config] public string AsOfUtc  { get; set; }
    [Config] public string Active   { get; set; }
    [Config] public string Locked   { get; set; }

    private void apply(UserListFilter f, string val, string fld, JsonDataMap state)
    {
      if (val.IsNullOrWhiteSpace()) return;
      val = Eval(val, state);
      if (val.IsNullOrWhiteSpace()) return;

      var ft = f.Schema[fld].Type;

      f[fld] = val.AsType(ft, false);
    }

    protected override UserListFilter GetFilter(JsonDataMap state)
    {
      var filter = base.GetFilter(state);

      apply(filter, UserName, nameof(filter.Name), state);
      apply(filter, LoginId,  nameof(filter.LoginId), state);
      apply(filter, Gdid,     nameof(filter.Gdid), state);
      apply(filter, Guid,     nameof(filter.Guid), state);
      apply(filter, Level,    nameof(filter.Level), state);
      apply(filter, OrgUnit,  nameof(filter.OrgUnit), state);
      apply(filter, AsOfUtc,  nameof(filter.AsOfUtc), state);
      apply(filter, Active,   nameof(filter.Active), state);
      apply(filter, Locked,   nameof(filter.Locked), state);

      return filter;
    }
  }

}
