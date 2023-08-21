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
using Azos.IO.Console;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Tools.idp
{
  public sealed class KioskVerb : Verb
  {
    public KioskVerb(IIdpUserAdminLogic logic) : base(logic) {  }

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
        ConsoleUtils.WriteMarkupContent(@"<push><f color=white>
Menu:
  <f color=darkgray>[<f color=white>1<f color=darkgray>] - <f color=yellow>Find<f color=darkgray> user.<f color=darkcyan> Once found you will be able to <f color=yellow>Modify<f color=darkcyan> or <f color=yellow>Lock<f color=darkcyan> user record
  <f color=darkgray>[<f color=white>2<f color=darkgray>] - <f color=yellow>Add<f color=darkgray> a new user
  <f color=darkgray>[<f color=white>x<f color=darkgray>] - <f color=yellow>Exit<f color=darkgray> back

<pop>");
        Console.Write("Please enter your choice: ");

        var choice = Console.ReadLine();
        if (choice.EqualsOrdIgnoreCase("1"))  FindUser();
        else if (choice.EqualsOrdIgnoreCase("2"))  AddNewUser();
        else if (choice.EqualsOrdIgnoreCase("x")) return;
      }
    }

    public bool FindUser()
    {
      Console.WriteLine("* * * Find User * * *");
      var filter = FilterBuilder.Build();

      Console.WriteLine(filter.ToJson());
      return true;
    }

    public bool AddNewUser()
    {
      Console.WriteLine("* * * Add New User * * *");

      var user = UserBuilder.Build();

      Console.WriteLine(user.ToJson());
      return true;
    }
  }
}
