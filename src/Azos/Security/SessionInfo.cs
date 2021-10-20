/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.Security
{
  [Bix("a7cdfa7e-11cc-4263-b56b-5d8d21a202a7")]
  [Schema(Immutable = true, Description = "Provides user session information snapshot in time")]
  public sealed class SessionInfo : TransientModel
  {
    /// <summary>
    /// Defines visibility of ACL levels
    /// </summary>
    public enum AclLevelVisibility
    {
      None = 0,
      Pub,
      PubCascade,
    }

    /// <summary>
    /// If ACL contains an attribute with such name, then the visibility from the level above is changed accordingly
    /// </summary>
    public const string ACL_LEVEL_VISIBILITY_ATTRIBUTE = "--info-visibility";

    /// <summary>
    /// If ACL contains an attribute with such name, then the system will return its value as LEVEL,
    /// this way it is possible to specify general-purpose information access level (e.g. for User interfaces)
    /// </summary>
    public const string ACL_LEVEL_OVERRIDE_ATTRIBUTE = "--info-level";


    [Field(Required = true, Description = "Information timestamp as of which this data was obtained")]
    public DateTime Utc { get; set; }

    [Field(Required = false, Description = "When session was logged-in the last time")]
    public DateTime? LastLoginUtc { get; set; }

    [Field(Required = false, Description = "Types of session login: Unspecified|Human|Robot")]
    public SessionLoginType LastLoginType { get; set; }

    [Field(Required = false, Description = "Session language ISO-639 code; Zero value denotes no preference")]
    public Atom LanguageIso { get; set; }

    [Field(Required = false, Description = "User principal entity id if supported, or null")]
    public EntityId? UserId { get; set; }

    [Field(Required = true, Description = "Principal name")]
    public string UserName { get; set; }

    [Field(Required = true, Description = "Principal description")]
    public string UserDescription { get; set; }

    [Field(Required = true, Description = "Principal status such as: Invalid|User|Admin|System")]
    public UserStatus UserStatus { get; set; }

    [Field(Required = true, Description = "Rights/ACL which this principal has granted")]
    public IConfigSectionNode UserRights { get; set; }


    /// <summary>
    /// Returns an info object initialized from the current ambient call flow.
    /// Takes optional functor to obtain `EntityId` for the user principal object
    /// </summary>
    public static SessionInfo ForAmbientCaller(Func<User, EntityId> fUserId = null) => ForSession(Ambient.CurrentCallSession, fUserId);

    /// <summary>
    /// Returns information about current session. The ACL/Rights is redacted to not disclose non-public information.
    /// Takes optional functor to obtain `EntityId` for the user principal object
    /// </summary>
    public static SessionInfo ForSession(ISession session, Func<User, EntityId> fUserId = null)
    {
      if (session == null) session = NOPSession.Instance;

      var result = new SessionInfo()
      {
        Utc = Ambient.UTCNow,
        LastLoginUtc = session.LastLoginUTC,
        LastLoginType = session.LastLoginType,
        LanguageIso = session.LanguageISOCode,

        UserId = fUserId?.Invoke(session.User),
        UserName = session.User.Name,
        UserDescription = session.User.Description,
        UserStatus = session.User.Status
      };

      var cfg = new MemoryConfiguration();
      cfg.CreateFromNode(session.User.Rights.Root);

      redactAclLevel(cfg.Root, false);
      cfg.Root.ResetModified();

      result.UserRights = cfg.Root;
      return result;
    }

    private static void redactAclLevel(ConfigSectionNode level, bool cascadePub)
    {
      //1 Check visibility
      var visibility = level.Of(ACL_LEVEL_VISIBILITY_ATTRIBUTE).ValueAsEnum(cascadePub ? AclLevelVisibility.PubCascade : AclLevelVisibility.None);

      if (visibility == AclLevelVisibility.None)
      {
        level.Delete();
        return;
      }

      //2 Process this level
      var effectiveAccessLevel = level.Of(ACL_LEVEL_OVERRIDE_ATTRIBUTE, AccessLevel.CONFIG_LEVEL_ATTR).ValueAsInt(0);

      level.DeleteAllAttributes();
      if (effectiveAccessLevel > 0)
      {
        level.AddAttributeNode(AccessLevel.CONFIG_LEVEL_ATTR, effectiveAccessLevel);
      }

      //3 Loop through all child nodes
      foreach (var child in level.Children)
      {
        redactAclLevel(child, visibility == AclLevelVisibility.PubCascade);
      }
    }

  }
}

