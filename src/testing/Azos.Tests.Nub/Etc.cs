/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using Azos.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Azos.Tests.Nub
{
  /// <summary>
  /// Junk test fixture for temp testing
  /// </summary>
  [Runnable]
  public class Etc
  {
    ////////[Run("!write-through", null)]
    ////////public void FileWriteTime()
    ////////{
    ////////  var time = Timeter.StartNew();
    ////////  write("c:\\azos\\t", "abcd", "f1.txt", 12345678);
    ////////  time.Stop();

    ////////  "Passed: {0:n3}".SeeArgs(time.ElapsedSec);
    ////////}

    ////////private void write(string path, string scope, string fname, int v)
    ////////{
    ////////  if (!Directory.Exists(path))
    ////////    Directory.CreateDirectory(path);

    ////////  var scopeDir = Path.Combine(path, scope);
    ////////  if (!Directory.Exists(scopeDir))
    ////////    Directory.CreateDirectory(scopeDir);

    ////////  //On a spinning disk
    ////////  //< 200 ms  (75 ms..190 ms) with fsync()
    ////////  //<   1 ms  without fsync()

    ////////  //On an SSD disk
    ////////  //<6 ms with fsync()
    ////////  //<1 ms  without fsync()
    ////////  using (var fs = new FileStream(Path.Combine(scopeDir, fname), FileMode.Create, FileAccess.Write, FileShare.None, 0xff, FileOptions.WriteThrough))
    ////////  {
    ////////    var buf = Encoding.ASCII.GetBytes(v.ToString());
    ////////    fs.Write(buf, 0, buf.Length);
    ////////    fs.Flush(true);
    ////////  }
    ////////}

  }
}
