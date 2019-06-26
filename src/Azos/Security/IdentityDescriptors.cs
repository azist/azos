/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Security
{
  /// <summary>
  /// Marker interface denoting entities that represent information about various users.
  /// The actual implementation depends on a particular application security system, e.g.
  /// in a social site system one could use descriptors for "user groups", "friend circles", "organizations" etc.
  /// </summary>
  public interface IIdentityDescriptor
  {
    /// <summary>
    /// Represents identity of the descriptor such as User.ID, User.GDID etc.
    /// Specifics depend on the system
    /// </summary>
    object IdentityDescriptorID { get; }

    /// <summary>
    /// Provides descriptor name such as User.Name, User.ScreenName etc.
    /// </summary>
    string IdentityDescriptorName { get; }

    /// <summary>
    /// Denotes types of identities: Users, Groups etc.
    /// </summary>
    IdentityType IdentityDescriptorType { get; }
  }

  /// <summary>
  /// Describes an identity based on a User-derived object
  /// </summary>
  public struct UserIdentityDescriptor : IIdentityDescriptor
  {
    public UserIdentityDescriptor(object id, string name)
    {
      IdentityDescriptorID = id;
      IdentityDescriptorName = name;
    }

    public IdentityType IdentityDescriptorType => IdentityType.User;

    public object IdentityDescriptorID { get; }
    public string IdentityDescriptorName { get; }
  }
}
