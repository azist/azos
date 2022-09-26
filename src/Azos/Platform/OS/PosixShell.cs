/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Platform.OS
{
  /// <summary>
  /// Facilitates execution of POSIX shell commands like `chmod`, `chown` etc.
  /// </summary>
  public static class PosixShell
  {
    private static readonly string SHELL;

    static PosixShell()
    {
      if (System.IO.File.Exists("/bin/bash")) SHELL = "/bin/bash";
      else
        SHELL = "/bin/sh";
    }


    /// <summary>
    /// Executes a command using default system shell (e.g. `bash`)
    /// </summary>
    public static (int exitCode, string stdOut, bool hasExited, bool wasKilled) Execute(string cmdLine)
    {
      cmdLine.NonBlank(nameof(cmdLine));

      try
      {
        return IOUtils.RunAndCompleteProcess(SHELL, $"-c \"{cmdLine}\"");
      }
      catch(Exception error)
      {
        throw new OsException("{0}.{1} process failed: {2}".Args(nameof(PosixShell), nameof(Execute), error.ToMessageWithType()), error);
      }
    }

    /// <summary>
    /// Executes chmod on a file/dir. Permissions is a standard chmod spec e.g. `777` or `+x` or `a+x` (grant execute for all) etc..
    /// </summary>
    public static bool Chmod(string path, string permissions, bool recurse = false)
    {
      path.NonBlank(nameof(path));
      permissions.NonBlank(nameof(permissions));
      var cmd = recurse ? $"chmod -R {permissions} {path}" : $"chmod {permissions} {path}";

      try
      {
        var got = Execute(cmd);
        return got.hasExited && got.exitCode == 0;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Changes execution bit for a user
    /// </summary>
    public static bool ChmodUserExecute(string path, bool grant, bool recurse) => Chmod(path, $"u{(grant?'+':'-')}x", recurse);


  }
}
