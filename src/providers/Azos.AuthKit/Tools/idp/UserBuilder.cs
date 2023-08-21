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
using Azos.Security;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Tools.idp
{
  public static class UserBuilder
  {
    public static UserEntity Build()
    {
      var result = new UserEntity();

      while(true)
      {
        OneField("Name", () => result.Name, v => result.Name = v);
        OneField("Level", () => result.Level.ToString(), v => result.Level = v.AsEnum(UserStatus.Invalid));
        OneField("Description", () => result.Description, v => result.Description = v);
        OneField("Valid UTC Start", () => result.ValidSpanUtc?.Start?.ToString(), v => result.ValidSpanUtc = new Azos.Time.DateRange(v.AsDateTime(), result.ValidSpanUtc?.End));
        OneField("Valid UTC End", () => result.ValidSpanUtc?.End?.ToString(), v => result.ValidSpanUtc = new Azos.Time.DateRange(result.ValidSpanUtc?.Start, v.AsDateTime()));


        var ve = result.Validate();
        if (ve == null)
        {
          if (OK())
          {
            return result;
          }
          continue;
        }

        ConsoleUtils.Error(ve.ToMessageWithType());
      }
    }


    public static void OneField(string fn,  Func<string> getter, Action<string> setter)
    {
      ConsoleUtils.WriteMarkupContent($"<f color=gray>Please enter <f color=white>{fn}<f color=gray>: <f color=darkcyan>{getter()}  <f color=gray>");
      var val = Console.ReadLine();
      if (val.IsNullOrWhiteSpace()) return;
      setter(val);
    }


    public static bool OK()
    {
      Console.WriteLine();
      ConsoleUtils.WriteMarkupContent($"<f color=gray>Does the data above look correct (<f color=white>Y/<f color=red>n<f color=gray>)?");
      var ok = Console.ReadLine().EqualsIgnoreCase("y");
      Console.WriteLine();
      return ok;
    }


  }
}
