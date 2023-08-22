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
using Azos.Conf;
using Azos.Data;
using Azos.IO.Console;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.AuthKit.Tools.idp
{
  public static class UserBuilder
  {
    public static UserEntity Build()
    {
      var result = new UserEntity();

      ConfigSectionNode prop = Configuration.NewEmptyRoot(Constraints.CONFIG_PROP_ROOT_SECTION);
      ConfigSectionNode claims = prop.AddChildNode(Constraints.CONFIG_CLAIMS_SECTION);
      result.Props = new ConfigVector(prop);

      while(true)
      {
        OneField("Name", () => result.Name, v => result.Name = v);
        OneField("Level (<push><f color=darkred>Usr|Adm|Sys<pop>)", () => result.Level.ToString(), v => result.Level = v.AsEnum(UserStatus.Invalid, ConvertErrorHandling.Throw));
        OneField("Description", () => result.Description, v => result.Description = v);
        OneField("Validity Utc span start (mm/dd/yyyy hh:mm)", () => result.ValidSpanUtc?.Start?.ToString(), v => result.ValidSpanUtc = new DateRange(v.AsDateTime(CoreConsts.UTC_TIMESTAMP_STYLES), result.ValidSpanUtc?.End));
        OneField("Validity Utc span end (mm/dd/yyyy hh: mm)", () => result.ValidSpanUtc?.End?.ToString(), v => result.ValidSpanUtc = new DateRange(result.ValidSpanUtc?.Start, v.AsDateTime(CoreConsts.UTC_TIMESTAMP_STYLES)));
        OneField("Org Unit", () => result.OrgUnit?.AsString, v => result.OrgUnit = v.AsEntityId(EntityId.EMPTY, ConvertErrorHandling.Throw));
        OneField("Role", () => prop.ValOf(Constraints.CONFIG_ROLE_ATTR), v => prop.AttrByName(Constraints.CONFIG_ROLE_ATTR, autoCreate: true).Value = v.AsNullableEntityId(null, ConvertErrorHandling.Throw));
        OneField("Note", () => result.Note, v => result.Note = v);

        var ve = result.Validate();
        if (ve == null)
        {
          ConsoleUtils.WriteMarkupContent(@$"
  <f color=darkgray>Name: <f color=white>{result.Name}
  <f color=darkgray>Level: <f color=white>{result.Level}
  <f color=darkgray>Descr: <f color=white>{result.Description}
  <f color=darkgray>Valid span: <f color=white>{result.ValidSpanUtc}
  <f color=darkgray>Org unit: <f color=white>{result.OrgUnit}
  <f color=darkgray>Props: <f color=cyan>
     {result.Props.Content}
  <f color=darkgray>Rights: <f color=cyan>
     {result.Rights?.Content}
  <f color=darkgray>Note: <f color=white>
     {result.Note}

          ");
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
      ConsoleUtils.WriteMarkupContent($"<f color=darkgray>Please enter <f color=white>{fn}<f color=gray>: <f color=darkcyan>{getter()}  <f color=gray>");

      while(true)
      {
        var val = Console.ReadLine();
        if (val.IsNullOrWhiteSpace()) return;

        try
        {
          setter(val);
          break;
        }
        catch(Exception error)
        {
          ConsoleUtils.Error("You have entered bad field value: " + error.ToMessageWithType());
          Console.WriteLine("Reenter the value: ");
        }
      }
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
