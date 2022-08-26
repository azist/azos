/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;
using System.Reflection;

using Azos;
using Azos.Conf;
using Azos.Platform.ProcessActivation;

namespace sky
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      SkyProgramBody.Main(args, assemblyResolverFixup);
    }

    //AZ #738
    //Occurs when the resolution of an assembly fails when attempting to load into
    //this assembly load context.
    private static void assemblyResolverFixup(IConfigSectionNode manifest)
    {
      manifest.NonNull(nameof(manifest));
      System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += delegate (System.Runtime.Loader.AssemblyLoadContext loadContext, AssemblyName asmName)
      {
        const string CURRENT_DIR = "./";
        foreach (var node in manifest.ChildrenNamed("load-from"))
        {
          var path = node.ValOf("path");//default: co-located DLL file
          if (path.IsNullOrWhiteSpace()) continue;
          if (path.StartsWith(CURRENT_DIR))
          {
            path = Path.Combine(
                                 Directory.GetCurrentDirectory(),
                                 path.Length > CURRENT_DIR.Length ? path.Substring(CURRENT_DIR.Length) : string.Empty
                               ).Trim();
          }

          path = Path.Combine(path, "{0}.dll".Args(asmName.Name));

          if (File.Exists(path))
          {
            return Assembly.LoadFrom(path);
          }
        }

        return null;
      };
    }
  }
}


