/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using System.IO;

namespace Azos.Platform.Abstraction
{
  /// <summary>
  /// Provides functions for working with file system
  /// </summary>
  public interface IPALFileSystem
  {
    /// <summary>
    /// Creates directory and immediately grants it accessibility rules for everyone if it does not exists,
    ///  or returns the existing directory
    /// </summary>
    DirectoryInfo EnsureAccessibleDirectory(string path);
  }
}
