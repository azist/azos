/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Azos.Scripting.Packaging
{
  /// <summary>
  /// Facilitates installation of packages on local machine.
  /// Note: since installation is not a high-perm operation this class uses sync-only method for simplicity
  /// </summary>
  public class Installer
  {
    public delegate void ProgressHandler(string status);

    public Installer(string rootPath, Package package)
    {
      m_RootPath = rootPath.NonBlank();
      System.IO.Directory.Exists(m_RootPath).IsTrue("Existing path `{0}`".Args(m_RootPath));
    }

    private string m_RootPath;

    public event ProgressHandler Progress;


    /// <summary>
    /// Starts the installation
    /// </summary>
    public void Run()
    {

    }

  }
}
