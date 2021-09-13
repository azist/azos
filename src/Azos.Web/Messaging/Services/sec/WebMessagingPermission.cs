/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Security.Web
{
  /// <summary>
  /// Defines the scopes of allowed messaging actions
  /// </summary>
  public enum MessagingAccessLevel
  {
    Denied = AccessLevel.DENIED,

    /// <summary> The caller can query messages that they sent </summary>
    QueryOwn = AccessLevel.VIEW,
    Minimum = QueryOwn,

    /// <summary> Can send messages </summary>
    Send = AccessLevel.VIEW_CHANGE,

    /// <summary> Can send, query and delete any message </summary>
    Maximum = AccessLevel.VIEW_CHANGE_DELETE
  }


  /// <summary>
  /// The grantee must be able to send web messages or manage message archive
  /// </summary>
  public sealed class MessagingPermission : DataContextualPermission
  {
    public MessagingPermission() : base(AccessLevel.VIEW){ }
    public MessagingPermission(MessagingAccessLevel level) : base((int)level){ }

    public override string Description => "The grantee must be able to send web messages or manage message archive";
  }
}
