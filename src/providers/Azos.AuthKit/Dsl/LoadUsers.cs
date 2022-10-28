/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;
using Azos.Data.Business;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Dsl
{
  /// <summary>
  /// Saves the whole user entity graph including logins
  /// </summary>
  public class LoadUsers : Step
  {
    public class UserData : FragmentModel
    {
      [Field] public UserEntity User { get; set; }
      [Field] public IEnumerable<LoginEntity> Logins { get; set;}
    }


    public LoadUsers(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order) { }

    [Config] public string Data { get; set; }

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var logic = LoadModule.Get<IIdpUserAdminLogic>();

      var manyUsersData = GetUserData(state);

      var results = new List<ChangeResult>();

      foreach(var oneUser in manyUsersData)
      {
        var result = await logic.SaveUserAsync(oneUser.User).ConfigureAwait(false);
        results.Add(result);

        foreach(var login in oneUser.Logins)
        {

          var result2 = await logic.SaveLoginAsync(login).ConfigureAwait(false);
        }
      }

      Runner.SetResult(results);

      return null;
    }

    protected virtual IEnumerable<UserData> GetUserData(JsonDataMap state)
    {
      var json = Eval(Data, state);

      if (json.IsNullOrWhiteSpace()) return Enumerable.Empty<UserData>();

      var dobj = JsonReader.DeserializeDataObject(json);

      if (dobj == null) return Enumerable.Empty<UserData>();

      if (dobj is JsonDataMap jmap)
      {
        return JsonReader.ToDoc<UserData>(jmap).ToEnumerable();
      }
      else if (dobj is JsonDataArray jarr)
      {
        return jarr.OfType<JsonDataMap>()
                   .Select(one => JsonReader.ToDoc<UserData>(one));
      }
      else throw new Scripting.ScriptingException($"{nameof(LoadUsers)} does not support data of type: " + dobj.GetType().DisplayNameWithExpandedGenericArgs());
    }
  }

}
