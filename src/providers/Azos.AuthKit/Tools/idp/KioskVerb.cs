/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Data;
using Azos.IO.Console;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Tools.idp
{
  public sealed class KioskVerb : Verb
  {
    public KioskVerb(IIdpUserAdminLogic logic) : base(logic) {  }

    /// <summary>
    /// Currently selected user
    /// </summary>
    public UserInfo CurrentUser{ get; set; }


    public override void Run()
    {
      ConsoleUtils.WriteMarkupContent(@"
<push>

<f color=green> ██▓▓█████▄  ██▓███    <f color=cyan>  ██ ▄█▀ ██▓ ▒█████    ██████  ██ ▄█▀
<f color=green>▓██▒▒██▀ ██▌▓██░  ██▒  <f color=cyan>  ██▄█▒ ▓██▒▒██▒  ██▒▒██    ▒  ██▄█▒ 
<f color=green>▒██▒░██   █▌▓██░ ██▓▒  <f color=cyan> ▓███▄░ ▒██▒▒██░  ██▒░ ▓██▄   ▓███▄░ 
<f color=green>░██░░▓█▄   ▌▒██▄█▓▒ ▒  <f color=cyan> ▓██ █▄ ░██░▒██   ██░  ▒   ██▒▓██ █▄ 
<f color=green>░██░░▒████▓ ▒██▒ ░  ░  <f color=cyan> ▒██▒ █▄░██░░ ████▓▒░▒██████▒▒▒██▒ █▄
<f color=green>░▓   ▒▒▓  ▒ ▒▓▒░ ░  ░  <f color=cyan> ▒ ▒▒ ▓▒░▓  ░ ▒░▒░▒░ ▒ ▒▓▒ ▒ ░▒ ▒▒ ▓▒
<f color=green> ▒ ░ ░ ▒  ▒ ░▒ ░       <f color=cyan> ░ ░▒ ▒░ ▒ ░  ░ ▒ ▒░ ░ ░▒  ░ ░░ ░▒ ▒░
<f color=green> ▒ ░ ░ ░  ░ ░░         <f color=cyan> ░ ░░ ░  ▒ ░░ ░ ░ ▒  ░  ░  ░  ░ ░░ ░ 
<f color=green> ░     ░               <f color=cyan> ░  ░    ░      ░ ░        ░  ░  ░   
<f color=green>     ░                 <f color=cyan>                                     

<f color=white>IDP management Interactive Kiosk mode  <f color=darkgray>
<f color=darkcyan>We will guide you step-by-step through various user creation and maintenance flows


<pop>");

      while (true)
      {
        ShowCurrentUser();

        ConsoleUtils.WriteMarkupContent(@"<f color=white>
Menu:
  <f color=darkgray>[<f color=white>1<f color=darkgray>] - <f color=yellow>Find<f color=darkgray> user
  <f color=darkgray>[<f color=white>2<f color=darkgray>] - <f color=yellow>Add<f color=darkgray> a new user
  <f color=darkgray>[<f color=white>3<f color=darkgray>] - <f color=yellow>Modify<f color=darkgray> a selected user
  <f color=darkgray>[<f color=white>4<f color=darkgray>] - <f color=yellow>Lock<f color=darkgray> selected user record
  <f color=darkgray>[<f color=white>x<f color=darkgray>] - <f color=yellow>Exit<f color=darkgray> back

<f color=gray>");
        Console.Write("Please enter your menu choice: ");

        var choice = Console.ReadLine();
        if (choice.EqualsOrdIgnoreCase("1"))  FindUser();
        else if (choice.EqualsOrdIgnoreCase("2"))  AddNewUser();
        else if (choice.EqualsOrdIgnoreCase("3")) ModifyUser();
        else if (choice.EqualsOrdIgnoreCase("x")) return;
      }
    }

    public void ShowCurrentUser()
    {
      if (CurrentUser == null) return;
      ConsoleUtils.WriteMarkupContent("\n <f color=white> SELECTED USER \n <f color=gray>--------------- \n");
      DumpUserInfo(CurrentUser);
    }

    public UserInfo GetUserInfo(GDID gUser)
    {
      var result = Logic.GetUserListAsync(new UserListFilter { Gdid = gUser })
                        .AwaitResult()
                        .FirstOrDefault();
      return result;
    }

    public UserInfo GetUserInfo(UserListFilter filter)
    {
      var result = Logic.GetUserListAsync(filter.NonNull(nameof(filter)))
                        .AwaitResult()
                        .FirstOrDefault();
      return result;
    }

    public static void DumpUserInfo(UserInfo info)
    {
      if (info==null) return;
      Console.WriteLine();
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Realm      | <f color=White>{info.Realm} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Gdid       | <f color=White>{info.Gdid} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Guid       | <f color=White>{info.Guid} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Name       | <f color=White>{info.Name} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Level      | <f color=White>{info.Level} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Description| <f color=White>{info.Description} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Validity   | <f color=White>{info.ValidSpanUtc} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>OrgUnit    | <f color=White>{info.OrgUnit} \n");
      ConsoleUtils.WriteMarkupContent($"<f color=darkgray>Props:\n    <f color=cyan> {info.Props.Content} \n");
      if (info.Rights != null && info.Rights.Content.IsNotNullOrWhiteSpace())
      {
        ConsoleUtils.WriteMarkupContent($"<f color=darkgray>Rights:\n   <f color=cyan> {info.Rights.Content} \n");
      }

      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Note: \n    <f color=cyan>{info.Note} \n");

      ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Version: \n    <f color=darkgreen>{info.CreateVersion.ToJson()} \n");
      ConsoleUtils.WriteMarkupContent($"    <f color=darkgreen>{info.DataVersion.ToJson()} \n");

      if (info.LockSpanUtc.HasValue && (info.LockSpanUtc.Value.Start.HasValue || info.LockSpanUtc.Value.End.HasValue))
      {
        ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Lock Span  | <f color=darkred>{info.LockSpanUtc} \n");
      }

      if (info.LockActor.HasValue)
      {
        ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Lock Actor | <f color=darkred{info.LockActor} \n");
      }

      if (info.LockNote.IsNotNullOrWhiteSpace())
      {
        ConsoleUtils.WriteMarkupContent($"<f color=darkGray>Lock Note: \n    <f color=darkred>{info.LockNote} \n");
      }

      Console.WriteLine();
    }


    public bool FindUser()
    {
      Console.WriteLine("* * * Find User * * *");
      var filter = FilterBuilder.Build();

      //Console.WriteLine(filter.ToJson());
      var user = GetUserInfo(filter);

      CurrentUser = user;

      return true;
    }

    public bool AddNewUser()
    {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("* * * Add New User * * *");
      Console.ForegroundColor = ConsoleColor.Gray;

      var user = UserBuilder.BuildUserEntity();
      Console.WriteLine("User data is ready.");

      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine("POST ");
      Console.ForegroundColor = ConsoleColor.DarkYellow;
      Console.WriteLine(user.ToJson());
      try
      {
        var userChange = Logic.SaveUserAsync(user).AwaitResult();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("SERVER RESPONSE: ");
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(userChange.ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII));
        Console.ForegroundColor = ConsoleColor.Gray;

        var data = (userChange.Data as JsonDataMap).NonNull("User change server response map");
        var gUser = data["Id_Gdid"].AsGDID(GDID.ZERO, handling: ConvertErrorHandling.Throw);
        ConsoleUtils.WriteMarkupContent($"<f color=gray> User GDID: <f color=white>{gUser}<f color=gray>");

        this.CurrentUser = GetUserInfo(gUser).NonNull("User record roundtrip");
      }
      catch(Exception err)
      {
        ConsoleUtils.Error(new WrappedExceptionData(err).ToJson());
        return false;
      }

      return true;
    }

    public bool ModifyUser()
    {
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("* * * Modify User * * *");
      Console.ForegroundColor = ConsoleColor.Gray;

      if (CurrentUser==null)
      {
        ConsoleUtils.Error("Select the user first");
        return false;
      }

      var userInfo = GetUserInfo(CurrentUser.Gdid);
      if (userInfo==null)
      {
        ConsoleUtils.Error("User not found");
        return false;
      }

      var user = UserEntity.FromUserInfo(userInfo);
      user = UserBuilder.BuildUserEntity(user);

      try
      {
        var userChange = Logic.SaveUserAsync(user).AwaitResult();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("SERVER RESPONSE: ");
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(userChange.ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII));
        Console.ForegroundColor = ConsoleColor.Gray;

        var data = (userChange.Data as JsonDataMap).NonNull("User change server response map");
        var gUser = data["Id_Gdid"].AsGDID(GDID.ZERO, handling: ConvertErrorHandling.Throw);
        ConsoleUtils.WriteMarkupContent($"<f color=gray> User GDID: <f color=white>{gUser}<f color=gray>");

        this.CurrentUser = GetUserInfo(gUser).NonNull("User record roundtrip");
      }
      catch (Exception err)
      {
        ConsoleUtils.Error(new WrappedExceptionData(err).ToJson());
        return false;
      }

      return true;
    }

  }
}
