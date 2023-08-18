/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Tools.idp
{
  public sealed class UserListVerb : Verb
  {
    public UserListVerb(IIdpUserAdminLogic logic, bool silent) : base(logic, silent)
    {
    }


    public override void Run()
    {
      Console.WriteLine("Come on Starbear!");
      var filter = new UserListFilter{ };
      var users = m_Logic.GetUserListAsync(filter).AwaitResult();

      if (m_Silent)
      {
        var json = users.ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII);
        Console.WriteLine(json);
      }
      else
      {
        var markup = BuildUserListMarkup(users);
        Azos.IO.Console.ConsoleUtils.WriteMarkupContent(markup);
      }
    }

    public static string BuildUserListMarkup(IEnumerable<UserInfo> data)
    {
      var sb = new StringBuilder();
      foreach(var one in data)
      {
        sb.Append($"<J width=10 dir=right text='{one.Gdid}'>");
        sb.Append($"<J width=8 dir=right text='.{one.Guid.ToString().TakeLastChars(5)}'>");
        sb.Append($"<J width=25 dir=right text='{one.Name}'>");
        sb.Append($"<J width=10 dir=right text='{one.Level}'>");
        sb.AppendLine();
      }
      return sb.ToString();
    }

  }
}
