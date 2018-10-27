/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Azos.Platform.Abstraction.NetFramework
{
  internal class PALFileSystem : IPALFileSystem
  {
    public PALFileSystem()
    {
    }

    public DirectoryInfo EnsureAccessibleDirectory(string path)
    {
      FileSystemAccessRule ausersRule = new FileSystemAccessRule(
                  new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), //  "Authenticated Users",
                  FileSystemRights.FullControl,
                  InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                  PropagationFlags.None,
                  AccessControlType.Allow);

      FileSystemAccessRule usersRule = new FileSystemAccessRule(
                  new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), // "Users",
                  FileSystemRights.FullControl,
                  InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                  PropagationFlags.None,
                  AccessControlType.Allow);

      FileSystemAccessRule adminsRule = new FileSystemAccessRule(
                  new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), // "Administrators",
                  FileSystemRights.FullControl,
                  InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                  PropagationFlags.None,
                  AccessControlType.Allow);

      FileSystemAccessRule sysRule = new FileSystemAccessRule(
                  new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), // "SYSTEM",
                  FileSystemRights.FullControl,
                  InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                  PropagationFlags.None,
                  AccessControlType.Allow);

      DirectorySecurity dirSec = new DirectorySecurity();
      dirSec.AddAccessRule(ausersRule);
      dirSec.AddAccessRule(usersRule);
      dirSec.AddAccessRule(adminsRule);
      dirSec.AddAccessRule(sysRule);

      return Directory.CreateDirectory(path, dirSec);
    }

  }
}
