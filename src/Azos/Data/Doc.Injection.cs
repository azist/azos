/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections;

using Azos.Apps.Injection;

namespace Azos.Data
{
  public partial class Doc
  {
    bool IApplicationInjection.InjectApplication(IApplicationDependencyInjector injector)
    {
      var completed = DoInjectApplication(injector);
      return completed;
    }

    /// <summary>
    /// Override to perform custom dependency injection, the default one loops through the field of non-primitive types and injects on those.
    /// Return true if the injection handled already by your code and should not continue
    /// </summary>
    protected virtual bool DoInjectApplication(IApplicationDependencyInjector injector)
    {
      foreach(var fdef in Schema)
      {
        if (fdef.NonNullableType.IsValueType) continue;//no  DI on value types

        var v = GetFieldValue(fdef);
        if (v==null) continue;

        if (v is Doc vdoc) //Order of pattern matches is important
        {
          injector.App.InjectInto(vdoc);
        }
        else if (v is IDictionary vdict) //then DICT as it is also IEnumerable
        {
          foreach(var dv in vdict.Values)
          {
            if (dv==null) continue;
            if (dv.GetType().IsValueType) continue;
            injector.App.InjectInto(dv);
          }
        }
        else if (v is IEnumerable vedoc) //only then IEnumerable
        {
          foreach(var ev in vedoc)
          {
            if (ev==null) continue;
            if (ev.GetType().IsValueType) continue;
            injector.App.InjectInto(ev);
          }
        }

      }
      return false;
    }
  }
}
