/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Tools.idp
{
  public sealed class UserListVerb : Verb
  {
    public UserListVerb(IIdpUserAdminLogic logic) : base(logic) {  }

    public override void Run()
    {
      var filter = FilterBuilder.Build(); //new UserListFilter{};//// Name = "dkh" };
      var users = Logic.GetUserListAsync(filter).AwaitResult();

      if (IsJson)
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
      var now = Ambient.UTCNow;
      var sb = new StringBuilder();
      foreach(var one in data)
      {
        sb.Append("<push>");
        sb.Append("<f color=Cyan>" + one.Gdid.ToString().PadLeft(16));
        sb.Append("<f color=Gray>");
        sb.Append(" ");
        sb.Append(one.Guid.ToString("N").TakeLastChars(5).PadLeft(6, '.'));
        sb.Append("<f color=DarkGray> ");
        var cc = one.Level >= Security.UserStatus.System ? "Magenta" :
                 one.Level == Security.UserStatus.User ? "Green" :
                 one.Level == Security.UserStatus.Admin ? "Yellow" : "Red";
        sb.Append($"[<f color={cc}>" + one.Level.ToString().TakeFirstChars(3).ToUpperInvariant() + "<f color=DarkGray>]");
        sb.Append(" ");
        sb.Append("<f color=White>" + one.Name.Default("?").PadRight(32));
        sb.Append("<f color=Gray>");
        sb.Append(one.Description.Default(" ").TakeFirstChars(12, "..").PadRight(14));
        sb.Append(" ");
        sb.Append((one.OrgUnit?.AsString).Default(" ").TakeLastChars(12, "..").PadRight(14));
        sb.Append(" ");
        sb.Append("<f color=DarkGray>" + dt(one.CreateVersion.Utc));
        sb.Append(" ");
        sb.Append($"<f color={(one.ValidSpanUtc.Contains(now) ? "DarkGray" : "DarkRed")}>(" + dt(one.ValidSpanUtc.Start) + ".." + dt(one.ValidSpanUtc.End)+ ")");
        sb.Append(" ");
        if (one.LockSpanUtc.HasValue)
        {
          sb.Append($"<f color={(one.LockSpanUtc.Value.Contains(now) ? "Red" : "DarkYellow")}>(" + dt(one.LockSpanUtc.Value.Start) + ".." + dt(one.LockSpanUtc.Value.End) + ")");
        }
        else
        {
          sb.Append(" ".PadLeft(10, '·'));
        }
        sb.Append("<f color=DarkCyan>");
        sb.Append(" ");
        sb.Append(one.Note.Default(" ").TakeFirstChars(31, "..").PadRight(32, '·'));

        sb.Append("<pop>");
        sb.AppendLine();

      }
      return sb.ToString();
    }

    private static string dt(DateTime d) => d.ToString("MM/dd/yyyy");
    private static string dt(DateTime? d) => d.HasValue ? dt(d.Value) : "";

  }
}
