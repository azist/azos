/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Apps.Hosting.Skyod
{
  public static class Constraints
  {
    public const int MAX_COMPONENT_NAME_LEN = 16;
    public static void CheckComponentName(string name, string what)
    {
      what.NonBlank(nameof(what));
      if (name.IsNotNullOrWhiteSpace()) throw new AppHostingException("{0} is null or empty".Args(what));
      if (name.Length > MAX_COMPONENT_NAME_LEN) throw new AppHostingException("{0} is too long".Args(what));

      for(int i = 0; i < name.Length; i++)
      {
        var c = name[i];
        if (c == '.' || c == '-' || c=='_') continue;
        if (c >= '0' && c <= '9') continue;
        if (c >= 'A' && c <= 'Z') continue;
        if (c >= 'a' && c <= 'z') continue;
        throw new AppHostingException("{0} contains invalid char `{1}`".Args(what, c));
      }
    }
  }
}
