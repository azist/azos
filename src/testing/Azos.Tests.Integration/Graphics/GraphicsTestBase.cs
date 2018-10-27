/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Azos.Tests.Integration.Graphics
{
  public class GraphicsTestBase
  {
    public Stream GetResource(string name)
    {
      var assembly = Assembly.GetExecutingAssembly();
      var resourceName = "Azos.Tests.Integration.Graphics."+name;

      return assembly.GetManifestResourceStream(resourceName);
    }

  }
}
