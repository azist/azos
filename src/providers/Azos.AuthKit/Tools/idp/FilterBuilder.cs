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
  public static class FilterBuilder
  {
    public static UserListFilter Build()
    {
      var result = new UserListFilter();
      while(true)
      {
        Show(result);

        Console.Write("Enter a field name to set, or 'x' to go back: ");
        var fn = Console.ReadLine();
        if (fn.EqualsOrdIgnoreCase("x")) return result;

        var fld = result.Schema[fn];
        if (fld == null) continue;

        Console.Write($"Field `{fld.Name}` currently equals `{result.GetFieldValue(fld)}`. Enter new value: ");
        var val = Console.ReadLine();
        if (val=="") val = null;
        try
        {
          result.SetFieldValue(fld, StringValueConversion.AsType(val, fld.Type));
        }
        catch(Exception ce)
        {
          ConsoleUtils.Error(ce.ToMessageWithType());
        }
      }
    }

    private static void Show(UserListFilter filter)
    {
      var sb = new StringBuilder();
      sb.AppendLine("<push>");
      foreach(var fld in filter.Schema)
      {
        var atr = fld.Attrs.FirstOrDefault();
        sb.AppendLine($"<f color=white>{fld.Name}<f color=darkgray>: <f color=yellow>{filter.GetFieldValue(fld)}<f color=gray> <f color = darkcyan>{ fld.NonNullableType.DisplayNameWithExpandedGenericArgs()}<f color=darkgray> - {fld.Description}");
      }

      sb.AppendLine("<pop>");
      ConsoleUtils.WriteMarkupContent(sb.ToString());
    }

  }
}
