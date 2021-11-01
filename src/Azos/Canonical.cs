/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;

namespace Azos
{
  /// <summary>
  /// Handles various canonical aspects like EntityIds
  /// </summary>
  public static class Canonical
  {
    /// <summary>
    /// Canonical entity ids
    /// </summary>
    public static class EntityIds
    {
      /// <summary>
      /// Invalid address constant: [n/a]
      /// </summary>
      public const string ADDRESS_INVALID = "[n/a]";

      public static readonly Atom SYS_IDP = Atom.Encode("idp");
      public static readonly Atom ETP_INVALID = Atom.Encode("invalid");
      public static readonly Atom ETP_IDP_USER_NAME = Atom.Encode("usrn");

      /// <summary>
      /// Gets EntityId for the <see cref="Azos.Security.User"/> principal.
      /// If you do not pass the principal then the system takes it from the ambient call flow
      /// </summary>
      public static EntityId OfUser(Azos.Security.User user = null)
      {
        if (user == null) user = Ambient.CurrentCallUser;

        if (user == null || user.Status < Azos.Security.UserStatus.User)
        {
          return new EntityId(SYS_IDP, ETP_INVALID, Atom.ZERO, ADDRESS_INVALID);
        }

        return new EntityId(SYS_IDP, ETP_IDP_USER_NAME, Atom.ZERO, user.Name);
      }
    }
  }
}
